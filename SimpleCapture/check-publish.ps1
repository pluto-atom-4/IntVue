#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Verify publish folder integrity and check for missing dependencies
#>

param(
    [string]$PublishDir = "bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish"
)

Write-Host "SimpleCapture Publish Folder Diagnostics" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green
Write-Host ""

if (-not (Test-Path $PublishDir)) {
    Write-Host "ERROR: Publish folder not found: $PublishDir" -ForegroundColor Red
    Write-Host ""
    Write-Host "Publish first with:" -ForegroundColor Yellow
    Write-Host "  dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64" -ForegroundColor Cyan
    exit 1
}

Write-Host "Publish Directory: $PublishDir" -ForegroundColor Cyan
Write-Host ""

# Check main executable
$exe = Join-Path $PublishDir "SimpleCapture.exe"
if (Test-Path $exe) {
    $exeSize = (Get-Item $exe).Length / 1KB
    Write-Host "✓ SimpleCapture.exe found ($($exeSize.ToString('F1')) KB)" -ForegroundColor Green
} else {
    Write-Host "✗ SimpleCapture.exe NOT FOUND" -ForegroundColor Red
    exit 1
}

# Check critical runtime DLLs
$criticalDlls = @(
    "coreclr.dll",
    "clrjit.dll",
    "clretwrc.dll",
    "mscorlib.dll"
)

Write-Host ""
Write-Host "Critical Runtime DLLs:" -ForegroundColor Cyan
$missingDlls = @()
foreach ($dll in $criticalDlls) {
    $dllPath = Join-Path $PublishDir $dll
    if (Test-Path $dllPath) {
        $size = (Get-Item $dllPath).Length / 1KB
        Write-Host "  ✓ $dll ($($size.ToString('F1')) KB)" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $dll NOT FOUND" -ForegroundColor Red
        $missingDlls += $dll
    }
}

if ($missingDlls.Count -gt 0) {
    Write-Host ""
    Write-Host "ERROR: Missing critical DLLs!" -ForegroundColor Red
    Write-Host "Try re-publishing:" -ForegroundColor Yellow
    Write-Host "  dotnet clean" -ForegroundColor Cyan
    Write-Host "  dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64" -ForegroundColor Cyan
    exit 1
}

# Check WinUI DLLs
Write-Host ""
Write-Host "WinUI Dependencies:" -ForegroundColor Cyan
$winuiDlls = Get-ChildItem $PublishDir -Filter "Microsoft.UI*.dll" -ErrorAction SilentlyContinue
if ($winuiDlls.Count -gt 0) {
    Write-Host "  ✓ Found $($winuiDlls.Count) WinUI DLLs" -ForegroundColor Green
    foreach ($dll in $winuiDlls | Select-Object -First 5) {
        Write-Host "    - $($dll.Name)" -ForegroundColor Gray
    }
    if ($winuiDlls.Count -gt 5) {
        Write-Host "    ... and $($winuiDlls.Count - 5) more" -ForegroundColor Gray
    }
} else {
    Write-Host "  ✗ No WinUI DLLs found!" -ForegroundColor Red
    exit 1
}

# Total size
$totalSize = (Get-ChildItem $PublishDir -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB
Write-Host ""
Write-Host "Total Folder Size: $($totalSize.ToString('F1')) MB" -ForegroundColor Cyan

# System info
Write-Host ""
Write-Host "System Information:" -ForegroundColor Cyan
$osVersion = [System.Environment]::OSVersion
Write-Host "  Windows Version: $osVersion" -ForegroundColor Gray
Write-Host "  Framework: .NET 10.0" -ForegroundColor Gray

Write-Host ""
Write-Host "✓ Publish folder appears to be intact" -ForegroundColor Green
Write-Host ""
Write-Host "To run the app, execute:" -ForegroundColor Yellow
Write-Host "  .\$PublishDir\SimpleCapture.exe" -ForegroundColor Cyan
Write-Host ""
Write-Host "To debug, run from Visual Studio debugger or:" -ForegroundColor Yellow
Write-Host "  .\diagnose.ps1" -ForegroundColor Cyan
Write-Host ""
