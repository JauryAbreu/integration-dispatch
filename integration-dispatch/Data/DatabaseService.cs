using Dapper;
using integration_dispatch.Interfaces;
using integration_dispatch.Model;
using System.Data.SqlClient;

namespace integration_dispatch.Data
{
    public class DatabaseService : IDatabaseService
    {
        private readonly Config _config;

        public DatabaseService(Config config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        public async Task<List<Transaction>> GetTransactionsAsync(SqlConnection conn, DateTime startDate, TableMapping table)
        {
            var columns = string.Join(", ", table.ColumnMappings.Select(c => c.SourceColumn));
            var date = table.WhereMappings?.FirstOrDefault(w => w.DestinationKey == "CreatedDate");
            var receipt = table.WhereMappings?.FirstOrDefault(w => w.DestinationKey == "ReceiptId");
            var payment = table.WhereMappings?.FirstOrDefault(w => w.DestinationKey == "Payment");

            if (date == null || receipt == null || payment == null)
                throw new InvalidOperationException("Missing mapping for date, receipt, or payment.");

            var query = $"SELECT {columns} FROM {table.SourceTable} WHERE {date.SourceKey} >= @date AND {receipt.SourceKey} != '' AND {payment.SourceKey} >= 0";
            var parameters = new { date = startDate };

            var rows = await conn.QueryAsync(query, parameters);
            return rows.Select(row => new Transaction { Values = row }).ToList();
        }

        public async Task<List<Transaction>> GetRelatedRecordsAsync(SqlConnection conn, string key, TableMapping table)
        {
            var where = table.WhereMappings?.FirstOrDefault();
            if (where == null) return null;

            var baseColumns = table.ColumnMappings.Select(c => $"{table.SourceTable}.{c.SourceColumn}").ToList();
            var joins = new List<string>();
            var allColumns = new List<string>(baseColumns);

            if (table.JoinTables?.Any() == true)
            {
                foreach (var jt in table.JoinTables)
                {
                    var joinCols = jt.ColumnMappings.Select(c => $"{jt.SourceTable}.{c.SourceColumn}");
                    allColumns.AddRange(joinCols);
                    joins.Add($"LEFT JOIN {jt.SourceTable} ON {table.SourceTable}.{jt.JoinCondition.SourceKey} = {jt.SourceTable}.{jt.JoinCondition.TargetKey}");
                }
            }

            var query = $"SELECT {string.Join(", ", allColumns)} FROM {table.SourceTable} {string.Join(" ", joins)} WHERE {table.SourceTable}.{where.SourceKey} = @key";
            var rows = await conn.QueryAsync(query, new { key });
            return rows.Select(row => new Transaction { Values = row }).ToList();
        }

        public async Task<int> InsertHeaderAsync(SqlConnection conn, SqlTransaction trans, Transaction transaction, TableMapping table)
        {
            var where = table.WhereMappings?.FirstOrDefault(w => w.DestinationKey == "ReceiptId");

            if (where != null)
            {
                var sourceCol = table.ColumnMappings.FirstOrDefault(c => c.DestinationColumn == where.DestinationKey)?.SourceColumn;
                if (sourceCol != null && transaction.Values.TryGetValue(sourceCol, out var value) && value != null)
                {
                    var checkQuery = $"SELECT Id FROM {table.DestinationTable} WHERE {where.DestinationKey} = @key";
                    var result = await conn.ExecuteScalarAsync<int?>(checkQuery, new { key = value }, trans);
                    if (result.HasValue)
                        return result.Value;
                }
            }

            var columns = table.ColumnMappings.Select(c => c.DestinationColumn).ToList();
            var values = columns.Select(c => "@" + c).ToList();
            var query = $"INSERT INTO {table.DestinationTable} ({string.Join(", ", columns)}) OUTPUT INSERTED.Id VALUES ({string.Join(", ", values)})";
            var parameters = table.ColumnMappings.ToDictionary(c => c.DestinationColumn, c => transaction.Values.TryGetValue(c.SourceColumn, out var val) ? val : DBNull.Value);

            return await conn.ExecuteScalarAsync<int>(query, parameters, trans);
        }

        public async Task InsertRecordsAsync(SqlConnection conn, SqlTransaction trans, List<Transaction> records, TableMapping table)
        {
            foreach (var record in records)
            {
                var whereMappings = table.WhereMappings;
                if (whereMappings?.Count > 0)
                {
                    var conditions = new List<string>();
                    var dataParameters = new Dictionary<string, object>();

                    foreach (var where in whereMappings)
                    {
                        var sourceColumn = table.ColumnMappings.FirstOrDefault(c => c.DestinationColumn == where.DestinationKey)?.SourceColumn;
                        if (!string.IsNullOrEmpty(sourceColumn) && record.Values.TryGetValue(sourceColumn, out var value) && value != null)
                        {
                            var paramName = $"@{where.DestinationKey}";
                            conditions.Add($"{where.DestinationKey} = {paramName}");
                            dataParameters[paramName] = value;
                        }
                    }

                    if (conditions.Count > 0)
                    {
                        var whereClause = string.Join(" AND ", conditions);
                        var checkQuery = $"SELECT COUNT(1) FROM {table.DestinationTable} WHERE {whereClause}";
                        var exists = await conn.ExecuteScalarAsync<int>(checkQuery, dataParameters, trans);
                        if (exists > 0) continue;
                    }
                }

                var columns = new List<string>();
                var mappings = new List<ColumnMapping>();
                columns.AddRange(table.ColumnMappings.Select(c => c.DestinationColumn));
                mappings.AddRange(table.ColumnMappings);

                if (table.JoinTables?.Any() == true)
                {
                    var joinTable = table.JoinTables.First();
                    columns.AddRange(joinTable.ColumnMappings.Select(c => c.DestinationColumn));
                    mappings.AddRange(joinTable.ColumnMappings);
                }

                var values = columns.Select(c => "@" + c);
                var insertQuery = $"INSERT INTO {table.DestinationTable} ({string.Join(", ", columns)}) VALUES ({string.Join(", ", values)})";
                var parameters = new Dictionary<string, object>();
                bool save = true;

                foreach (var map in mappings)
                {
                    var paramName = "@" + map.DestinationColumn;
                    var paramValue = record.Values.TryGetValue(map.SourceColumn, out var val) ? val : DBNull.Value;

                    if (_config.LinesTable == table.DestinationTable)
                    {
                        if (_config.CanBeDispatched == map.DestinationColumn)
                        {
                            try
                            {
                                if (!Convert.ToBoolean(record.Values[map.SourceColumn]))
                                    save = false;
                                else
                                {
                                    var updateQuery = $"UPDATE {_config.HeaderTable} SET STATUS = 4 WHERE ID = @headerId AND STATUS = 5";
                                    await conn.ExecuteAsync(updateQuery, new { headerId = record.Values[_config.HeaderId] }, trans);
                                }
                            }
                            catch
                            {
                                save = false;
                            }
                        }
                        if (_config.Quantity == map.DestinationColumn)
                            paramValue = Math.Abs(Convert.ToInt32(record.Values[map.SourceColumn])).ToString() ?? "1";
                    }

                    parameters[paramName] = paramValue;
                }

                if (save)
                    await conn.ExecuteAsync(insertQuery, parameters, trans);
            }
        }
    }
}
