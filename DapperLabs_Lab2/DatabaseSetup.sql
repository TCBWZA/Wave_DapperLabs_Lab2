-- Dapper Labs Database Setup Script
-- This script creates the database and tables if you prefer manual setup
-- The application can also create these automatically via DatabaseInitializer.cs

-- Create Database
IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = 'DapperLabs')
BEGIN
    CREATE DATABASE [DapperLabs];
END
GO

USE [DapperLabs];
GO

-- Create Customers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Customers')
BEGIN
    CREATE TABLE [Customers] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [Name] NVARCHAR(200),
        [Email] NVARCHAR(200)
    );
    
    CREATE UNIQUE INDEX IX_Customers_Email ON Customers(Email);
    
    PRINT 'Customers table created';
END
ELSE
BEGIN
    PRINT 'Customers table already exists';
END
GO

-- Create Invoices Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'Invoices')
BEGIN
    CREATE TABLE [Invoices] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [InvoiceNumber] NVARCHAR(50) NOT NULL,
        [CustomerId] BIGINT NOT NULL,
        [InvoiceDate] DATETIME NOT NULL,
        [Amount] DECIMAL(18,2) NOT NULL,
        CONSTRAINT FK_Invoices_Customers FOREIGN KEY (CustomerId) 
            REFERENCES Customers(Id) ON DELETE CASCADE,
        CONSTRAINT CK_Invoice_Amount CHECK (Amount >= 0)
    );
    
    CREATE UNIQUE INDEX IX_Invoices_InvoiceNumber ON Invoices(InvoiceNumber);
    
    PRINT 'Invoices table created';
END
ELSE
BEGIN
    PRINT 'Invoices table already exists';
END
GO

-- Create TelephoneNumbers Table
IF NOT EXISTS (SELECT * FROM sys.tables WHERE name = 'TelephoneNumbers')
BEGIN
    CREATE TABLE [TelephoneNumbers] (
        [Id] BIGINT IDENTITY(1,1) PRIMARY KEY,
        [CustomerId] BIGINT NOT NULL,
        [Type] NVARCHAR(20),
        [Number] NVARCHAR(50),
        CONSTRAINT FK_TelephoneNumbers_Customers FOREIGN KEY (CustomerId) 
            REFERENCES Customers(Id) ON DELETE CASCADE,
        CONSTRAINT CK_TelephoneNumber_Type CHECK (Type IN ('Mobile', 'Work', 'DirectDial'))
    );
    
    PRINT 'TelephoneNumbers table created';
END
ELSE
BEGIN
    PRINT 'TelephoneNumbers table already exists';
END
GO

PRINT 'Database setup complete!';
GO

-- Optional: View table structure
SELECT 
    t.name AS TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLength,
    c.is_nullable AS IsNullable,
    c.is_identity AS IsIdentity
FROM sys.tables t
INNER JOIN sys.columns c ON t.object_id = c.object_id
INNER JOIN sys.types ty ON c.user_type_id = ty.user_type_id
WHERE t.name IN ('Customers', 'Invoices', 'TelephoneNumbers')
ORDER BY t.name, c.column_id;
GO

-- View constraints
SELECT 
    t.name AS TableName,
    con.name AS ConstraintName,
    con.type_desc AS ConstraintType
FROM sys.tables t
INNER JOIN sys.check_constraints con ON t.object_id = con.parent_object_id
WHERE t.name IN ('Customers', 'Invoices', 'TelephoneNumbers')
UNION
SELECT 
    t.name AS TableName,
    fk.name AS ConstraintName,
    'FOREIGN KEY' AS ConstraintType
FROM sys.tables t
INNER JOIN sys.foreign_keys fk ON t.object_id = fk.parent_object_id
WHERE t.name IN ('Customers', 'Invoices', 'TelephoneNumbers')
UNION
SELECT 
    t.name AS TableName,
    i.name AS ConstraintName,
    'INDEX' AS ConstraintType
FROM sys.tables t
INNER JOIN sys.indexes i ON t.object_id = i.object_id
WHERE t.name IN ('Customers', 'Invoices', 'TelephoneNumbers')
    AND i.is_primary_key = 0
    AND i.type > 0
ORDER BY TableName, ConstraintType;
GO
