# IntVue Troubleshooting Guide

## Camera Issues

### No cameras appear in device list

**Symptom:** MainPage shows empty camera list or "No camera devices found!" error

**Causes:**
- Camera disconnected or disabled
- Camera driver outdated or broken
- Windows device permission denied
- Another app holding exclusive access to camera

**Solutions:**
1. Check Windows Settings → Privacy & Security → Camera is enabled
2. Disconnect and reconnect camera device
3. Update camera drivers via Windows Device Manager
4. Close other applications using the camera (Zoom, Teams, OBS, etc.)
5. Restart the application
6. If USB camera, try a different USB port

### Preview shows black screen

**Symptom:** Camera initializes (button enabled) but preview displays nothing

**Causes:**
- Preview source enumeration failed
- MediaFrameReader not initialized correctly
- Camera driver doesn't support selected format
- Insufficient system resources

**Solutions:**
1. Click "Initialize Device" again to retry
2. Select a different camera from the dropdown (if multiple available)
3. Restart the application
4. Close other memory-intensive applications
5. Update GPU drivers

### Camera initialization timeout

**Symptom:** "Calling MediaCapture.InitializeAsync()..." message hangs or takes >5 seconds

**Causes:**
- Slow USB bus or outdated USB driver
- Overloaded system (CPU/memory usage high)
- Incompatible or corrupted camera driver

**Solutions:**
1. Check Task Manager for high CPU/memory usage; close unnecessary apps
2. Update USB drivers (chipset drivers from motherboard vendor)
3. Try a different camera if available
4. Restart the computer

---

## Recording Issues

### Recording won't start

**Symptom:** "Start Recording" button enabled but clicking does nothing or shows error

**Causes:**
- Camera not initialized (click "Initialize Device" first)
- MediaCapture or recording service failed to initialize
- Permission denied (camera access not granted)

**Solutions:**
1. Ensure you clicked "Initialize Device" before recording
2. Verify camera appears in device list
3. Check Windows Settings → Privacy & Security → Camera is enabled for this app
4. Wait for countdown timer to complete before recording starts
5. Restart the application

### Recording file is empty or corrupted

**Symptom:** Recording completes but playback shows nothing or plays corrupted audio/video

**Causes:**
- Recording was interrupted (app suspended, media handle lost)
- Insufficient disk space
- Recording format incompatible with playback

**Solutions:**
1. Ensure sufficient disk space on C: drive (> 1 GB recommended)
2. Don't close the application during recording
3. Don't put computer to sleep while recording
4. Verify media file exists in ApplicationData folder
5. Try a different camera if available

### Disk is too full to record

**Symptom:** "System tracking limit threshold breached" or similar disk error

**Causes:**
- Local storage (C: drive) is full or nearly full
- Recording attempted to ApplicationData folder on full drive
- Insufficient space for OS operations

**Solutions:**
1. Free up disk space by deleting old files/recordings
2. Move files to external drive if needed
3. Increase C: drive space (resize partition or add disk)
4. Close other applications consuming disk space
5. Run Disk Cleanup (Settings → System → Storage)

---

## Playback Issues

### "Play Recording" button remains disabled

**Symptom:** After recording completes, Play button is still grayed out

**Causes:**
- Recording file was not saved successfully
- File was deleted by another process
- ApplicationData folder path not accessible

**Solutions:**
1. Verify recording completed (InfoBar shows success message)
2. Check file exists: `%LOCALAPPDATA%\IntVue\recordings\`
3. Restart the application to refresh file state
4. Try recording again with different filename

### Playback stutters or glitches

**Symptom:** Video plays but has audio/video sync issues, frame drops, or stuttering

**Causes:**
- MediaPlayer resources contention (camera still running)
- Media file format incompatibility
- Disk I/O contention (slow drive, full disk)
- System under heavy load

**Solutions:**
1. Stop camera preview before playing recording (click Preview button to toggle)
2. Close other applications consuming resources
3. Verify recording is WebM format (V8 video codec)
4. Move recording file to faster drive (SSD recommended)

---

## Product Review Feature Issues

### Product Review button not visible

**Symptom:** Button missing from main screen

**Causes:**
- Feature not enabled (need to launch with `--feature:product-review` flag)
- Feature flag not set in initialization

**Solutions:**
1. Launch with CLI flag: `dotnet run -- --feature:product-review`
2. Verify feature flag in code is properly initialized
3. Restart the application

### Questions won't load from directory

**Symptom:** ProductReviewPage shows "0 questions" or error message

**Causes:**
- Directory path is invalid or doesn't exist
- Directory contains no .webm files
- Permission denied on directory access
- Files don't match expected format

**Solutions:**
1. Verify directory path is correct (full path, exists)
2. Ensure directory contains .webm video files
3. Check read permissions on directory (right-click → Properties)
4. Verify files are WebM format (not MP4, MKV, etc.)
5. Check file names are valid (no special characters if possible)

### Countdown timer not working on ProductReviewPage

**Symptom:** Countdown button doesn't trigger timer or timer doesn't work

**Causes:**
- Countdown service not initialized
- Previous countdown still in progress
- Cancellation token not cleared

**Solutions:**
1. Click "Cancel Countdown" if timer appears stuck
2. Navigate back to MainPage and return to ProductReviewPage
3. Verify countdown works on MainPage first
4. Restart the application

---

## General Issues

### Application crashes on startup

**Symptom:** App launches then immediately closes or shows exception

**Causes:**
- Dependency injection configuration issue
- Required service not registered
- Unhandled exception in App.OnLaunched()

**Solutions:**
1. Check debug output for exception message
2. Verify all services registered in App.xaml.cs
3. Ensure .NET 10.0 runtime is installed
4. Clear application cache: delete AppData\Local\Packages\IntVue... folder
5. Reinstall the application

### Tests failing after code change

**Symptom:** Unit tests show failures after modifying services/ViewModels

**Causes:**
- New code breaks test assumptions
- Mock setup is outdated
- Test dependencies not updated

**Solutions:**
1. Read test error message for hint about what failed
2. Run single failing test: `dotnet test --filter "TestMethodName"`
3. Update test mocks to match new code signature
4. Verify service interfaces match implementations
5. Check if tests need async/await modifications

### High CPU usage or memory leaks

**Symptom:** App uses excessive CPU/memory, system becomes sluggish

**Causes:**
- MediaCapture not properly disposed
- Event handlers not unsubscribed
- Infinite loops or unoptimized algorithms
- Preview frames being rendered continuously

**Solutions:**
1. Stop camera preview when not in use
2. Navigate away from ProductReviewPage to clear resource
3. Check Task Manager for abnormal CPU/memory usage
4. Close and restart the application
5. Verify sufficient RAM available (4GB+ recommended)

---

## Performance Issues

### Slow camera initialization (>3 seconds)

**Symptom:** "MediaCapture initialized in 3500ms (target: <3000ms)" warning

**Causes:**
- Camera driver is slow
- USB bus contention
- System under load

**Solutions:**
1. Close other camera-using applications
2. Update camera/USB drivers
3. Check system resource usage (Task Manager)
4. Try built-in camera instead of external USB camera

### Slow question directory load (>2 seconds)

**Symptom:** ProductReviewPage takes long time to load questions

**Causes:**
- Large directory with many files (>1000 questions)
- Slow disk I/O (external/network drive)
- Files being indexed or scanned

**Solutions:**
1. Use local SSD drive instead of network/external
2. Reduce number of question files in directory
3. Check disk usage (Settings → Storage)
4. Close background applications

---

## Before Reporting a Bug

**Collect diagnostic information:**
1. Note exact app version (visible in window title or About)
2. Record steps to reproduce the issue
3. Check Event Viewer for Windows errors (Windows Logs → System)
4. Check application debug output for error messages
5. Note Windows version (Settings → System → About) and camera model

**When creating an issue:**
- Title: Short description of the problem
- Description: Steps to reproduce, expected vs actual behavior
- Environment: Windows version, camera model, any error messages
- Attachments: Screenshots, error logs (if applicable)

---

## Getting Help

**For common issues:** Check this guide first

**For camera-specific issues:** Visit camera manufacturer support or Windows camera troubleshooting

**For recording/codec issues:** Verify WebM format is supported on your system

**For other issues:** Check application debug output (debug console in Visual Studio) for detailed error messages
