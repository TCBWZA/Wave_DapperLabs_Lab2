# Quick Start Guide - Dapper Labs Lab 2

## Prerequisites

Ensure you have:
- ? .NET 8 SDK installed
- ? SQL Server or SQL Server Express running
- ? Access to SQL Server (either Windows Auth or SQL Auth)

## Step 1: Configure Connection String

Edit `appsettings.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DapperLabs;User Id=YourUsername;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

### Connection String Options

**SQL Server Authentication:**
```
Server=localhost;Database=DapperLabs;User Id=YourUsername;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true
```

**Windows Authentication:**
```
Server=localhost;Database=DapperLabs;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

**SQL Server Express:**
```
Server=localhost\\SQLEXPRESS;Database=DapperLabs;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true
```

## Step 2: Restore Packages

```powershell
cd DapperLabs_Lab2
dotnet restore
```

## Step 3: Run the Application

```powershell
dotnet run
```

## What Happens?

1. **Database Creation**: Creates `DapperLabs` database if it doesn't exist
2. **Table Creation**: Creates `Customers`, `Invoices`, and `TelephoneNumbers` tables
3. **Data Seeding**: Generates 1000 customers with invoices and phone numbers (if enabled)
4. **Examples Execution**: Runs 9 different examples demonstrating Dapper features

## Expected Output

```
=== Dapper Labs - Lab 2 ===

Initializing database...
? Database 'DapperLabs' already exists.
Creating tables...
? Customers table ready
? Invoices table ready
? TelephoneNumbers table ready

Seeding database...
Database is empty. Starting seed process...
Generating 1000 customers with invoices and phone numbers...
? Created 1000 customers
? Generated 3000 invoices
? Generated 2000 phone numbers

? Database seeded successfully!
  Total: 1000 customers, 3000 invoices, 2000 phone numbers

=== Running Dapper Examples ===

--- Example 1: Get All Customers (without related data) ---
Found 1000 customers
  - Acme Corporation (acme@example.com)
  - Beta Industries (beta@example.com)
  - Gamma Ltd (gamma@example.com)

--- Example 2: Get Customer with Related Data ---
Customer: Acme Corporation
  Invoices: 3
  Phone Numbers: 2
  Balance: $4,567.89

[... more examples ...]

=== Examples Complete ===

Press any key to exit...
```

## Configuration Options

### Disable Seeding

If you don't want to seed data (e.g., running multiple times):

```json
{
  "SeedSettings": {
    "EnableSeeding": false
  }
}
```

### Customize Seed Data

```json
{
  "SeedSettings": {
    "EnableSeeding": true,
    "CustomerCount": 100,              // Number of customers
    "MinInvoicesPerCustomer": 1,       // Min invoices per customer
    "MaxInvoicesPerCustomer": 5,       // Max invoices per customer
    "MinPhoneNumbersPerCustomer": 1,   // Min phone numbers per customer
    "MaxPhoneNumbersPerCustomer": 3    // Max phone numbers per customer
  }
}
```

## Troubleshooting

### Issue: Cannot connect to SQL Server

**Error:**
```
A network-related or instance-specific error occurred while establishing a connection to SQL Server
```

**Solutions:**
1. Check SQL Server is running:
   ```powershell
   # Windows Services
   Get-Service -Name MSSQLSERVER
   # Or for SQL Express
   Get-Service -Name 'MSSQL$SQLEXPRESS'
   ```

2. Verify server name:
   - `localhost` for default instance
   - `localhost\\SQLEXPRESS` for Express edition
   - `(localdb)\\mssqllocaldb` for LocalDB

3. Check authentication:
   - For SQL Auth: Ensure user has proper permissions
   - For Windows Auth: Use `Integrated Security=True`

### Issue: Login failed for user

**Error:**
```
Login failed for user 'YourUsername'
```

**Solutions:**
1. Check password is correct
2. Verify SQL Server authentication is enabled
3. Try Windows Authentication instead:
   ```json
   "DefaultConnection": "Server=localhost;Database=DapperLabs;Integrated Security=True;TrustServerCertificate=True"
   ```

### Issue: Database already exists

**Error:**
```
Database already contains X customers. Skipping seed.
```

**Solution:**
This is normal behavior. To reseed:

**Option 1 - Disable seeding:**
```json
"EnableSeeding": false
```

**Option 2 - Drop and recreate database:**
```sql
DROP DATABASE DapperLabs;
```
Then run the application again.

**Option 3 - Use PowerShell:**
```powershell
# Using SqlServer module
Invoke-Sqlcmd -Query "DROP DATABASE IF EXISTS DapperLabs" -ServerInstance "localhost"
```

### Issue: Trust server certificate

**Error:**
```
The certificate chain was issued by an authority that is not trusted
```

**Solution:**
Ensure `TrustServerCertificate=True` is in your connection string:
```
Server=localhost;Database=DapperLabs;User Id=YourUsername;Password=YOUR_PASSWORD;TrustServerCertificate=True
```

## Manual Database Setup (Alternative)

If you prefer to create the database manually:

1. Open SQL Server Management Studio (SSMS)
2. Connect to your SQL Server instance
3. Open and execute `DatabaseSetup.sql`
4. Update `appsettings.json` with your connection string
5. Run the application

## Exploring the Code

### Key Files:

| File | Purpose |
|------|---------|
| `Program.cs` | Main entry point, runs all examples |
| `Repositories/CustomerRepository.cs` | Customer CRUD operations |
| `Repositories/InvoiceRepository.cs` | Invoice CRUD operations |
| `Repositories/TelephoneNumberRepository.cs` | Phone number CRUD operations |
| `Examples/AdvancedExamples.cs` | Advanced Dapper patterns |
| `Examples/OtherAdvancedExamples.cs` | Additional advanced patterns |
| `DatabaseInitializer.cs` | Creates database and tables |
| `BogusDataGenerator.cs` | Generates test data |

### Learning Path:

1. **Start with repositories** - See basic CRUD operations
2. **Review Program.cs** - See how examples are executed
3. **Study AdvancedExamples.cs** - Learn advanced patterns
4. **Explore OtherAdvancedExamples.cs** - See additional techniques
5. **Read README.md** - Understand the project structure
6. **Compare EF_CORE_VS_DAPPER.md** - See differences

## Running Specific Examples

To modify which examples run, edit `Program.cs` and comment out sections you don't want to execute:

```csharp
// Comment out to skip
// Console.WriteLine("--- Example 1: Get All Customers ---");
// var customers = await customerRepository.GetAllAsync();
```

## Performance Testing

To test performance:

1. Increase seed count:
   ```json
   "CustomerCount": 10000
   ```

2. Add timing to Program.cs:
   ```csharp
   var stopwatch = System.Diagnostics.Stopwatch.StartNew();
   var customers = await customerRepository.GetAllAsync();
   stopwatch.Stop();
   Console.WriteLine($"Time: {stopwatch.ElapsedMilliseconds}ms");
   ```

## Next Steps

After running the examples:

1. ? Modify queries in repositories to experiment
2. ? Add new examples in `AdvancedExamples.cs`
3. ? Compare performance with EF Core version
4. ? Try different SQL patterns (CTEs, window functions, etc.)
5. ? Implement additional features (sorting, filtering, etc.)

## Resources

- **Dapper Documentation**: https://github.com/DapperLib/Dapper
- **Dapper Tutorial**: https://www.learndapper.com/
- **SQL Server Documentation**: https://docs.microsoft.com/sql/

## Support

If you encounter issues:

1. Check the error message carefully
2. Review the Troubleshooting section above
3. Verify your SQL Server connection
4. Check that all packages are restored (`dotnet restore`)
5. Ensure .NET 8 SDK is installed (`dotnet --version`)

## Success Indicators

You'll know it's working when you see:
- ? No errors during database creation
- ? Data seeding completes successfully
- ? All examples execute without errors
- ? Results are displayed in the console

Happy coding with Dapper! ??
