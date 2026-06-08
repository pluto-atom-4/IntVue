# Development Notes: Issues #34, #35, and Recording/Preview Control Flow

**Date:** 2026-06-07  
**Status:** Investigation in progress  
**Priority:** High - blocking preview/recording functionality

---

## Problem Summary

Three interconnected issues prevent preview and recording from working together on Surface Pro 7:

1. **Stop Preview button not rendering on UI** - Despite PropertyChanged firing and visibility being set correctly in ViewModel
2. **App crashes when starting recording** - Exception during MediaCapture reinitialization
3. **Preview and Recording mutual exclusivity** - Windows Media Capture hardware limitation on Surface camera

---

## Issue #1: Stop Preview Button Not Visible

### Symptoms
- Button property `StopPreviewButtonVisibility` changes from `Collapsed` → `Visible` (confirmed in traces)
- PropertyChanged event fires (confirmed: `MainPage: Detected StopPreviewButtonVisibility change to Visible`)
- PropertyChanged subscription code executes and sets `BtnStopPreview.Visibility`
- **BUT:** Button does not appear on UI at all

### What We Tried
1. **XAML binding approach** (`Visibility="{x:Bind viewModel.StopPreviewButtonVisibility, Mode=OneWay}"`)
   - Made `viewModel` field a public property (required for x:Bind)
   - Result: Binding didn't work; button never showed
   
2. **Code-behind PropertyChanged subscription fallback**
   - Added subscription in MainPage constructor to set button visibility on PropertyChanged
   - Result: Subscription fires, visibility is set, but UI doesn't update
   
3. **IsEnabled binding approach** (current)
   - Changed to bind `IsEnabled="{x:Bind viewModel.IsPreviewing, Mode=OneWay}"`
   - Button should always be visible, just disabled when no preview
   - Added explicit `Visibility="Visible"` to XAML
   - Result: Button still doesn't appear

### Root Cause Hypothesis
- **UI Thread Issue**: PropertyChanged subscription may be running on background thread; setting button.Visibility from async context doesn't trigger UI update
- **Binding Path Issue**: x:Bind might not properly resolve to public property despite correct syntax
- **Layout Issue**: StackPanel may have layout caching or child visibility rules that override manual visibility changes
- **Threading Race Condition**: Visibility is set before UI is fully initialized, or DispatcherQueue marshaling fails silently

### Traces That Work
- `InterviewViewModel.StartPreviewAsync: Preview started successfully.` - Preview initializes correctly
- `InterviewViewModel.StopPreviewButtonVisibility: Changed to Visible` - ViewModel updates correctly
- `MainPage: Detected StopPreviewButtonVisibility change to Visible` - Event subscription fires

---

## Issue #2: App Crashes During Recording Initialization

### Symptoms
- Traces stop abruptly at `[IntVue.Debug] MediaCaptureService.InitializeAsync: Enumerating video capture devices...`
- No error dialog shown
- App terminates without unhandled exception message
- Happens when `StartRecordingAsync()` tries to reinitialize MediaCapture after preview stops

### What We Tried
1. **Disposed MediaCapture after preview stops**
   - Added `mediaCapture?.Dispose()` in `StopPreviewAsync()`
   - Set `this.initialized = false` to allow reinitialization
   - Result: Allows MediaCapture to be recreated for recording, but crashes during reinitialization
   
2. **Added try-catch blocks around initialization**
   - Wrapped entire `InitializeAsync()` in try-catch
   - Inner try-catch around `mediaCapture.InitializeAsync(settings)`
   - Outer try-catch around device enumeration
   - Result: Exception still not caught; app still crashes
   
3. **Improved error handling in recording flow**
   - Added COMException and generic Exception handlers in `BtnStartRecording_Click`
   - Added error dialogs for user feedback
   - Result: Error dialogs never appear; app crashes before reaching them

### Root Cause Hypothesis
- **Device Enumeration Exception**: `DeviceInformation.FindAllAsync()` throws exception that escapes the try-catch (possibly on unhandled exception path)
- **Unhandled Task Exception**: Exception in async task not properly awaited, resulting in unhandled exception on thread pool
- **Windows API Constraint**: After disposing MediaCapture, device may not be immediately available for reinitialization
- **Surface Camera Hardware Lock**: Surface integrated camera might require longer recovery time between preview/recording mode switches

### Critical Trace Points
- Device enumeration completes successfully on first initialization (15 FPS color source selected)
- Exact point of crash: After `Enumerating video capture devices...` on second initialization (during recording start)

---

## Issue #3: Preview and Recording Mutual Exclusivity

### Root Cause (Confirmed via Microsoft Documentation)
Windows Media Capture architecture on Surface Pro 7:
- **MediaFrameSource** (used for preview rendering) creates exclusive hardware lock
- **LowLagMediaRecording** cannot start while hardware is locked by MediaFrameSource
- Solution: Stop preview (dispose MediaFrameSource → dispose MediaCapture) before recording

### What We Implemented
- Removed `if (!this.IsPreviewing)` check that was blocking recording
- Added preview-stop logic in `BtnStartRecording_Click` before `StartRecordingAsync()`
- Modified `StopPreviewAsync()` to dispose entire MediaCapture to release hardware
- Result: Recording can now theoretically start without preview, but crashes during reinitialization

---

## Current Code State

### Files Modified
- **MainPage.xaml**
  - Changed Stop Preview button binding from `Visibility="{x:Bind ...}"` to `IsEnabled="{x:Bind viewModel.IsPreviewing, Mode=OneWay}"`
  - Added explicit `Visibility="Visible"` attribute
  
- **MainPage.xaml.cs**
  - Made `viewModel` a public property (not private field) for x:Bind access
  - Added PropertyChanged subscription fallback to set button visibility
  - Added error handling with COMException-specific catch
  
- **InterviewViewModel.cs**
  - Removed `if (!this.IsPreviewing)` check from `StartRecordingAsync()` to allow independent recording
  - Added `StopPreviewButtonVisibility` property for UI binding
  
- **MediaCaptureService.cs**
  - Added `previewFrameSource` field to track frame source
  - Modified `StopPreviewAsync()` to dispose entire MediaCapture (not just MediaPlayer/MediaSource)
  - Set `this.initialized = false` after disposal to allow reinitialization
  - Added nested try-catch blocks in `InitializeAsync()` for error handling
  - Changed error handling to not rethrow (graceful degradation)

### Known Working State
- Preview initialization succeeds on first call
- Camera device enumeration works (selects 15 FPS Color source correctly)
- MediaPlayer binding to MediaFrameSource works
- Play() call enables video playback
- Preview stops and MediaCapture disposes without errors
- All traces execute in correct order up to reinitialization crash point

---

## Next Steps to Investigate

1. **Stop Preview Button**
   - [ ] Verify StackPanel children layout behavior with collapsed/visible toggle
   - [ ] Test if button needs to be removed/readded to StackPanel instead of toggling visibility
   - [ ] Try setting button visibility directly in code-behind OnPageLoaded instead of via binding
   - [ ] Check if DispatcherQueue.TryEnqueue is properly synchronizing UI updates

2. **Recording Initialization Crash**
   - [ ] Add try-catch specifically around `DeviceInformation.FindAllAsync()` call
   - [ ] Add delay/sleep between disposing MediaCapture and reinitializing (device recovery time)
   - [ ] Test if creating new MediaCapture instance from thread pool vs UI thread matters
   - [ ] Check if device needs time to reset after hardware lock release
   - [ ] Verify exception is being thrown and not swallowed by WinRT runtime

3. **Alternative Architecture**
   - [ ] Consider creating separate MediaCapture instances for preview vs recording (instead of reusing one)
   - [ ] Or: Redesign to not stop preview when recording (find alternative recording approach)
   - [ ] Or: Pre-allocate both preview and recording resources at init, switch between them

---

## References

- **WinUI 3 Camera Quickstart**: https://learn.microsoft.com/en-us/windows/apps/develop/camera/camera-quickstart-winui3
- **MediaCapture Class**: https://learn.microsoft.com/en-us/uwp/api/windows.media.capture.mediacapture
- **LowLagMediaRecording**: https://learn.microsoft.com/en-us/uwp/api/windows.media.capture.lowlagmediarecording
- **MediaFrameSource**: https://learn.microsoft.com/en-us/uwp/api/windows.media.capture.frames.mediaframesource

---

## Trace Log Excerpt (Stop Preview Button Issue)

```
[IntVue.Debug] InterviewViewModel.StartPreviewAsync: Preview started successfully.
[IntVue.Debug] InterviewViewModel.StopPreviewButtonVisibility: Changed to Visible
[IntVue.Debug] MainPage: Detected StopPreviewButtonVisibility change to Visible
[IntVue.Debug] MainPage.BtnStartPreview_Click: Exception - COMException:
```

**Analysis**: StopPreviewButtonVisibility property is set to Visible, PropertyChanged fires, fallback subscription detects the change - BUT button never renders on screen.

---

## Trace Log Excerpt (Recording Crash)

```
[IntVue.Debug] MediaCaptureService.InitializeAsync: Starting initialization...
[IntVue.Debug] MediaCaptureService.InitializeAsync: MediaCapture instance created.
[IntVue.Debug] MediaCaptureService.InitializeAsync: Enumerating video capture devices...
[APP CRASH - NO EXCEPTION MESSAGE]
```

**Analysis**: MediaCapture instance is created successfully, but exception occurs during or after `DeviceInformation.FindAllAsync()` on second initialization attempt. Exception escapes try-catch and terminates application.

---

## Developer Notes

- Surface Pro 7 has integrated camera with ID prefix containing "DISPLAY" - successfully detected
- Frame sources include Color (15 FPS), Image (30 FPS), Audio, and duplicate Color source
- Two ColorFrame sources suggest complex camera configuration on Surface hardware
- First preview initialization works perfectly; second initialization (for recording) fails
- Issue is specific to reinitializing after previous MediaCapture was disposed

**Potential blocker**: Windows API may not support rapid create/dispose/recreate cycle of MediaCapture on Surface integrated camera hardware.
