# Starts the .NET backend and React dev server together.
# Open browser at: http://localhost:5173

Write-Host "Starting .NET backend on http://localhost:5236..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot'; dotnet run --launch-profile http" -WindowStyle Normal

Write-Host "Waiting for backend to start..." -ForegroundColor Yellow
Start-Sleep -Seconds 4

Write-Host "Starting React dev server on http://localhost:5173..." -ForegroundColor Cyan
Start-Process powershell -ArgumentList "-NoExit", "-Command", "cd '$PSScriptRoot\ClientApp'; npm run dev" -WindowStyle Normal

Start-Sleep -Seconds 3
Write-Host ""
Write-Host "App is running at: http://localhost:5173" -ForegroundColor Green
Start-Process "http://localhost:5173"
