#!/usr/bin/env pwsh
param()
Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'
$ScriptDir = Split-Path -Parent $MyInvocation.MyCommand.Definition
Push-Location $ScriptDir
try {
    # Reuse pre-commit checks
    Write-Host 'Running pre-commit checks as part of pre-push...'
    & .\pre-commit.ps1
    $rc = $LASTEXITCODE
    if ($rc -ne 0) { Exit $rc }

    # Optional: run full tests if requested (set RUN_FULL_TESTS=1 in environment)
    # Can also set SKIP_TESTS_ON_FAILURE=1 to warn instead of block on test failures
    if ($env:RUN_FULL_TESTS -eq '1') {
        Write-Host 'RUN_FULL_TESTS=1 detected — running full test suite...'
        $arch = $env:PROCESSOR_ARCHITECTURE
        $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
        $t = Start-Process -FilePath 'dotnet' -ArgumentList @('test','-c','Debug',"-p:Platform=$Platform") -NoNewWindow -Wait -PassThru
        if ($t.ExitCode -ne 0 -and $env:SKIP_TESTS_ON_FAILURE -ne '1') {
            Exit $t.ExitCode
        }
    }

    Exit 0
} finally {
    Pop-Location
}
