# EF Core vs Dapper Comparison

This document compares the implementation differences between the EfCoreLab (Entity Framework Core) and DapperLabs_Lab2 (Dapper) projects.

## Overview

Both projects implement the same functionality:
- Customer, Invoice, and TelephoneNumber entities
- CRUD operations via repository pattern
- Advanced query examples (aggregation, projection, transactions)
- Data seeding with Bogus

However, the approach and implementation details differ significantly.

## Code Comparison

### 1. Database Context vs Connection String

**EF Core:**
```csharp
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }
    
    public DbSet<Customer> Customers { get; set; } = null!;
    public DbSet<Invoice> Invoices { get; set; } = null!;
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Configure entities, relationships, constraints
        modelBuilder.Entity<Customer>(b =>
        {
            b.HasKey(c => c.Id);
            b.HasMany(c => c.Invoices).WithOne().HasForeignKey(i => i.CustomerId);
            b.HasIndex(c => c.Email).IsUnique();
        });
    }
}
```

**Dapper:**
```csharp
public class CustomerRepository
{
    private readonly string _connectionString;
    
    public CustomerRepository(string connectionString)
    {
        _connectionString = connectionString;
    }
    
    // Direct SQL queries
    // No context, no configuration
}
```

### 2. Querying Data

**EF Core - Simple Query:**
```csharp
public async Task<Customer?> GetByIdAsync(long id)
{
    return await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);
}
```

**Dapper - Simple Query:**
```csharp
public async Task<Customer?> GetByIdAsync(long id)
{
    using var connection = new SqlConnection(_connectionString);
    var sql = "SELECT * FROM Customers WHERE Id = @Id";
    return await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
}
```

### 3. Loading Related Data

**EF Core - Eager Loading:**
```csharp
public async Task<Customer?> GetByIdAsync(long id, bool includeRelated = false)
{
    var query = _context.Customers.AsQueryable();
    
    if (includeRelated)
    {
        query = query
            .Include(c => c.Invoices)      // Automatic JOIN
            .Include(c => c.PhoneNumbers); // Automatic JOIN
    }
    
    return await query.FirstOrDefaultAsync(c => c.Id == id);
}
```

**Dapper - Manual Loading:**
```csharp
public async Task<Customer?> GetByIdAsync(long id, bool includeRelated = false)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = "SELECT * FROM Customers WHERE Id = @Id";
    var customer = await connection.QueryFirstOrDefaultAsync<Customer>(sql, new { Id = id });
    
    if (customer != null && includeRelated)
    {
        // Separate queries for related data
        var invoiceSql = "SELECT * FROM Invoices WHERE CustomerId = @CustomerId";
        customer.Invoices = (await connection.QueryAsync<Invoice>(
            invoiceSql, new { CustomerId = customer.Id })).ToList();
        
        var phoneSql = "SELECT * FROM TelephoneNumbers WHERE CustomerId = @CustomerId";
        customer.PhoneNumbers = (await connection.QueryAsync<TelephoneNumber>(
            phoneSql, new { CustomerId = customer.Id })).ToList();
    }
    
    return customer;
}
```

**Dapper - Multi-Mapping (Alternative):**
```csharp
// Load in a single query with JOINs
var sql = @"
    SELECT c.*, i.*, t.*
    FROM Customers c
    LEFT JOIN Invoices i ON c.Id = i.CustomerId
    LEFT JOIN TelephoneNumbers t ON c.Id = t.CustomerId
    WHERE c.Id = @Id";

await connection.QueryAsync<Customer, Invoice, TelephoneNumber, Customer>(
    sql,
    (customer, invoice, phone) =>
    {
        // Manual mapping logic
        return customer;
    },
    new { Id = id },
    splitOn: "Id,Id"
);
```

### 4. Creating Records

**EF Core:**
```csharp
public async Task<Customer> CreateAsync(Customer customer)
{
    _context.Customers.Add(customer);
    await _context.SaveChangesAsync();
    return customer; // ID automatically populated
}
```

**Dapper:**
```csharp
public async Task<Customer> CreateAsync(Customer customer)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        INSERT INTO Customers (Name, Email) 
        VALUES (@Name, @Email);
        SELECT CAST(SCOPE_IDENTITY() as bigint)";
    
    customer.Id = await connection.ExecuteScalarAsync<long>(sql, customer);
    return customer;
}
```

### 5. Updating Records

**EF Core:**
```csharp
public async Task<Customer> UpdateAsync(Customer customer)
{
    _context.Customers.Update(customer); // Marks as modified
    await _context.SaveChangesAsync();   // Generates UPDATE
    return customer;
}
```

**Dapper:**
```csharp
public async Task<Customer> UpdateAsync(Customer customer)
{
    using var connection = new SqlConnection(_connectionString);
    
    var sql = @"
        UPDATE Customers 
        SET Name = @Name, Email = @Email 
        WHERE Id = @Id";
    
    await connection.ExecuteAsync(sql, customer);
    return customer;
}
```

### 6. Filtering and Searching

**EF Core:**
```csharp
public async Task<IEnumerable<Customer>> SearchAsync(
    string? name, string? email, decimal? minBalance)
{
    var query = _context.Customers.AsQueryable();
    
    if (!string.IsNullOrEmpty(name))
    {
        query = query.Where(c => c.Name.Contains(name));
    }
    
    if (!string.IsNullOrEmpty(email))
    {
        query = query.Where(c => c.Email.Contains(email));
    }
    
    if (minBalance.HasValue)
    {
        query = query.Where(c => c.Invoices.Sum(i => i.Amount) >= minBalance.Value);
    }
    
    return await query.ToListAsync();
}
```

**Dapper:**
```csharp
public async Task<IEnumerable<Customer>> SearchAsync(
    string? name, string? email, decimal? minBalance)
{
    using var connection = new SqlConnection(_connectionString);
    
    var conditions = new List<string>();
    var parameters = new DynamicParameters();
    
    if (!string.IsNullOrEmpty(name))
    {
        conditions.Add("Name LIKE @Name");
        parameters.Add("Name", $"%{name}%");
    }
    
    if (!string.IsNullOrEmpty(email))
    {
        conditions.Add("Email LIKE @Email");
        parameters.Add("Email", $"%{email}%");
    }
    
    if (minBalance.HasValue)
    {
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
    
    return await connection.QueryAsync<Customer>(sql, parameters);
}
```

### 7. Aggregations

**EF Core:**
```csharp
var statistics = await _context.Invoices
    .GroupBy(i => i.InvoiceDate.Year)
    .Select(g => new
    {
        Year = g.Key,
        TotalRevenue = g.Sum(i => i.Amount),
        InvoiceCount = g.Count(),
        AverageAmount = g.Average(i => i.Amount)
    })
    .ToListAsync();
```

**Dapper:**
```csharp
var sql = @"
    SELECT 
        YEAR(i.InvoiceDate) AS Year,
        SUM(i.Amount) AS TotalRevenue,
        COUNT(*) AS InvoiceCount,
        AVG(i.Amount) AS AverageAmount
    FROM Invoices i
    GROUP BY YEAR(i.InvoiceDate)
    ORDER BY YEAR(i.InvoiceDate)";

var statistics = await connection.QueryAsync<InvoiceStatistics>(sql);
```

### 8. Transactions

**EF Core:**
```csharp
using var transaction = await _context.Database.BeginTransactionAsync();

try
{
    // Multiple operations using _context
    await _context.SaveChangesAsync();
    await _context.SaveChangesAsync();
    
    await transaction.CommitAsync();
}
catch
{
    await transaction.RollbackAsync();
    throw;
}
```

**Dapper:**
```csharp
using var connection = new SqlConnection(_connectionString);
await connection.OpenAsync();

using var transaction = connection.BeginTransaction();

try
{
    // Multiple operations
    await connection.ExecuteAsync(sql1, params1, transaction);
    await connection.ExecuteAsync(sql2, params2, transaction);
    
    transaction.Commit();
}
catch
{
    transaction.Rollback();
    throw;
}
```

### 9. Pagination

**EF Core:**
```csharp
var items = await query
    .OrderBy(c => c.Name)
    .Skip((page - 1) * pageSize)
    .Take(pageSize)
    .ToListAsync();
```

**Dapper:**
```csharp
var sql = @"
    SELECT * FROM Customers 
    ORDER BY Name 
    OFFSET @Offset ROWS 
    FETCH NEXT @PageSize ROWS ONLY";

var items = await connection.QueryAsync<Customer>(
    sql, 
    new { Offset = (page - 1) * pageSize, PageSize = pageSize });
```

## Features Comparison

| Feature | EF Core | Dapper |
|---------|---------|--------|
| **Change Tracking** | Yes (automatic) | No |
| **Query Generation** | Automatic (LINQ) | Manual (SQL) |
| **Migrations** | Built-in | Manual |
| **Relationships** | Automatic navigation | Manual mapping |
| **Performance** | Good | Excellent |
| **Code Volume** | Less (more abstraction) | More (explicit SQL) |
| **Learning Curve** | Steeper | Gentler (if you know SQL) |
| **Debugging** | Can be complex | Easy (see actual SQL) |
| **Stored Procedures** | Supported | Excellent support |
| **Bulk Operations** | Requires extensions | Native support |
| **Complex Queries** | Can get complex | Straightforward SQL |

## Examples Removed from Dapper Version

### 1. SplitQuery
**EF Core Feature:**
```csharp
var customers = await _context.Customers
    .AsSplitQuery()  // Execute separate queries for each collection
    .Include(c => c.Invoices)
    .Include(c => c.PhoneNumbers)
    .ToListAsync();
```

**Why Not in Dapper:**
Dapper doesn't have automatic eager loading, so you always control whether to use:
- Single query with JOINs (multi-mapping)
- Multiple separate queries (manual loading)

### 2. AsNoTracking
**EF Core Feature:**
```csharp
var customers = await _context.Customers
    .AsNoTracking()  // Disable change tracking for performance
    .ToListAsync();
```

**Why Not in Dapper:**
Dapper never tracks entities - it always returns "no tracking" results. This is one reason why Dapper is faster.

### 3. Explicit Loading
**EF Core Feature:**
```csharp
var customer = await _context.Customers.FirstOrDefaultAsync(c => c.Id == id);

// Load invoices on-demand
await _context.Entry(customer)
    .Collection(c => c.Invoices)
    .LoadAsync();
```

**Why Not in Dapper:**
All loading in Dapper is "explicit" - you always write the query. The equivalent in Dapper is just another query:
```csharp
customer.Invoices = await connection.QueryAsync<Invoice>(
    "SELECT * FROM Invoices WHERE CustomerId = @Id", 
    new { Id = customer.Id });
```

## Performance Characteristics

### EF Core Advantages:
- **Caching**: Query compilation is cached
- **Batching**: Can batch multiple inserts/updates
- **Change Detection**: Only updates modified properties

### Dapper Advantages:
- **Less Overhead**: No change tracking, no query compilation
- **Direct Mapping**: Maps directly from SqlDataReader
- **No Magic**: What you write is what executes

### Benchmark Results (Typical):
```
Operation              | EF Core | Dapper | Winner
-----------------------|---------|--------|--------
Simple Query (1 row)   | 0.5ms   | 0.3ms  | Dapper
Query (100 rows)       | 5ms     | 3ms    | Dapper
Query with Joins       | 8ms     | 5ms    | Dapper
Insert (1 record)      | 1ms     | 0.7ms  | Dapper
Bulk Insert (1000)     | 500ms   | 50ms   | Dapper
Update (1 record)      | 1ms     | 0.8ms  | Dapper
Complex Query          | 10ms    | 7ms    | Dapper
```

## When to Choose Which?

### Choose EF Core When:
- ? Rapid development is priority
- ? Domain-driven design with complex relationships
- ? You need migrations
- ? Your team prefers LINQ over SQL
- ? Change tracking is beneficial
- ? You have complex business logic tied to entities

### Choose Dapper When:
- ? Performance is critical
- ? You have complex SQL queries
- ? You need to work with stored procedures
- ? Your team is strong in SQL
- ? Microservices with simple data access
- ? You want full control over SQL
- ? Reading existing database schema

### Use Both When:
- ? Use EF Core for writes (benefits from change tracking)
- ? Use Dapper for reads (better performance)
- ? Use EF Core for complex domain logic
- ? Use Dapper for reporting queries

## Migration from EF Core to Dapper

### Steps Taken in This Project:

1. **Remove EF Core Dependencies**
   - Remove `Microsoft.EntityFrameworkCore` packages
   - Add `Dapper` and `Microsoft.Data.SqlClient` packages

2. **Replace DbContext with Connection String**
   - Remove AppDbContext class
   - Pass connection string to repositories

3. **Convert LINQ Queries to SQL**
   - Replace `.Where()`, `.Include()`, etc. with SQL
   - Use parameterized queries

4. **Manual Relationship Loading**
   - Replace `.Include()` with separate queries or JOINs
   - Implement multi-mapping for complex scenarios

5. **Manual Schema Management**
   - Replace migrations with DatabaseInitializer
   - Create SQL scripts for schema changes

6. **Update Repository Methods**
   - Replace EF Core methods with Dapper methods
   - Handle SCOPE_IDENTITY for inserts
   - Manage connections explicitly

## Conclusion

Both EF Core and Dapper are excellent tools:

- **EF Core** is a full-featured ORM with high-level abstractions
- **Dapper** is a lightweight micro-ORM with minimal overhead

The choice depends on your specific needs:
- Need productivity and abstraction? ? EF Core
- Need performance and control? ? Dapper
- Large enterprise application? ? EF Core
- Microservice or performance-critical? ? Dapper

This project demonstrates that the same functionality can be achieved with both approaches, each with its own trade-offs.
