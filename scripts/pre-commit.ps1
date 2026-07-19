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

        # Priority 1: Look for IntVue.csproj (main project)
        $mainProj = Join-Path $root 'IntVue.csproj'
        if (Test-Path $mainProj) { return $mainProj }

        # Priority 2: Look for any csproj in repo root that matches repo name
        $repoName = Split-Path -Leaf $root
        $projRoot = Get-ChildItem -Path $root -Filter "$repoName.csproj" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($projRoot) { return $projRoot.FullName }

        # Priority 3: Look for solution files (*.slnx, *.sln) at repo root
        $slnx = Get-ChildItem -Path $root -Filter *.slnx -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($slnx) { return $slnx.FullName }

        $sln = Get-ChildItem -Path $root -Filter *.sln -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($sln) { return $sln.FullName }

        # Priority 4: Fall back to any csproj in repo root
        $anyRootProj = Get-ChildItem -Path $root -MaxDepth 1 -Filter *.csproj -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($anyRootProj) { return $anyRootProj.FullName }

        # Priority 5: Fall back to any csproj in the tree
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
                if (-not (Test-Path $workspace)) {
                    Write-Host "ERROR: Workspace file not found: $workspace" -ForegroundColor Red
                    Exit 1
                }
                $workspaceArg = @($workspace)
            } else {
                Write-Host "No .sln or .csproj found; running 'dotnet format' in repository root." -ForegroundColor Yellow
                $workspaceArg = @()
            }

            $formatArgs = @('format','--verify-no-changes') + $workspaceArg
            Write-Host "Running: dotnet $($formatArgs -join ' ')"
            $proc = Start-Process -FilePath $formatExe -ArgumentList $formatArgs -NoNewWindow -Wait -PassThru
            if ($proc.ExitCode -ne 0) {
                Write-Host "dotnet format detected formatting issues (exit code $($proc.ExitCode)). Please run 'dotnet format $($workspaceArg -join ' ')' to fix." -ForegroundColor Yellow
                Exit $proc.ExitCode
            }
        }
    } catch {
        Write-Host "Failed to run 'dotnet format': $($_.Exception.Message)" -ForegroundColor Red
        Write-Host "Ensure dotnet-format is available (dotnet tool install -g dotnet-format) and IntVue.csproj exists." -ForegroundColor Red
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

    # 4) Context token validation (warning, not blocking)
    if (Test-Path 'scripts/count-context-tokens.ps1') {
        Write-Host 'Checking context token budgets...'
        & .\scripts\count-context-tokens.ps1 -Mode check
        $tokenCheckExitCode = $LASTEXITCODE

        if ($tokenCheckExitCode -ne 0) {
            Write-Host "⚠️  Context token budget check FAILED (exit code $tokenCheckExitCode)." -ForegroundColor Yellow
            Write-Host "   This is a WARNING, not blocking the commit." -ForegroundColor Yellow
            Write-Host "   Consider optimizing context files before the next push." -ForegroundColor Yellow
        }
    }

    Write-Host 'Pre-commit checks passed.' -ForegroundColor Green
    Exit 0
} finally {
    Pop-Location
}

