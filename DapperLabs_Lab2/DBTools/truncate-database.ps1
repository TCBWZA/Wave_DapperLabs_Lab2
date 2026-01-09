# Dapper Labs - Database Truncate Script
# This script clears all data from Customers, Invoices, and TelephoneNumbers tables

Write-Host "====================================" -ForegroundColor Red
Write-Host "  DATABASE TRUNCATE WARNING" -ForegroundColor Red
Write-Host "====================================" -ForegroundColor Red
Write-Host ""
Write-Host "This will DELETE ALL DATA from:" -ForegroundColor Yellow
Write-Host "  - Customers" -ForegroundColor Yellow
Write-Host "  - Invoices" -ForegroundColor Yellow
Write-Host "  - TelephoneNumbers" -ForegroundColor Yellow
Write-Host ""
Write-Host "This action CANNOT be undone!" -ForegroundColor Red
Write-Host ""

# Ask for confirmation
$confirmation = Read-Host "Are you sure you want to continue? Type 'YES' to confirm"

if ($confirmation -ne "YES") {
    Write-Host ""
    Write-Host "Operation cancelled." -ForegroundColor Yellow
    Write-Host ""
    exit
}

Write-Host ""
Write-Host "====================================" -ForegroundColor Cyan
Write-Host "  Truncating Database Tables..." -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Get connection string from appsettings.json
$appSettingsPath = Join-Path $PSScriptRoot "appsettings.json"

if (-not (Test-Path $appSettingsPath)) {
    Write-Host "Error: appsettings.json not found at $appSettingsPath" -ForegroundColor Red
    exit 1
}

try {
    $appSettings = Get-Content $appSettingsPath -Raw | ConvertFrom-Json
    $connectionString = $appSettings.ConnectionStrings.DefaultConnection
    
    if ([string]::IsNullOrEmpty($connectionString)) {
        Write-Host "Error: Connection string not found in appsettings.json" -ForegroundColor Red
        exit 1
    }
    
    Write-Host "Connection string loaded successfully" -ForegroundColor Green
}
catch {
    Write-Host "Error reading appsettings.json: $_" -ForegroundColor Red
    exit 1
}

# Parse connection string to get server and database
$connectionString -match "Server=([^;]+);.*Database=([^;]+);" | Out-Null
$server = $Matches[1]
$database = $Matches[2]

Write-Host "Server: $server" -ForegroundColor Gray
Write-Host "Database: $database" -ForegroundColor Gray
Write-Host ""

# SQL commands to truncate tables
# Note: TRUNCATE cannot be used due to foreign key constraints
# Using DELETE instead, which respects foreign key cascade rules
$sqlCommands = @"
-- Delete all data (CASCADE DELETE will handle related records)
DELETE FROM Customers;

-- Reset identity columns to start from 1
DBCC CHECKIDENT ('Customers', RESEED, 0);
DBCC CHECKIDENT ('Invoices', RESEED, 0);
DBCC CHECKIDENT ('TelephoneNumbers', RESEED, 0);

-- Verify deletion
SELECT 
    (SELECT COUNT(*) FROM Customers) AS CustomerCount,
    (SELECT COUNT(*) FROM Invoices) AS InvoiceCount,
    (SELECT COUNT(*) FROM TelephoneNumbers) AS PhoneNumberCount;
"@

try {
    Write-Host "Connecting to database..." -ForegroundColor Yellow
    
    # Use SqlClient to execute commands
    Add-Type -AssemblyName "System.Data"
    $connection = New-Object System.Data.SqlClient.SqlConnection
    $connection.ConnectionString = $connectionString
    $connection.Open()
    
    Write-Host "Connected successfully" -ForegroundColor Green
    Write-Host ""
    Write-Host "Deleting data..." -ForegroundColor Yellow
    
    # Execute the SQL commands
    $command = $connection.CreateCommand()
    $command.CommandText = $sqlCommands
    $command.CommandTimeout = 30
    
    $reader = $command.ExecuteReader()
    
    # Read the verification counts
    if ($reader.Read()) {
        $customerCount = $reader["CustomerCount"]
        $invoiceCount = $reader["InvoiceCount"]
        $phoneCount = $reader["PhoneNumberCount"]
        
        Write-Host ""
        Write-Host "====================================" -ForegroundColor Green
        Write-Host "  Truncate Complete!" -ForegroundColor Green
        Write-Host "====================================" -ForegroundColor Green
        Write-Host ""
        Write-Host "Remaining records:" -ForegroundColor Gray
        Write-Host "  Customers: $customerCount" -ForegroundColor Gray
        Write-Host "  Invoices: $invoiceCount" -ForegroundColor Gray
        Write-Host "  Phone Numbers: $phoneCount" -ForegroundColor Gray
        Write-Host ""
        
        if ($customerCount -eq 0 -and $invoiceCount -eq 0 -and $phoneCount -eq 0) {
            Write-Host "All data deleted successfully!" -ForegroundColor Green
            Write-Host "Identity columns reset to start from 1" -ForegroundColor Green
        }
        else {
            Write-Host "Warning: Some records may still exist" -ForegroundColor Yellow
        }
    }
    
    $reader.Close()
    $connection.Close()
    
    Write-Host ""
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host "  Next Steps" -ForegroundColor Cyan
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "To re-seed the database, update appsettings.json:" -ForegroundColor Gray
    Write-Host '  "EnableSeeding": true' -ForegroundColor Gray
    Write-Host ""
    Write-Host "Then run the API:" -ForegroundColor Gray
    Write-Host "  .\run.ps1" -ForegroundColor Gray
    Write-Host ""
}
catch {
    Write-Host ""
    Write-Host "Error: $_" -ForegroundColor Red
    Write-Host ""
    Write-Host "Stack Trace:" -ForegroundColor Red
    Write-Host $_.ScriptStackTrace -ForegroundColor Red
    
    if ($connection -and $connection.State -eq "Open") {
        $connection.Close()
    }
    
    exit 1
}

Write-Host "Press any key to exit..."
$null = $Host.UI.RawUI.ReadKey("NoEcho,IncludeKeyDown")
