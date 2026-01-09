# T-SQL Truncate Scripts - Documentation

## Overview

The T-SQL truncate scripts provide database-native methods to clear all data from Customers, Invoices, and TelephoneNumbers tables. These scripts can be executed directly in SQL Server Management Studio (SSMS), Azure Data Studio, or via sqlcmd command-line tool.

## Available T-SQL Scripts

### 1. `truncate-database.sql` (Full-Featured)

**File:** `DapperLabs_Lab2\truncate-database.sql`

**Features:**
- ? **Transaction Safety** - Uses BEGIN TRANSACTION/COMMIT/ROLLBACK
- ? **3-Second Delay** - Time to cancel with Ctrl+C before execution
- ? **Before/After Counts** - Shows records deleted
- ? **Error Handling** - Comprehensive TRY/CATCH with rollback
- ? **Progress Messages** - Detailed output at each step
- ? **Verification** - Confirms all records deleted
- ? **Final Statistics** - Summary table at end

**Best for:** Manual execution, development, learning

### 2. `truncate-database-quick.sql` (Minimal)

**File:** `DapperLabs_Lab2\truncate-database-quick.sql`

**Features:**
- ? **Fast Execution** - No delays or confirmations
- ? **Minimal Code** - Simple and straightforward
- ? **Results Display** - Shows final counts
- ?? **No Transaction** - Immediate deletion
- ?? **No Confirmation** - Executes immediately

**Best for:** Automation, CI/CD pipelines, batch jobs

## Usage Methods

### Method 1: SQL Server Management Studio (SSMS)

1. **Open SSMS**
   - Connect to your SQL Server instance

2. **Open the Script**
   - File ? Open ? File
   - Navigate to `DapperLabs_Lab2\truncate-database.sql`

3. **Ensure Correct Database**
   - Verify "DapperLabs" is selected in database dropdown
   - Or script contains: `USE DapperLabs;`

4. **Execute**
   - Press **F5** or click **Execute** button
   - Watch Messages tab for progress

5. **Review Results**
   - Check Results tab for final statistics
   - Messages tab shows detailed progress

**To Cancel:**
- Press **Alt+Break** or click **Cancel Executing Query** button
- Must cancel within 3-second delay window

### Method 2: Azure Data Studio

1. **Open Azure Data Studio**
   - Connect to SQL Server

2. **Open Script**
   - File ? Open File
   - Select `truncate-database.sql`

3. **Execute**
   - Press **F5** or click **Run** button
   - View results in Results pane

**To Cancel:**
- Click **Cancel Query** button
- Must cancel within 3-second delay

### Method 3: sqlcmd Command Line

**With Windows Authentication:**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database.sql
```

**With SQL Authentication:**
```cmd
sqlcmd -S localhost -U YourUsername -P YOUR_PASSWORD -d DapperLabs -i truncate-database.sql
```

**From Project Directory:**
```cmd
cd path\to\your\DapperLabs_Lab2
sqlcmd -S localhost -d DapperLabs -i truncate-database.sql
```

**Output to File:**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database.sql -o output.txt
```

### Method 4: PowerShell with Invoke-Sqlcmd

```powershell
# Install SqlServer module if needed
Install-Module -Name SqlServer -Scope CurrentUser

# Execute script
Invoke-Sqlcmd -ServerInstance "localhost" -Database "DapperLabs" -InputFile "truncate-database.sql"
```

**With authentication:**
```powershell
$cred = Get-Credential
Invoke-Sqlcmd -ServerInstance "localhost" -Database "DapperLabs" -Credential $cred -InputFile "truncate-database.sql"
```

## Script Execution Flow

### truncate-database.sql (Detailed Flow)

```
1. USE DapperLabs
   ?
2. Display Warning Messages
   ?
3. WAITFOR DELAY '00:00:03'  ? 3-second cancellation window
   ?
4. BEGIN TRANSACTION
   ?
5. BEGIN TRY
   ?
6. Count Records (Before)
   - Customers: 1000
   - Invoices: 3500
   - Phone Numbers: 2000
   ?
7. DELETE FROM Customers  ? CASCADE DELETE removes related records
   ?
8. Reset Identity Columns
   - DBCC CHECKIDENT ('Customers', RESEED, 0)
   - DBCC CHECKIDENT ('Invoices', RESEED, 0)
   - DBCC CHECKIDENT ('TelephoneNumbers', RESEED, 0)
   ?
9. Count Records (After)
   - Verify all are 0
   ?
10. IF All Zero ? COMMIT TRANSACTION
    ELSE ? ROLLBACK TRANSACTION
    ?
11. Display Statistics
    ?
12. END TRY
    ?
13. BEGIN CATCH
    - ROLLBACK if error
    - Display error details
    - RAISERROR
    END CATCH
```

### truncate-database-quick.sql (Simple Flow)

```
1. USE DapperLabs
   ?
2. DELETE FROM Customers  ? Immediate execution
   ?
3. Reset Identity Columns
   ?
4. Display Results
   ?
5. Done
```

## Output Examples

### truncate-database.sql Output

```
========================================
  DATABASE TRUNCATE WARNING
========================================

This will DELETE ALL DATA from:
  - Customers
  - Invoices
  - TelephoneNumbers

This action CANNOT be undone!

Press Ctrl+C to cancel or continue executing to proceed...

========================================
  Starting Truncate Operation...
========================================

Counting records before deletion...
Records before deletion:
  Customers: 1000
  Invoices: 3500
  Phone Numbers: 2000

Deleting all customers (CASCADE DELETE will remove invoices and phone numbers)...
Data deleted successfully!

Resetting identity columns...
Identity columns reset to start from 1

Verifying deletion...
Records after deletion:
  Customers: 0
  Invoices: 0
  Phone Numbers: 0

========================================
  Truncate Complete!
========================================

All data deleted successfully!
Total records deleted:
  Customers: 1000
  Invoices: 3500
  Phone Numbers: 2000

Transaction committed.

========================================
  Next Steps
========================================

To re-seed the database:
1. Update appsettings.json:
   "EnableSeeding": true
2. Run the API:
   .\run.ps1

Status              Customers   Invoices    TelephoneNumbers
------------------- ----------- ----------- -----------------
Final Statistics    0           0           0

Script execution completed.
```

### truncate-database-quick.sql Output

```
(1000 rows affected)

Status              Customers   Invoices    TelephoneNumbers
------------------- ----------- ----------- -----------------
Truncate Complete   0           0           0

All data deleted. Identity columns reset to 1.
```

## Transaction Safety

### Why Transactions Matter

**truncate-database.sql uses transactions:**

```sql
BEGIN TRANSACTION;
BEGIN TRY
    -- Delete operations
    DELETE FROM Customers;
    
    -- Reset operations
    DBCC CHECKIDENT ('Customers', RESEED, 0);
    
    -- Verify
    IF all_records_deleted
        COMMIT TRANSACTION;  -- Make changes permanent
    ELSE
        ROLLBACK TRANSACTION;  -- Undo changes
END TRY
BEGIN CATCH
    ROLLBACK TRANSACTION;  -- Undo on error
    RAISERROR(...);
END CATCH
```

**Benefits:**
1. **Atomic Operation** - All or nothing
2. **Error Safety** - Rollback on any error
3. **Verification** - Can check before committing
4. **Recovery** - Can undo if something goes wrong

**truncate-database-quick.sql does NOT use transactions:**

```sql
-- No transaction - immediate execution
DELETE FROM Customers;
-- Point of no return - data is gone!
```

**Trade-offs:**
- ? Faster execution
- ? Simpler code
- ? No rollback capability
- ? No error recovery

## Error Handling

### truncate-database.sql Error Messages

**If Error Occurs:**

```
========================================
  ERROR OCCURRED
========================================

Error Number: 547
Error Message: The DELETE statement conflicted with the REFERENCE constraint "FK_Invoices_Customers"
Error Severity: 16
Error State: 0
Error Line: 45

Transaction has been rolled back.
No data was deleted.
```

**Common Errors:**

| Error | Cause | Solution |
|-------|-------|----------|
| 547 | Foreign key constraint | Check CASCADE DELETE is configured |
| 229 | Permission denied | Grant DELETE permission |
| 4060 | Database not found | Verify database name |
| 18456 | Login failed | Check credentials |

## Cancellation

### How to Cancel

**In SSMS:**
1. Within 3-second delay window
2. Press **Alt+Break**
3. Or click **Cancel Executing Query** (red square button)

**In Azure Data Studio:**
1. Within 3-second delay window
2. Click **Cancel Query** button

**With sqlcmd:**
1. Press **Ctrl+C**
2. Must be within 3-second delay

**After 3 seconds:**
- Transaction has started
- Cannot easily cancel
- Must let it complete or kill connection

## Performance Considerations

### Speed Comparison

| Records | truncate-database.sql | truncate-database-quick.sql |
|---------|----------------------|----------------------------|
| 1,000 | ~2-3 seconds | ~1 second |
| 10,000 | ~5-7 seconds | ~2-3 seconds |
| 100,000 | ~30-45 seconds | ~15-20 seconds |

**Factors affecting speed:**
- Number of records
- Server performance
- Network latency
- Transaction overhead (full script)
- Index count
- Foreign key checks

### Optimization Tips

1. **For Large Datasets:**
   ```sql
   -- Disable triggers if any
   ALTER TABLE Customers DISABLE TRIGGER ALL;
   
   -- Delete data
   DELETE FROM Customers;
   
   -- Re-enable triggers
   ALTER TABLE Customers ENABLE TRIGGER ALL;
   ```

2. **For Very Large Datasets:**
   ```sql
   -- Delete in batches
   WHILE EXISTS (SELECT 1 FROM Customers)
   BEGIN
       DELETE TOP (10000) FROM Customers;
       CHECKPOINT;  -- Write to disk periodically
   END
   ```

3. **Fastest Method (if constraints allow):**
   ```sql
   -- Temporarily drop foreign keys
   ALTER TABLE Invoices NOCHECK CONSTRAINT FK_Invoices_Customers;
   ALTER TABLE TelephoneNumbers NOCHECK CONSTRAINT FK_TelephoneNumbers_Customers;
   
   -- Use TRUNCATE (much faster than DELETE)
   TRUNCATE TABLE TelephoneNumbers;
   TRUNCATE TABLE Invoices;
   TRUNCATE TABLE Customers;
   
   -- Re-enable constraints
   ALTER TABLE Invoices CHECK CONSTRAINT FK_Invoices_Customers;
   ALTER TABLE TelephoneNumbers CHECK CONSTRAINT FK_TelephoneNumbers_Customers;
   ```

## Integration with CI/CD

### Azure DevOps Pipeline

```yaml
steps:
- task: SqlAzureDacpacDeployment@1
  displayName: 'Truncate Database'
  inputs:
    azureSubscription: 'YourSubscription'
    ServerName: '$(SqlServer)'
    DatabaseName: 'DapperLabs'
    SqlUsername: '$(SqlUser)'
    SqlPassword: '$(SqlPassword)'
    deployType: 'SqlTask'
    SqlFile: '$(Build.SourcesDirectory)/DapperLabs_Lab2/truncate-database-quick.sql'
```

### GitHub Actions

```yaml
steps:
- name: Truncate Database
  run: |
    sqlcmd -S ${{ secrets.SQL_SERVER }} -U ${{ secrets.SQL_USER }} -P ${{ secrets.SQL_PASSWORD }} -d DapperLabs -i truncate-database-quick.sql
```

### Jenkins

```groovy
stage('Truncate Database') {
    steps {
        bat 'sqlcmd -S localhost -d DapperLabs -i truncate-database-quick.sql'
    }
}
```

## Comparison with PowerShell Script

| Feature | truncate-database.sql | truncate-database.ps1 |
|---------|---------------------|---------------------|
| Confirmation | 3-sec delay | Type 'YES' |
| Transaction | Yes | No |
| Error Handling | TRY/CATCH | Try/Catch |
| Config Reading | No | Yes (appsettings.json) |
| Platform | SQL Server only | Windows + PowerShell |
| Automation | Easy (sqlcmd) | Easy (PowerShell) |
| Progress | Detailed | Detailed |
| Best for | DBAs, SSMS users | Developers, scripting |

## Best Practices

### ? DO

1. **Use Full Script for Manual Execution**
   ```
   truncate-database.sql for SSMS/Azure Data Studio
   ```

2. **Use Quick Script for Automation**
   ```
   truncate-database-quick.sql for CI/CD pipelines
   ```

3. **Backup Before Truncating**
   ```sql
   BACKUP DATABASE DapperLabs TO DISK = 'C:\Backups\DapperLabs_BeforeTruncate.bak';
   ```

4. **Run in Development/Testing**
   ```
   Development ?
   Testing ?
   Production ? (extreme caution)
   ```

5. **Verify Database Name**
   ```sql
   USE DapperLabs;  -- Check this!
   GO
   ```

### ? DON'T

1. **Don't Skip the Delay**
   ```sql
   -- Don't remove WAITFOR DELAY
   -- It's your safety net!
   ```

2. **Don't Run in Production Without Backup**
   ```
   Production + No Backup = Career-Limiting Move
   ```

3. **Don't Modify Transaction Logic**
   ```sql
   -- Keep BEGIN TRANSACTION / COMMIT / ROLLBACK
   ```

4. **Don't Remove Error Handling**
   ```sql
   -- Keep TRY/CATCH blocks
   ```

## Troubleshooting

### Issue: Script Not Found

**Error:**
```
Sqlcmd: Error: Error occurred while opening or operating on file 'truncate-database.sql'.
```

**Solution:**
```cmd
# Check current directory
cd

# Navigate to project directory
cd path\to\your\DapperLabs_Lab2

# Use full path
sqlcmd -S localhost -d DapperLabs -i "path\to\your\DapperLabs_Lab2\truncate-database.sql"
```

### Issue: Permission Denied

**Error:**
```
The DELETE permission was denied on the object 'Customers'
```

**Solution:**
```sql
-- Grant permissions
USE DapperLabs;
GRANT DELETE ON Customers TO [YourUser];
GRANT EXECUTE ON DATABASE::DapperLabs TO [YourUser];  -- For DBCC CHECKIDENT
```

### Issue: Database Does Not Exist

**Error:**
```
Cannot open database "DapperLabs" requested by the login
```

**Solution:**
```sql
-- Check database exists
SELECT name FROM sys.databases WHERE name = 'DapperLabs';

-- Or create it
CREATE DATABASE DapperLabs;
```

### Issue: Transaction Rolled Back

**Message:**
```
Transaction has been rolled back.
No data was deleted.
```

**Cause:** Verification failed (some records remain)

**Solution:**
1. Check for locking issues
2. Verify CASCADE DELETE is configured
3. Check for orphaned records

## Summary

### Quick Reference

**Execute in SSMS:**
1. Open `truncate-database.sql`
2. Press F5
3. Wait for completion

**Execute with sqlcmd:**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database.sql
```

**Quick truncate:**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database-quick.sql
```

### Script Comparison

| Need | Use This |
|------|----------|
| Manual execution | truncate-database.sql |
| Automation | truncate-database-quick.sql |
| Maximum safety | truncate-database.sql |
| Maximum speed | truncate-database-quick.sql |
| Learning SQL | truncate-database.sql |
| CI/CD pipeline | truncate-database-quick.sql |

---

**For complete project documentation, see: [README.md](README.md)**
**For PowerShell scripts, see: [TRUNCATE_DATABASE_GUIDE.md](TRUNCATE_DATABASE_GUIDE.md)**
