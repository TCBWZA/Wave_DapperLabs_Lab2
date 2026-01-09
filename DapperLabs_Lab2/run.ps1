# Dapper Labs API Launcher (Simple Version)
# This script starts the API and automatically opens Swagger UI

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "  Starting Dapper Labs API..." -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Start the API in background
$process = Start-Process -FilePath "dotnet" -ArgumentList "run" -WorkingDirectory $PSScriptRoot -PassThru -NoNewWindow

# Wait for API to start
Write-Host "Waiting for API to start (5 seconds)..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Open browser
Write-Host "Opening browser to Swagger UI..." -ForegroundColor Green
Start-Process "http://localhost:5000"

Write-Host ""
Write-Host "====================================" -ForegroundColor Green
Write-Host "  API Started!" -ForegroundColor Green
Write-Host "  Swagger UI: http://localhost:5000" -ForegroundColor Green  
Write-Host "  API URLs:" -ForegroundColor Green
Write-Host "    - Customers: http://localhost:5000/api/customers" -ForegroundColor Gray
Write-Host "    - Invoices: http://localhost:5000/api/invoices" -ForegroundColor Gray
Write-Host "    - Phone Numbers: http://localhost:5000/api/telephonenumbers" -ForegroundColor Gray
Write-Host "====================================" -ForegroundColor Green
Write-Host ""
Write-Host "API is running in process ID: $($process.Id)" -ForegroundColor Yellow
Write-Host "To stop the API, press Ctrl+C or close this window" -ForegroundColor Yellow
Write-Host ""

# Keep the script running
try {
    Wait-Process -Id $process.Id
}
catch {
    Write-Host "API stopped." -ForegroundColor Yellow
}
