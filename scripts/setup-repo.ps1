# Repository setup script
# - Configures git hooks (core.hooksPath -> .githooks)
# - Verifies .NET SDK presence and runs dotnet restore
# - Offers to install dotnet-format tool if not present
# - Performs an initial dotnet build to surface issues

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

$ScriptRoot = $PSScriptRoot
$RepoRoot = Resolve-Path (Join-Path $ScriptRoot '..')
Write-Host "Repository root: $RepoRoot"

Push-Location $RepoRoot
try {
    # Ensure git is available
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Error 'git is not available on PATH. Install Git and retry.'
        Exit 1
    }

    # Configure hooks path
    Write-Host 'Configuring git hooks path to .githooks...'
    git config core.hooksPath .githooks
    Write-Host "git core.hooksPath set to: $(git config --get core.hooksPath)"

    # Ensure dotnet is available
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error 'dotnet CLI is not available on PATH. Install .NET SDK and retry.'
        Exit 1
    }

    # Run dotnet restore
    Write-Host 'Running dotnet restore...'
    $r = Start-Process -FilePath 'dotnet' -ArgumentList 'restore' -NoNewWindow -Wait -PassThru
    if ($r.ExitCode -ne 0) { Write-Error "dotnet restore failed (exit code $($r.ExitCode))."; Exit $r.ExitCode }

    # Check dotnet-format
    $formatInstalled = $false
    try {
        # dotnet tool list -g outputs installed global tools
        $output = & dotnet tool list -g 2>$null
        if ($output -match 'dotnet-format') { $formatInstalled = $true }
    } catch {
        # If tool list fails, assume not installed
        $formatInstalled = $false
    }

    if (-not $formatInstalled) {
        Write-Host "The 'dotnet-format' tool was not found globally. It's recommended for formatting checks."
        $answer = Read-Host "Install dotnet-format globally now? (y/N)"
        if ($answer -and $answer.ToLower().StartsWith('y')) {
            Write-Host 'Installing dotnet-format globally...'
            $inst = Start-Process -FilePath 'dotnet' -ArgumentList 'tool','install','-g','dotnet-format' -NoNewWindow -Wait -PassThru
            if ($inst.ExitCode -ne 0) { Write-Warning "Failed to install dotnet-format (exit code $($inst.ExitCode)). You can install manually with: dotnet tool install -g dotnet-format" }
            else { Write-Host 'dotnet-format installed successfully.' }
        } else {
            Write-Host 'Skipping dotnet-format installation at user request.'
        }
    } else {
        Write-Host 'dotnet-format already installed.'
    }

    # Initial build to surface analyzer/warning/errors
    Write-Host 'Running initial dotnet build to surface issues...'
    $arch = $env:PROCESSOR_ARCHITECTURE
    $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
    $build = Start-Process -FilePath 'dotnet' -ArgumentList 'build','-c','Debug',"-p:Platform=$Platform" -NoNewWindow -Wait -PassThru
    if ($build.ExitCode -ne 0) {
        Write-Warning "Initial build failed with exit code $($build.ExitCode). Fix build issues before contributing."
    } else {
        Write-Host 'Initial build succeeded.' -ForegroundColor Green
    }

    Write-Host "Repository setup complete. To enable pre-commit checks, ensure the hooks path is set (git config --get core.hooksPath) and try committing."
    Write-Host "You can run '.\scripts\pre-commit.ps1' manually to preview what the hook will run."
} finally {
    Pop-Location
}

