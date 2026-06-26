# Pre-commit script for Windows/PowerShell
# Runs formatting check, build, and tests/analyzers before allowing a commit.
# Exit non-zero to block the commit.

param()

Set-StrictMode -Version Latest
$ErrorActionPreference = 'Stop'

# Ensure dotnet from user install (~\/.dotnet) is on PATH for non-login shells
if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
    $userDotnet = Join-Path $env:USERPROFILE '.dotnet'
    $userDotnetExe = Join-Path $userDotnet 'dotnet.exe'
    if (Test-Path $userDotnetExe) {
        Write-Host "Adding user dotnet to PATH: $userDotnet"
        $env:PATH = "$userDotnet;" + $env:PATH
    }
}

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

    function Get-FormatWorkspace {
        param([string]$root)

        # Look for solution files (*.sln, *.slnx) at repo root
        $sln = Get-ChildItem -Path $root -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if (-not $sln) {
            $sln = Get-ChildItem -Path $root -Filter *.slnx -File -ErrorAction SilentlyContinue | Select-Object -First 1
        }
        if ($sln) { return $sln.FullName }

        # Look for a project file in repo root
        $projRoot = Get-ChildItem -Path $root -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($projRoot) { return $projRoot.FullName }

        # Otherwise, try to find a csproj whose name matches the repository folder name
        $repoName = Split-Path -Leaf $root
        $matching = Get-ChildItem -Path $root -Recurse -Filter *.csproj -File -ErrorAction SilentlyContinue | Where-Object { $_.BaseName -eq $repoName } | Select-Object -First 1
        if ($matching) { return $matching.FullName }

        # Fall back to any csproj in the tree
        $anyProj = Get-ChildItem -Path $root -Recurse -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($anyProj) { return $anyProj.FullName }

        return $null
    }

    # Determine workspace
    $workspace = Get-FormatWorkspace -root $RepoRoot

    try {
        # Check whether dotnet-format is installed globally
        $formatInstalled = $false
        try {
            $output = & dotnet tool list -g 2>$null
            if ($output -match 'dotnet-format') { $formatInstalled = $true }
        } catch {
            $formatInstalled = $false
        }

        if (-not $formatInstalled) {
            Write-Host "The 'dotnet-format' tool was not found globally. Skipping formatting check." -ForegroundColor Yellow
        } else {
            if ($workspace) {
                Write-Host "Using workspace for dotnet format: $workspace"
                $workspaceArg = @($workspace)
            } else {
                Write-Host "No .sln or .csproj found; running 'dotnet format' in repository root (may require explicit workspace)." -ForegroundColor Yellow
                $workspaceArg = @()
            }

            $formatArgs = @('format','--verify-no-changes') + $workspaceArg
            $proc = Start-Process -FilePath $formatExe -ArgumentList $formatArgs -NoNewWindow -Wait -PassThru
            if ($proc.ExitCode -ne 0) {
                Write-Host "dotnet format detected formatting issues (exit code $($proc.ExitCode)). Please run 'dotnet format' to fix." -ForegroundColor Yellow
                Exit $proc.ExitCode
            }
        }
    } catch {
        Write-Host "Failed to run 'dotnet format'. Ensure dotnet-format is available (dotnet tool install -g dotnet-format) and that the workspace can be resolved." -ForegroundColor Red
        Exit 1
    }

    # 2) Build (this will run analyzers during build)
    Write-Host 'Running dotnet build...'
    $buildArgs = @('build','-c','Debug',"-p:Platform=$Platform")
    $proc = Start-Process -FilePath 'dotnet' -ArgumentList $buildArgs -NoNewWindow -Wait -PassThru
    if ($proc.ExitCode -ne 0) {
        Write-Host "dotnet build failed (exit code $($proc.ExitCode)). Fix build errors before committing." -ForegroundColor Red
        Exit $proc.ExitCode
    }

    # 3) Run tests (optional -- will ensure test suite still passes)
    # Running tests can be slow; set RUN_TESTS=0 to skip or SKIP_TESTS_ON_FAILURE=1 to warn instead of block.
    if ($env:RUN_TESTS -eq '0') {
        Write-Host 'Skipping tests (RUN_TESTS=0).' -ForegroundColor Yellow
    } else {
        Write-Host 'Running dotnet test (tests may be slow)...'
        $testArgs = @('test','-c','Debug',"-p:Platform=$Platform")
        $proc = Start-Process -FilePath 'dotnet' -ArgumentList $testArgs -NoNewWindow -Wait -PassThru
        if ($proc.ExitCode -ne 0) {
            if ($env:SKIP_TESTS_ON_FAILURE -eq '1') {
                Write-Host "dotnet test failed (exit code $($proc.ExitCode)). Tests are failing but commit allowed (SKIP_TESTS_ON_FAILURE=1)." -ForegroundColor Yellow
            } else {
                Write-Host "dotnet test failed (exit code $($proc.ExitCode)). Fix failing tests before committing. Set SKIP_TESTS_ON_FAILURE=1 to bypass." -ForegroundColor Red
                Exit $proc.ExitCode
            }
        }
    }

    Write-Host 'Pre-commit checks passed.' -ForegroundColor Green
    Exit 0
} finally {
    Pop-Location
}

