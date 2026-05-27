# Repository setup script
# - Configures git hooks (core.hooksPath -> .githooks)
# - Verifies .NET SDK presence and runs dotnet restore
# - Offers to install dotnet-format tool if not present
# - Performs an initial dotnet build to surface issues

param(
    [switch]$FixFormatting,
    [switch]$Yes,
    [string]$Workspace = ''
)

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
        $install = $false
        if ($Yes) { $install = $true } else {
            $answer = Read-Host "Install dotnet-format globally now? (y/N)"
            if ($answer -and $answer.ToLower().StartsWith('y')) { $install = $true }
        }

        if ($install) {
            Write-Host 'Installing dotnet-format globally...'
            $inst = Start-Process -FilePath 'dotnet' -ArgumentList 'tool','install','-g','dotnet-format' -NoNewWindow -Wait -PassThru
            if ($inst.ExitCode -ne 0) { Write-Warning "Failed to install dotnet-format (exit code $($inst.ExitCode)). You can install manually with: dotnet tool install -g dotnet-format" }
            else { Write-Host 'dotnet-format installed successfully.'; $formatInstalled = $true }
        } else {
            Write-Host 'Skipping dotnet-format installation at user request.'
        }
    } else {
        Write-Host 'dotnet-format already installed.'
    }

    # Run formatting checks. By default, run verify-only so we don't change files during setup.
    if ($formatInstalled) {
        # Determine workspace (prefer solution (.sln, .slnx) then root csproj then any csproj matching repo folder name)
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

        if ($Workspace -and (Test-Path $Workspace)) {
            $workspace = (Resolve-Path $Workspace).Path
            Write-Host "Using explicit workspace provided: $workspace"
        } else {
            $workspace = Get-FormatWorkspace -root $RepoRoot
        }
        if (-not $workspace) {
            Write-Host "No .sln or .csproj found; running 'dotnet format' in repository root (may require explicit workspace)." -ForegroundColor Yellow
            $workspaceArg = @()
        } else {
            Write-Host "Using workspace for dotnet format: $workspace"
            $workspaceArg = @($workspace)
        }

        if ($FixFormatting) {
            Write-Host "Running 'dotnet format' to apply formatting fixes..."
            $args = @('format') + $workspaceArg
            $fmt = Start-Process -FilePath 'dotnet' -ArgumentList $args -NoNewWindow -Wait -PassThru
            if ($fmt.ExitCode -ne 0) { Write-Warning "dotnet format failed (exit code $($fmt.ExitCode))." }
            else { Write-Host "Formatting completed." -ForegroundColor Green }
        } else {
            Write-Host "Running 'dotnet format --verify-no-changes' to detect formatting issues..."
            $args = @('format','--verify-no-changes') + $workspaceArg
            $fmt = Start-Process -FilePath 'dotnet' -ArgumentList $args -NoNewWindow -Wait -PassThru
            if ($fmt.ExitCode -ne 0) {
                Write-Warning "Formatting issues were detected by 'dotnet format'. Run '.\scripts\setup-repo.ps1 -FixFormatting' to apply fixes or run 'dotnet format <workspace>' yourself."
            } else {
                Write-Host "No formatting changes required." -ForegroundColor Green
            }
        }
    } else {
        Write-Host "Skipping formatting step because 'dotnet-format' is not available." -ForegroundColor Yellow
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

