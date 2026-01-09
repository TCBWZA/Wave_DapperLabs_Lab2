using DapperLabs_Lab2.Data;
using Microsoft.Data.SqlClient; // [DAPPER] SqlConnection - Required for Dapper database connections
using Dapper; // [DAPPER] Dapper namespace - Core Dapper ORM functionality

namespace DapperLabs_Lab2.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(long id, bool includeRelated = false);
        Task<Customer?> GetByEmailAsync(string email);
        Task<IEnumerable<Customer>> GetAllAsync(bool includeRelated = false);
        Task<Customer> CreateAsync(Customer customer);
        Task<Customer> UpdateAsync(Customer customer);
        Task<bool> DeleteAsync(long id);
        Task<bool> ExistsAsync(long id);
        Task<bool> EmailExistsAsync(string email, long? excludeCustomerId = null);
        Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(int page, int pageSize, bool includeRelated = false);
        Task<IEnumerable<Customer>> SearchAsync(string? name, string? email, decimal? minBalance);
    }

    public class CustomerRepository : ICustomerRepository
    {
        // [DAPPER] Connection string - Dapper uses connection strings directly (no DbContext)
        private readonly string _connectionString;

        public CustomerRepository(string connectionString)
        {
            _connectionString = connectionString;
        }

        public async Task<Customer?> GetByIdAsync(long id, bool includeRelated = false)
        {
            // [DAPPER] SqlConnection with using statement - Manual connection management
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] Raw SQL - Dapper executes SQL directly
            var sql = "SELECT * FROM Customers WHERE Id = @Id";
            // [DAPPER] QueryFirstOrDefaultAsync<T> - Maps single row to object, returns null if not found
            // [DAPPER] Anonymous object for parameters - Dapper automatically parameterizes
            var customer = await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
            
            if (customer != null && includeRelated)
            {
                // [DAPPER] Manual loading of related data - No automatic eager loading like EF Core
                await LoadRelatedDataAsync(connection, customer);
            }
            
            return customer;
        }

        public async Task<Customer?> GetByEmailAsync(string email)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Customers WHERE Email = @Email";
            // [DAPPER] Parameterized query - SQL injection safe
            return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Email = email });
        }

        public async Task<IEnumerable<Customer>> GetAllAsync(bool includeRelated = false)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT * FROM Customers";
            // [DAPPER] QueryAsync<T> - Maps multiple rows to objects
            var customers = (await connection.QueryAsync<Customer>(sql)).ToList();
            
            if (includeRelated && customers.Any())
            {
                // [DAPPER] Manual batch loading to avoid N+1 queries
                await LoadRelatedDataForMultipleAsync(connection, customers);
            }
            
            return customers;
        }

        public async Task<(IEnumerable<Customer> Items, int TotalCount)> GetPagedAsync(
            int page, 
            int pageSize, 
            bool includeRelated = false)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var countSql = "SELECT COUNT(*) FROM Customers";
            // [DAPPER] ExecuteScalarAsync<T> - Returns single value (for COUNT, SUM, etc.)
            var totalCount = await connection.ExecuteScalarAsync<int>(countSql);
            
            // [DAPPER] SQL OFFSET/FETCH for pagination - Direct SQL pagination
            var sql = @"
                SELECT * FROM Customers 
                ORDER BY Name 
                OFFSET @Offset ROWS 
                FETCH NEXT @PageSize ROWS ONLY";
            
            // [DAPPER] Multiple parameters - Dapper maps object properties to SQL parameters
            var customers = (await connection.QueryAsync<Customer>(
                sql, 
                new { Offset = (page - 1) * pageSize, PageSize = pageSize })).ToList();
            
            if (includeRelated && customers.Any())
            {
                await LoadRelatedDataForMultipleAsync(connection, customers);
            }
            
            return (customers, totalCount);
        }

        public async Task<IEnumerable<Customer>> SearchAsync(
            string? name, 
            string? email, 
            decimal? minBalance)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var conditions = new List<string>();
            // [DAPPER] DynamicParameters - For building dynamic SQL with parameters
            var parameters = new DynamicParameters();
            
            // [DAPPER] Dynamic WHERE clause building - Safe parameter addition
            if (!string.IsNullOrEmpty(name))
            {
                conditions.Add("Name LIKE @Name");
                parameters.Add("Name", $"%{name}%"); // [DAPPER] LIKE query parameter
            }
            
            if (!string.IsNullOrEmpty(email))
            {
                conditions.Add("Email LIKE @Email");
                parameters.Add("Email", $"%{email}%");
            }
            
            if (minBalance.HasValue)
            {
                // [DAPPER] Subquery in WHERE clause - Complex SQL logic
                conditions.Add(@"(SELECT ISNULL(SUM(Amount), 0) 
                                 FROM Invoices 
                                 WHERE CustomerId = Customers.Id) >= @MinBalance");
                parameters.Add("MinBalance", minBalance.Value);
            }
            
            var sql = "SELECT * FROM Customers";
            if (conditions.Any())
            {
                sql += " WHERE " + string.Join(" AND ", conditions);
            }
            
            // [DAPPER] QueryAsync with DynamicParameters - Execute dynamic SQL safely
            return await connection.QueryAsync<Customer>(sql, parameters);
        }

        public async Task<Customer> CreateAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] INSERT with OUTPUT clause - Get auto-generated ID (modern approach, preferred over SCOPE_IDENTITY)
            // OUTPUT clause returns the inserted row immediately, avoiding separate query
            var sql = @"
                INSERT INTO Customers (Name, Email) 
                OUTPUT INSERTED.Id
                VALUES (@Name, @Email)";
            
            // [DAPPER] ExecuteScalarAsync returns the OUTPUT value (inserted ID)
            // [DAPPER] Object parameter - Dapper maps object properties to SQL parameters
            customer.Id = await connection.ExecuteScalarAsync<long>(sql, customer);
            return customer;
        }

        public async Task<Customer> UpdateAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] UPDATE statement - Direct SQL execution
            var sql = @"
                UPDATE Customers 
                SET Name = @Name, Email = @Email 
                WHERE Id = @Id";
            
            // [DAPPER] ExecuteAsync - Executes non-query (INSERT, UPDATE, DELETE)
            await connection.ExecuteAsync(sql, customer);
            return customer;
        }

        public async Task<bool> DeleteAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] DELETE statement
            var sql = "DELETE FROM Customers WHERE Id = @Id";
            // [DAPPER] ExecuteAsync returns rows affected
            var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });
            return rowsAffected > 0;
        }

        public async Task<bool> ExistsAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] COUNT(1) for existence check - Efficient existence query
            var sql = "SELECT COUNT(1) FROM Customers WHERE Id = @Id";
            var count = await connection.ExecuteScalarAsync<int>(sql, new { Id = id });
            return count > 0;
        }

        public async Task<bool> EmailExistsAsync(string email, long? excludeCustomerId = null)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var sql = "SELECT COUNT(1) FROM Customers WHERE Email = @Email";
            var parameters = new DynamicParameters();
            parameters.Add("Email", email);
            
            // [DAPPER] Conditional parameter addition - Dynamic SQL building
            if (excludeCustomerId.HasValue)
            {
                sql += " AND Id != @ExcludeId";
                parameters.Add("ExcludeId", excludeCustomerId.Value);
            }
            
            var count = await connection.ExecuteScalarAsync<int>(sql, parameters);
            return count > 0;
        }

        // [DAPPER] Manual related data loading - Load child entities separately
        private async Task LoadRelatedDataAsync(SqlConnection connection, Customer customer)
        {
            var invoiceSql = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId";
            // [DAPPER] QueryAsync maps result to Invoice objects
            customer.Invoices = (await connection.QueryAsync<Invoice>(
                invoiceSql, 
                new { CustomerId = customer.Id })).ToList();
            
            var phoneSql = "SELECT * FROM TelephoneNumbers WHERE CustomerId = @CustomerId";
            customer.PhoneNumbers = (await connection.QueryAsync<TelephoneNumber>(
                phoneSql, 
                new { CustomerId = customer.Id })).ToList();
        }

        // [DAPPER] Batch loading to avoid N+1 queries - Load related data for multiple entities at once
        private async Task LoadRelatedDataForMultipleAsync(SqlConnection connection, List<Customer> customers)
        {
            var customerIds = customers.Select(c => c.Id).ToList();
            
            // [DAPPER] IN clause with collection - Dapper handles collection parameters
            var invoiceSql = "SELECT * FROM Invoices WHERE CustomerId IN @CustomerIds";
            var invoices = (await connection.QueryAsync<Invoice>(
                invoiceSql, 
                new { CustomerIds = customerIds })).ToList();
            
            var phoneSql = "SELECT * FROM TelephoneNumbers WHERE CustomerId IN @CustomerIds";
            var phones = (await connection.QueryAsync<TelephoneNumber>(
                phoneSql, 
                new { CustomerIds = customerIds })).ToList();
            
            // [DAPPER] Manual relationship mapping - Map loaded data to parent objects
            foreach (var customer in customers)
            {
                customer.Invoices = invoices.Where(i => i.CustomerId == customer.Id).ToList();
                customer.PhoneNumbers = phones.Where(p => p.CustomerId == customer.Id).ToList();
            }
        }
    }
}
