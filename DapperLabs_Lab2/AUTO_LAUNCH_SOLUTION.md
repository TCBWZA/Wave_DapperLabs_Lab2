# Auto-Launch Solution - Summary

## Problem Solved ?

**Issue:** Swagger UI page does not automatically load when running `dotnet run`

**Root Cause:** The `dotnet run` command doesn't support automatic browser launch. This is standard .NET behavior - only IDE launches (Visual Studio, VS Code) can open browsers automatically.

## Solution Implemented

Created **three launcher scripts** that automatically open the browser to Swagger UI:

### 1. ? `run.ps1` (Recommended)
**Best for:** Quick daily use

```powershell
.\run.ps1
```

**Features:**
- Starts API in background
- Waits 5 seconds
- Opens browser automatically
- Shows friendly status messages
- Press Ctrl+C to stop

**Perfect for:** Students, developers, quick testing

### 2. ? `start-api.bat`
**Best for:** Command Prompt users

```cmd
start-api.bat
```

**Features:**
- Opens API in separate window
- Waits 5 seconds
- Opens browser automatically
- Simple batch file

**Perfect for:** Windows Command Prompt users, legacy environments

### 3. ? `start-api.ps1` (Advanced)
**Best for:** Production-like reliability

```powershell
.\start-api.ps1
```

**Features:**
- Starts API as background job
- **Health checks** - waits until API actually responds
- Tries up to 10 times to confirm API is ready
- Only opens browser when confirmed working
- Shows API console output

**Perfect for:** Slower machines, CI/CD, demos, teaching

## Files Created

| File | Purpose | Location |
|------|---------|----------|
| `run.ps1` | Simple PowerShell launcher | `DapperLabs_Lab2\run.ps1` |
| `start-api.bat` | Windows batch launcher | `DapperLabs_Lab2\start-api.bat` |
| `start-api.ps1` | Advanced PowerShell with health checks | `DapperLabs_Lab2\start-api.ps1` |
| `AUTO_LAUNCH_GUIDE.md` | Complete documentation | `DapperLabs_Lab2\AUTO_LAUNCH_GUIDE.md` |

## Files Updated

| File | Changes |
|------|---------|
| `README.md` | Added auto-launch scripts to Quick Start (Option 1) |
| `README.md` | Added scripts to Troubleshooting section |
| `README.md` | Added AUTO_LAUNCH_GUIDE.md to Documentation links |

## User Experience Comparison

### Before ?
```
User: dotnet run
API: *starts*
User: *waits for browser*
User: *nothing happens*
User: *confused*
User: *manually opens browser*
User: *types http://localhost:5000*
```

### After ?
```
User: .\run.ps1
Script: Starting Dapper Labs API...
Script: Opening browser to Swagger UI...
Browser: *opens automatically to Swagger UI*
Script: API Started!
User: *starts testing immediately* ??
```

## How It Works

### Simple Version (`run.ps1`)
```powershell
1. Start dotnet run in background
2. Wait 5 seconds (fixed delay)
3. Open browser to http://localhost:5000
4. Keep running until Ctrl+C
```

### Advanced Version (`start-api.ps1`)
```powershell
1. Start dotnet run as background job
2. Loop up to 10 times:
   - Try to fetch http://localhost:5000/swagger/v1/swagger.json
   - If successful (HTTP 200), API is ready
   - If failed, wait 1 second and try again
3. If ready: open browser
4. Show API console output
5. Keep running until Ctrl+C
```

## Usage Instructions

### Quick Start (3 Steps)

1. **Open PowerShell in project directory:**
   ```powershell
   cd D:\Users\tbw_\source\repos\DapperLabs_Lab2\DapperLabs_Lab2
   ```

2. **Run the script:**
   ```powershell
   .\run.ps1
   ```

3. **Wait for browser to open automatically** (5 seconds)

That's it! ??

### First Time Setup (If Needed)

If you get an execution policy error:

```powershell
Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
```

Then run the script again.

## Advantages

### ? User Experience
- **One command** instead of two (run + open browser)
- **Automatic** - no manual steps
- **Consistent** - works same every time
- **Beginner-friendly** - no URLs to remember

### ? Teaching Benefits
- Students can start quickly
- Less time spent on setup
- More time on actual learning
- Reduces "it doesn't work" issues

### ? Developer Workflow
- Faster iteration
- Less context switching
- No interruption to flow
- Works from terminal

### ? Reliability
- Health checks (advanced script)
- Error handling
- Clear status messages
- Process management

## Comparison Table

| Method | Browser Opens | Setup Steps | Best For |
|--------|---------------|-------------|----------|
| `.\run.ps1` | ? Yes (auto) | 0 | Everyone |
| `start-api.bat` | ? Yes (auto) | 0 | CMD users |
| `.\start-api.ps1` | ? Yes (auto) | 0 | Reliability |
| `dotnet run` | ? No (manual) | 2 | CI/CD only |
| Visual Studio F5 | ? Yes (auto) | 0 | VS users |

## Technical Details

### Why Scripts Are Needed

**The Problem:**
- `launchSettings.json` has `launchBrowser: true`
- But `dotnet run` **ignores** this setting
- Only Visual Studio and VS Code read `launchSettings.json`
- This is **by design** in .NET CLI

**The Solution:**
- Scripts explicitly launch browser using OS commands
- `Start-Process` (PowerShell) or `start` (batch)
- Scripts can add delays, health checks, error handling
- Works regardless of IDE

### Browser Launch Commands

**PowerShell:**
```powershell
Start-Process "http://localhost:5000"
```

**Batch:**
```batch
start http://localhost:5000
```

**Linux/Mac:**
```bash
open http://localhost:5000  # Mac
xdg-open http://localhost:5000  # Linux
```

## Future Enhancements (Optional)

### Possible Additions

1. **Linux/Mac Support:**
   - Create `run.sh` for bash
   - Use `xdg-open` or `open` commands

2. **Configuration File:**
   - Allow custom ports
   - Configurable delays
   - Preferred browser selection

3. **Desktop Shortcut:**
   - Create Windows shortcut
   - Double-click to start

4. **Watch Mode:**
   - Auto-restart on file changes
   - Use `dotnet watch run`

5. **Multi-Environment:**
   - Production vs Development
   - Different ports per environment

## For Students

### What You Need to Know

1. **Use the script** - It's easier!
   ```powershell
   .\run.ps1
   ```

2. **Wait 5 seconds** - Browser opens automatically

3. **Press Ctrl+C to stop** - When you're done

4. **If script won't run** - Fix execution policy once:
   ```powershell
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   ```

That's literally all you need to know! ??

## For Instructors

### Lab Setup

1. **First day of class:**
   ```powershell
   # Fix execution policy on all machines
   Set-ExecutionPolicy -ExecutionPolicy RemoteSigned -Scope CurrentUser
   
   # Trust HTTPS certificates
   dotnet dev-certs https --trust
   ```

2. **Every lab session:**
   ```powershell
   # Students run
   .\run.ps1
   
   # Browser opens automatically
   # Start teaching!
   ```

3. **Cleanup:**
   ```powershell
   # Press Ctrl+C in all terminals
   ```

### Benefits for Teaching
- ? Less time on setup
- ? More time on concepts
- ? Fewer "technical difficulties"
- ? Consistent student experience
- ? Focus on learning, not configuration

## Build Status

? **All scripts created successfully**
? **Documentation updated**
? **Build successful**
? **Ready to use**

## Testing Done

- ? PowerShell script runs
- ? Batch script runs
- ? Browser opens automatically
- ? API starts correctly
- ? Swagger UI loads
- ? All endpoints accessible
- ? Documentation complete

## Summary

**Problem:** Swagger doesn't auto-open with `dotnet run`

**Solution:** Use `.\run.ps1` - browser opens automatically!

**Result:** One-command startup with automatic browser launch ?

---

**Quick Reference:**

```powershell
# Start API with auto-open browser
.\run.ps1

# Stop API
Ctrl + C

# That's it!
```

**For complete details, see: [AUTO_LAUNCH_GUIDE.md](AUTO_LAUNCH_GUIDE.md)**
