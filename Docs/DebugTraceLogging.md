# Debug Trace Logging Reference

**Available in:** Debug builds only (disabled in Release builds)
**Output destination:** `System.Diagnostics.Debug.WriteLine()` (visible in debugger console)
**Prefix:** `[IntVue.Debug]` for easy filtering

---

## Overview

Comprehensive trace logging has been added to diagnose camera preview initialization issues. Logs track:

- Device enumeration (finding cameras)
- MediaCapture initialization
- Preview binding and rendering
- Recording lifecycle
- User consent flow
- Permission requests

## Building and Running with Debug Logs

### Debug Build (includes trace logging)
```powershell
dotnet build -c Debug -p:Platform=x64
```

### Run with Debugger (see logs)
```powershell
# In JetBrains Rider:
# Run → Debug (Shift+F9)
# Open: Run → Debugger Tabs → Console (or press Alt+0)
# Launch app and interact with preview

# Via command line:
dotnet run -c Debug -p:Platform=x64
# Logs output to console stdout
```

### Release Build (no trace logging)
```powershell
dotnet build -c Release -p:Platform=x64
# Logging calls compiled out (#if DEBUG directives)
```

---

## Sample Debug Output

### Full User Session: Camera Access & Preview

```
[IntVue.Debug] MainPage.OnPageLoaded: Page loaded, initializing UI...
[IntVue.Debug] MainPage.OnPageLoaded: Checking for saved consent...
[IntVue.Debug] MainPage.OnPageLoaded: No saved consent, showing consent dialog...
[IntVue.Debug] MainPage.OnPageLoaded: Consent dialog result: True
[IntVue.Debug] InterviewViewModel.ConsentGiv
[IntVue.Debug] MainPage.OnPageLoaded: Page initialization complete.

--- User clicks "Start Preview" button ---

[IntVue.Debug] MainPage.BtnStartPreview_Click: Start Preview button clicked.
[IntVue.Debug] MainPage.BtnStartPreview_Click: ConsentGiven=True, PreviewControl type=MediaPlayerElement
[IntVue.Debug] MainPage.BtnStartPreview_ClicviewAsync()...
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Starting preview command...
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: User consent confirmed. Requesting permissions...
[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Checking camera and microphone permissions...
[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Camera permission=Allowed, Microphone permission=Allowed
[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Both allowed=True
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Permissions granted: True
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Initializing media capture...
[IntVue.Debug] MediaCaptureService.InitializeAsync: Starting initialization...
[IntVue.Debug] MediaCaptureService.InitializeAsync: MediaCapture instance created.
[IntVue.Debug] MediaCaptureService.Initializture devices...
[IntVue.Debug] MediaCaptureService.InitializeAsync: Found 1 video device(s).
[IntVue.Debug] MediaCaptureService.InitializeAsync: Device[0]: Name='HP HD Camera', ID='\\?\usb#vid_0408&pid_1000#...',
 EnclosureLocation=Front
[IntVue.Debug] MediaCaptureService.Initializ - 'HP HD Camera'
[IntVue.Debug] MediaCaptureService.InitializeAsync: Selected device ID: '\\?\usb#vid_0408&pid_1000#...'
[IntVue.Debug] MediaCaptureService.InitializeAsync: Calling MediaCapture.InitializeAsync()...
[IntVue.Debug] MediaCaptureService.InitializeAsync: MediaCapture.InitializeAsync() completed successfully.
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Starting preview on media service...
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Starting preview...
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: MediaCapture is initialized.
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Preview host is MediaPlayerElement.
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: FrameSources count: 1
[IntVue.Debug] MediaCaptureService.StartPreved - MediaFrameSource
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Creating MediaSource from frame source...
[IntVue.Debug] MediaCaptureService.StartPrev.
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Creating MediaPlayer...
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Setting MediaPlayer on MediaPlayerElement...
[IntVue.Debug] MediaCaptureService.StartPreviewAsync: Preview started successfully. MediaPlayer is now rendering.
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Preview started successfully.
[IntVue.Debug] MainPage.BtnStartPreview_Click: StartPreviewAsync returned True
[IntVue.Debug] MainPage.BtnStartPreview_Clicly.

--- Camera preview is now visible on screen

--- User clicks "Start Recording" button ---

[IntVue.Debug] MainPage.BtnStartRecording_Click: Start Recording button clicked.
[IntVue.Debug] MainPage.BtnStartRecording_Click: IsPreviewing=True
[IntVue.Debug] MainPage.BtnStartRecording_Click: Calling viewModel.StartRecordingAsync()...
[IntVue.Debug] InterviewViewModel.StartRecor with base name 'recording'...
[IntVue.Debug] InterviewViewModel.StartRecordingAsync: Preview is active. Starting recording on media service...
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Starting recording with base name 'recording'...
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: MediaCapture is initialized.
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Creating recording file 'recording.mp4'...
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Recording file created at 'C:\Users\...\recording.mp4'
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Preparing low-lag recording...
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Low-lag recording prepared. Starting recording...
[IntVue.Debug] MediaCaptureService.StartRecordingAsync: Recording started successfully. File: C:\Users\...\recording.mp
4
[IntVue.Debug] InterviewViewModel.StartRecordingAsync: Recording started. File path: C:\Users\...\recording.mp4
[IntVue.Debug] MainPage.BtnStartRecording_Clsfully.

--- Recording is now in progress ---
```

---

## Trace Points by Component

### MediaCaptureService (Services/MediaCaptu

#### InitializeAsync()
- Device enumeration start
- Device count found
- Each device name, ID, and panel location
- Selected camera (front or first)
- Selected device ID
- MediaCapture initialization start/end
- Errors during initialization

#### RequestPermissionsAsync()
- Permission check start
- Camera permission status
- Microphone permission status
- Overall result
- Errors during check

#### StartPreviewAsync()
- Preview start
- MediaCapture null check
- Preview host type validation
- Frame sources count
- Frame source obtained
- MediaSource creation
- MediaPlayer creation
- Setting MediaPlayer on element
- Success confirmation
- Errors with stack trace

#### StopPreviewAsync()
- Preview stop
- MediaPlayer disposal
- MediaSource disposal
- Success confirmation
- Errors

#### StartRecordingAsync()
- Recording start with filename
- MediaCapture null check
- Recording file creation
- File path
- Low-lag recording preparation
- Recording start
- Success with file path
- Errors

#### StopRecordingAsync()
- Recording stop
- Async stop/finish calls
- Success confirmation

### InterviewViewModel (ViewModels/InterviewViewModel.cs)

#### ConsentGiven property setter
- Consent change and new value

#### StartPreviewAsync()
- Command start
- Consent check result
- Permission request
- Permissions granted/denied result
- MediaCapture initialization
- Preview start on service
- Success confirmation

#### StopPreviewAsync()
- Stop preview command
- Success confirmation

#### StartRecordingAsync()
- Recording command with filename
- Preview active check
- Recording service call
- File path result
- Success confirmation

#### StopRecordingAsync()
- Stop recording command
- Success confirmation

### MainPage (Views/MainPage.xaml.cs)

#### OnPageLoaded()
- Page loaded
- Consent saved state check
- Consent dialog display
- Dialog result
- Consent set
- Page initialization complete

#### BtnStartPreview_Click()
- Button click
- Consent and preview control checks
- ViewModel call
- Result handling
- Error types and messages

#### BtnStartRecording_Click()
- Button click
- Preview active check
- ViewModel call
- Error handling

#### BtnStopRecording_Click()
- Button click
- Recording status
- ViewModel call
- Error handling

---

## Filtering Debug Output

### In Visual Studio/Rider Debugger

1. Open **Debug Console** (Run menu → Debugger Tabs → Console)
2. Use the **Search** field to filter:
   - `[IntVue.Debug]` - All IntVue logs
   - `StartPreviewAsync` - Preview flow only
   - `InitializeAsync` - Initialization flow only
   - `RequestPermissionsAsync` - Permission flow only

### Via Command Line

When running with `dotnet run`, logs output to console. Pipe to grep:

```powershell
# Run and filter for preview logs only
dotnet run -c Debug -p:Platform=x64 2>&1 | f

# Run and filter for initialization logs only
dotnet run -c Debug -p:Platform=x64 2>&1 | findstr /I "InitializeAsync"

# Run and see all IntVue logs
dotnet run -c Debug -p:Platform=x64 2>&1 | findstr /I "IntVue.Debug"
```

---

## Troubleshooting with Logs

### Preview Not Showing

Check logs for:
1. **Consent check:** `ConsentGiven: Changed to True`
2. **Permissions:** `Both allowed=True`
3. **Device enumeration:** `Found X video detected
4. **Frame sources:** `FrameSources count: 1` - if 0, MediaCapture has no video
5. **MediaPlayer binding:** `Setting MediaPlayer on MediaPlayerElement...` followed by success

### Camera Not Found

Look for:
```
[IntVue.Debug] MediaCaptureService.InitializeAsync: Found 0 video device(s).
[IntVue.Debug] MediaCaptureService.InitializeAsync: WARNING - No camera device found. Preview mode disabled.
```

**Solution:** Ensure camera hardware is connected and drivers are installed. On Desktop PC without camera, this is expe
cted (app gracefully handles it).

### Permissions Denied

Look for:
```
[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Camera permission=Denied, Microphone permission=Denied
[IntVue.Debug] MediaCaptureService.RequestPermissionsAsync: Both allowed=False
```

**Solution:** Grant camera and microphone permissions in device settings.

### MediaCapture Initialization Failed

Look for:
```
[IntVue.Debug] MediaCaptureService.InitializeAsync: ERROR - ExceptionType: error message
```

**Solution:** Check the specific exception message for the root cause. Common causes:
- Device already in use by another app
- Missing permissions
- Driver issues

### Preview Started but No Video Visible

Check that all these logged successfully:
1. `MediaCapture.InitializeAsync() completed successfully`
2. `Frame source obtained`
3. `MediaSource created`
4. `Setting MediaPlayer on MediaPlayerElemen
5. `Preview started successfully. MediaPlaye

If all logged but no video:
- UI control may not be visible (check XAML layout)
- MediaPlayer may not be connected to UI (ve
- Preview may be disabled due to no camera device

---

## Release Build

In Release builds, all `#if DEBUG` blocks are compiled out:

```csharp
#if DEBUG
    Debug.WriteLine("[IntVue.Debug] ..."); // Not included in Release
#endif
```

## Further Reading

- **Debugging:** See `Docs/RemoteDebugSetup.md` for remote debugging setup
- **System.Diagnostics.Debug:** https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.debug
- **Rider Debugging:** https://www.jetbrains.com/help/rider/Debugging-Code.html

