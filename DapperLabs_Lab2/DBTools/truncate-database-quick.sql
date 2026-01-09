/*
 * Dapper Labs - Quick Truncate Script (T-SQL)
 * 
 * Simple script to quickly clear all data.
 * 
 * WARNING: This immediately deletes all data with no confirmation!
 * Use with caution!
 */

USE DapperLabs;
GO

-- Delete all customers (CASCADE DELETE removes invoices and phone numbers)
DELETE FROM Customers;
GO

-- Reset identity columns
DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('Invoices', RESEED, 0);
DBCC CHECKIDENT ('TelephoneNumbers', RESEED, 0);
GO

-- Show results
SELECT 
    'Truncate Complete' AS Status,
    (SELECT COUNT(*) FROM Customers) AS Customers,
    (SELECT COUNT(*) FROM Invoices) AS Invoices,
    (SELECT COUNT(*) FROM TelephoneNumbers) AS TelephoneNumbers;
GO

PRINT 'All data deleted. Identity columns reset to 1.';
GO
