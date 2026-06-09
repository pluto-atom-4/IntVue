# SimpleCapture Deployment Guide

## What You Have After Publishing

When you run `dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64`, the output folder contains:

```
bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/publish/
├── SimpleCapture.exe              ← Main executable (292 KB)
├── coreclr.dll                    ← .NET runtime core
├── clrjit.dll                     ← Just-in-time compiler
├── System.*.dll                   ← .NET system libraries
├── Microsoft.Windows.*.dll        ← Windows App SDK libraries
├── WinUICompat.dll               ← WinUI compatibility layer
└── [multiple language packs and resource folders]
```

**Total Size:** ~218 MB (includes all .NET runtime and WinUI libraries)

**Key Point:** This folder is **completely self-contained**. You can copy it to any Windows 10.0.26100.0+ machine and run `SimpleCapture.exe` directly **without installing .NET runtime**.

## Deployment Steps

### Step 1: Publish on Development Machine

```powershell
cd C:\Users\nobu\RiderProjects\IntVue\SimpleCapture

# Option A: Use the script
.\build-and-run.ps1 -PublishOnly -Platform x64 -Configuration Debug

# Option B: Manual publish command
dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64
```

### Step 2: Locate the Published Folder

```
C:\Users\nobu\RiderProjects\IntVue\SimpleCapture\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish\
```

### Step 3: Copy to Surface Pro 7

**Option A: USB Drive (Recommended)**

```powershell
# On dev machine, copy the entire publish folder
$source = "C:\Users\nobu\RiderProjects\IntVue\SimpleCapture\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish"
$destination = "E:\SimpleCapture"  # USB drive letter

Copy-Item -Path $source -Destination $destination -Recurse -Force
```

Then on Surface Pro 7:
- Insert USB drive
- Copy `E:\SimpleCapture` folder to local drive (e.g., `C:\Users\{username}\Desktop\SimpleCapture`)
- Run `SimpleCapture.exe`

**Option B: Network Share**

```powershell
# On dev machine, share the publish folder
net share simplecapture=C:\Users\nobu\RiderProjects\IntVue\SimpleCapture\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish /grant:everyone,FULL
```

Then on Surface Pro 7:
```powershell
# Map network drive
net use Z: \\{dev-machine-ip}\simplecapture

# Run directly from network
Z:\SimpleCapture.exe
```

### Step 4: Run on Surface Pro 7

Simply double-click `SimpleCapture.exe` - no installation needed!

```powershell
# Or from command line
C:\Users\{username}\Desktop\SimpleCapture\SimpleCapture.exe
```

## Folder Size Optimization

If the 218 MB folder is too large for your deployment medium, you can reduce it:

### Remove Language Packs (Not English)

```powershell
# In the publish folder, delete language packs except en-US
cd bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish

# Keep only English and remove others
$keep = @('en-US')
Get-ChildItem | Where-Object { $_.Name -match '^[a-z]{2}(-|_)[A-Z]{2}' } | 
  Where-Object { $_.Name -notin $keep } | 
  Remove-Item -Recurse -Force
```

**Result:** ~180 MB (38 MB smaller)

### Alternative: Trimmed Self-Contained Publish

For maximum size reduction, you can use ReadyToRun (R2R) compilation:

```powershell
dotnet publish -c Release -p:Platform=x64 --self-contained true --runtime win-x64 -p:PublishTrimmed=true -p:PublishReadyToRun=true
```

**Result:** ~100-120 MB (much smaller but Release mode)

## Verification Checklist

After copying to Surface Pro 7, verify:

- [ ] Folder copied successfully
- [ ] `SimpleCapture.exe` file exists (292 KB)
- [ ] No permission errors when running exe
- [ ] Application window appears
- [ ] Camera dialog launches when clicking "Capture Video"
- [ ] Can record video/photo
- [ ] Recorded media plays back

## Troubleshooting

### "The application failed to start"

**Cause:** Missing Windows App SDK runtime or incompatible Windows version  
**Solution:** Verify Surface Pro 7 is running Windows 10.0.26100.0 or later

```powershell
# On Surface Pro 7, check Windows version
winver
```

### "SimpleCapture.exe is not a valid Win32 application"

**Cause:** File corruption during copy or wrong architecture  
**Solution:** 
- Re-copy the folder from USB/network
- Verify you're using the correct architecture (win-x64 for Surface Pro 7)

### Camera permission error in app

**Cause:** Windows privacy settings blocking camera access  
**Solution:**
1. Open Settings → Privacy & Security → Camera
2. Ensure "Camera access" is enabled
3. Allow SimpleCapture to access camera

### File is in use / Cannot delete

**Cause:** App still running or file locked  
**Solution:** 
1. Close all instances of SimpleCapture
2. Wait 2-3 seconds
3. Try again

## Size Reference

| Build Type | Size | Self-Contained | Deployment |
|-----------|------|-----------------|------------|
| Debug (publish) | 218 MB | ✅ Yes | 🟢 Good |
| Release (publish) | 180 MB | ✅ Yes | 🟢 Better |
| Release (trimmed) | 100 MB | ✅ Yes | 🟢 Best |

## Quick Deployment Script

Create `deploy-to-usb.ps1`:

```powershell
param(
    [Parameter(Mandatory)]
    [string]$USBDrive = 'E:',
    
    [string]$AppName = 'SimpleCapture'
)

$source = "C:\Users\nobu\RiderProjects\IntVue\SimpleCapture\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish"
$destination = "$USBDrive\$AppName"

Write-Host "Copying $source to $destination..."
Copy-Item -Path $source -Destination $destination -Recurse -Force

Write-Host "Done! Copy the '$AppName' folder from USB drive to Surface Pro 7 and run SimpleCapture.exe"
Write-Host "Folder size: $(((Get-ChildItem $destination -Recurse | Measure-Object -Property Length -Sum).Sum / 1MB).ToString('F1')) MB"
```

Run: `.\deploy-to-usb.ps1 -USBDrive E:`

## Next Steps

1. ✅ Publish on dev machine: `dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64`
2. ✅ Copy `publish` folder to USB/network
3. ✅ Transfer to Surface Pro 7
4. ✅ Run `SimpleCapture.exe`
5. ✅ Test camera capture
6. Report findings back to IntVue debugging

---

**Remember:** This standalone executable requires **no .NET installation** on the target machine. It's a complete, independent application.
