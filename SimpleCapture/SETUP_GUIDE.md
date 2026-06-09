# SimpleCapture Setup Guide

## Overview

**SimpleCapture** is a minimal WinUI 3 test application based on Microsoft's official documentation for `CameraCaptureUI`. It's designed to test camera capture functionality in isolation from the complex IntVue architecture.

## Build vs Publish: Key Difference

**`dotnet build`** creates:
- DLL files (compiled code)
- Still requires .NET runtime installed to run
- Used for development/testing with `dotnet run`

**`dotnet publish`** creates:
- EXE file (executable)
- **Includes all .NET runtime files** (~218 MB)
- Standalone — can run on any Windows 10.0.26100.0+ machine **without installing .NET**
- Best for deployment to Surface Pro 7

**For SimpleCapture, always use `dotnet publish`** to create the self-contained executable.

## Why SimpleCapture?

IntVue uses low-level `MediaCapture` with custom frame-based rendering. When camera issues arise, it's difficult to determine whether they're:
- Specific to IntVue's architecture
- Related to Surface Pro 7 hardware
- Caused by the MediaCapture/MediaFrameSource mutual exclusivity on the device

**SimpleCapture uses the high-level `CameraCaptureUI`**, which is:
- Microsoft's recommended approach for simple capture scenarios
- Minimal code (~200 lines)
- Uses Windows' built-in camera UI
- Good baseline to isolate hardware issues

## Quick Start

### Step 1: Navigate to SimpleCapture Directory

```powershell
cd C:\Users\nobu\RiderProjects\IntVue\SimpleCapture
```

### Step 2: Publish with Self-Contained Runtime

This creates a standalone executable with all .NET runtime files included:

```powershell
dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64
```

This generates a `publish` folder at:
```
bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/publish/
```

Or use the convenience script:

```powershell
.\build-and-run.ps1
```

### Step 3: Run the Published Executable

The standalone executable is at:
```
bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/publish/SimpleCapture.exe
```

Run it directly:

```powershell
# Using the script (recommended)
.\build-and-run.ps1

# Or run the exe directly
.\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish\SimpleCapture.exe
```

A window should appear with:
- **Capture Video** button → launches Windows camera UI for video recording
- **Capture Photo** button → launches Windows camera UI for photo capture
- **Log panel** → shows operation timestamps and any errors

## Testing on Surface Pro 7

Once you have the SimpleCapture app working:

1. **Test Video Capture**
   - Click "Capture Video"
   - Record a short video using the built-in camera UI
   - Verify the video plays back in the app

2. **Test Photo Capture**
   - Click "Capture Photo"
   - Take a photo using the built-in camera UI
   - Verify the photo displays in the app

3. **Observe Behavior**
   - Does the camera UI appear immediately?
   - Does preview work in the capture dialog?
   - Does captured media play/display correctly?
   - Are there any errors or delays?

## Troubleshooting

### Build Errors

**"Microsoft.Windows.SDK.BuildTools version mismatch"**
- The project file pins specific SDK versions to match IntVue
- Delete `bin/` and `obj/` directories and rebuild

```powershell
rm -r bin, obj
dotnet build -c Debug -p:Platform=x64 --self-contained true
```

### Camera Issues

**"Camera not found"**
- Check Windows Settings → Privacy & Security → Camera
- Ensure camera access is enabled
- Verify no other app is using the camera

**"CameraCaptureUI dialog doesn't appear"**
- Ensure you're running on Windows 10.0.26100.0 or later
- Run Event Viewer and check System logs for errors

**"Video playback fails"**
- Check disk space on C:\Users\nobu\AppData\Local (temp capture location)
- Verify H.264 codec support
- Try capturing with different resolution

## Comparing SimpleCapture vs IntVue

| Feature | SimpleCapture | IntVue |
|---------|---------------|--------|
| **Complexity** | Minimal | Full MVVM architecture |
| **Capture UI** | Windows built-in | Custom implementation |
| **Code Size** | ~200 lines | ~2000+ lines |
| **Frame Access** | None | Full MediaFrameReader |
| **Preview** | Simple playback | Continuous frame streaming |
| **Use Case** | Hardware testing | Production interview app |

## Deployment to Surface Pro 7

Once SimpleCapture builds and runs successfully on your development machine:

### Option 1: Copy Built Executable (Recommended)

The self-contained build includes all dependencies:

```powershell
# Build creates: bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/SimpleCapture.exe
# Copy this directory to Surface Pro 7
xcopy "bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64" "X:\SimpleCapture" /E /I

# Run on Surface Pro 7
X:\SimpleCapture\SimpleCapture.exe
```

### Option 2: Source Deploy

```powershell
# Copy entire SimpleCapture folder to Surface Pro 7
# Then on Surface Pro 7:
cd SimpleCapture
dotnet run -c Debug -p:Platform=x64 --self-contained true
```

## Performance Notes

- First launch may take a few seconds (loading .NET runtime)
- Subsequent launches are faster
- Camera enumeration typically takes 1-2 seconds
- Video playback may stutter briefly on first frame (normal for MediaPlayerElement)

## Collecting Diagnostics

If you encounter issues on Surface Pro 7, collect:

1. **Console Output**
   - Screenshots of any error messages from SimpleCapture
   - Log entries from the app's log panel

2. **Windows Event Viewer**
   - Event ID in System log related to camera
   - Any kernel-mode driver errors

3. **Device Manager**
   - Check if camera driver is installed correctly
   - Note the exact camera model

## Next Steps

- [Return to IntVue README](../README.md)
- [Check IntVue CLAUDE.md](../CLAUDE.md) for main project guidelines
- [Review Microsoft CameraCaptureUI Docs](https://learn.microsoft.com/en-us/windows/apps/develop/camera/cameracaptureui)

## Questions or Issues?

If SimpleCapture works but IntVue doesn't, the issue is likely:
- IntVue's MediaCapture initialization logic
- Surface camera device enumeration differences
- MediaFrameSource/recording mutual exclusivity handling

If SimpleCapture fails, the issue is likely:
- Surface camera drivers
- Windows API permissions
- System-level camera configuration
