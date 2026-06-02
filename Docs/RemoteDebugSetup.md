# Remote Debugging Setup Guide
## JetBrains Rider + Surface Tablet from Desktop PC

**Last Updated:** June 1, 2026  
**Difficulty:** Intermediate  
**Time to Complete:** 1 hour (one-time setup)

---

## Overview

This guide enables debugging IntVue app running on a Surface Tablet from JetBrains Rider on your Desktop PC. This is essential for testing camera/microphone features on actual hardware.

### Your Setup
- **Development Machine:** Desktop PC (Windows 11+, no camera)
- **Target Device:** Surface Tablet (Windows 11+ with ARM64 processor, has camera/microphone)
- **Debugger:** JetBrains Rider 2024.3+

---

## Step 1: Enable Developer Mode (Both Machines)

Developer Mode allows unsigned app deployment and remote debugging.

### On Desktop PC
```powershell
# Open Settings
Settings > System > For developers

# Enable:
☑ Developer Mode
☑ Device Discovery (optional, helps Surface discovery)
```

### On Surface Tablet
```powershell
# Same steps on the tablet itself
Settings > System > For developers

# Enable:
☑ Developer Mode
☑ Device Portal (optional, for web-based diagnostics)
```

**Verify:** Restart both machines after enabling.

---

## Step 2: Install Visual Studio Remote Tools (Surface Tablet)

Remote Tools runs on Surface to accept debug connections from Rider.

### Download Correct Version
1. Go to: https://visualstudio.microsoft.com/downloads/#remote-tools-for-visual-studio-2026
2. Download **Visual Studio 2026 Remote Tools** 
3. **Important:** Select **ARM64** version (Surface devices use ARM64)
   - If you're unsure of your Surface architecture, run on Surface:
     ```powershell
     $env:PROCESSOR_ARCHITECTURE  # Output: ARM64
     ```

### Install on Surface Tablet
1. Copy installer to Surface (USB drive or network share)
2. Run `RemoteTools_<version>_arm64.exe` on Surface
3. Accept license agreement
4. Choose installation path (default is fine): `C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger`
5. **Do NOT start the service automatically** (we'll start it manually when needed)

---

## Step 3: Verify Network Connectivity

Both machines must be on the same LAN and able to communicate.

### Get Surface IP Address
On Surface Tablet, open PowerShell and run:
```powershell
ipconfig

# Look for "IPv4 Address" under your active connection
# Example: 192.168.1.100
# Write this down: Surface IP = _______________
```

### Verify Connection (From Desktop)
On Desktop PC, open PowerShell and run:
```powershell
$SurfaceIP = "192.168.1.100"  # Replace with actual Surface IP

# Test connectivity
ping $SurfaceIP

# Expected output:
# Reply from 192.168.1.100: bytes=32 time=15ms TTTl=64
```

**If ping fails:**
- Check Wi-Fi connection (both on same network)
- Verify Surface IP is correct
- Disable firewall temporarily to test (then re-enable with rules)

---

## Step 4: Start Remote Debugging Monitor (Surface)

On Surface Tablet, start the remote debugging monitor:

```powershell
# Run as Administrator
cd "C:\Program Files\Microsoft Visual Studio 2026\Remote Debugger"
.\msvsmon.exe /anyuser /nosecuritywarn

# Expected output:
# "Microsoft Visual Studio Remote Debugging Monitor v17.13..."
# "Waiting for connection on tcp:4026"
# 
# Keep this window open while debugging
```

**Note:** Port `4026` is the default. If blocked, you can specify a different port:
```powershell
.\msvsmon.exe /anyuser /nosecuritywarn /port 4030
```

---

## Step 5: Configure Rider (Desktop PC)

### Create a Run Configuration

1. **Open Rider** on Desktop PC
2. **Run → Edit Configurations**
3. Click **+** to add new configuration
4. Select **.NET Executable**
5. Configure:
   - **Name:** `Remote Debug - Surface`
   - **Project:** `IntVue`
   - **Target framework:** `net10.0-windows10.0.26100.0` (match your project)
   - **Executable:** Leave blank (or select the built app EXE)
   - **Program arguments:** (leave empty)

6. **Remote Host Configuration:**
   - Check: ☑ **Debug on Remote Machine**
   - **Host:** `192.168.1.100` (Surface IP from Step 3)
   - **Port:** `4026` (or your custom port from Step 4)
   - **Username:** (leave blank if same network)
   - **Password:** (leave blank if same network)

7. Click **OK**

### Alternative: Attach to Running Process

If you prefer to run the app manually on Surface and attach from Rider:

1. **Run → Attach to Process**
2. **Connection type:** Remote (TCP/IP)
3. **Remote host:** `192.168.1.100`
4. **Port:** `4026`
5. **Click "Find"** to list processes on Surface
6. Select `IntVue.exe` from the list
7. Click **Attach**

---

## Step 6: Deploy App to Surface

Choose one deployment method:

### Option A: Fast Deploy with `winapp run` (Recommended for debugging)

```powershell
# On Desktop PC, in project directory
cd C:\Users\nobu\RiderProjects\IntVue

$SurfaceIP = "192.168.1.100"

# Build for ARM64
dotnet build -c Debug -p:Platform=ARM64

# Deploy and run on Surface
winapp run --architecture ARM64 --device $SurfaceIP
```

### Option B: Manual File Copy + Launch

```powershell
# Build on Desktop
dotnet build -c Debug -p:Platform=ARM64

# Copy app folder to Surface
$Source = ".\bin\ARM64\Debug\net10.0-windows10.0.26100.0\win-arm64"
$Destination = "\\$SurfaceIP\Users\Public\Downloads\IntVue"

Copy-Item -Recurse -Force $Source $Destination

# On Surface, run the app:
# C:\Users\Public\Downloads\IntVue\IntVue.exe
```

---

## Step 7: Debug Your Code

### Set Breakpoints
1. In Rider, open any source file (e.g., `MainWindow.xaml.cs`)
2. Click in the gutter to set a breakpoint (red circle appears)
3. Example: Set breakpoint on the first line of `MainWindow()` constructor

### Start Debugging
1. **Run → Debug** (or press **Shift+F9**)
   - If using Run Configuration from Step 5
   - OR **Run → Attach to Process** if using manual deployment

2. The app launches on Surface Tablet

3. When breakpoint is hit:
   - Rider pauses execution
   - Variables panel shows current state
   - Call Stack panel shows where you are
   - You can Step Over (**F10**), Step Into (**F11**), Continue (**F9**)

### Example Session
```
MainWindow.xaml.cs: MainWindow() [breakpoint]
↓ (app hits breakpoint on Surface)
Rider pauses, shows:
  Variables:
    this = IntVue.MainWindow
    this.Title = "IntVue"
  Call Stack:
    MainWindow()
    App.Current.InitializeComponent()
    ...
↓ (Step Over to next line)
```

---

## Step 8: Verification Checklist

- [ ] Developer Mode enabled on both Desktop and Surface
- [ ] Visual Studio Remote Tools installed on Surface (ARM64 version)
- [ ] `msvsmon.exe` running on Surface, listening on port 4026
- [ ] Ping from Desktop to Surface IP successful
- [ ] Rider Run Configuration created and pointing to Surface IP:4026
- [ ] App deployed to Surface and running
- [ ] Breakpoint set in Rider source code
- [ ] Breakpoint hits when app reaches that code on Surface
- [ ] Variables visible in Rider debugger
- [ ] Can step through code with F10/F11

---

## Troubleshooting

### "Cannot connect to remote machine"
**Causes:**
- Surface not on same LAN
- Wrong Surface IP address
- `msvsmon.exe` not running on Surface
- Firewall blocking port 4026

**Solution:**
```powershell
# On Desktop, verify Surface IP again
$IP = "192.168.1.100"
ping $IP

# On Surface, verify Remote Monitor is running
# Check Windows Task Manager → Services → "Remote Debugging Monitor"
# Or run msvsmon.exe manually again
```

### "Could not attach to process"
**Causes:**
- App not running on Surface
- Wrong app name in process list
- Debug symbols not found

**Solution:**
- Manually launch app on Surface first
- Verify app appears in process list via **Run → Attach to Process**
- Ensure `.pdb` files are in `bin` folder (Debug build includes these)

### "Breakpoint does not hit"
**Causes:**
- Source code on Desktop differs from Surface
- Build not up-to-date
- Debug symbols not loaded

**Solution:**
```powershell
# Full rebuild
dotnet clean
dotnet build -c Debug -p:Platform=ARM64

# Verify .pdb files exist
ls ".\bin\ARM64\Debug\net10.0-windows10.0.26100.0\win-arm64\*.pdb"

# Redeploy to Surface
winapp run --architecture ARM64 --device 192.168.1.100
```

### "App crashes on startup with null reference"
**Cause:**
- Desktop app tried to initialize camera (not found on Desktop)

**Solution:**
- Already handled by MediaCaptureService graceful device-not-found fix (#13)
- If still crashing, verify fix is deployed to Surface

### "Remote debugging is very slow"
**Causes:**
- Network latency
- Too many breakpoints set
- Large amounts of data in variables

**Solution:**
- Remove unnecessary breakpoints
- Use Conditional Breakpoints (right-click breakpoint → Condition)
- Only inspect variables you need

---

## Next Steps After Setup

### 1. Test Camera/Microphone Features
With Surface connected, you can now test:
- Camera preview rendering
- Microphone input
- Recording functionality
- Permission dialogs

### 2. Test UI on Actual Hardware
- Test touch interactions on Surface screen
- Verify layout on tablet screen size
- Test keyboard navigation

### 3. Automated Testing on Desktop
For unit tests without hardware:
- Use `MockMediaCaptureService` from the documentation
- Run: `dotnet test`

---

## Reference

- **Implementation Planning:** `Docs/ImplementationPlanning/impl-mvp.md` (lines 182-431)
- **Media Service:** `Services/MediaCaptureService.cs`
- **Visual Studio Remote Debugging:** https://learn.microsoft.com/en-us/visualstudio/debugger/remote-debugging
- **Rider Debugger Help:** https://www.jetbrains.com/help/rider/Debugging-Code.html

---

## Support

If you encounter issues not covered here:

1. **Check Prerequisites:**
   - Both machines on same LAN
   - Developer Mode enabled
   - Remote Tools installed (correct architecture)

2. **Verify Connectivity:**
   - `ping <SurfaceIP>` from Desktop
   - `msvsmon.exe` running on Surface

3. **Review Logs:**
   - Rider Debug Console (shows connection attempts)
   - Surface Event Viewer (shows Remote Tools errors)

4. **Reference Documentation:**
   - `Docs/ImplementationPlanning/impl-mvp.md` (comprehensive guide)
   - Microsoft Visual Studio Remote Debugging docs

---

**Status:** Ready for implementation (Updated June 1, 2026)
