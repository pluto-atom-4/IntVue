#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and run SimpleCapture application with self-contained runtime executable.
.DESCRIPTION
    Uses 'dotnet publish' to create a standalone executable that can run on the target PC
    (Surface Pro 7) without requiring .NET runtime installed separately.

    The published exe and all runtime files will be in:
    ./bin/{Platform}/{Configuration}/net10.0-windows10.0.26100.0/win-{Platform}/publish/
#>

param(
    [ValidateSet('Debug', 'Release')]
    [string]$Configuration = 'Debug',

    [ValidateSet('x64', 'x86', 'ARM64')]
    [string]$Platform = 'x64',

    [switch]$PublishOnly
)

Write-Host "SimpleCapture - Build and Publish Script" -ForegroundColor Green
Write-Host "Configuration: $Configuration" -ForegroundColor Cyan
Write-Host "Platform: $Platform" -ForegroundColor Cyan
Write-Host "Self-Contained: true (exe includes all .NET runtime files)" -ForegroundColor Cyan
Write-Host ""

$rid = "win-$Platform"
$publishDir = "bin\$Platform\$Configuration\net10.0-windows10.0.26100.0\$rid\publish"

# Publish (creates self-contained exe with all runtime files)
Write-Host "Publishing SimpleCapture as self-contained executable..." -ForegroundColor Yellow
$publishResult = & dotnet publish -c $Configuration -p:Platform=$Platform --self-contained true --runtime $rid
if ($LASTEXITCODE -ne 0) {
    Write-Host "Publish failed!" -ForegroundColor Red
    exit 1
}

Write-Host "Publish succeeded!" -ForegroundColor Green
Write-Host "Published files: $publishDir" -ForegroundColor Cyan
Write-Host ""

if ($PublishOnly) {
    Write-Host "Publish-only mode: executable created but not launched." -ForegroundColor Green
    Write-Host "To run: .\$publishDir\SimpleCapture.exe" -ForegroundColor Cyan
    exit 0
}

# Run the published exe directly (not through dotnet)
Write-Host "Running SimpleCapture..." -ForegroundColor Yellow
Write-Host "Application window should appear shortly..." -ForegroundColor Cyan
Write-Host ""

$exePath = Join-Path $publishDir "SimpleCapture.exe"
if (-not (Test-Path $exePath)) {
    Write-Host "Error: Executable not found at $exePath" -ForegroundColor Red
    exit 1
}

& $exePath

if ($LASTEXITCODE -ne 0) {
    Write-Host "Application exited with error code: $LASTEXITCODE" -ForegroundColor Red
} else {
    Write-Host "Application closed successfully." -ForegroundColor Green
}
