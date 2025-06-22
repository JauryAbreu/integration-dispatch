using integration_dispatch.Model;
using System.Data.SqlClient;

namespace integration_dispatch.Interfaces
{
    public interface IDatabaseService
    {
        Task<List<Transaction>> GetTransactionsAsync(SqlConnection conn, DateTime startDate, TableMapping table);
        Task<List<Transaction>> GetRelatedRecordsAsync(SqlConnection conn, string key, TableMapping table);
        Task<int> InsertHeaderAsync(SqlConnection conn, SqlTransaction trans, Transaction transaction, TableMapping table);
        Task InsertRecordsAsync(SqlConnection conn, SqlTransaction trans, List<Transaction> records, TableMapping table);
    }
}
