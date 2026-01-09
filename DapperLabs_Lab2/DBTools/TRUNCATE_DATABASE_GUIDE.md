# Database Truncate Scripts - Documentation

## Overview

The database truncate scripts allow you to completely clear all data from the Customers, Invoices, and TelephoneNumbers tables. This is useful for:
- Resetting the database to a clean state
- Testing data seeding
- Starting fresh with new data
- Classroom/lab resets

## ?? WARNING

**These scripts will DELETE ALL DATA from:**
- ? Customers
- ? Invoices (cascade deleted when customers are deleted)
- ? TelephoneNumbers (cascade deleted when customers are deleted)

**This action CANNOT be undone!**

Always ensure you have backups if needed before running these scripts.

## Available Scripts

### 1. `truncate-database.ps1` (PowerShell)

**Usage:**
```powershell
.\truncate-database.ps1
```

**Features:**
- ? Confirmation prompt (must type 'YES')
- ? Reads connection string from appsettings.json
- ? Deletes all customer data (cascades to invoices and phone numbers)
- ? Resets identity columns to start from 1
- ? Verifies deletion was successful
- ? Provides next steps

### 2. `truncate-database.bat` (Batch File)

**Usage:**
```cmd
truncate-database.bat
```

**Features:**
- ? Warning message
- ? Calls the PowerShell script
- ? Works from Command Prompt

### 3. `truncate-database.sql` (T-SQL Script)

**Usage in SSMS/Azure Data Studio:**
1. Open SQL Server Management Studio or Azure Data Studio
2. Connect to your SQL Server instance
3. Open `truncate-database.sql`
4. Press F5 or click Execute

**Usage with sqlcmd:**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database.sql
```

**Or with authentication:**
```cmd
sqlcmd -S localhost -U YourUsername -P YOUR_PASSWORD -d DapperLabs -i truncate-database.sql
```

**Features:**
- ? Transaction safety (rollback on error)
- ? 3-second delay for cancellation (Ctrl+C)
- ? Before/after record counts
- ? Comprehensive error handling
- ? Progress messages
- ? Verification checks
- ? Pure T-SQL (no PowerShell required)

### 4. `truncate-database-quick.sql` (Quick T-SQL)

**Usage:**
```cmd
sqlcmd -S localhost -d DapperLabs -i truncate-database-quick.sql
```

**Features:**
- ? Fast execution (no delays or confirmations)
- ? Simple and straightforward
- ? Minimal output
- ?? No transaction safety
- ?? No confirmation prompt

**Best for:** Automated scripts, CI/CD pipelines

## Choosing the Right Script

| Script | Best For | Confirmation | Transaction | Platform |
|--------|----------|--------------|-------------|----------|
| `truncate-database.ps1` | Daily use | Required (type YES) | Yes | PowerShell |
| `truncate-database.bat` | Command Prompt | Required (type YES) | Yes | Windows |
| `truncate-database.sql` | SSMS/Database tools | 3-sec delay | Yes | SQL Server |
| `truncate-database-quick.sql` | Automation/CI | None | No | SQL Server |

## How It Works

### Step-by-Step Process

1. **Display Warning**
   - Shows what will be deleted
   - Requires confirmation

2. **Read Configuration**
   - Loads connection string from `appsettings.json`
   - Validates configuration

3. **Connect to Database**
   - Uses SqlClient to connect to SQL Server
   - Validates connection

4. **Delete Data**
   ```sql
   DELETE FROM Customers;
   ```
   - Deletes all customers
   - CASCADE DELETE automatically removes invoices and phone numbers

5. **Reset Identity Columns**
   ```sql
   DBCC CHECKIDENT ('Customers', RESEED, 0);
   DBCC CHECKIDENT ('Invoices', RESEED, 0);
   DBCC CHECKIDENT ('TelephoneNumbers', RESEED, 0);
   ```
   - Resets auto-increment IDs to start from 1
   - Next inserted record will have ID = 1

6. **Verify Deletion**
   ```sql
   SELECT 
       (SELECT COUNT(*) FROM Customers) AS CustomerCount,
       (SELECT COUNT(*) FROM Invoices) AS InvoiceCount,
       (SELECT COUNT(*) FROM TelephoneNumbers) AS PhoneNumberCount;
   ```
   - Confirms all tables are empty

## Usage Examples

### Example 1: Simple Reset

```powershell
# Truncate database
.\truncate-database.ps1

# Type 'YES' when prompted
YES

# Wait for completion
# API will show: "All data deleted successfully!"
```

### Example 2: Truncate and Re-seed

```powershell
# Step 1: Truncate database
.\truncate-database.ps1
# Type 'YES'

# Step 2: Ensure seeding is enabled in appsettings.json
# "EnableSeeding": true

# Step 3: Run API to re-seed
.\run.ps1

# API will automatically seed new data
```

### Example 3: Fresh Start with Different Data

```powershell
# Truncate
.\truncate-database.ps1

# Update appsettings.json with new seed settings
# "CustomerCount": 5000

# Run API
.\run.ps1

# New data generated with 5000 customers
```

## Output Example

### Successful Truncate

```
====================================
  DATABASE TRUNCATE WARNING
====================================

This will DELETE ALL DATA from:
  - Customers
  - Invoices
  - TelephoneNumbers

This action CANNOT be undone!

Are you sure you want to continue? Type 'YES' to confirm: YES

====================================
  Truncating Database Tables...
====================================

Connection string loaded successfully
Server: localhost
Database: DapperLabs

Connecting to database...
Connected successfully

Deleting data...

====================================
  Truncate Complete!
====================================

Remaining records:
  Customers: 0
  Invoices: 0
  Phone Numbers: 0

All data deleted successfully!
Identity columns reset to start from 1

====================================
  Next Steps
====================================

To re-seed the database, update appsettings.json:
  "EnableSeeding": true

Then run the API:
  .\run.ps1

Press any key to exit...
```

## Technical Details

### Why DELETE Instead of TRUNCATE?

```sql
-- TRUNCATE is faster but doesn't work with foreign keys
TRUNCATE TABLE Customers;  -- ERROR: Cannot truncate table due to foreign key constraints

-- DELETE works with foreign keys and respects CASCADE DELETE
DELETE FROM Customers;     -- Works! Cascades to Invoices and TelephoneNumbers
```

**Benefits of DELETE:**
- ? Respects foreign key constraints
- ? CASCADE DELETE automatically removes related records
- ? Can be rolled back if in transaction
- ? Triggers fire (if any exist)

**TRUNCATE limitations:**
- ? Cannot be used with tables that have foreign key constraints
- ? Cannot be rolled back
- ? Triggers don't fire

### Identity Column Reset

```sql
-- Reset to 0, next insert will be 1
DBCC CHECKIDENT ('Customers', RESEED, 0);

-- After this, the next customer will have ID = 1
INSERT INTO Customers (Name, Email) VALUES ('Test', 'test@example.com');
-- New customer has ID = 1
```

### Foreign Key Cascade

Database schema uses CASCADE DELETE:

```sql
CREATE TABLE Invoices (
    Id BIGINT PRIMARY KEY IDENTITY,
    CustomerId BIGINT NOT NULL,
    FOREIGN KEY (CustomerId) REFERENCES Customers(Id) ON DELETE CASCADE
);
```

When you delete a customer:
1. Customer record deleted
2. All invoices for that customer automatically deleted
3. All phone numbers for that customer automatically deleted

## Safety Features

### 1. Confirmation Required

```powershell
Are you sure you want to continue? Type 'YES' to confirm:
```

Must type exactly 'YES' (case-sensitive). Any other input cancels:
- 'yes' ? Cancelled
- 'y' ? Cancelled
- 'YES' ? Proceeds

### 2. Connection String Validation

```powershell
if ([string]::IsNullOrEmpty($connectionString)) {
    Write-Host "Error: Connection string not found" -ForegroundColor Red
    exit 1
}
```

Validates configuration before attempting connection.

### 3. Error Handling

```powershell
try {
    # Truncate operations
}
catch {
    Write-Host "Error: $_" -ForegroundColor Red
    # Close connection
    # Exit with error code
}
```

Catches and reports all errors with full stack trace.

### 4. Verification

```sql
SELECT COUNT(*) FROM Customers;
SELECT COUNT(*) FROM Invoices;
SELECT COUNT(*) FROM TelephoneNumbers;
```

Confirms deletion was successful.

## Troubleshooting

### Issue: Permission Denied

**Error:**
```
Error: The DELETE permission was denied on the object 'Customers'
```

**Solution:**
```sql
-- Grant DELETE permission to your SQL Server user
GRANT DELETE ON Customers TO [YourUser];
GRANT DELETE ON Invoices TO [YourUser];
GRANT DELETE ON TelephoneNumbers TO [YourUser];
```

### Issue: Cannot Connect to Database

**Error:**
```
Error: A network-related or instance-specific error occurred
```

**Solution:**
1. Verify SQL Server is running
2. Check connection string in appsettings.json
3. Test connection: `sqlcmd -S localhost -U YourUsername -P YOUR_PASSWORD`

### Issue: Execution Policy Error

**Error:**
```
.\truncate-database.ps1 cannot be loaded because running scripts is disabled
```

**Solution:**
```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Issue: Foreign Key Constraint Error

**Error:**
```
Error: The DELETE statement conflicted with the REFERENCE constraint
```

**This should not happen** because:
1. We only delete from Customers table
2. Foreign keys have CASCADE DELETE
3. Related records automatically deleted

**If this occurs:**
1. Check database schema has CASCADE DELETE
2. Manually run DatabaseInitializer to fix schema
3. Contact database administrator

## Best Practices

### ? DO

1. **Always confirm you want to delete data**
   ```powershell
   # Read the warning carefully before typing YES
   ```

2. **Have a backup if data is important**
   ```sql
   -- Backup before truncate
   BACKUP DATABASE DapperLabs TO DISK = 'C:\Backups\DapperLabs.bak'
   ```

3. **Use in development/testing environments**
   ```
   Development ?
   Testing ?
   Production ? (use with extreme caution)
   ```

4. **Check remaining records**
   ```
   Verify the counts are all 0 after truncate
   ```

### ? DON'T

1. **Don't use in production without backup**
   ```
   Production + No Backup = Bad Idea!
   ```

2. **Don't skip the confirmation**
   ```
   Always read the warning message
   ```

3. **Don't run while API is running**
   ```powershell
   # Stop API first
   Ctrl + C

   # Then truncate
   .\truncate-database.ps1
   ```

4. **Don't truncate if you need the data**
   ```
   This is permanent! Check twice, delete once.
   ```

## Common Scenarios

### Scenario 1: Testing Data Seeding

```powershell
# 1. Truncate existing data
.\truncate-database.ps1

# 2. Update seed settings
# Edit appsettings.json:
# "CustomerCount": 100

# 3. Re-run API to seed
.\run.ps1

# 4. Verify in Swagger UI
```

### Scenario 2: Classroom Reset

```powershell
# At end of each class session:

# 1. Stop all student APIs
Get-Process -Name dotnet | Stop-Process

# 2. Truncate database
.\truncate-database.ps1

# 3. Next class: Fresh start
.\run.ps1
```

### Scenario 3: Development Reset

```powershell
# Made a mistake? Start over:

# 1. Truncate
.\truncate-database.ps1

# 2. Re-seed with default settings
# Ensure "EnableSeeding": true

# 3. Restart API
.\run.ps1
```

### Scenario 4: Performance Testing

```powershell
# Test with different data volumes:

# Test 1: 1000 customers
.\truncate-database.ps1
# Edit appsettings.json: "CustomerCount": 1000
.\run.ps1

# Test 2: 10000 customers
.\truncate-database.ps1
# Edit appsettings.json: "CustomerCount": 10000
.\run.ps1

# Compare performance
```

## Integration with Other Scripts

### After Truncate, Re-seed Data

```powershell
# Automated workflow
.\truncate-database.ps1  # Clear data
.\run.ps1                # Start API (auto-seeds if enabled)
```

### Check Data Before Truncate

```powershell
# Count records before truncating
sqlcmd -S localhost -d DapperLabs -Q "SELECT (SELECT COUNT(*) FROM Customers) AS Customers, (SELECT COUNT(*) FROM Invoices) AS Invoices"

# If you want to keep data, backup first
sqlcmd -Q "BACKUP DATABASE DapperLabs TO DISK = 'C:\Temp\backup.bak'"

# Then truncate
.\truncate-database.ps1
```

## For Students

### What This Script Does

**Simple explanation:**
1. Deletes all customers
2. Deletes all invoices (automatically)
3. Deletes all phone numbers (automatically)
4. Resets IDs to start from 1 again

**When to use:**
- Want to start over
- Testing data creation
- Made mistakes in data

**How to use:**
```powershell
.\truncate-database.ps1
```

Type `YES` when asked.

### Important Notes

?? **This deletes EVERYTHING!**
- Can't undo
- All data gone
- Need to re-seed to get data back

? **To get data back:**
```powershell
# Make sure seeding is on in appsettings.json
# "EnableSeeding": true

# Run API
.\run.ps1

# Data comes back automatically
```

## For Instructors

### Lab Setup

**Start of semester:**
```powershell
# Initial seed
.\run.ps1
```

**Between lab sessions:**
```powershell
# Reset to clean state
.\truncate-database.ps1

# Students start fresh next time
```

**For graded assignments:**
```powershell
# Give students empty database
.\truncate-database.ps1

# They must implement data creation
```

### Advantages for Teaching

1. **Consistent Starting Point**
   - Every student starts with empty database
   - Or same seeded data

2. **Easy Reset**
   - Fix student mistakes quickly
   - Start over without reinstalling

3. **Test Scenarios**
   - Empty database tests
   - Full database tests
   - Custom seed scenarios

## Summary

### Quick Reference

**Truncate database:**
```powershell
.\truncate-database.ps1
```

**Truncate and re-seed:**
```powershell
.\truncate-database.ps1
.\run.ps1
```

**Truncate from Command Prompt:**
```cmd
truncate-database.bat
```

### What Gets Deleted

| Table | Records Deleted | ID Reset |
|-------|----------------|----------|
| Customers | All | ? Reset to 1 |
| Invoices | All (cascade) | ? Reset to 1 |
| TelephoneNumbers | All (cascade) | ? Reset to 1 |

### Safety Checklist

- ? Warning message shown
- ? Confirmation required (type 'YES')
- ? Connection validated
- ? Error handling
- ? Verification of deletion
- ? Next steps provided

### Recovery

**If you truncated by mistake:**
1. ? **Cannot undo** - data is gone
2. ? **Can re-seed** - automatic data generation
3. ? **Can restore** - if you have a backup

---

**For complete project documentation, see: [README.md](README.md)**
