#!/usr/bin/env pwsh
<#
.SYNOPSIS
Build and test script for IntVue project.

.DESCRIPTION
This script detects the platform architecture, builds the IntVue project in Debug mode,
and runs the test suite if the build succeeds.

.EXAMPLE
.\build-and-test.ps1
#>

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# Colors for output
$colors = @{
    Success = 'Green'
    Error = 'Red'
    Info = 'Cyan'
    Warning = 'Yellow'
}

function Write-Info {
    param([string]$Message)
    Write-Host "ℹ️  $Message" -ForegroundColor $colors.Info
}

function Write-Success {
    param([string]$Message)
    Write-Host "✅ $Message" -ForegroundColor $colors.Success
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "❌ $Message" -ForegroundColor $colors.Error
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "⚠️  $Message" -ForegroundColor $colors.Warning
}

# Detect platform
Write-Info "Detecting platform architecture..."
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
Write-Success "Platform detected: $Platform"

# Set project directory
$ProjectRoot = Split-Path -Parent $MyInvocation.MyCommand.Path
Write-Info "Project root: $ProjectRoot"

# Build
Write-Info "`nBuilding IntVue project in Debug mode..."
Write-Info "Running: dotnet build -c Debug -p:Platform=$Platform`n"

Push-Location $ProjectRoot
try {
    $buildOutput = dotnet build -c Debug -p:Platform=$Platform 2>&1
    $buildExitCode = $LASTEXITCODE

    Write-Output $buildOutput

    if ($buildExitCode -eq 0) {
        Write-Success "`nBuild succeeded!`n"

        # Run tests
        Write-Info "Running tests..."
        Write-Info "Running: dotnet test Tests/IntVue.Tests/IntVue.Tests.csproj -c Debug -p:Platform=$Platform`n"

        $testOutput = dotnet test Tests/IntVue.Tests/IntVue.Tests.csproj -c Debug -p:Platform=$Platform 2>&1
        $testExitCode = $LASTEXITCODE

        Write-Output $testOutput

        if ($testExitCode -eq 0) {
            # Extract test count from output
            $passedMatch = $testOutput | Select-String -Pattern 'Passed:\s+(\d+)' | Select-Object -First 1
            if ($passedMatch -and $passedMatch.Matches.Groups[1].Value) {
                $testCount = $passedMatch.Matches.Groups[1].Value
                Write-Success "`nTests passed! All $testCount tests are working correctly.`n"
            } else {
                Write-Success "`nTests passed! All tests are working correctly.`n"
            }
            exit 0
        } else {
            Write-Error-Custom "`nTests failed! Please review the output above.`n"
            exit 1
        }
    } else {
        Write-Error-Custom "`nBuild failed! Please fix the errors above before running tests.`n"
        exit 1
    }
}
finally {
    Pop-Location
}
