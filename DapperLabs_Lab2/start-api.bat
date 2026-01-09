@echo off
echo ====================================
echo   Starting Dapper Labs API...
echo ====================================
echo.

REM Start the API in a new window
start "Dapper Labs API" dotnet run

REM Wait for API to start (adjust if needed)
echo Waiting for API to start...
timeout /t 5 /nobreak >nul

REM Open browser to Swagger UI
echo Opening browser to Swagger UI...
start http://localhost:5000

echo.
echo ====================================
echo   API is running!
echo   Swagger UI: http://localhost:5000
echo ====================================
echo.
echo Press any key in the API window to stop...
