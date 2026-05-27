# Pre-commit script for Windows/PowerShell
# Runs formatting check, build, and tests/analyzers before allowing a commit.
# Exit non-zero to block the commit.

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Determine repository root (parent of scripts folder)
$RepoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
Write-Host "Repository root: $RepoRoot"

# Determine platform (match MSBuild Platform value)
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
Write-Host "Detected architecture: $arch -> Platform: $Platform"

Push-Location $RepoRoot
try {
    # 1) Formatting check
    Write-Host 'Running dotnet format (verify no changes)...'
    $formatExe = 'dotnet'
    $formatArgs = @('format', '--verify-no-changes')
    try {
        $proc = Start-Process -FilePath $formatExe -ArgumentList $formatArgs -NoNewWindow -Wait -PassThru -WindowStyle Hidden
        if ($proc.ExitCode -ne 0) {
            Write-Host "dotnet format detected formatting issues (exit code $($proc.ExitCode)). Please run 'dotnet format' to fix." -ForegroundColor Yellow
            Exit $proc.ExitCode
        }
    } catch {
        Write-Host "Failed to run 'dotnet format'. Ensure dotnet-format is available (dotnet tool install -g dotnet-format) or that 'dotnet format' is supported by your SDK." -ForegroundColor Red
        Exit 1
    }

    # 2) Build (this will run analyzers during build)
    Write-Host 'Running dotnet build...'
    $buildArgs = @('build','-c','Debug',"-p:Platform=$Platform")
    $proc = Start-Process -FilePath 'dotnet' -ArgumentList $buildArgs -NoNewWindow -Wait -PassThru -WindowStyle Hidden
    if ($proc.ExitCode -ne 0) {
        Write-Host "dotnet build failed (exit code $($proc.ExitCode)). Fix build errors before committing." -ForegroundColor Red
        Exit $proc.ExitCode
    }

    # 3) Run tests (optional -- will ensure test suite still passes)
    # Running tests can be slow; if you prefer to skip tests in pre-commit, comment the following block out.
    Write-Host 'Running dotnet test (tests may be slow)...'
    $testArgs = @('test','-c','Debug',"-p:Platform=$Platform")
    $proc = Start-Process -FilePath 'dotnet' -ArgumentList $testArgs -NoNewWindow -Wait -PassThru -WindowStyle Hidden
    if ($proc.ExitCode -ne 0) {
        Write-Host "dotnet test failed (exit code $($proc.ExitCode)). Fix failing tests before committing." -ForegroundColor Red
        Exit $proc.ExitCode
    }

    Write-Host 'Pre-commit checks passed.' -ForegroundColor Green
    Exit 0
} finally {
    Pop-Location
}

