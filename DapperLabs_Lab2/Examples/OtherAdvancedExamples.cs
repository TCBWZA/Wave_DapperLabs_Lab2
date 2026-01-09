using DapperLabs_Lab2.Data;
using Microsoft.Data.SqlClient;
using Dapper;
using System.Data;

namespace DapperLabs_Lab2.Examples
{
    /// <summary>
    /// Additional advanced Dapper examples for learning more patterns and techniques.
    /// These examples build on the patterns in AdvancedExamples.cs and demonstrate
    /// additional real-world scenarios and optimization techniques.
    /// </summary>
    public class OtherAdvancedExamples
    {
        private readonly string _connectionString;

        public OtherAdvancedExamples(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// EXAMPLE: Stored Procedure Execution
        /// 
        /// Demonstrates executing stored procedures with Dapper.
        /// Stored procedures provide better performance for complex operations
        /// and allow database-side logic encapsulation.
        /// 
        /// Benefits:
        /// - Pre-compiled execution plans
        /// - Better security (can grant execute without table permissions)
        /// - Easier to optimize and maintain complex queries
        /// - Reduced network traffic
        /// 
        /// Note: This example shows the pattern. You'll need to create the stored
        /// procedure in your database first.
        /// </summary>
        public async Task<IEnumerable<Customer>> GetCustomersByStoredProcedureAsync(decimal minBalance)
        {
            using var connection = new SqlConnection(_connectionString);

            // Example stored procedure call
            // The stored procedure would need to be created first:
            // CREATE PROCEDURE GetCustomersWithMinBalance
            //   @MinBalance DECIMAL(18,2)
            // AS
            // BEGIN
            //   SELECT c.* FROM Customers c
            //   WHERE (SELECT ISNULL(SUM(Amount), 0) FROM Invoices WHERE CustomerId = c.Id) >= @MinBalance
            // END

            var customers = await connection.QueryAsync<Customer>(
                "GetCustomersWithMinBalance",
                new { MinBalance = minBalance },
                commandType: CommandType.StoredProcedure
            );

            return customers;
        }

        /// <summary>
        /// EXAMPLE: Table-Valued Parameters for Bulk Insert
        /// 
        /// Demonstrates using table-valued parameters for efficient bulk inserts.
        /// This is much faster than inserting one row at a time.
        /// 
        /// Performance comparison for 1000 records:
        /// - Individual inserts: ~10-15 seconds
        /// - Table-valued parameter: ~0.5-1 second
        /// 
        /// Setup required:
        /// 1. Create user-defined table type
        /// 2. Create stored procedure accepting table type
        /// 
        /// SQL Setup:
        /// CREATE TYPE CustomerTableType AS TABLE
        /// (
        ///     Name NVARCHAR(200),
        ///     Email NVARCHAR(200)
        /// )
        /// 
        /// CREATE PROCEDURE BulkInsertCustomers
        ///     @Customers CustomerTableType READONLY
        /// AS
        /// BEGIN
        ///     INSERT INTO Customers (Name, Email)
        ///     SELECT Name, Email FROM @Customers
        /// END
        /// </summary>
        public async Task<int> BulkInsertCustomersAsync(List<Customer> customers)
        {
            using var connection = new SqlConnection(_connectionString);

            // Create DataTable for table-valued parameter
            var dataTable = new DataTable();
            dataTable.Columns.Add("Name", typeof(string));
            dataTable.Columns.Add("Email", typeof(string));

            foreach (var customer in customers)
            {
                dataTable.Rows.Add(customer.Name, customer.Email);
            }

            var parameters = new DynamicParameters();
            parameters.Add("@Customers", dataTable.AsTableValuedParameter("CustomerTableType"));

            return await connection.ExecuteAsync(
                "BulkInsertCustomers",
                parameters,
                commandType: CommandType.StoredProcedure
            );
        }

        /// <summary>
        /// EXAMPLE: Query Multiple Result Sets
        /// 
        /// Demonstrates using QueryMultiple to execute multiple queries in one round trip.
        /// This is more efficient than making separate database calls.
        /// 
        /// Benefits:
        /// - Single network round trip
        /// - Better performance than separate queries
        /// - All results returned atomically
        /// 
        /// Use cases:
        /// - Dashboard data loading
        /// - Master-detail views
        /// - Related statistics
        /// </summary>
        public async Task<CustomerWithStats> GetCustomerWithStatsAsync(long customerId)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                SELECT * FROM Customers WHERE Id = @Id;
                
                SELECT * FROM Invoices WHERE CustomerId = @Id;
                
                SELECT * FROM TelephoneNumbers WHERE CustomerId = @Id;
                
                SELECT 
                    COUNT(*) AS InvoiceCount,
                    ISNULL(SUM(Amount), 0) AS TotalAmount,
                    ISNULL(AVG(Amount), 0) AS AverageAmount,
                    MAX(InvoiceDate) AS LastInvoiceDate
                FROM Invoices 
                WHERE CustomerId = @Id;";

            using var multi = await connection.QueryMultipleAsync(sql, new { Id = customerId });

            var customer = await multi.ReadFirstOrDefaultAsync<Customer>();
            if (customer == null)
                return null!;

            customer.Invoices = (await multi.ReadAsync<Invoice>()).ToList();
            customer.PhoneNumbers = (await multi.ReadAsync<TelephoneNumber>()).ToList();
            var stats = await multi.ReadFirstOrDefaultAsync<InvoiceStats>();

            return new CustomerWithStats
            {
                Customer = customer,
                InvoiceCount = stats?.InvoiceCount ?? 0,
                TotalAmount = stats?.TotalAmount ?? 0,
                AverageAmount = stats?.AverageAmount ?? 0,
                LastInvoiceDate = stats?.LastInvoiceDate
            };
        }

        /// <summary>
        /// EXAMPLE: Query with Buffering Control
        /// 
        /// Demonstrates controlling query buffering for memory optimization.
        /// By default, Dapper buffers all results in memory.
        /// For large result sets, unbuffered queries can reduce memory usage.
        /// 
        /// Buffered (default):
        /// - Faster enumeration
        /// - Higher memory usage
        /// - Connection released immediately
        /// 
        /// Unbuffered (buffered: false):
        /// - Streaming results
        /// - Lower memory usage
        /// - Connection held open during enumeration
        /// - Must enumerate or connection stays open
        /// </summary>
        public async Task<IEnumerable<Customer>> GetCustomersUnbufferedAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            // Unbuffered query - streams results instead of loading all into memory
            // Important: Connection must stay open while enumerating
            var sql = "SELECT * FROM Customers";
            var customers = await connection.QueryAsync<Customer>(sql);

            // Note: In Dapper 2.x, buffering control is handled differently
            // For truly unbuffered queries, you would use Query (not QueryAsync) 
            // and enumerate with yield return in a custom method
            // This example shows the async pattern which is buffered
            return customers;
        }

        /// <summary>
        /// EXAMPLE: Custom Type Handlers
        /// 
        /// Demonstrates using custom type handlers for complex type mapping.
        /// Type handlers allow you to control how Dapper maps between .NET types
        /// and database types.
        /// 
        /// Common use cases:
        /// - JSON columns
        /// - Encrypted data
        /// - Custom value objects
        /// - Enum handling
        /// 
        /// Note: Register handler once at application startup:
        /// SqlMapper.AddTypeHandler(new JsonTypeHandler<List<string>>());
        /// </summary>
        public class JsonTypeHandler<T> : SqlMapper.TypeHandler<T>
        {
            public override void SetValue(IDbDataParameter parameter, T value)
            {
                parameter.Value = System.Text.Json.JsonSerializer.Serialize(value);
                parameter.DbType = DbType.String;
            }

            public override T Parse(object value)
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(value.ToString()!)!;
            }
        }

        /// <summary>
        /// EXAMPLE: Window Functions for Advanced Analytics
        /// 
        /// Demonstrates using SQL window functions for analytics.
        /// Window functions provide powerful analytical capabilities.
        /// 
        /// Common window functions:
        /// - ROW_NUMBER(): Sequential number
        /// - RANK(): Ranking with gaps
        /// - DENSE_RANK(): Ranking without gaps
        /// - NTILE(): Distribution into buckets
        /// - LAG/LEAD: Previous/next row values
        /// 
        /// Use cases:
        /// - Rankings and leaderboards
        /// - Running totals
        /// - Moving averages
        /// - Percentiles
        /// </summary>
        public async Task<IEnumerable<CustomerRanking>> GetCustomerRankingsByRevenueAsync()
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                WITH CustomerRevenue AS (
                    SELECT 
                        c.Id,
                        c.Name,
                        c.Email,
                        ISNULL(SUM(i.Amount), 0) AS TotalRevenue
                    FROM Customers c
                    LEFT JOIN Invoices i ON c.Id = i.CustomerId
                    GROUP BY c.Id, c.Name, c.Email
                )
                SELECT 
                    Id,
                    Name,
                    Email,
                    TotalRevenue,
                    ROW_NUMBER() OVER (ORDER BY TotalRevenue DESC) AS Rank,
                    NTILE(10) OVER (ORDER BY TotalRevenue DESC) AS Decile,
                    PERCENT_RANK() OVER (ORDER BY TotalRevenue) AS PercentRank
                FROM CustomerRevenue
                ORDER BY TotalRevenue DESC";

            return await connection.QueryAsync<CustomerRanking>(sql);
        }

        /// <summary>
        /// EXAMPLE: Common Table Expressions (CTEs)
        /// 
        /// Demonstrates using CTEs for complex hierarchical or multi-step queries.
        /// CTEs make complex queries more readable and maintainable.
        /// 
        /// Benefits:
        /// - Better readability
        /// - Reusable within query
        /// - Can reference itself (recursive CTEs)
        /// - Easier to debug and test
        /// 
        /// Use cases:
        /// - Hierarchical data (org charts, categories)
        /// - Multi-step calculations
        /// - Breaking down complex queries
        /// </summary>
        public async Task<IEnumerable<MonthlyRevenue>> GetMonthlyRevenueWithTrendAsync(int year)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                WITH MonthlyData AS (
                    SELECT 
                        YEAR(InvoiceDate) AS Year,
                        MONTH(InvoiceDate) AS Month,
                        SUM(Amount) AS Revenue
                    FROM Invoices
                    WHERE YEAR(InvoiceDate) = @Year
                    GROUP BY YEAR(InvoiceDate), MONTH(InvoiceDate)
                ),
                RevenueWithPrevious AS (
                    SELECT 
                        Year,
                        Month,
                        Revenue,
                        LAG(Revenue) OVER (ORDER BY Year, Month) AS PreviousMonthRevenue
                    FROM MonthlyData
                )
                SELECT 
                    Year,
                    Month,
                    Revenue,
                    PreviousMonthRevenue,
                    CASE 
                        WHEN PreviousMonthRevenue IS NULL THEN 0
                        WHEN PreviousMonthRevenue = 0 THEN 0
                        ELSE ((Revenue - PreviousMonthRevenue) / PreviousMonthRevenue * 100)
                    END AS GrowthPercentage
                FROM RevenueWithPrevious
                ORDER BY Year, Month";

            return await connection.QueryAsync<MonthlyRevenue>(sql, new { Year = year });
        }

        /// <summary>
        /// EXAMPLE: Retry Logic for Transient Failures
        /// 
        /// Demonstrates implementing retry logic for transient database failures.
        /// Important for cloud databases and network issues.
        /// 
        /// Common transient errors:
        /// - Timeout errors
        /// - Connection errors
        /// - Deadlocks
        /// 
        /// Strategy:
        /// - Exponential backoff
        /// - Limited retry attempts
        /// - Only retry transient errors
        /// </summary>
        public async Task<T> ExecuteWithRetryAsync<T>(Func<SqlConnection, Task<T>> operation, int maxRetries = 3)
        {
            int retryCount = 0;
            int delayMs = 100;

            while (true)
            {
                try
                {
                    using var connection = new SqlConnection(_connectionString);
                    return await operation(connection);
                }
                catch (SqlException ex) when (IsTransientError(ex) && retryCount < maxRetries)
                {
                    retryCount++;
                    Console.WriteLine($"Transient error occurred. Retry attempt {retryCount}/{maxRetries}. Waiting {delayMs}ms...");
                    await Task.Delay(delayMs);
                    delayMs *= 2; // Exponential backoff
                }
            }
        }

        private bool IsTransientError(SqlException ex)
        {
            // Common transient error numbers
            int[] transientErrorNumbers = new[]
            {
                -2,     // Timeout
                -1,     // Connection broken
                1205,   // Deadlock victim
                40197,  // Service unavailable
                40501,  // Service busy
                40613,  // Database unavailable
                49918,  // Cannot process request
                49919,  // Too many requests
                49920   // Not enough resources
            };

            return transientErrorNumbers.Contains(ex.Number);
        }

        /// <summary>
        /// EXAMPLE: Query Caching Pattern
        /// 
        /// Demonstrates implementing a simple cache for query results.
        /// Reduces database load for frequently accessed, slowly changing data.
        /// 
        /// When to use:
        /// - Reference data (rarely changes)
        /// - Expensive queries
        /// - High read frequency
        /// 
        /// Note: In production, use a real caching library like:
        /// - MemoryCache (built-in)
        /// - Redis
        /// - Distributed cache
        /// </summary>
        private static readonly Dictionary<string, (DateTime Expiry, object Data)> _cache = new();
        private static readonly object _cacheLock = new();

        public async Task<IEnumerable<Customer>> GetCustomersWithCacheAsync(int cacheMinutes = 5)
        {
            string cacheKey = "all_customers";

            lock (_cacheLock)
            {
                if (_cache.TryGetValue(cacheKey, out var cached) && cached.Expiry > DateTime.UtcNow)
                {
                    Console.WriteLine("Returning cached data");
                    return (IEnumerable<Customer>)cached.Data;
                }
            }

            Console.WriteLine("Fetching from database");
            using var connection = new SqlConnection(_connectionString);
            var customers = (await connection.QueryAsync<Customer>("SELECT * FROM Customers")).ToList();

            lock (_cacheLock)
            {
                _cache[cacheKey] = (DateTime.UtcNow.AddMinutes(cacheMinutes), customers);
            }

            return customers;
        }

        /// <summary>
        /// EXAMPLE: Upsert Pattern (Insert or Update)
        /// 
        /// Demonstrates implementing upsert (insert if not exists, update if exists).
        /// Uses SQL MERGE statement for atomic operation.
        /// 
        /// Benefits:
        /// - Atomic operation
        /// - Single database round trip
        /// - Handles race conditions
        /// 
        /// Alternative: Use ON DUPLICATE KEY UPDATE (MySQL) or INSERT ... ON CONFLICT (PostgreSQL)
        /// </summary>
        public async Task<Customer> UpsertCustomerAsync(Customer customer)
        {
            using var connection = new SqlConnection(_connectionString);

            var sql = @"
                MERGE Customers AS target
                USING (SELECT @Id AS Id, @Name AS Name, @Email AS Email) AS source
                ON (target.Id = source.Id)
                WHEN MATCHED THEN
                    UPDATE SET Name = source.Name, Email = source.Email
                WHEN NOT MATCHED THEN
                    INSERT (Name, Email) VALUES (source.Name, source.Email)
                OUTPUT INSERTED.*;";

            var result = await connection.QuerySingleAsync<Customer>(sql, customer);
            return result;
        }
    }

    #region DTOs for Examples

    public class CustomerWithStats
    {
        public Customer Customer { get; set; } = null!;
        public int InvoiceCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public DateTime? LastInvoiceDate { get; set; }
    }

    public class InvoiceStats
    {
        public int InvoiceCount { get; set; }
        public decimal TotalAmount { get; set; }
        public decimal AverageAmount { get; set; }
        public DateTime? LastInvoiceDate { get; set; }
    }

    public class CustomerRanking
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public decimal TotalRevenue { get; set; }
        public long Rank { get; set; }
        public int Decile { get; set; }
        public double PercentRank { get; set; }
    }

    public class MonthlyRevenue
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public decimal Revenue { get; set; }
        public decimal? PreviousMonthRevenue { get; set; }
        public decimal GrowthPercentage { get; set; }
    }

    #endregion
}
