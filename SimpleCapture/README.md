# SimpleCapture - Minimal WinUI 3 Camera Capture Test

A minimal, focused WinUI 3 application using Microsoft's built-in `CameraCaptureUI` for basic video and photo capture testing.

## Purpose

This sub-project is designed to:
- Test camera capture functionality in isolation from the complex IntVue architecture
- Validate that CameraCaptureUI works on the target hardware (Surface Pro 7)
- Provide a reference implementation of CameraCaptureUI from Microsoft documentation
- Diagnose whether camera issues are Surface-specific or IntVue-specific

## Building

### Prerequisites
- .NET 10.0 SDK or later
- Windows App SDK 1.8.0+
- Windows 10.0.26100.0 or later (matches IntVue target)

### Understanding Build vs Publish

**`dotnet build`** — Compiles source to DLL (still requires .NET runtime to run)  
**`dotnet publish`** — Creates self-contained executable with all runtime files included (can run standalone)

### Build Command

Compile the project:

```powershell
dotnet build -c Debug -p:Platform=x64
```

### Publish Command (For Self-Contained Executable)

Create a standalone executable with all .NET runtime files included:

```powershell
dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64
```

This creates:
- **Output:** `bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/publish/`
- **Executable:** `SimpleCapture.exe`
- **Includes:** All .NET runtime files, WinUI libraries, and dependencies

### Running the Published Executable

```powershell
# Run the standalone exe directly (no .NET runtime needed on target machine)
.\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish\SimpleCapture.exe
```

### Using the Convenience Script (Recommended)

```powershell
# Publish and run automatically
.\build-and-run.ps1

# Or publish only (for deployment)
.\build-and-run.ps1 -PublishOnly
```

The script outputs the full path to the published executable, which you can copy to Surface Pro 7 and run directly.

## What It Does

1. **Capture Video**
   - Launches Windows' built-in camera capture UI
   - Records video in standard definition (SD)
   - Allows trimming of captured video
   - Plays back the captured video in a MediaPlayerElement

2. **Capture Photo**
   - Launches Windows' built-in camera capture UI
   - Captures photo in JPEG format
   - Allows cropping of captured photo
   - Displays the captured photo in an Image control

3. **Logging**
   - Timestamps all operations
   - Shows success/failure feedback
   - Displays exception details for debugging

## Project Structure

```
SimpleCapture/
  SimpleCapture.csproj           Project file (self-contained, multi-platform)
  App.xaml                       Application root
  App.xaml.cs                    Application code-behind
  MainWindow.xaml                Main UI layout
  MainWindow.xaml.cs             Main UI logic (capture handlers)
  README.md                       This file
```

## Troubleshooting

### Camera Not Found
- Verify camera permissions in Windows Settings
- Check if another app is using the camera
- Restart the application

### CameraCaptureUI Dialog Doesn't Appear
- Ensure you're running on Windows 10.0.26100.0 or later
- Check Event Viewer for system errors
- Verify AppWindow.Id is correctly passed

### Video Playback Fails
- Check that the captured file is not corrupted
- Verify H.264 codec support
- Check memory/disk space

## Differences from IntVue

| Aspect | SimpleCapture | IntVue |
|--------|--------------|--------|
| **Capture Method** | CameraCaptureUI (built-in OS dialog) | MediaCapture (custom UI) |
| **Complexity** | Minimal (~200 lines) | Complex (full MVVM, services) |
| **Hardware Control** | Simplified by OS | Direct frame-based rendering |
| **Use Case** | Test capture mechanism | Full interview app with preview |
| **Frame Access** | No access to frames | Full MediaFrameReader access |

## References

- [Microsoft CameraCaptureUI Documentation](https://learn.microsoft.com/en-us/windows/apps/develop/camera/cameracaptureui)
- [Windows App SDK Camera Support](https://learn.microsoft.com/en-us/windows/windows-app-sdk/windows-app-sdk-stable-release)
- [MediaCapture vs CameraCaptureUI](https://learn.microsoft.com/en-us/windows/apps/develop/camera/basic-photo-capture)

## Notes

- This app uses **self-contained** deployment mode (includes runtime)
- Target platforms: x86, x64, ARM64
- Requires camera and microphone permissions
