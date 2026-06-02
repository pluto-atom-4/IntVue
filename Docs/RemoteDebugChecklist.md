# Remote Debugging Setup Checklist
## Surface Tablet from Desktop PC with JetBrains Rider

**Date Started:** ________________  
**Surface IP Address:** ________________  
**Completed Date:** ________________

---

## Prerequisites

### Desktop PC
- [ ] Windows 11 Pro or Enterprise (required for remote debugging)
- [ ] Developer Mode enabled
  - Go to: Settings > System > For developers
  - Toggle ON: Developer Mode
- [ ] JetBrains Rider 2024.3+ installed
- [ ] .NET 8+ SDK installed
  - Verify: Open PowerShell and run `dotnet --version`
- [ ] IntVue project cloned/accessible

### Surface Tablet
- [ ] Windows 11 or Windows 12 (must be ARM64-based Surface)
- [ ] Developer Mode enabled (same steps as Desktop)
- [ ] Processor confirmed to be ARM64
  - Verify: Open PowerShell and run `$env:PROCESSOR_ARCHITECTURE`
  - Expected output: `ARM64`

---

## Network Setup

- [ ] Both devices on same LAN
  - [ ] Desktop connected to Wi-Fi/Ethernet
  - [ ] Surface connected to same Wi-Fi/Ethernet
- [ ] Surface IP address obtained
  - [ ] Run on Surface: `ipconfig`
  - [ ] Write down IPv4 address: **_____________________**
- [ ] Network connectivity verified
  - [ ] From Desktop PowerShell: `ping <SurfaceIP>`
  - [ ] Ping successful (response received)

---

## Remote Tools Installation (Surface Only)

- [ ] Visual Studio Remote Tools downloaded
  - [ ] Source: https://visualstudio.microsoft.com/downloads/#remote-tools-for-visual-studio-2026
  - [ ] Version: Visual Studio 2026
  - [ ] **Architecture: ARM64** (CRITICAL - must match Surface processor)
- [ ] Remote Tools installed on Surface
  - [ ] Installation path: `C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger`
  - [ ] Installation verified: File explorer shows `msvsmon.exe` exists
  - [ ] Do NOT start service automatically (we'll start manually)

---

## Build & Deploy (Desktop PC)

- [ ] Project built for target architecture
  - [ ] Open PowerShell in project root
  - [ ] Run: `dotnet build -c Debug -p:Platform=ARM64`
  - [ ] Build succeeded (0 errors, warnings acceptable)
- [ ] Build artifacts verified
  - [ ] Navigate to: `.\bin\ARM64\Debug\net10.0-windows10.0.26100.0\win-arm64`
  - [ ] Files present:
    - [ ] `IntVue.exe` (application)
    - [ ] `IntVue.pdb` (debug symbols - required for breakpoints)
- [ ] App deployed to Surface
  - [ ] Option A - Using `winapp run`:
    - [ ] Run: `winapp run --architecture ARM64 --device <SurfaceIP>`
  - [ ] Option B - Manual copy:
    - [ ] Copy app folder to Surface
    - [ ] Run `IntVue.exe` manually on Surface

---

## Rider Remote Debugging Configuration

- [ ] Rider opened on Desktop
- [ ] Run Configuration created
  - [ ] Go to: **Run → Edit Configurations**
  - [ ] Click **+** to add new configuration
  - [ ] Select **.NET Executable**
  - [ ] Configuration name: `Remote Debug - Surface`
  - [ ] Project selected: `IntVue`
  - [ ] Target framework: `net10.0-windows10.0.26100.0`
- [ ] Remote Machine settings configured
  - [ ] ☑ **Debug on Remote Machine** (checkbox enabled)
  - [ ] Host: `<SurfaceIP>` (from earlier)
  - [ ] Port: `4026` (default Rider debug port)
  - [ ] Username: (leave blank for same network)
  - [ ] Password: (leave blank for same network)
- [ ] Configuration saved
  - [ ] Click **OK**
  - [ ] Configuration appears in Run dropdown

---

## Start Remote Debugging

### On Surface Tablet (Before Each Debug Session)
- [ ] Open PowerShell as Administrator
- [ ] Navigate to: `C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger`
- [ ] Run: `.\msvsmon.exe /anyuser /nosecuritywarn`
- [ ] Verify output shows: `"Waiting for connection on tcp:4026"`
- [ ] Leave window open (keep monitoring running)

### On Desktop (In Rider)
- [ ] Set breakpoint in source code
  - [ ] Open file: `MainWindow.xaml.cs` or similar
  - [ ] Click in gutter to place red circle (breakpoint)
- [ ] Start debugging
  - [ ] Select configuration: `Remote Debug - Surface`
  - [ ] Click **Debug** button (or press **Shift+F9**)
- [ ] Verify connection
  - [ ] Rider shows "Connecting to <SurfaceIP>:4026..."
  - [ ] Debugger console shows successful connection
- [ ] App launches on Surface
- [ ] Breakpoint is hit
  - [ ] Rider pauses execution
  - [ ] Variables panel shows current state
  - [ ] Can step through code with **F10** (over) / **F11** (into)

---

## Verification Testing

### Breakpoint Test
- [ ] Set breakpoint at start of `MainWindow()` constructor
  - [ ] Expected: Breakpoint hits immediately when app launches
  - [ ] Variables visible: Can inspect `this`, parameters
  - [ ] Can step through code

### Code Navigation Test
- [ ] From breakpoint, press **F10** (step over)
  - [ ] Expected: Execution moves to next line
  - [ ] Source code highlighted shows current position
- [ ] Press **F11** (step into)
  - [ ] Expected: Enters called method, debugger follows
- [ ] Press **F9** (continue)
  - [ ] Expected: Execution resumes on Surface
  - [ ] App returns to normal operation

### Variables Inspection Test
- [ ] At any breakpoint, check **Variables** panel
  - [ ] Current local variables displayed
  - [ ] Can expand objects to see properties
  - [ ] Can check state of UI elements
- [ ] Hover over variable in code
  - [ ] Expected: Tooltip shows variable value

### Call Stack Test
- [ ] Check **Call Stack** panel at any breakpoint
  - [ ] Shows function call hierarchy
  - [ ] Can click to navigate between frames

---

## Troubleshooting Checklist

### "Cannot connect to remote machine"
- [ ] Verify `msvsmon.exe` is running on Surface
  - [ ] Check console window shows "Waiting for connection on tcp:4026"
  - [ ] If not, restart the application
- [ ] Verify network connectivity
  - [ ] From Desktop: `ping <SurfaceIP>` should work
  - [ ] If fails, check LAN connection
- [ ] Verify port 4026 is not blocked
  - [ ] Check Windows Firewall on Surface
  - [ ] May need to add inbound rule for port 4026

### "Breakpoint does not hit"
- [ ] Verify debug symbols (.pdb) exist
  - [ ] Check: `.\bin\ARM64\Debug\net10.0-windows10.0.26100.0\win-arm64\IntVue.pdb`
  - [ ] If missing: Rebuild with `dotnet clean` then `dotnet build`
- [ ] Verify source code matches Surface version
  - [ ] Ensure latest code is deployed to Surface
  - [ ] Redeploy if code changed
- [ ] Try simplest breakpoint first
  - [ ] Set breakpoint in `MainWindow()` constructor
  - [ ] Does it hit? If yes, other issues are likely environmental

### "App crashes on startup"
- [ ] Check Rider Debug Console for exception details
  - [ ] Look for "null reference exception" or similar
- [ ] Verify MediaCaptureService graceful handling
  - [ ] App should run on Desktop PC (no camera)
  - [ ] Camera features disabled, rest functional
- [ ] Check Surface logs
  - [ ] Event Viewer on Surface may show app-related errors

### "Debugging is very slow"
- [ ] Reduce number of active breakpoints
  - [ ] Only set breakpoints where needed
  - [ ] Delete breakpoints in loops or frequently-called code
- [ ] Disable expression evaluation in debugger
  - [ ] Rider preferences: Editor > Debugger
- [ ] Check network latency
  - [ ] `ping <SurfaceIP>` shows latency
  - [ ] High latency (>100ms) makes debugging slow

### "Remote Tools won't start"
- [ ] Verify it's the ARM64 version
  - [ ] Check installer filename: `RemoteTools_..._arm64.exe`
  - [ ] If not: Reinstall with correct architecture
- [ ] Check installation path exists
  - [ ] Verify: `C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger\msvsmon.exe`
  - [ ] If not: Reinstall Remote Tools
- [ ] Run as Administrator
  - [ ] Right-click PowerShell window
  - [ ] Select "Run as administrator"
  - [ ] Then run `msvsmon.exe`

---

## Success Indicators

✓ **All of these should be true:**

1. Desktop PC can ping Surface IP successfully
2. `msvsmon.exe` is running on Surface (shows waiting message)
3. Rider connection shows "Connected to <SurfaceIP>"
4. App launches on Surface when you click Debug
5. Breakpoints in code hit and pause execution
6. Rider shows variables and call stack at breakpoints
7. Stepping through code (F10/F11) works smoothly
8. Can inspect variable values in Riders debugger panels
9. No errors in Rider Debug Console
10. App continues normally when debugging is paused

---

## Quick Commands

**Build project:**
```powershell
dotnet build -c Debug -p:Platform=ARM64
```

**Deploy and run:**
```powershell
winapp run --architecture ARM64 --device 192.168.1.100
```

**Start remote monitor (on Surface):**
```powershell
cd "C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger"
.\msvsmon.exe /anyuser /nosecuritywarn
```

**Check Surface IP (on Surface):**
```powershell
ipconfig
```

**Verify network (on Desktop):**
```powershell
ping 192.168.1.100
```

---

## Support Resources

- **Setup Guide:** `Docs/RemoteDebugSetup.md` (comprehensive walkthrough)
- **Automation Script:** `Scripts/RemoteDebugSetup.ps1` (verify prerequisites)
- **Implementation Planning:** `Docs/ImplementationPlanning/impl-mvp.md` (technical details)
- **Rider Help:** https://www.jetbrains.com/help/rider/Debugging-Code.html
- **Microsoft Remote Debugging:** https://learn.microsoft.com/en-us/visualstudio/debugger/remote-debugging

---

**Status:** Setup Complete ✓  
**Last Verified:** ________________
