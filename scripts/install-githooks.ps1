# Install repository githooks by setting git config core.hooksPath to .githooks
# Run this once per clone to enable the pre-commit hook in this repo.

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Push-Location $RepoRoot
try {
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Error 'git is not available on PATH. Install Git and retry.'
        Exit 1
    }

    git config core.hooksPath .githooks
    Write-Host "Configured git to use .githooks for hooks (run 'git config --get core.hooksPath' to verify)." -ForegroundColor Green
} finally {
    Pop-Location
}

