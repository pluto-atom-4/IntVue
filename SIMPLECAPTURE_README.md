# SimpleCapture Sub-Project

A minimal, focused WinUI 3 application for testing camera capture functionality based on [Microsoft's CameraCaptureUI documentation](https://learn.microsoft.com/en-us/windows/apps/develop/camera/cameracaptureui).

## Quick Links

- **Setup Guide:** [SimpleCapture/SETUP_GUIDE.md](SimpleCapture/SETUP_GUIDE.md)
- **Build Instructions:** [SimpleCapture/README.md](SimpleCapture/README.md)
- **Source Code:** [SimpleCapture/](SimpleCapture/)

## Why SimpleCapture Exists

IntVue uses complex low-level `MediaCapture` API with custom preview rendering and recording. When camera issues occur, it's difficult to isolate whether they're caused by:

1. **IntVue's architecture** (MVVM, services, state management)
2. **Surface Pro 7 hardware constraints** (exclusive camera control, driver quirks)
3. **MediaCapture/MediaFrameSource mutual exclusivity** (can't preview and record simultaneously on same instance)

**SimpleCapture bypasses all complexity** by using Microsoft's high-level `CameraCaptureUI` class, which handles camera control through Windows' built-in camera app. This provides a clean baseline to test:

- Does the camera hardware respond?
- Are Windows permissions configured correctly?
- Can captured media be played back?

## Architecture Comparison

### SimpleCapture (This Project)

```
User clicks "Capture Video" 
  ↓
CameraCaptureUI.CaptureFileAsync(CameraCaptureUIMode.Video)
  ↓
Windows built-in camera UI launches
  ↓
User records video via Windows dialog
  ↓
Returns StorageFile
  ↓
MediaPlayerElement displays video
```

**Total code:** ~200 lines  
**Complexity:** Minimal  
**Hardware control:** Managed by OS  

### IntVue (Main Project)

```
User clicks "Start Preview"
  ↓
MediaCaptureService.InitializeAsync()
  ↓
MediaCapture instance created
  ↓
MediaFrameSource enumerated and selected
  ↓
MediaFrameReader created
  ↓
Frame callback → Win2D CanvasControl rendering
  ↓
UI preview updates continuously
```

**Total code:** ~2000+ lines  
**Complexity:** Full MVVM, services, state management  
**Hardware control:** Direct access to camera frames  

## Getting Started

### Build and Run SimpleCapture

```powershell
cd SimpleCapture
.\build-and-run.ps1
```

Or manually:

```powershell
cd SimpleCapture
dotnet build -c Debug -p:Platform=x64 --self-contained true
dotnet run -c Debug -p:Platform=x64 --self-contained true
```

### Deployment to Surface Pro 7

The `--self-contained true` flag creates a standalone executable with .NET runtime included, so you can copy and run on the Surface Pro 7 without installing .NET separately.

## Testing Workflow

1. **Verify SimpleCapture works on dev machine**
   - ✓ Builds successfully
   - ✓ Launches without errors
   - ✓ Camera UI appears
   - ✓ Can capture and play video

2. **Deploy SimpleCapture to Surface Pro 7**
   - Copy `bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/` folder
   - Run `SimpleCapture.exe` on the device
   - Test video/photo capture

3. **If SimpleCapture works on Surface Pro 7:**
   - Camera hardware is functional
   - Windows API access is working
   - Issue is likely IntVue-specific

4. **If SimpleCapture fails on Surface Pro 7:**
   - Camera driver issue
   - Windows permissions problem
   - Hardware compatibility issue
   - Needs investigation at OS level (not app-level)

## Project Structure

```
SimpleCapture/
├── SimpleCapture.csproj           Project file (targets same framework/SDK as IntVue)
├── App.xaml                       Application root
├── App.xaml.cs                    Application code-behind
├── MainWindow.xaml                Main UI layout
├── MainWindow.xaml.cs             Camera capture handlers (~150 lines)
├── build-and-run.ps1             Convenience build/run script
├── README.md                      Basic build instructions
├── SETUP_GUIDE.md                 Detailed setup and troubleshooting
└── bin/                           Build output (with self-contained runtime)
```

## Key Features

- **Simple UI:** Two buttons (Capture Video, Capture Photo)
- **Real-time Logging:** Timestamped log of all operations
- **Error Handling:** Clear error messages and exception details
- **Self-Contained:** Includes .NET 10 runtime for standalone deployment
- **Multi-Platform:** Builds for x86, x64, ARM64

## Dependencies

- .NET 10.0 SDK (for development only if deploying source)
- Windows App SDK 1.8.260317003 (self-contained in exe)
- Windows 10.0.26100.0 or later

## Current Build Status

✅ **Builds successfully**  
✅ **No compiler warnings**  
✅ **Self-contained mode working**  
✅ **Ready for testing on Surface Pro 7**

## Next Steps

1. **Build and test on dev machine:**
   ```powershell
   cd SimpleCapture
   .\build-and-run.ps1
   ```

2. **If successful, test on Surface Pro 7:**
   - Copy the built exe to the device
   - Run and test camera functionality

3. **Report findings:**
   - If SimpleCapture works → IntVue issue is architecture-specific
   - If SimpleCapture fails → hardware/driver-level issue

## References

- [Microsoft CameraCaptureUI Documentation](https://learn.microsoft.com/en-us/windows/apps/develop/camera/cameracaptureui)
- [Windows App SDK Samples](https://github.com/microsoft/WindowsAppSDK-Samples)
- [MediaCapture vs CameraCaptureUI](https://learn.microsoft.com/en-us/windows/apps/develop/camera/basic-photo-capture)

## Questions?

See [SimpleCapture/SETUP_GUIDE.md](SimpleCapture/SETUP_GUIDE.md) for detailed troubleshooting and configuration options.
