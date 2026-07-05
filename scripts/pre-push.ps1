#!/usr/bin/env pwsh
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition

# Detect platform at script level
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
Write-Host "Detected architecture: $arch -> Platform: $Platform"

Push-Location $ScriptDir
try {
    # Ensure dotnet from user install (~\/.dotnet) is on PATH
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        $userDotnet = Join-Path $env:USERPROFILE '.dotnet'
        $userDotnetExe = Join-Path $userDotnet 'dotnet.exe'
        if (Test-Path $userDotnetExe) {
            Write-Host "Adding user dotnet to PATH: $userDotnet"
            $env:PATH = "$userDotnet;" + $env:PATH
        }
    }

    # Reuse pre-commit checks
    Write-Host 'Running pre-commit checks as part of pre-push...'
    & .\pre-commit.ps1
    $rc = $LASTEXITCODE
    if ($rc -ne 0) { Exit $rc }

    # Optional: run full tests if requested (set RUN_FULL_TESTS=1 in environment)
    # Can also set SKIP_TESTS_ON_FAILURE=1 to warn instead of block on test failures
    if ($env:RUN_FULL_TESTS -eq '1') {
        Write-Host 'RUN_FULL_TESTS=1 detected — running full test suite...'
        $t = Start-Process -FilePath 'dotnet' -ArgumentList @('test','-c','Debug',"-p:Platform=$Platform") -NoNewWindow -Wait -PassThru
        if ($t.ExitCode -ne 0 -and $env:SKIP_TESTS_ON_FAILURE -ne '1') {
            Write-Host "Full test suite failed (exit code $($t.ExitCode)). Fix failing tests before pushing." -ForegroundColor Red
            Exit $t.ExitCode
        }
    }

    Write-Host 'Pre-push checks passed.' -ForegroundColor Green
    Exit 0
} finally {
    Pop-Location
}
