#Requires -RunAsAdministrator
<#
.SYNOPSIS
    Remote Debugging Setup Helper for IntVue

.DESCRIPTION
    This script automates infrastructure setup for remote debugging:
    - Verifies Developer Mode is enabled
    - Tests network connectivity to Surface Tablet
    - Verifies Visual Studio Remote Tools installation
    - Builds and deploys app to Surface
    - Provides Rider configuration guidance

.PARAMETER SurfaceIP
    IP address of Surface Tablet (e.g., 192.168.1.100)
    Run 'ipconfig' on Surface to find this

.PARAMETER Architecture
    Target architecture: ARM64 (default) or x64
    Most Surface devices use ARM64

.PARAMETER ProjectPath
    Path to IntVue project (auto-detected if in project directory)

.EXAMPLE
    .\RemoteDebugSetup.ps1 -SurfaceIP 192.168.1.100
    .\RemoteDebugSetup.ps1 -SurfaceIP 192.168.1.100 -Architecture ARM64 -ProjectPath "C:\path\to\IntVue"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$SurfaceIP,

    [Parameter(Mandatory = $false)]
    [ValidateSet("ARM64", "x64")]
    [string]$Architecture = "ARM64",

    [Parameter(Mandatory = $false)]
    [string]$ProjectPath = (Get-Location).Path
)

# Color output helpers
function Write-Success { Write-Host "[✓]" -ForegroundColor Green -NoNewline; Write-Host " $args" }
function Write-Error-Custom { Write-Host "[✗]" -ForegroundColor Red -NoNewline; Write-Host " $args" }
function Write-Warning-Custom { Write-Host "[!]" -ForegroundColor Yellow -NoNewline; Write-Host " $args" }
function Write-Info { Write-Host "[i]" -ForegroundColor Cyan -NoNewline; Write-Host " $args" }

Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "Remote Debugging Setup for IntVue" -ForegroundColor Cyan
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Cyan

# Verify we're running as Administrator
$isAdmin = ([Security.Principal.WindowsPrincipal][Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)
if (-not $isAdmin) {
    Write-Error-Custom "This script must run as Administrator"
    exit 1
}
Write-Success "Running as Administrator"

# Check Project Path
if (-not (Test-Path "$ProjectPath\IntVue.csproj")) {
    Write-Error-Custom "IntVue.csproj not found in $ProjectPath"
    Write-Info "Run this script from the project root directory"
    exit 1
}
Write-Success "Project found at: $ProjectPath"

# Step 1: Check Developer Mode
Write-Host "`n--- Step 1: Checking Developer Mode ---`n"

$regPath = "HKLM:\SOFTWARE\Microsoft\Windows\CurrentVersion\AppModelUnlock"
$developerModeEnabled = (Get-ItemProperty $regPath -ErrorAction SilentlyContinue).AllowDevelopmentWithoutDevLicense -eq 1

if ($developerModeEnabled) {
    Write-Success "Developer Mode is enabled on Desktop PC"
} else {
    Write-Error-Custom "Developer Mode NOT enabled on Desktop PC"
    Write-Info "Enable at: Settings > System > For developers > Developer Mode (toggle ON)"
    Write-Info "Then restart this script"
    exit 1
}

# Step 2: Verify Network Connectivity
Write-Host "`n--- Step 2: Testing Network Connectivity ---`n"

Write-Info "Testing connection to Surface Tablet ($SurfaceIP)..."
$pingTest = Test-Connection $SurfaceIP -Count 1 -ErrorAction SilentlyContinue

if ($pingTest) {
    Write-Success "Surface Tablet is reachable (latency: $($pingTest.ResponseTime)ms)"
} else {
    Write-Error-Custom "Cannot reach Surface Tablet at $SurfaceIP"
    Write-Info "Verify:"
    Write-Info "  1. Both machines are on the same LAN (Wi-Fi or Ethernet)"
    Write-Info "  2. Surface IP is correct (run 'ipconfig' on Surface)"
    Write-Info "  3. Firewall allows ICMP (ping)"
    exit 1
}

# Step 3: Check Remote Tools Installation
Write-Host "`n--- Step 3: Checking Remote Tools Installation ---`n"

$remoteToolsPaths = @(
    "C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger\msvsmon.exe",
    "C:\Program Files\Microsoft Visual Studio 2025\Remote Debugger\msvsmon.exe",
    "C:\Program Files\Microsoft Visual Studio 17\Remote Debugger\msvsmon.exe"
)

$remoteToolsFound = $false
$remoteToolsPath = $null

foreach ($path in $remoteToolsPaths) {
    if (Test-Path $path) {
        $remoteToolsFound = $true
        $remoteToolsPath = $path
        break
    }
}

if ($remoteToolsFound) {
    Write-Success "Visual Studio Remote Tools found at: $remoteToolsPath"
} else {
    Write-Warning-Custom "Visual Studio Remote Tools not found on this machine"
    Write-Info "Note: Remote Tools must be installed ON THE SURFACE TABLET, not Desktop"
    Write-Info "Action for Surface Tablet:"
    Write-Info "  1. Download from: https://visualstudio.microsoft.com/downloads/#remote-tools-for-visual-studio-2026"
    Write-Info "  2. Select version matching your VS/Rider version"
    Write-Info "  3. Select $Architecture architecture version"
    Write-Info "  4. Run installer and complete installation"
    Write-Info "  5. Before debugging, start: C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger\msvsmon.exe"
}

# Step 4: Check .NET SDK
Write-Host "`n--- Step 4: Checking .NET SDK ---`n"

$dotnetVersion = dotnet --version
if ($?) {
    Write-Success ".NET SDK version: $dotnetVersion"
} else {
    Write-Error-Custom ".NET SDK not found"
    Write-Info "Install from: https://dotnet.microsoft.com/en-us/download"
    exit 1
}

# Step 5: Build the Project
Write-Host "`n--- Step 5: Building IntVue for $Architecture ---`n"

Write-Info "Building project... (this may take a minute)"
Push-Location $ProjectPath

$buildOutput = dotnet build -c Debug -p:Platform=$Architecture 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Success "Build completed successfully"
} else {
    Write-Error-Custom "Build failed"
    Write-Host $buildOutput
    exit 1
}

# Step 6: Verify Build Artifacts
Write-Host "`n--- Step 6: Verifying Build Artifacts ---`n"

$binPath = ".\bin\$Architecture\Debug\net10.0-windows10.0.26100.0\win-$Architecture"
if (Test-Path $binPath) {
    Write-Success "Build output found at: $binPath"

    $exePath = Join-Path $binPath "IntVue.exe"
    if (Test-Path $exePath) {
        Write-Success "App executable ready: IntVue.exe"
    }

    $pdbPath = Join-Path $binPath "IntVue.pdb"
    if (Test-Path $pdbPath) {
        Write-Success "Debug symbols found: IntVue.pdb (required for debugging)"
    } else {
        Write-Warning-Custom "Debug symbols (PDB) not found - breakpoints may not work"
    }
} else {
    Write-Error-Custom "Build output directory not found: $binPath"
    exit 1
}

Pop-Location

# Step 7: Summary and Next Steps
Write-Host "`n═══════════════════════════════════════════════════════════════" -ForegroundColor Green
Write-Host "Setup Verification Complete" -ForegroundColor Green
Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Green

Write-Host "✓ Desktop PC is ready for remote debugging`n"

Write-Host "Next Steps:`n" -ForegroundColor Yellow

Write-Host "1. On SURFACE TABLET:"
Write-Host "   • Download Remote Tools for Visual Studio 2026"
Write-Host "   • Get $Architecture version from:"
Write-Host "     https://visualstudio.microsoft.com/downloads/#remote-tools-for-visual-studio-2026"
Write-Host "   • Install and run: C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger\msvsmon.exe`n"

Write-Host "2. Start Debugging:"
Write-Host "   • Ensure msvsmon.exe is running on Surface (shows 'Waiting for connection')"
Write-Host "   • In Rider: Run → Edit Configurations"
Write-Host "   • Create .NET Executable configuration"
Write-Host "   • Enable 'Debug on Remote Machine'"
Write-Host "   • Set Host: $SurfaceIP"
Write-Host "   • Set Port: 4026`n"

Write-Host "3. Deploy and Debug:"
Write-Host "   • Click 'Run' or 'Debug' in Rider"
Write-Host "   • App will launch on Surface Tablet"
Write-Host "   • Breakpoints will hit in Rider when code executes on Surface`n"

Write-Host "Reference Documentation:"
Write-Host "   • Full setup guide: Docs\RemoteDebugSetup.md"
Write-Host "   • Implementation planning: Docs\ImplementationPlanning\impl-mvp.md"
Write-Host "   • Rider debugging: https://www.jetbrains.com/help/rider/Debugging-Code.html`n"

Write-Host "═══════════════════════════════════════════════════════════════`n" -ForegroundColor Green
