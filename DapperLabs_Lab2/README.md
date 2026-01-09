# Dapper Labs - Lab 2

A .NET 8 Web API demonstrating Dapper ORM with SQL Server, featuring RESTful endpoints, Swagger documentation, and comprehensive logging.

## Quick Start

### Run the API

```powershell
dotnet restore
dotnet run
```

**Then manually open your browser to: http://localhost:5000**

The console will display:
```
===========================================
Dapper Labs API Started Successfully!
===========================================
Swagger UI: http://localhost:5000
API Base URL: http://localhost:5000/api
===========================================
Open your browser and navigate to:
  http://localhost:5000
===========================================
```

**Note:** When using `dotnet run`, the browser does not automatically open. You must manually navigate to the URL shown in the console.

### Alternative: Run with Visual Studio

If using Visual Studio, press F5 to run the project. The browser will automatically open to the Swagger UI.

## Project Overview

This project demonstrates professional Dapper usage patterns in a real-world Web API:

- **RESTful API** with ASP.NET Core 8.0
- **Swagger/OpenAPI** documentation
- **Dapper ORM** for database operations
- **Repository Pattern** for data access
- **Comprehensive Logging** with ILogger
- **Input Validation** with Data Annotations
- **CRUD Operations** for all entities
- **Pagination Support** for large datasets
- **Search Functionality** with dynamic filters
- **Automatic Database Setup** and seeding

## Project Structure

```
DapperLabs_Lab2/
|-- Controllers/          # API Controllers
|   |-- CustomersController.cs
|   |-- InvoicesController.cs
|   +-- TelephoneNumbersController.cs
|-- Data/                # Entity Models
|   |-- Customer.cs
|   |-- Invoice.cs
|   +-- TelephoneNumber.cs
|-- DTOs/                # Data Transfer Objects
|   |-- CustomerDto.cs
|   |-- InvoiceDto.cs
|   |-- TelephoneNumberDto.cs
|   +-- PagedResultDto.cs
|-- Repositories/        # Data Access Layer
|   |-- CustomerRepository.cs
|   |-- InvoiceRepository.cs
|   +-- TelephoneNumberRepository.cs
|-- Examples/           # Advanced Dapper Patterns
|   |-- AdvancedExamples.cs
|   +-- OtherAdvancedExamples.cs
|-- Mappings/           # Entity-DTO Mappings
|   +-- MappingExtensions.cs
|-- DatabaseInitializer.cs
|-- BogusDataGenerator.cs
|-- Program.cs
+-- appsettings.json
```

## Features

### API Endpoints

#### Customers
- `GET /api/customers` - Get all customers (paginated)
- `GET /api/customers/{id}` - Get customer by ID
- `GET /api/customers/search` - Search customers
- `POST /api/customers` - Create customer
- `PUT /api/customers/{id}` - Update customer
- `DELETE /api/customers/{id}` - Delete customer

#### Invoices
- `GET /api/invoices` - Get all invoices
- `GET /api/invoices/{id}` - Get invoice by ID
- `GET /api/invoices/customer/{customerId}` - Get customer invoices
- `POST /api/invoices` - Create invoice
- `PUT /api/invoices/{id}` - Update invoice
- `DELETE /api/invoices/{id}` - Delete invoice

#### Telephone Numbers
- `GET /api/telephonenumbers` - Get all phone numbers
- `GET /api/telephonenumbers/{id}` - Get phone number by ID
- `GET /api/telephonenumbers/customer/{customerId}` - Get customer phones
- `POST /api/telephonenumbers` - Create phone number
- `PUT /api/telephonenumbers/{id}` - Update phone number
- `DELETE /api/telephonenumbers/{id}` - Delete phone number

### Dapper Patterns Demonstrated

**AdvancedExamples.cs:**
- Projection queries
- Aggregations (GROUP BY, COUNT, SUM, AVG)
- Transactions
- Multi-mapping (JOINs)
- Bulk operations
- Dynamic SQL with parameters

**OtherAdvancedExamples.cs:**
- Stored procedures
- Table-valued parameters
- Multiple result sets (QueryMultiple)
- Window functions
- Common Table Expressions (CTEs)
- Retry logic
- Query caching
- Upsert operations

## Configuration

### appsettings.json

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=DapperLabs;User Id=YourUsername;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "SeedSettings": {
    "EnableSeeding": true,
    "CustomerCount": 1000,
    "MinInvoicesPerCustomer": 1,
    "MaxInvoicesPerCustomer": 5,
    "MinPhoneNumbersPerCustomer": 1,
    "MaxPhoneNumbersPerCustomer": 3
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "DapperLabs_Lab2": "Information"
    }
  }
}
```

### Environment Variables

Set `ASPNETCORE_ENVIRONMENT` to control behavior:
- `Development` - Enables Swagger UI, detailed errors
- `Production` - Production-ready configuration

## Documentation

- **[TRUNCATE_DATABASE_GUIDE.md](DBTools/TRUNCATE_DATABASE_GUIDE.md)** - Clear all data (PowerShell scripts)
- **[TRUNCATE_SQL_GUIDE.md](DBTools/TRUNCATE_SQL_GUIDE.md)** - Clear all data (T-SQL scripts)
- **[API_GUIDE.md](API_GUIDE.md)** - Complete API documentation with examples
- **[CODE_ANNOTATIONS.md](Docs/CODE_ANNOTATIONS.md)** - Guide to [API] and [DAPPER] code annotations
- **[OUTPUT_CLAUSE.md](Docs/OUTPUT_CLAUSE.md)** - OUTPUT vs SCOPE_IDENTITY explanation
- **[QUICK_START.md](Docs/QUICK_START.md)** - Getting started guide
- **[EF_CORE_VS_DAPPER.md](Docs/EF_CORE_VS_DAPPER.md)** - EF Core vs Dapper comparison
- **[PROJECT_SUMMARY.md](Docs/PROJECT_SUMMARY.md)** - Project overview

## Database Management

### Clear All Data

To reset the database and delete all customers, invoices, and phone numbers:

**PowerShell:**
```powershell
.\truncate-database.ps1
```

**Command Prompt:**
```cmd
truncate-database.bat
```

**SQL Server Management Studio / Azure Data Studio:**
```sql
-- Open and execute truncate-database.sql
-- Or use sqlcmd:
sqlcmd -S localhost -d DapperLabs -i truncate-database.sql
```

**Quick Truncate (no confirmation):**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database-quick.sql
```

**Features:**
- Confirmation required (PowerShell/Batch scripts)
- Deletes all customer data
- Resets auto-increment IDs to start from 1
- CASCADE DELETE removes related invoices and phone numbers automatically
- Transaction safety (SQL scripts use transactions)

**To re-seed after truncate:**
1. Ensure `"EnableSeeding": true` in appsettings.json
2. Run the API: `dotnet run`
3. Data will be automatically regenerated

See **[TRUNCATE_DATABASE_GUIDE.md](DBTools/TRUNCATE_DATABASE_GUIDE.md)** for complete documentation.

## Database Schema

### Customers
- Id (BIGINT, Identity, PK)
- Name (NVARCHAR(200))
- Email (NVARCHAR(200), Unique)

### Invoices
- Id (BIGINT, Identity, PK)
- InvoiceNumber (NVARCHAR(50), Unique)
- CustomerId (BIGINT, FK to Customers)
- InvoiceDate (DATETIME)
- Amount (DECIMAL(18,2))

### TelephoneNumbers
- Id (BIGINT, Identity, PK)
- CustomerId (BIGINT, FK to Customers)
- Type (NVARCHAR(20), CHECK: Mobile, Work, DirectDial)
- Number (NVARCHAR(50))

## Testing the API

### Using Swagger UI (Recommended)

1. Run the application
2. Navigate to `http://localhost:5000`
3. Use the interactive UI to test endpoints

### Using curl

```powershell
# Get all customers
curl http://localhost:5000/api/customers

# Create a customer
curl -X POST http://localhost:5000/api/customers `
  -H "Content-Type: application/json" `
  -d '{"name":"Test Corp","email":"test@example.com"}'

# Get customer by ID
curl http://localhost:5000/api/customers/1?includeRelated=true

# Search customers
curl "http://localhost:5000/api/customers/search?name=Corp&minBalance=1000"
```

## Logging

The application includes comprehensive logging:

```
info: DapperLabs_Lab2.Controllers.CustomersController[0]
      Getting customers - Page: 1, PageSize: 10, IncludeRelated: True
info: DapperLabs_Lab2.Controllers.CustomersController[0]
      Getting customer 1, IncludeRelated: True
```

Logs include:
- All API requests with parameters
- Database operations
- Validation errors
- Exception details

## Security Features

- **Parameterized Queries** - SQL injection prevention
- **Input Validation** - Data annotation validation
- **CORS Configuration** - Configured for development (update for production)
- **HTTPS Support** - Enabled by default

## Performance Features

- **Pagination** - Efficient handling of large datasets
- **Lazy Loading** - Related data loaded only when requested
- **Connection Pooling** - Automatic connection management
- **Async/Await** - Non-blocking I/O operations
- **Efficient Queries** - Direct SQL with Dapper

## Learning Objectives

### For Students

This project teaches:

1. **RESTful API Design**
   - Resource-based URLs
   - HTTP verbs (GET, POST, PUT, DELETE)
   - Status codes (200, 201, 400, 404)
   - Request/response patterns

2. **Dapper ORM**
   - Query and QueryAsync
   - Execute and ExecuteAsync
   - ExecuteScalar for identity values
   - Multi-mapping
   - Dynamic parameters

3. **Repository Pattern**
   - Separation of concerns
   - Testability
   - Clean architecture

4. **Dependency Injection**
   - Service registration
   - Constructor injection
   - Scoped lifetimes

5. **API Documentation**
   - Swagger/OpenAPI
   - XML comments
   - Interactive testing

6. **Logging**
   - Structured logging
   - Log levels
   - Contextual information

## Troubleshooting

### API won't start
```powershell
# Check if port is in use
netstat -ano | findstr :5000

# Kill the process if needed
taskkill /PID <process_id> /F
```

### Database connection issues
- Verify SQL Server is running
- Check connection string in appsettings.json
- Ensure database permissions

### Swagger UI not loading
- Confirm `ASPNETCORE_ENVIRONMENT` is set to "Development"
- Check Program.cs Swagger configuration
- Try clearing browser cache

## Additional Resources

- **Dapper**: https://github.com/DapperLib/Dapper
- **ASP.NET Core**: https://docs.microsoft.com/aspnet/core/
- **Swagger**: https://swagger.io/docs/
- **SQL Server**: https://docs.microsoft.com/sql/

## Contributing

This is a learning project. Feel free to:
- Add new examples
- Improve documentation
- Add more advanced patterns
- Create tests

## License

This is a learning/demonstration project.

------