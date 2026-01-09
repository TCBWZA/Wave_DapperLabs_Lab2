using Microsoft.Data.SqlClient;
using Dapper;

namespace DapperLabs_Lab2
{
    public static class DatabaseInitializer
    {
        public static async Task InitializeDatabaseAsync(string connectionString)
        {
            var builder = new SqlConnectionStringBuilder(connectionString);
            var databaseName = builder.InitialCatalog;
            builder.InitialCatalog = "master";
            var masterConnectionString = builder.ConnectionString;

            using (var connection = new SqlConnection(masterConnectionString))
            {
                var checkDbSql = $"SELECT COUNT(*) FROM sys.databases WHERE name = '{databaseName}'";
                var dbExists = await connection.ExecuteScalarAsync<int>(checkDbSql) > 0;

                if (!dbExists)
                {
                    Console.WriteLine($"Creating database '{databaseName}'...");
                    await connection.ExecuteAsync($"CREATE DATABASE [{databaseName}]");
                    Console.WriteLine($"? Database '{databaseName}' created successfully.");
                }
                else
                {
                    Console.WriteLine($"? Database '{databaseName}' already exists.");
                }
            }

            using (var connection = new SqlConnection(connectionString))
            {
                await CreateTablesAsync(connection);
            }
        }

        private static async Task CreateTablesAsync(SqlConnection connection)
        {
            var createCustomersTable = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
                BEGIN
                    CREATE TABLE Customers (
                        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                        Name NVARCHAR(200),
                        Email NVARCHAR(200)
                    );
                    CREATE UNIQUE INDEX IX_Customers_Email ON Customers(Email);
                END";

            var createInvoicesTable = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
                BEGIN
                    CREATE TABLE Invoices (
                        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                        InvoiceNumber NVARCHAR(50) NOT NULL,
                        CustomerId BIGINT NOT NULL,
                        InvoiceDate DATETIME NOT NULL,
                        Amount DECIMAL(18,2) NOT NULL,
                        CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
                        CONSTRAINT CK_Invoice_Amount CHECK (Amount >= 0)
                    );
                    CREATE UNIQUE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
                END";

            var createTelephoneNumbersTable = @"
                IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TelephoneNumbers')
                BEGIN
                    CREATE TABLE TelephoneNumbers (
                        Id BIGINT IDENTITY(1,1) PRIMARY KEY,
                        CustomerId BIGINT NOT NULL,
                        Type NVARCHAR(20),
                        Number NVARCHAR(50),
                        CONSTRAINT FK_TelephoneNumbers_Customers FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE,
                        CONSTRAINT CK_TelephoneNumber_Type CHECK (Type IN ('Mobile', 'Work', 'DirectDial'))
                    );
                END";

            Console.WriteLine("Creating tables...");
            await connection.ExecuteAsync(createCustomersTable);
            Console.WriteLine("? Customers table ready");
            
            await connection.ExecuteAsync(createInvoicesTable);
            Console.WriteLine("? Invoices table ready");
            
            await connection.ExecuteAsync(createTelephoneNumbersTable);
            Console.WriteLine("? TelephoneNumbers table ready");
        }
    }
}
