#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Run SimpleCapture.exe with diagnostic output capture
.DESCRIPTION
    Launches the executable and captures any error output.
    Creates a log file that will help diagnose startup issues.
#>

param(
    [string]$ExePath = "bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish\SimpleCapture.exe"
)

Write-Host "SimpleCapture Diagnostic Launcher" -ForegroundColor Green
Write-Host "================================" -ForegroundColor Green
Write-Host ""

if (-not (Test-Path $ExePath)) {
    Write-Host "ERROR: Executable not found at $ExePath" -ForegroundColor Red
    Write-Host ""
    Write-Host "You must publish first:" -ForegroundColor Yellow
    Write-Host "  dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64" -ForegroundColor Cyan
    exit 1
}

Write-Host "Executable: $ExePath" -ForegroundColor Cyan
Write-Host "Size: $(Get-Item $ExePath | ForEach-Object { [math]::Round($_.Length / 1KB, 2) }) KB" -ForegroundColor Cyan
Write-Host ""

# Check for error log location
$logPath = "$env:LOCALAPPDATA\SimpleCapture\error.log"
$logDir = Split-Path $logPath

Write-Host "Diagnostics will be logged to:" -ForegroundColor Yellow
Write-Host "  $logPath" -ForegroundColor Cyan
Write-Host ""

# Clear previous log
if (Test-Path $logPath) {
    Remove-Item $logPath -Force
}

Write-Host "Launching SimpleCapture..." -ForegroundColor Yellow
Write-Host "Please wait. App window should appear in 2-3 seconds..." -ForegroundColor Cyan
Write-Host "If app closes immediately, diagnostics will be shown below." -ForegroundColor Cyan
Write-Host ""

# Launch with error capture
try {
    $process = Start-Process -FilePath $ExePath -PassThru -NoNewWindow -ErrorAction Stop
    $processId = $process.Id

    Write-Host "Process started (PID: $processId)" -ForegroundColor Green

    # Wait for process to complete
    $process.WaitForExit(10000)  # 10 second timeout

    if ($process.HasExited) {
        $exitCode = $process.ExitCode
        Write-Host ""
        Write-Host "Process exited with code: $exitCode" -ForegroundColor $(if ($exitCode -eq 0) { 'Green' } else { 'Red' })
    } else {
        Write-Host ""
        Write-Host "App is still running (window may be hidden)" -ForegroundColor Yellow
        $process.Dispose()
    }
}
catch {
    Write-Host "ERROR launching process: $_" -ForegroundColor Red
}

# Check for and display error log
Write-Host ""
Write-Host "Checking for diagnostic logs..." -ForegroundColor Yellow

if (Test-Path $logPath) {
    Write-Host ""
    Write-Host "=== DIAGNOSTIC LOG ===" -ForegroundColor Yellow
    Write-Host (Get-Content $logPath | Out-String)
    Write-Host "=== END LOG ===" -ForegroundColor Yellow
} else {
    Write-Host "No diagnostic log found (app may have launched successfully)" -ForegroundColor Green
}

Write-Host ""
Write-Host "Next steps:" -ForegroundColor Cyan
Write-Host "  1. Check the diagnostic output above"
Write-Host "  2. Verify camera permissions in Settings > Privacy & Security > Camera"
Write-Host "  3. Ensure another app isn't using the camera"
Write-Host "  4. Check Event Viewer (Admin) > Windows Logs > System for errors"
Write-Host ""
