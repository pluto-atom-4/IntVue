#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Publish SimpleCapture as self-contained executable for Surface Pro 7
.DESCRIPTION
    Creates a standalone exe with all .NET runtime included.
    Output: bin/x64/Release/net10.0-windows10.0.26100.0/win-x64/publish/
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Release',

    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64'
)

Write-Host "SimpleCapture Self-Contained Publish" -ForegroundColor Green
Write-Host "=====================================" -ForegroundColor Green
Write-Host ""
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Platform: $Platform" -ForegroundColor Cyan
Write-Host "Self-Contained: Yes (includes .NET runtime)" -ForegroundColor Cyan
Write-Host ""

# Kill running processes
Write-Host "Stopping any running SimpleCapture instances..." -ForegroundColor Yellow
Get-Process SimpleCapture -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Write-Host "Done" -ForegroundColor Green

# Clean
Write-Host "Cleaning previous build..." -ForegroundColor Yellow
dotnet clean -c $Configuration -p:Platform=$Platform
Write-Host "Done" -ForegroundColor Green

# Publish
Write-Host ""
Write-Host "Publishing as self-contained executable..." -ForegroundColor Yellow
Write-Host "This may take 1-2 minutes (bundling .NET runtime)..." -ForegroundColor Cyan
Write-Host ""

$rid = "win-$Platform"
$publishOutput = "bin\$Platform\$Configuration\net10.0-windows10.0.26100.0\$rid\publish"

dotnet publish -c $Configuration -p:Platform=$Platform --self-contained true --runtime $rid

if ($LASTEXITCODE -ne 0) {
    Write-Host ""
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host ""
Write-Host "✓ Publish succeeded!" -ForegroundColor Green
Write-Host ""
Write-Host "Published files located at:" -ForegroundColor Cyan
Write-Host "  $publishOutput" -ForegroundColor Gray
Write-Host ""

# Verify executable exists
$exePath = Join-Path $publishOutput "SimpleCapture.exe"
if (Test-Path $exePath) {
    $exeSize = (Get-Item $exePath).Length / 1MB
    Write-Host "✓ SimpleCapture.exe ($($exeSize.ToString('F1')) MB)" -ForegroundColor Green
} else {
    Write-Host "✗ SimpleCapture.exe not found!" -ForegroundColor Red
    exit 1
}

# Count DLLs
$dllCount = (Get-ChildItem $publishOutput -Filter "*.dll" -Recurse).Count
Write-Host "✓ $dllCount DLLs included (.NET runtime + WinUI)" -ForegroundColor Green

# Total size
$totalSize = (Get-ChildItem $publishOutput -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host "✓ Total publish size: $($totalSize.ToString('F1')) MB" -ForegroundColor Green
Write-Host ""

Write-Host "Next steps:" -ForegroundColor Yellow
Write-Host "  1. Copy entire '$publishOutput' folder to USB drive" -ForegroundColor Gray
Write-Host "  2. Transfer to Surface Pro 7" -ForegroundColor Gray
Write-Host "  3. Run: SimpleCapture.exe" -ForegroundColor Gray
Write-Host "     (No .NET installation needed!)" -ForegroundColor Gray
