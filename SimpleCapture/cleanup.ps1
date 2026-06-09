#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Kill running SimpleCapture processes and clean build artifacts
#>

Write-Host "SimpleCapture Cleanup" -ForegroundColor Green

# Kill running processes
Write-Host "Stopping any running SimpleCapture instances..." -ForegroundColor Yellow
taskkill /F /IM SimpleCapture.exe /T 2>$null | Out-Null
Write-Host "Done" -ForegroundColor Green

# Clean build artifacts
Write-Host "Cleaning build artifacts..." -ForegroundColor Yellow
Remove-Item -Path "bin" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
Remove-Item -Path "obj" -Recurse -Force -ErrorAction SilentlyContinue | Out-Null
Write-Host "Done" -ForegroundColor Green

Write-Host ""
Write-Host "Ready to build. Run:" -ForegroundColor Cyan
Write-Host "  dotnet build -c Debug -p:Platform=x64" -ForegroundColor Gray
Write-Host "  dotnet run -c Debug -p:Platform=x64" -ForegroundColor Gray
