using DapperLabs_Lab2.Data;
using Microsoft.Data.SqlClient;
using Dapper;

namespace DapperLabs_Lab2.Repositories
{
    public interface ITelephoneNumberRepository
    {
        Task<TelephoneNumber?> GetByIdAsync(long id);
        Task<IEnumerable<TelephoneNumber>> GetAllAsync();
        Task<IEnumerable<TelephoneNumber>> GetByCustomerIdAsync(long customerId);
        Task<TelephoneNumber> CreateAsync(TelephoneNumber telephoneNumber);
        Task<TelephoneNumber> UpdateAsync(TelephoneNumber telephoneNumber);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
    }

    public class TelephoneNumberRepository : ITelephoneNumberRepository
    {
        private readonly string _connectionString;

        public TelephoneNumberRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<TelephoneNumber?> GetByIdAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM TelephoneNumbers WHERE Id = @Id";
            return await connection.QueryFirstOrDefaultAsync<TelephoneNumber>(sql, new { Id = id });
        }

        public async Task<IEnumerable<TelephoneNumber>> GetAllAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM TelephoneNumbers";
            return await connection.QueryAsync<TelephoneNumber>(sql);
        }

        public async Task<IEnumerable<TelephoneNumber>> GetByCustomerIdAsync(long customerId)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM TelephoneNumbers WHERE CustomerId = @CustomerId";
            return await connection.QueryAsync<TelephoneNumber>(sql, new { CustomerId = customerId });
        }

        public async Task<TelephoneNumber> CreateAsync(TelephoneNumber telephoneNumber)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] INSERT with OUTPUT clause - Get auto-generated ID
            var sql = @"
                INSERT INTO TelephoneNumbers (CustomerId, Type, Number) 
                OUTPUT INSERTED.Id
                VALUES (@CustomerId, @Type, @Number)";
            
            // [DAPPER] ExecuteScalarAsync with OUTPUT clause
            telephoneNumber.Id = await connection.ExecuteScalarAsync<long>(sql, telephoneNumber);
            return telephoneNumber;
        }

        public async Task<TelephoneNumber> UpdateAsync(TelephoneNumber telephoneNumber)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = @"
                UPDATE TelephoneNumbers 
                SET Type = @Type, Number = @Number 
                WHERE Id = @Id";
            
            await connection.ExecuteAsync(sql, telephoneNumber);
            return telephoneNumber;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "DELETE FROM TelephoneNumbers WHERE Id = @Id";
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT COUNT(1) FROM TelephoneNumbers WHERE Id = @Id";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }
    }
}
