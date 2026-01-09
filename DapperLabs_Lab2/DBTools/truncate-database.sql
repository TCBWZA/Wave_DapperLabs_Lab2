/*
 * Dapper Labs - Database Truncate Script (T-SQL)
 * 
 * This script clears all data from Customers, Invoices, and TelephoneNumbers tables.
 * 
 * WARNING: This will DELETE ALL DATA from these tables!
 * This action CANNOT be undone!
 * 
 * Execute this script in SQL Server Management Studio (SSMS) or Azure Data Studio
 * or use sqlcmd:
 *   sqlcmd -S localhost -d DapperLabs -i truncate-database.sql
 */

USE DapperLabs;
GO

-- Display warning message
PRINT '========================================';
PRINT '  DATABASE TRUNCATE WARNING';
PRINT '========================================';
PRINT '';
PRINT 'This will DELETE ALL DATA from:';
PRINT '  - Customers';
PRINT '  - Invoices';
PRINT '  - TelephoneNumbers';
PRINT '';
PRINT 'This action CANNOT be undone!';
PRINT '';
PRINT 'Press Ctrl+C to cancel or continue executing to proceed...';
PRINT '';
GO

-- Wait for a moment (gives time to cancel if needed)
WAITFOR DELAY '00:00:03';
GO

PRINT '========================================';
PRINT '  Starting Truncate Operation...';
PRINT '========================================';
PRINT '';
GO

-- Begin transaction for safety
BEGIN TRANSACTION;

BEGIN TRY
    PRINT 'Counting records before deletion...';
    
    -- Count records before deletion
    DECLARE @CustomerCountBefore INT;
    DECLARE @InvoiceCountBefore INT;
    DECLARE @PhoneCountBefore INT;
    
    SELECT @CustomerCountBefore = COUNT(*) FROM Customers;
    SELECT @InvoiceCountBefore = COUNT(*) FROM Invoices;
    SELECT @PhoneCountBefore = COUNT(*) FROM TelephoneNumbers;
    
    PRINT 'Records before deletion:';
    PRINT '  Customers: ' + CAST(@CustomerCountBefore AS VARCHAR(10));
    PRINT '  Invoices: ' + CAST(@InvoiceCountBefore AS VARCHAR(10));
    PRINT '  Phone Numbers: ' + CAST(@PhoneCountBefore AS VARCHAR(10));
    PRINT '';
    
    -- Delete all data
    -- Note: We only need to delete from Customers because CASCADE DELETE
    -- will automatically remove all related Invoices and TelephoneNumbers
    PRINT 'Deleting all customers (CASCADE DELETE will remove invoices and phone numbers)...';
    DELETE FROM Customers;
    
    PRINT 'Data deleted successfully!';
    PRINT '';
    
    -- Reset identity columns to start from 1
    PRINT 'Resetting identity columns...';
    DBCC CHECKIDENT ('Customers', RESEED, 0);
    DBCC CHECKIDENT ('Invoices', RESEED, 0);
    DBCC CHECKIDENT ('TelephoneNumbers', RESEED, 0);
    
    PRINT 'Identity columns reset to start from 1';
    PRINT '';
    
    -- Verify deletion
    DECLARE @CustomerCountAfter INT;
    DECLARE @InvoiceCountAfter INT;
    DECLARE @PhoneCountAfter INT;
    
    SELECT @CustomerCountAfter = COUNT(*) FROM Customers;
    SELECT @InvoiceCountAfter = COUNT(*) FROM Invoices;
    SELECT @PhoneCountAfter = COUNT(*) FROM TelephoneNumbers;
    
    PRINT 'Verifying deletion...';
    PRINT 'Records after deletion:';
    PRINT '  Customers: ' + CAST(@CustomerCountAfter AS VARCHAR(10));
    PRINT '  Invoices: ' + CAST(@InvoiceCountAfter AS VARCHAR(10));
    PRINT '  Phone Numbers: ' + CAST(@PhoneCountAfter AS VARCHAR(10));
    PRINT '';
    
    -- Check if all records were deleted
    IF @CustomerCountAfter = 0 AND @InvoiceCountAfter = 0 AND @PhoneCountAfter = 0
    BEGIN
        PRINT '========================================';
        PRINT '  Truncate Complete!';
        PRINT '========================================';
        PRINT '';
        PRINT 'All data deleted successfully!';
        PRINT 'Total records deleted:';
        PRINT '  Customers: ' + CAST(@CustomerCountBefore AS VARCHAR(10));
        PRINT '  Invoices: ' + CAST(@InvoiceCountBefore AS VARCHAR(10));
        PRINT '  Phone Numbers: ' + CAST(@PhoneCountBefore AS VARCHAR(10));
        PRINT '';
        
        -- Commit the transaction
        COMMIT TRANSACTION;
        PRINT 'Transaction committed.';
    END
    ELSE
    BEGIN
        PRINT 'WARNING: Some records may still exist!';
        PRINT 'Rolling back transaction...';
        ROLLBACK TRANSACTION;
        PRINT 'Transaction rolled back.';
    END
    
    PRINT '';
    PRINT '========================================';
    PRINT '  Next Steps';
    PRINT '========================================';
    PRINT '';
    PRINT 'To re-seed the database:';
    PRINT '1. Update appsettings.json:';
    PRINT '   "EnableSeeding": true';
    PRINT '2. Run the API:';
    PRINT '   .\run.ps1';
    PRINT '';
END TRY
BEGIN CATCH
    -- Error occurred, rollback transaction
    IF @@TRANCOUNT > 0
    BEGIN
        ROLLBACK TRANSACTION;
        PRINT '';
        PRINT '========================================';
        PRINT '  ERROR OCCURRED';
        PRINT '========================================';
        PRINT '';
    END
    
    -- Display error information
    DECLARE @ErrorMessage NVARCHAR(4000);
    DECLARE @ErrorSeverity INT;
    DECLARE @ErrorState INT;
    
    SELECT 
        @ErrorMessage = ERROR_MESSAGE(),
        @ErrorSeverity = ERROR_SEVERITY(),
        @ErrorState = ERROR_STATE();
    
    PRINT 'Error Number: ' + CAST(ERROR_NUMBER() AS VARCHAR(10));
    PRINT 'Error Message: ' + @ErrorMessage;
    PRINT 'Error Severity: ' + CAST(@ErrorSeverity AS VARCHAR(10));
    PRINT 'Error State: ' + CAST(@ErrorState AS VARCHAR(10));
    PRINT 'Error Line: ' + CAST(ERROR_LINE() AS VARCHAR(10));
    PRINT '';
    PRINT 'Transaction has been rolled back.';
    PRINT 'No data was deleted.';
    PRINT '';
    
    -- Re-throw the error
    RAISERROR(@ErrorMessage, @ErrorSeverity, @ErrorState);
END CATCH;
GO

-- Display final statistics
SELECT 
    'Final Statistics' AS Status,
    (SELECT COUNT(*) FROM Customers) AS Customers,
    (SELECT COUNT(*) FROM Invoices) AS Invoices,
    (SELECT COUNT(*) FROM TelephoneNumbers) AS TelephoneNumbers;
GO

PRINT '';
PRINT 'Script execution completed.';
PRINT '';
GO
