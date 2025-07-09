using integration_dispatch.Interfaces;
using integration_dispatch.Model;
using System.Data.SqlClient;

namespace integration_dispatch.Utils
{
    public class DataTransferService : IDataTransferService
    {
        private readonly Config _config;
        private readonly IDatabaseService _dbService;
        private readonly Action<string> _updateStatus;
        private readonly Action<string> _updateCounter;
        private readonly LastRunManager _lastRunManager;

        public DataTransferService(Config config, IDatabaseService dbService, Action<string> updateStatus, Action<string> updateCounter, LastRunManager lastRunManager = null)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dbService = dbService ?? throw new ArgumentNullException(nameof(dbService));
            _updateStatus = updateStatus ?? throw new ArgumentNullException(nameof(updateStatus));
            _updateCounter = updateCounter ?? throw new ArgumentNullException(nameof(updateCounter));
            _lastRunManager = lastRunManager ?? new LastRunManager();
        }

        public async Task RunDataTransferAsync(DateTime startDate)
        {
            using var sourceConn = new SqlConnection(_config.SourceConnectionString);
            using var destConn = new SqlConnection(_config.DestinationConnectionString);

            await sourceConn.OpenAsync();
            await destConn.OpenAsync();

            var tableMappings = _config.TableMappings.ToDictionary(t => t.SourceTable, t => t, StringComparer.OrdinalIgnoreCase);

            if (string.IsNullOrEmpty(_config.TransactionTable) || !tableMappings.TryGetValue(_config.TransactionTable, out var transactionsTable))
                throw new InvalidOperationException("Transaction table mapping not found.");

            var transactions = await _dbService.GetTransactionsAsync(sourceConn, startDate, transactionsTable);
            int total = transactions.Count;
            _updateStatus($"\r\nTotal Orders: {total}\r\n");
            int count = 0;

            foreach (var transaction in transactions)
            {
                count++;
                var keyValues = GetKeyValues(transaction, transactionsTable.WhereMappings);

                using var destTrans = destConn.BeginTransaction();

                try
                {
                    var headerId = await _dbService.InsertHeaderAsync(destConn, destTrans, transaction, transactionsTable);
                    _updateCounter($"{count}/{total}");

                    foreach (var mapping in tableMappings.Values.Where(m => m.SourceTable != transactionsTable.SourceTable))
                    {
                        var where = mapping.WhereMappings?.FirstOrDefault(w => keyValues.ContainsKey(w.SourceKey));
                        if (where == null) continue;

                        var key = keyValues[where.SourceKey];
                        if (string.IsNullOrEmpty(key)) continue;

                        var records = await _dbService.GetRelatedRecordsAsync(sourceConn, key, mapping);
                        if (records == null) continue;

                        if (mapping.DestinationTable.Equals("Lines", StringComparison.OrdinalIgnoreCase))
                        {
                            foreach (var record in records)
                            {
                                var headerMapping = mapping.ColumnMappings.FirstOrDefault(c => c.DestinationColumn == "HeaderId");
                                if (headerMapping != null)
                                    record.Values[headerMapping.SourceColumn] = headerId;
                            }
                        }

                        await _dbService.InsertRecordsAsync(destConn, destTrans, records, mapping);
                    }

                    await destTrans.CommitAsync();
                }
                catch
                {
                    await destTrans.RollbackAsync();
                    throw;
                }
            }

            await _lastRunManager.SaveLastRunDateAsync(startDate);
        }

        private Dictionary<string, string> GetKeyValues(Transaction transaction, List<WhereMapping> whereMappings)
        {
            var result = new Dictionary<string, string>();

            foreach (var mapping in whereMappings ?? new())
            {
                if (!transaction.Values.ContainsKey(mapping.SourceKey)) continue;

                var value = transaction.Values[mapping.SourceKey]?.ToString();
                result[mapping.SourceKey] = value;

                if (mapping.SourceKey == "CUSTACCOUNT")
                    result["ACCOUNTNUM"] = value;

                if (mapping.SourceKey == "STORE")
                    result["STOREID"] = value;
            }

            return result;
        }
    }
}