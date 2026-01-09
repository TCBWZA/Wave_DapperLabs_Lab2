# Wave Dapper Lab - Lab 2

A comprehensive .NET 8 Web API educational project demonstrating Dapper ORM with SQL Server. This project serves as a learning resource for students to understand Web API development, Dapper ORM patterns, and best practices in data access.

## About This Project

**Author:** TCBWZA  
**Purpose:** Educational reference for students learning ASP.NET Core Web APIs and Dapper ORM  
**Target Framework:** .NET 8  

This project was created as a teaching tool to demonstrate:
- RESTful API design with ASP.NET Core
- Dapper micro-ORM for efficient data access
- Repository pattern implementation
- CRUD operations and advanced query patterns
- Swagger/OpenAPI documentation

## Quick Start

### Prerequisites
- .NET 8 SDK
- SQL Server, SQL Server Express, or SQL Server LocalDB
- Visual Studio 2022, VS Code, or any text editor

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone https://github.com/TCBWZA/Wave_DapperLab_Lab2.git
   cd Wave_DapperLab_Lab2
   ```

2. **Configure your database connection**
   
   Edit `DapperLabs_Lab2/appsettings.json` and update the connection string:
   
   **For SQL Server with SQL Authentication:**
   ```json
   "DefaultConnection": "Server=localhost;Database=DapperLabs;User Id=YOUR_USERNAME;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
   
   **For SQL Server with Windows Authentication:**
   ```json
   "DefaultConnection": "Server=localhost;Database=DapperLabs;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```
   
   **For SQL Server Express:**
   ```json
   "DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=DapperLabs;Integrated Security=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
   ```

3. **Restore packages and run**
   ```bash
   cd DapperLabs_Lab2
   dotnet restore
   dotnet run
   ```

4. **Access Swagger UI**
   
   Open your browser to: `http://localhost:5000`

## Project Structure

```
DapperLabs_Lab2/
|-- Controllers/          # API Controllers (RESTful endpoints)
|-- Data/                 # Entity Models
|-- DTOs/                 # Data Transfer Objects
|-- Repositories/         # Data Access Layer (Dapper implementations)
|-- Examples/            # Advanced Dapper pattern demonstrations
|-- Mappings/            # Entity-DTO mapping extensions
|-- Docs/                # Comprehensive documentation
|-- DatabaseInitializer.cs
|-- BogusDataGenerator.cs
|-- Program.cs
+-- appsettings.json
```

## Features

### API Endpoints
- **Customers API** - Full CRUD with pagination and search
- **Invoices API** - Invoice management with customer relationships
- **Telephone Numbers API** - Phone number management

### Dapper Patterns Demonstrated
- Parameterized queries
- Query projection and aggregation
- Database transactions
- Multi-mapping (JOINs)
- Bulk operations
- Dynamic SQL building
- Stored procedures
- Table-valued parameters
- Window functions
- Common Table Expressions (CTEs)

### Educational Features
- Code annotations (`[API]` and `[DAPPER]` tags)
- Comprehensive inline documentation
- Step-by-step examples
- Comparison with Entity Framework Core patterns

## Documentation

Extensive documentation is included in the `Docs/` folder:

- **[QUICK_START.md](DapperLabs_Lab2/Docs/QUICK_START.md)** - Getting started guide
- **[API_GUIDE.md](DapperLabs_Lab2/API_GUIDE.md)** - Complete API documentation
- **[CODE_ANNOTATIONS.md](DapperLabs_Lab2/Docs/CODE_ANNOTATIONS.md)** - Understanding code annotations
- **[EF_CORE_VS_DAPPER.md](DapperLabs_Lab2/Docs/EF_CORE_VS_DAPPER.md)** - EF Core vs Dapper comparison
- **[OUTPUT_CLAUSE.md](DapperLabs_Lab2/Docs/OUTPUT_CLAUSE.md)** - OUTPUT vs SCOPE_IDENTITY
- **[AUTO_LAUNCH_GUIDE.md](DapperLabs_Lab2/AUTO_LAUNCH_GUIDE.md)** - Auto-launch scripts
- **[TRUNCATE_DATABASE_GUIDE.md](DapperLabs_Lab2/DBTools/TRUNCATE_DATABASE_GUIDE.md)** - Database management

## Database Management

The project includes utilities for database management:

### Auto-seeding
The application automatically creates the database schema and seeds test data on first run (configurable in `appsettings.json`).

### Clear All Data
Scripts are provided to truncate all tables:
- PowerShell: `.\truncate-database.ps1`
- Batch: `truncate-database.bat`
- SQL: `truncate-database.sql`

## Learning Path

Recommended order for students:

1. **Start with the README** - Understand the project overview
2. **Review QUICK_START.md** - Get the project running
3. **Explore API_GUIDE.md** - Learn the API endpoints
4. **Study the Repositories** - See Dapper in action
5. **Review AdvancedExamples.cs** - Learn advanced patterns
6. **Read CODE_ANNOTATIONS.md** - Understand the architecture
7. **Compare with EF_CORE_VS_DAPPER.md** - Learn the differences

## Technologies Used

- **.NET 8** - Latest .NET framework
- **ASP.NET Core** - Web API framework
- **Dapper** - Micro-ORM for data access
- **SQL Server** - Database engine
- **Swagger/OpenAPI** - API documentation
- **Bogus** - Fake data generation

## Configuration Options

### Seed Settings
Control data generation in `appsettings.json`:

```json
"SeedSettings": {
  "EnableSeeding": true,
  "CustomerCount": 1000,
  "MinInvoicesPerCustomer": 1,
  "MaxInvoicesPerCustomer": 5,
  "MinPhoneNumbersPerCustomer": 1,
  "MaxPhoneNumbersPerCustomer": 3
}
```

### Environment Variables
- `ASPNETCORE_ENVIRONMENT=Development` - Enables Swagger UI and detailed errors
- `ASPNETCORE_ENVIRONMENT=Production` - Production configuration

## Security Notes

- **No hardcoded credentials** - All connection strings use placeholders
- **Parameterized queries** - SQL injection prevention
- **Input validation** - Data annotation validation
- **HTTPS support** - Enabled by default

**Important:** Never commit real passwords or connection strings to version control!

## For Students

This project is designed to help you learn:
- How to build RESTful APIs with ASP.NET Core
- How to use Dapper for efficient data access
- Repository pattern and separation of concerns
- Dependency injection in .NET
- API documentation with Swagger
- SQL best practices

## For Instructors

This project includes:
- Comprehensive code annotations
- Multiple documentation files
- Working examples of common patterns
- Comparison with Entity Framework Core
- Ready-to-use demonstration scripts

## Troubleshooting

### Cannot connect to database
- Verify SQL Server is running
- Check connection string in `appsettings.json`
- Ensure correct server name (localhost, localhost\SQLEXPRESS, etc.)

### Port already in use
```bash
netstat -ano | findstr :5000
taskkill /PID <process_id> /F
```

### Swagger UI not loading
- Ensure `ASPNETCORE_ENVIRONMENT=Development`
- Clear browser cache
- Check that you're navigating to `http://localhost:5000`

## Additional Resources

- **Dapper Documentation**: https://github.com/DapperLib/Dapper
- **ASP.NET Core Documentation**: https://docs.microsoft.com/aspnet/core/
- **SQL Server Documentation**: https://docs.microsoft.com/sql/
- **Swagger Documentation**: https://swagger.io/docs/

## License

This project is provided as-is for educational purposes. Feel free to use it for learning and teaching.

## Acknowledgments

Created as an educational resource for students learning modern .NET development with Dapper ORM.

---

**Repository:** https://github.com/TCBWZA/Wave_DapperLab_Lab2  
**Author:** TCBWZA  
**For:** Students and educators in .NET development
