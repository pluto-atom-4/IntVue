#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run SimpleCapture with detailed error capture and diagnostics
.DESCRIPTION
    This script helps diagnose why SimpleCapture.exe exits immediately.
    It captures stdout, stderr, and runs diagnostic checks.
#>

param(
    [string]$ExePath = "bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish\SimpleCapture.exe"
)

Write-Host "SimpleCapture - Debug Launch Script" -ForegroundColor Green
Write-Host "====================================" -ForegroundColor Green
Write-Host ""

# Step 1: Verify file exists
if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: Executable not found at $ExePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "Publish first with:" -ForegroundColor Yellow
    Write-Host "  dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64" -ForegroundColor Cyan
    exit 1
}

$exeFile = Get-Item $ExePath
Write-Host "Executable: $ExePath" -ForegroundColor Cyan
Write-Host "Size: $($exeFile.Length / 1KB) KB" -ForegroundColor Cyan
Write-Host "Modified: $($exeFile.LastWriteTime)" -ForegroundColor Cyan
Write-Host ""

# Step 2: Check publish folder completeness
$publishDir = Split-Path $ExePath
Write-Host "Checking publish folder for critical files..." -ForegroundColor Yellow
$criticalFiles = @("coreclr.dll", "clrjit.dll", "Microsoft.UI.Xaml.dll")
$allPresent = $true
foreach ($file in $criticalFiles) {
    $filePath = Join-Path $publishDir $file
    if (Test-Path $filePath) {
        Write-Host "  ✓ $file" -ForegroundColor Green
    } else {
        Write-Host "  ✗ $file MISSING!" -ForegroundColor Red
        $allPresent = $false
    }
}

if (-not $allPresent) {
    Write-Host ""
    Write-Host "ERROR: Missing critical DLLs. Publish folder may be corrupted." -ForegroundColor Red
    Write-Host "Try:" -ForegroundColor Yellow
    Write-Host "  dotnet clean" -ForegroundColor Cyan
    Write-Host "  dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64" -ForegroundColor Cyan
    exit 1
}

Write-Host "  ✓ All critical files present" -ForegroundColor Green
Write-Host ""

# Step 3: Check Windows version
Write-Host "System Information:" -ForegroundColor Yellow
$osVersion = [System.Environment]::OSVersion
$buildNumber = $osVersion.Version.Build
Write-Host "  Windows Version: $osVersion" -ForegroundColor Gray
Write-Host "  Build: $buildNumber" -ForegroundColor Gray
if ($buildNumber -lt 26100) {
    Write-Host "  WARNING: Windows version may be too old (need 26100+)" -ForegroundColor Red
}
Write-Host ""

# Step 4: Try to run with error capturing
Write-Host "Attempting to launch SimpleCapture.exe..." -ForegroundColor Yellow
Write-Host "If app closes immediately, error details will appear below." -ForegroundColor Cyan
Write-Host ""

try {
    # Capture output
    $output = & $ExePath 2>&1

    Write-Host "--- STDOUT/STDERR ---" -ForegroundColor Yellow
    if ($output) {
        Write-Host $output -ForegroundColor Cyan
    } else {
        Write-Host "[No output captured - app likely crashed before any output]" -ForegroundColor Red
    }
    Write-Host "--- END OUTPUT ---" -ForegroundColor Yellow
}
catch {
    Write-Host "Exception launching process: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "Troubleshooting steps:" -ForegroundColor Cyan
Write-Host "  1. Check Event Viewer > Windows Logs > Application for errors" -ForegroundColor Gray
Write-Host "  2. Run from Visual Studio debugger to see exact exception" -ForegroundColor Gray
Write-Host "  3. Check graphics drivers are up to date" -ForegroundColor Gray
Write-Host "  4. Verify Windows permissions (Admin mode not always needed)" -ForegroundColor Gray
Write-Host "  5. Try a full rebuild: dotnet clean && dotnet publish..." -ForegroundColor Gray
Write-Host ""

# Step 5: Suggest next action
Write-Host "RECOMMENDED: Run from Visual Studio for full debugging:" -ForegroundColor Yellow
Write-Host "  1. Open SimpleCapture.csproj in Visual Studio" -ForegroundColor Gray
Write-Host "  2. Set x64 platform" -ForegroundColor Gray
Write-Host "  3. Press F5 to debug" -ForegroundColor Gray
Write-Host "  4. Visual Studio will show the exact exception" -ForegroundColor Gray
