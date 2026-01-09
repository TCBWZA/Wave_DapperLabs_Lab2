# Dapper Labs API Launcher
# This script starts the API and automatically opens Swagger UI

Write-Host "====================================" -ForegroundColor Cyan
Write-Host "  Starting Dapper Labs API..." -ForegroundColor Cyan
Write-Host "====================================" -ForegroundColor Cyan
Write-Host ""

# Start the API as a background job
$apiJob = Start-Job -ScriptBlock {
    Set-Location $using:PSScriptRoot
    dotnet run
}

# Wait for API to start
Write-Host "Waiting for API to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 5

# Check if API is responding
$maxAttempts = 10
$attempt = 0
$apiReady = $false

while ($attempt -lt $maxAttempts -and -not $apiReady) {
    try {
        $response = Invoke-WebRequest -Uri "http://localhost:5000/swagger/v1/swagger.json" -Method Get -TimeoutSec 2 -ErrorAction SilentlyContinue
        if ($response.StatusCode -eq 200) {
            $apiReady = $true
            Write-Host "API is ready!" -ForegroundColor Green
        }
    }
    catch {
        $attempt++
        Write-Host "Attempt $attempt/$maxAttempts - Waiting for API..." -ForegroundColor Yellow
        Start-Sleep -Seconds 1
    }
}

if ($apiReady) {
    # Open browser to Swagger UI
    Write-Host "Opening browser to Swagger UI..." -ForegroundColor Green
    Start-Process "http://localhost:5000"
    
    Write-Host ""
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host "  API is running!" -ForegroundColor Green
    Write-Host "  Swagger UI: http://localhost:5000" -ForegroundColor Green
    Write-Host "====================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "Press Ctrl+C to stop the API" -ForegroundColor Yellow
    
    # Show API output
    Receive-Job -Job $apiJob -Wait
}
else {
    Write-Host "Failed to start API. Check the output above for errors." -ForegroundColor Red
    Stop-Job -Job $apiJob
    Remove-Job -Job $apiJob
}
