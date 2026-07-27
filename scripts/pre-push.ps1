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

    # Secret scanning (gitleaks)
    # Block push if secrets are detected in git history
    Write-Host 'Scanning for secrets (gitleaks)...' -ForegroundColor Cyan
    if (-not (Get-Command gitleaks -ErrorAction SilentlyContinue)) {
        Write-Host @"
⚠️  gitleaks is not installed. Secret scanning will be skipped.

To enable secret scanning, install gitleaks:
  winget install gitleaks
  https://github.com/gitleaks/gitleaks

Secret scanning helps prevent credentials/keys from being pushed.
"@ -ForegroundColor Yellow
        # Continue without blocking (gitleaks is optional on first install)
    } else {
        # Run gitleaks to detect secrets in git history
        $repoRoot = Split-Path $ScriptDir
        & gitleaks detect --source $repoRoot -v
        if ($LASTEXITCODE -ne 0) {
            Write-Host "❌ Secrets detected in repository! Fix before pushing." -ForegroundColor Red
            Write-Host "See output above for details. To bypass: SKIP_SECRET_SCAN=1 git push" -ForegroundColor Yellow
            if ($env:SKIP_SECRET_SCAN -ne '1') {
                Exit 1
            }
        }
        Write-Host 'Secret scan passed.' -ForegroundColor Green
    }

    Write-Host 'Pre-push checks passed.' -ForegroundColor Green
    Exit 0
} finally {
    Pop-Location
}
