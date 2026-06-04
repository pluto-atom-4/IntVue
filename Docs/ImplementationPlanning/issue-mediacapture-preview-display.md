# GitHub Issue: Enhance MediaCapture Preview Display in UI

## Title
**Support Real-Time MediaCapture Preview Rendering in WinUI 3 UI**

---

## Body

### Summary
The current implementation in `@Views/MainPage.xaml` uses a `MediaPlayerElement` to host camera preview, but **WinUI 3's MediaPlayerElement does not provide a direct, built-in API to display live MediaCapture output**. The preview stream is correctly initialized and runs internally with robust resource management and error handling, but the camera feed cannot be visually rendered to the user interface.

### Current State
- ✅ `MediaCapture` instance is properly initialized with audio and video settings
- ✅ All resource lifecycle management (initialization, suspension, cleanup) is in place
- ✅ Error handling and recovery mechanisms are implemented
- ✅ User consent and privacy UI indicators are present
- ❌ **Preview is not visible to the user** — the camera stream runs internally but does not display on screen

### Challenge
`MediaPlayerElement` is designed for playback of media files (via `MediaSource`), not for direct rendering of capture streams. Three viable technical approaches exist, each with trade-offs:

#### Option 1: Frame-Based Rendering (MediaFrameReader + Direct3D)
- **How it works:** Use `MediaFrameReader` to pull video frames from the capture device and render them via Direct3D/DXGI.
- **Pros:** Gives fine-grained control over frame data; enables advanced processing (filters, effects, overlays).
- **Cons:** Complex API surface; requires Direct3D expertise; lower-level P/Invoke; higher CPU overhead.
- **Best for:** Advanced scenarios (real-time effects, custom processing, or custom rendering targets).

#### Option 2: Win2D CanvasControl Integration
- **How it works:** Pipe MediaFrameReader frames into a Win2D `CanvasControl` for GPU-accelerated 2D rendering.
- **Pros:** Simpler API than raw Direct3D; GPU-accelerated rendering; good for overlays and basic effects.
- **Cons:** Requires Win2D NuGet package; still frame-based (not a zero-copy path).
- **Best for:** Interactive camera overlays, simple effects, or cross-platform .NET graphics.

#### Option 3: Direct3D Surface Rendering (MediaStreamSource)
- **How it works:** Use `MediaStreamSource` with a Direct3D backing surface to allow `MediaPlayerElement` to consume the live stream.
- **Pros:** Keeps the `MediaPlayerElement` in the stack; potentially lower latency if DXGI surfaces are shared.
- **Cons:** Most complex to set up; requires both Direct3D and MediaStreamSource expertise; fragile surface lifetime management.
- **Best for:** Scenarios where MediaPlayerElement transport controls or playback semantics are desired.

---

## Recommended Approach

**Start with Option 2 (Win2D CanvasControl)** as the path forward:
- Provides an excellent balance between simplicity and capability.
- Integrates well with the WinUI ecosystem.
- Enables future feature expansion (overlays, effects, annotation).
- Low barrier to entry compared to raw Direct3D.

---

## Proposed Implementation Plan

1. **Add Win2D NuGet Package**
   - Add `Win2D.uwp` or `Win2D.WinUI3` (confirm latest version) to the project file.
   - Verify compatibility with current `Microsoft.WindowsAppSDK` version.

2. **Replace MediaPlayerElement Preview with CanvasControl**
   - Substitute the `MediaPlayerElement` in `MainPage.xaml` with a `CanvasControl`.
   - Set appropriate layout properties (stretch behavior, aspect ratio preservation).

3. **Implement MediaFrameReader Pipeline**
   - Modify the `MediaCaptureService` to:
     - Create a `MediaFrameReader` from the initialized `MediaCapture` device.
     - Set up frame-arrival callback to pull video frames asynchronously.
     - Convert frame data into a format Win2D can consume (e.g., `CanvasBitmap` or raw pixel buffer).

4. **Render Frames on CanvasControl**
   - Hook the `Draw` event of `CanvasControl` to render incoming frames.
   - Implement frame buffering/synchronization to avoid tearing or dropped frames.
   - Add optional overlays (recording indicator, timestamp, debug info).

5. **Update Resource Cleanup**
   - Ensure `MediaFrameReader` is properly disposed on app suspend/navigation.
   - Update the existing disposal pattern in `MediaCaptureService` to include frame reader cleanup.

6. **Add Unit & Integration Tests**
   - Test frame reader initialization and teardown.
   - Verify correct frame rendering at different resolutions.
   - Test app suspension/resumption with active capture.

7. **Performance & Accessibility**
   - Measure CPU/GPU usage during preview.
   - Verify keyboard navigation of preview controls works end-to-end.
   - Ensure screen reader can announce recording status.

---

## Implementation Progress

| Step | Status | Notes |
|------|--------|-------|
| 1. Add Win2D NuGet | ⚙️ In Progress | Pending package version verification |
| 2. Replace MediaPlayerElement | ⚙️ In Progress | PreviewControl element creation underway |
| 3. MediaFrameReader Pipeline | 🔲 Not Started | Awaiting CanvasControl integration |
| 4. Render Frames | 🔲 Not Started | Blocked on step 3 |
| 5. Resource Cleanup | 🔲 Not Started | Follows frame rendering |
| 6. Unit & Integration Tests | 🔲 Not Started | Test suite design ready |
| 7. Performance & Accessibility | 🔲 Not Started | Final validation pass |

**Current Focus:** Steps 1–2 (Win2D integration and CanvasControl substitution)  
**Last Updated:** 2026-06-04

---

## Success Criteria

- [ ] Camera preview is rendered live on screen with <100ms latency.
- [ ] Frame rate is stable (≥24 fps on target devices).
- [ ] CPU usage is reasonable (~10–20% on a modern machine during preview).
- [ ] All resource lifecycle tests pass (suspend, resume, cleanup).
- [ ] Recording/consent UI remains visible and functional during preview.
- [ ] Code is well-documented and follows WinUI/SOLID design principles.

---

## Acceptance Tests

| Test Case | Expected Outcome |
|-----------|------------------|
| Click "Start Preview" | Live camera feed appears in the preview area immediately |
| Preview running for 5+ min | Frame rate remains stable; no memory leaks detected |
| Minimize and restore app | Preview resumes without artifacts or crashes |
| Click "Start Recording" with active preview | Recording begins while preview continues; no UI freeze |
| Click "Stop Recording" | Recording stops; preview remains visible |
| App suspend/resume cycle | All resources are cleaned up and re-initialized correctly |
| Unplug camera | App shows graceful error message; no crash |

---

## Dependencies

- **NuGet:** Win2D (`Win2D.uwp` or `Win2D.WinUI3` — verify latest stable version)
- **Existing:** `Windows.Media.Capture`, `Windows.Media.MediaProperties` (already in use)
- **Reference:** [WinAppSDK MediaCapture Docs](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/)

---

## Notes

- **Privacy & Security:** Ensure preview runs only after explicit user consent (already in place; verify no leaks).
- **Localization:** Recording indicator and status messages should use resource strings (`.resw` files).
- **Accessibility:** Keyboard focus and screen reader support must be verified for the new `CanvasControl`.
- **Architecture:** This change enhances the existing `MediaCaptureService` without breaking the current MVVM structure.

---

## Related Issues / PRs
- (Link to any PRs or related issues when created)

---

## Labels
`enhancement`, `media`, `camera`, `winui`, `platform-integration`

---

## Assignees
(Assign to appropriate team member(s))

---

**Created:** 2026-06-04  
**Status:** In Progress  
**Priority:** High (core feature for MVP)  
**Branch:** feat/issue-23-win2d-canvas-control
