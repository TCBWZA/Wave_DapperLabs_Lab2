using DapperLabs_Lab2.Data;
using DapperLabs_Lab2.Repositories;
using Microsoft.Data.SqlClient;
using Dapper; // [DAPPER] Core Dapper namespace

namespace DapperLabs_Lab2.Examples
{
    public class AdvancedExamples
    {
        // [DAPPER] Connection string - Dapper requires direct connection string
        private readonly string _connectionString;
        private readonly ICustomerRepository _customerRepository;

        public AdvancedExamples(string connectionString, ICustomerRepository customerRepository)
        {
            _connectionString = connectionString;
            _customerRepository = customerRepository;
        }

        /// <summary>
        /// EXAMPLE: Projection with custom SQL for efficient queries
        /// 
        /// Demonstrates using custom SQL to project data instead of loading full entities.
        /// This is MUCH more efficient than loading entities and mapping in memory.
        /// 
        /// Key Benefits:
        /// 1. Only selected columns retrieved from database (not entire rows)
        /// 2. Aggregations (Count, Sum, Max) calculated in SQL, not in memory
        /// 3. No unnecessary object creation
        /// 4. Significantly less data transferred from database
        /// 
        /// SQL Generated:
        /// SELECT 
        ///   c.Id,
        ///   c.Name, 
        ///   c.Email,
        ///   COUNT(i.Id) AS InvoiceCount,
        ///   ISNULL(SUM(i.Amount), 0) AS TotalAmount,
        ///   MAX(i.InvoiceDate) AS LastInvoiceDate
        /// FROM Customers c
        /// LEFT JOIN Invoices i ON c.Id = i.CustomerId
        /// GROUP BY c.Id, c.Name, c.Email
        /// 
        /// When to use:
        /// - List views (don't need all entity data)
        /// - Reports and dashboards
        /// - API responses (shape data for client needs)
        /// </summary>
        public async Task<IEnumerable<CustomerSummary>> GetCustomerSummaryAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] Complex JOIN with GROUP BY - All aggregations done in SQL
            var sql = @"
                SELECT 
                    c.Id,
                    c.Name,
                    c.Email,
                    COUNT(i.Id) AS InvoiceCount,
                    ISNULL(SUM(i.Amount), 0) AS TotalAmount,
                    MAX(i.InvoiceDate) AS LastInvoiceDate
                FROM Customers c
                LEFT JOIN Invoices i ON c.Id = i.CustomerId
                GROUP BY c.Id, c.Name, c.Email";
            
            // [DAPPER] QueryAsync<T> - Maps result directly to custom DTO
            return await connection.QueryAsync<CustomerSummary>(sql);
        }

        /// <summary>
        /// EXAMPLE: GroupBy and Aggregations
        /// 
        /// Demonstrates SQL aggregation functions.
        /// All calculations performed in database, not in application memory.
        /// 
        /// This example groups invoices by year and calculates statistics.
        /// Essential for reporting, dashboards, and analytics.
        /// 
        /// SQL Generated:
        /// SELECT 
        ///   YEAR(i.InvoiceDate) AS Year,
        ///   SUM(i.Amount) AS TotalRevenue,
        ///   COUNT(*) AS InvoiceCount,
        ///   AVG(i.Amount) AS AverageAmount,
        ///   MIN(i.Amount) AS MinAmount,
        ///   MAX(i.Amount) AS MaxAmount
        /// FROM Invoices i
        /// GROUP BY YEAR(i.InvoiceDate)
        /// ORDER BY YEAR(i.InvoiceDate)
        /// 
        /// Available aggregation functions:
        /// - Count(): Number of items
        /// - Sum(): Total of numeric values
        /// - Average(): Mean of numeric values
        /// - Min(): Smallest value
        /// - Max(): Largest value
        /// 
        /// Use cases:
        /// - Financial reports
        /// - Sales dashboards
        /// - Trend analysis
        /// - Executive summaries
        /// </summary>
        public async Task<IEnumerable<InvoiceStatistics>> GetInvoiceStatisticsAsync()
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] SQL aggregations (SUM, COUNT, AVG, MIN, MAX) - All calculated in database
            var sql = @"
                SELECT 
                    YEAR(i.InvoiceDate) AS Year,
                    SUM(i.Amount) AS TotalRevenue,
                    COUNT(*) AS InvoiceCount,
                    AVG(i.Amount) AS AverageAmount,
                    MIN(i.Amount) AS MinAmount,
                    MAX(i.Amount) AS MaxAmount
                FROM Invoices i
                GROUP BY YEAR(i.InvoiceDate)
                ORDER BY YEAR(i.InvoiceDate)";
            
            // [DAPPER] Automatic mapping to complex result type
            return await connection.QueryAsync<InvoiceStatistics>(sql);
        }

        /// <summary>
        /// EXAMPLE: Transactions for atomic operations
        /// 
        /// Demonstrates using database transactions to ensure data consistency.
        /// A transaction ensures that either ALL operations succeed or ALL fail.
        /// 
        /// What is a Transaction?
        /// - A unit of work that must be atomic (all-or-nothing)
        /// - If any operation fails, ALL changes are rolled back
        /// - Database remains consistent even if errors occur
        /// 
        /// Transaction guarantees (ACID):
        /// - Atomicity: All operations succeed or all fail
        /// - Consistency: Database rules are enforced
        /// - Isolation: Concurrent transactions don't interfere
        /// - Durability: Committed changes are permanent
        /// 
        /// Example scenario:
        /// Transfer all invoices from one customer to another.
        /// 
        /// When to use transactions:
        /// - Multiple related database operations
        /// - Operations that must succeed together
        /// - Financial operations (transfers, payments)
        /// - Complex business logic with multiple steps
        /// </summary>
        public async Task<(bool Success, int InvoicesTransferred, string Message)> TransferInvoicesAsync(
            long fromCustomerId, 
            long toCustomerId)
        {
            if (fromCustomerId == toCustomerId)
            {
                return (false, 0, "Cannot transfer to the same customer");
            }

            // [DAPPER] Manual connection management
            using var connection = new SqlConnection(_connectionString);
            await connection.OpenAsync();
            
            // [DAPPER] Database transaction - Ensures atomicity (all or nothing)
            using var transaction = connection.BeginTransaction();
            
            try
            {
                var toCustomerSql = "SELECT COUNT(*) FROM Customers WHERE Id = @Id";
                // [DAPPER] ExecuteScalarAsync with transaction - All operations in same transaction
                var toCustomerExists = await connection.ExecuteScalarAsync<int>(
                    toCustomerSql, 
                    new { Id = toCustomerId }, 
                    transaction) > 0;

                if (!toCustomerExists)
                {
                    return (false, 0, $"Target customer {toCustomerId} not found");
                }

                var getInvoicesSql = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId";
                // [DAPPER] QueryAsync within transaction
                var invoices = await connection.QueryAsync<Invoice>(
                    getInvoicesSql, 
                    new { CustomerId = fromCustomerId }, 
                    transaction);

                var invoiceList = invoices.ToList();
                if (!invoiceList.Any())
                {
                    return (false, 0, $"No invoices found for customer {fromCustomerId}");
                }

                // [DAPPER] Bulk UPDATE - Single SQL statement updates multiple rows
                var updateSql = "UPDATE Invoices SET CustomerId = @ToCustomerId WHERE CustomerId = @FromCustomerId";
                await connection.ExecuteAsync(
                    updateSql, 
                    new { ToCustomerId = toCustomerId, FromCustomerId = fromCustomerId }, 
                    transaction);

                // [DAPPER] Commit transaction - Makes all changes permanent
                transaction.Commit();

                Console.WriteLine($"Transferred {invoiceList.Count} invoices from customer {fromCustomerId} to {toCustomerId}");

                return (true, invoiceList.Count, "All invoices transferred successfully");
            }
            catch (Exception ex)
            {
                // [DAPPER] Rollback transaction - Undoes all changes if error occurs
                transaction.Rollback();
                Console.WriteLine($"Failed to transfer invoices: {ex.Message}");
                return (false, 0, $"Transaction failed and was rolled back: {ex.Message}");
            }
        }

        /// <summary>
        /// EXAMPLE: Multi-mapping for loading related data efficiently
        /// 
        /// Demonstrates Dapper's multi-mapping feature to load a customer with all related data
        /// in a single query using JOINs. This is more efficient than multiple round trips.
        /// 
        /// Multi-mapping allows you to map a single row to multiple objects.
        /// Dapper automatically splits the result based on the "splitOn" parameter.
        /// 
        /// Benefits:
        /// - Single database query instead of multiple queries
        /// - Reduces network round trips
        /// - Efficient for loading entity graphs
        /// 
        /// SQL Generated:
        /// SELECT c.*, i.*, t.*
        /// FROM Customers c
        /// LEFT JOIN Invoices i ON c.Id = i.CustomerId
        /// LEFT JOIN TelephoneNumbers t ON c.Id = t.CustomerId
        /// WHERE c.Id = @Id
        /// </summary>
        public async Task<Customer?> GetCustomerWithRelatedDataAsync(long id)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var customerDict = new Dictionary<long, Customer>();
            
            // [DAPPER] JOIN query - Single query to load all related data
            var sql = @"
                SELECT c.*, i.*, t.*
                FROM Customers c
                LEFT JOIN Invoices i ON c.Id = i.CustomerId
                LEFT JOIN TelephoneNumbers t ON c.Id = t.CustomerId
                WHERE c.Id = @Id";
            
            // [DAPPER] Multi-mapping QueryAsync - Maps single row to multiple objects
            // [DAPPER] splitOn parameter - Tells Dapper where to split the columns
            await connection.QueryAsync<Customer, Invoice, TelephoneNumber, Customer>(
                sql,
                (customer, invoice, phone) =>
                {
                    // [DAPPER] Manual mapping logic - Build object graph from flat result
                    if (!customerDict.TryGetValue(customer.Id, out var customerEntry))
                    {
                        customerEntry = customer;
                        customerEntry.Invoices = new List<Invoice>();
                        customerEntry.PhoneNumbers = new List<TelephoneNumber>();
                        customerDict.Add(customerEntry.Id, customerEntry);
                    }
                    
                    if (invoice != null && !customerEntry.Invoices!.Any(i => i.Id == invoice.Id))
                    {
                        customerEntry.Invoices.Add(invoice);
                    }
                    
                    if (phone != null && !customerEntry.PhoneNumbers!.Any(p => p.Id == phone.Id))
                    {
                        customerEntry.PhoneNumbers.Add(phone);
                    }
                    
                    return customerEntry;
                },
                new { Id = id },
                splitOn: "Id,Id" // [DAPPER] Split on Id columns to separate Customer, Invoice, TelephoneNumber
            );
            
            return customerDict.Values.FirstOrDefault();
        }

        /// <summary>
        /// EXAMPLE: Bulk operations for performance
        /// 
        /// Demonstrates using SQL for bulk updates, which is much faster than
        /// updating records one by one.
        /// 
        /// Performance comparison:
        /// - Individual updates (1000 records): ~2-3 seconds
        /// - Bulk update (1000 records): ~50ms
        /// 
        /// When to use bulk operations:
        /// - Updating many records at once
        /// - Data migrations
        /// - Batch processing
        /// - End-of-day processes
        /// </summary>
        public async Task<int> BulkUpdateInvoiceAmountsAsync(decimal increasePercentage)
        {
            using var connection = new SqlConnection(_connectionString);
            
            // [DAPPER] Bulk UPDATE - Single SQL updates all rows matching criteria
            var sql = @"
                UPDATE Invoices 
                SET Amount = Amount * (1 + @IncreasePercentage / 100.0)";
            
            // [DAPPER] ExecuteAsync returns number of rows affected
            return await connection.ExecuteAsync(sql, new { IncreasePercentage = increasePercentage });
        }

        /// <summary>
        /// EXAMPLE: Dynamic SQL with parameters
        /// 
        /// Demonstrates building dynamic queries safely using parameterized queries.
        /// This prevents SQL injection while allowing flexible querying.
        /// 
        /// IMPORTANT: Always use parameters, never concatenate user input into SQL!
        /// </summary>
        public async Task<IEnumerable<Customer>> DynamicSearchAsync(
            string? nameFilter = null,
            string? emailFilter = null,
            decimal? minBalance = null)
        {
            using var connection = new SqlConnection(_connectionString);
            
            var conditions = new List<string>();
            // [DAPPER] DynamicParameters - Safely build parameterized dynamic SQL
            var parameters = new DynamicParameters();
            
            var sql = "SELECT * FROM Customers c WHERE 1=1";
            
            // [DAPPER] Dynamic WHERE clause building - Add conditions based on parameters
            if (!string.IsNullOrEmpty(nameFilter))
            {
                conditions.Add("c.Name LIKE @NameFilter");
                parameters.Add("NameFilter", $"%{nameFilter}%");
            }
            
            if (!string.IsNullOrEmpty(emailFilter))
            {
                conditions.Add("c.Email LIKE @EmailFilter");
                parameters.Add("EmailFilter", $"%{emailFilter}%");
            }
            
            if (minBalance.HasValue)
            {
                // [DAPPER] Subquery in dynamic WHERE clause
                conditions.Add(@"(SELECT ISNULL(SUM(Amount), 0) FROM Invoices WHERE CustomerId = c.Id) >= @MinBalance");
                parameters.Add("MinBalance", minBalance.Value);
            }
            
            if (conditions.Any())
            {
                sql += " AND " + string.Join(" AND ", conditions);
            }
            
            // [DAPPER] Execute dynamic SQL with DynamicParameters - SQL injection safe
            return await connection.QueryAsync<Customer>(sql, parameters);
        }
    }

    // [DAPPER] Custom result class - Dapper maps SQL results to this POCO
    public class CustomerSummary
    {
        public long Id { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public int InvoiceCount { get; set; } // [DAPPER] Mapped from COUNT(i.Id)
        public decimal TotalAmount { get; set; } // [DAPPER] Mapped from SUM(i.Amount)
        public DateTime? LastInvoiceDate { get; set; } // [DAPPER] Mapped from MAX(i.InvoiceDate)
    }

    public class InvoiceStatistics
    {
        public int Year { get; set; }
        public decimal TotalRevenue { get; set; }
        public int InvoiceCount { get; set; }
        public decimal AverageAmount { get; set; }
        public decimal MinAmount { get; set; }
        public decimal MaxAmount { get; set; }
    }
}
