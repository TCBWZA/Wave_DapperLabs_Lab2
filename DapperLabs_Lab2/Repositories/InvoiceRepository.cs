using DapperLabs_Lab2.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace DapperLabs_Lab2.Repositories
{
    public interface IInvoiceRepository
    {
        Task<Invoice?> GetByIdAsync(long id);
        Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber);
        Task<IEnumerable<Invoice>> GetAllAsync();
        Task<IEnumerable<Invoice>> GetByCustomerIdAsync(long customerId);
        Task<Invoice> CreateAsync(Invoice invoice);
        Task<Invoice> UpdateAsync(Invoice invoice);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, long? excludeInvoiceId = null);
    }

    public class InvoiceRepository : IInvoiceRepository
    {
        private readonly string _connectionString;

        public InvoiceRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Invoice?> GetByIdAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Invoices WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<Invoice>(sql, new { Id = id });
        }

        public async Task<Invoice?> GetByInvoiceNumberAsync(string invoiceNumber)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Invoices WHERE InvoiceNumber = @InvoiceNumber";
            return await connection.QueryFirstOrDefaultAsync<Invoice>(sql, new { InvoiceNumber = invoiceNumber });
        }

        public async Task<IEnumerable<Invoice>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Invoices";
            return await connection.QueryAsync<Invoice>(sql);
        }

        public async Task<IEnumerable<Invoice>> GetByCustomerIdAsync(long customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId";
            return await connection.QueryAsync<Invoice>(sql, new { CustomerId = customerId });
        }

        public async Task<Invoice> CreateAsync(Invoice invoice)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] INSERT with OUTPUT clause - Modern approach to get auto-generated ID
            var sql = @"
                INSERT INTO Invoices (InvoiceNumber, CustomerId, InvoiceDate, Amount) 
                OUTPUT INSERTED.Id
                VALUES (@InvoiceNumber, @CustomerId, @InvoiceDate, @Amount)";
            
            // [DAPPER] ExecuteScalarAsync returns the OUTPUT value
            invoice.Id = await connection.ExecuteScalarAsync<long>(sql, invoice);
            return invoice;
        }

        public async Task<Invoice> UpdateAsync(Invoice invoice)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = @"
                UPDATE Invoices 
                SET InvoiceNumber = @InvoiceNumber, 
                    InvoiceDate = @InvoiceDate, 
                    Amount = @Amount 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, invoice);
            return invoice;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "DELETE FROM Invoices WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT COUNT(1) FROM Invoices WHERE Id = @Id";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> InvoiceNumberExistsAsync(string invoiceNumber, long? excludeInvoiceId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT COUNT(1) FROM Invoices WHERE InvoiceNumber = @InvoiceNumber";
            var parameters = new DynamicParameters();
            parameters.Add("InvoiceNumber", invoiceNumber);
            
            if (excludeInvoiceId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                parameters.Add("ExcludeId", excludeInvoiceId.Value);
            }
            
            var count = await connection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }
    }
}
