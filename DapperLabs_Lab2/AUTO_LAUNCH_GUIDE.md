# Auto-Launch Scripts Guide

## Overview

The Dapper Labs API includes convenient startup scripts that automatically open your browser to Swagger UI when the API starts.

## Available Scripts

### 1. `run.ps1` (PowerShell - Recommended)

**Location:** `DapperLabs_Lab2\run.ps1`

**Usage:**
```powershell
.\run.ps1
```

**Features:**
- Starts the API
- Waits 5 seconds for startup
- Automatically opens browser to http://localhost:5000
- Shows API status and URLs
- Keeps running until you press Ctrl+C

**Output:**
```
====================================
  Starting Dapper Labs API...
====================================

Waiting for API to start (5 seconds)...
Opening browser to Swagger UI...

====================================
  API Started!
  Swagger UI: http://localhost:5000
  API URLs:
    - Customers: http://localhost:5000/api/customers
    - Invoices: http://localhost:5000/api/invoices
    - Phone Numbers: http://localhost:5000/api/telephonenumbers
====================================

API is running in process ID: 12345
To stop the API, press Ctrl+C or close this window
```

### 2. `start-api.bat` (Windows Batch File)

**Location:** `DapperLabs_Lab2\start-api.bat`

**Usage:**
```cmd
start-api.bat
```

**Features:**
- Opens API in separate console window
- Waits 5 seconds for startup
- Automatically opens browser to http://localhost:5000
- Shows startup messages

**Best for:** Users who prefer Command Prompt or need the API in a separate window

### 3. `start-api.ps1` (Advanced PowerShell)

**Location:** `DapperLabs_Lab2\start-api.ps1`

**Usage:**
```powershell
.\start-api.ps1
```

**Features:**
- Starts API as background job
- Checks if API is actually responding (up to 10 attempts)
- Only opens browser when API is confirmed ready
- Shows API console output
- More reliable for slower machines

## Choosing the Right Script

| Script | Best For | Browser Opens | API Output |
|--------|----------|---------------|------------|
| `run.ps1` | Quick testing | After 5 sec delay | No (runs in background) |
| `start-api.bat` | Command Prompt users | After 5 sec delay | Yes (separate window) |
| `start-api.ps1` | Reliable startup | When API responds | Yes (in same window) |

## Troubleshooting

### PowerShell Execution Policy Error

**Error:**
```
.\run.ps1 : File cannot be loaded because running scripts is disabled on this system.
```

**Solution:**
```powershell
# Option 1: Bypass policy for this session only
Set-ExecutionPolicy -ExecutionPolicy Bypass -Scope Process
.\run.ps1

# Option 2: Allow local scripts (requires admin)
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

### Port Already in Use

**Error:**
```
Failed to bind to address http://127.0.0.1:5000
```

**Solution:**
```powershell
# Find and kill process using port 5000
netstat -ano | findstr :5000
taskkill /PID <process_id> /F
```

### Browser Doesn't Open

**Possible Causes:**
1. Default browser not set
2. 5-second delay too short (API still starting)
3. Browser is blocked by policy

**Solutions:**
1. Manually open: http://localhost:5000
2. Increase delay in script (change `Start-Sleep -Seconds 5` to `Start-Sleep -Seconds 10`)
3. Check browser settings

### API Doesn't Start

**Check:**
1. SQL Server is running
2. Connection string is correct in `appsettings.json`
3. .NET 8 SDK is installed: `dotnet --version`
4. No build errors: `dotnet build`

## Customizing the Scripts

### Adjust Startup Delay

**In `run.ps1`:**
```powershell
# Change this line
Start-Sleep -Seconds 5

# To (for slower machines)
Start-Sleep -Seconds 10
```

**In `start-api.bat`:**
```batch
REM Change this line
timeout /t 5 /nobreak >nul

REM To
timeout /t 10 /nobreak >nul
```

### Change Default Browser

**Windows:** Set your preferred browser as default in Windows Settings

**Or open specific browser in script:**
```powershell
# Chrome
Start-Process "chrome.exe" "http://localhost:5000"

# Firefox
Start-Process "firefox.exe" "http://localhost:5000"

# Edge
Start-Process "msedge.exe" "http://localhost:5000"
```

### Open to Specific Endpoint

**In any script, change:**
```powershell
Start-Process "http://localhost:5000"
```

**To:**
```powershell
# Open directly to Customers endpoint
Start-Process "http://localhost:5000/api/customers"

# Open to specific Swagger section
Start-Process "http://localhost:5000/#/Customers"
```

## Script Details

### What `run.ps1` Does

1. **Display startup message**
   ```powershell
   Write-Host "Starting Dapper Labs API..." -ForegroundColor Cyan
   ```

2. **Start API in background**
   ```powershell
   $process = Start-Process -FilePath "dotnet" -ArgumentList "run" -PassThru -NoNewWindow
   ```

3. **Wait for startup**
   ```powershell
   Start-Sleep -Seconds 5
   ```

4. **Open browser**
   ```powershell
   Start-Process "http://localhost:5000"
   ```

5. **Keep script running**
   ```powershell
   Wait-Process -Id $process.Id
   ```

### What `start-api.ps1` Does (Advanced)

1. **Start API as background job**
   ```powershell
   $apiJob = Start-Job -ScriptBlock { dotnet run }
   ```

2. **Health check loop**
   ```powershell
   $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/v1/swagger.json"
   if ($response.StatusCode -eq 200) {
       $apiReady = $true
   }
   ```

3. **Open browser only when ready**
   ```powershell
   if ($apiReady) {
       Start-Process "http://localhost:5000"
   }
   ```

4. **Show API output**
   ```powershell
   Receive-Job -Job $apiJob -Wait
   ```

## Manual Alternative

If you prefer not to use scripts:

### Visual Studio
1. Press **F5**
2. Browser opens automatically

### VS Code
1. Install C# extension
2. Press **F5** 
3. Browser opens automatically (with proper launch.json)

### Command Line Only
1. Run: `dotnet run`
2. Wait for: "Dapper Labs API Started Successfully!"
3. Manually open: http://localhost:5000

## Quick Reference

### Start Commands

```powershell
# PowerShell (recommended)
.\run.ps1

# Command Prompt
start-api.bat

# PowerShell (with health check)
.\start-api.ps1

# Manual (no auto-open)
dotnet run
```

### Stop Commands

```powershell
# In script window
Ctrl + C

# Find and kill process
Get-Process -Name dotnet | Stop-Process
```

### Useful URLs

- **Swagger UI:** http://localhost:5000
- **Swagger JSON:** http://localhost:5000/swagger/v1/swagger.json
- **Customers API:** http://localhost:5000/api/customers
- **Invoices API:** http://localhost:5000/api/invoices
- **Phone Numbers API:** http://localhost:5000/api/telephonenumbers

## Benefits of Using Scripts

### Convenience
- No need to remember URLs
- Automatic browser launch
- Consistent startup experience

### Reliability
- Wait for API to be ready
- Health checks (in advanced script)
- Error handling

### Developer Experience
- Faster development workflow
- Less context switching
- Clear status messages

### Beginner Friendly
- One command to start everything
- No manual steps to remember
- Clear instructions on screen

## For Instructors

### Teaching Setup

1. **First time setup:**
   ```powershell
   # Show students how to trust certificates
   dotnet dev-certs https --trust
   
   # Show execution policy fix
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

2. **During class:**
   ```powershell
   # Everyone runs
   .\run.ps1
   
   # Wait for "API Started!" message
   # Browser opens automatically
   # Start coding/testing
   ```

3. **Cleanup:**
   ```powershell
   # Press Ctrl+C in all terminals
   # Or
   Get-Process -Name dotnet | Stop-Process
   ```

### Lab Environment

For computer labs, create a desktop shortcut:

**Target:**
```
powershell.exe -ExecutionPolicy Bypass -File "D:\Labs\DapperLabs_Lab2\run.ps1"
```

**Start in:**
```
D:\Labs\DapperLabs_Lab2
```

## Summary

The auto-launch scripts make it easy to start the Dapper Labs API with Swagger UI opening automatically. Use `run.ps1` for the best experience!

**Recommended workflow:**
1. Open PowerShell in project directory
2. Run: `.\run.ps1`
3. Wait for browser to open
4. Start testing with Swagger UI
5. Press Ctrl+C when done

---

**Need help?** See [SWAGGER_TROUBLESHOOTING.md](SWAGGER_TROUBLESHOOTING.md) for more solutions.
