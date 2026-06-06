# Step 6: Unit & Integration Tests for MediaCaptureService

## Overview
Comprehensive unit and integration tests for `MediaCaptureService` to verify correct behavior of camera initialization, preview rendering, and recording functionality.

---

## Test Strategy

### Unit Tests (Mocked Dependencies)
These tests isolate `MediaCaptureService` logic by mocking Windows.Media.Capture APIs.

#### Test Class: `MediaCaptureServiceTests`

**1. Initialization Tests**
- `InitializeAsync_FirstCall_InitializesMediaCapture`
  - Verifies that `InitializeAsync()` creates a `MediaCapture` instance
  - Sets `initialized = true` after completion
  - Idempotent: second call returns immediately without re-creating

- `InitializeAsync_NoCameraFound_SetsInitializedTrue`
  - When `DeviceInformation.FindAllAsync()` returns empty list
  - Should log warning and set `initialized = true`
  - Should not throw; caller can check `IsRecording` to determine readiness

- `InitializeAsync_SelectsFrontCamera_IfAvailable`
  - When multiple cameras exist, prefers front-facing camera
  - Falls back to first camera if front not available
  - Verifies correct device is selected in `MediaCaptureInitializationSettings`

**2. Permission Tests**
- `RequestPermissionsAsync_BothAllowed_ReturnsTrue`
  - When both camera and microphone access are allowed
  - Returns `Task.FromResult(true)`

- `RequestPermissionsAsync_CameraBlocked_ReturnsFalse`
  - When camera `DeviceAccessStatus.Denied`
  - Returns `Task.FromResult(false)`

- `RequestPermissionsAsync_ExceptionThrown_ReturnsFalse`
  - When `DeviceAccessInformation` throws exception
  - Gracefully catches and returns `false`

**3. Preview Tests**
- `StartPreviewAsync_CreatesMediaSourceAndMediaPlayer`
  - Calls `InitializeAsync()` if not already done
  - Creates `MediaSource` from `MediaFrameSource`
  - Creates `MediaPlayer` and sets as source
  - Calls `SetMediaPlayer()` on `MediaPlayerElement`

- `StartPreviewAsync_InvalidPreviewHost_ThrowsArgumentException`
  - When `previewHost` is not `MediaPlayerElement`
  - Throws `ArgumentException` with clear message

- `StartPreviewAsync_MediaCaptureNotInitialized_InitializesFirst`
  - If `mediaCapture == null`, calls `InitializeAsync()` before proceeding

- `StartPreviewAsync_NoCameraDevice_ThrowsInvalidOperationException`
  - When `mediaCapture.FrameSources` is empty
  - Throws `InvalidOperationException` with descriptive message

- `StopPreviewAsync_DisposesMediaPlayerAndSource`
  - Disposes `MediaPlayer` and sets to `null`
  - Disposes `MediaSource` and sets to `null`
  - Does not throw even if already stopped

**4. Recording Tests**
- `StartRecordingAsync_CreatesFileAndStartsRecording`
  - Sanitizes filename via `FileHelpers.SanitizeFileName()`
  - Appends `.mp4` if not present
  - Creates file in `ApplicationData.LocalFolder`
  - Creates `MediaEncodingProfile` and `LowLagMediaRecording`
  - Starts recording and returns file path

- `StartRecordingAsync_GeneratesUniqueName_IfFileExists`
  - When file already exists, uses `CreationCollisionOption.GenerateUniqueName`
  - Returned path reflects the unique name

- `StartRecordingAsync_NotInitialized_InitializesFirst`
  - If `mediaCapture == null`, calls `InitializeAsync()` before proceeding

- `StopRecordingAsync_StopsAndFinishesRecording`
  - Calls `lowLagRecording.StopAsync()` and `FinishAsync()`
  - Sets `lowLagRecording = null`
  - Does not throw if `lowLagRecording == null`

**5. Resource Lifecycle Tests**
- `IsRecording_ReturnsTrueWhenLowLagRecordingActive`
  - When `lowLagRecording != null`, returns `true`
  - When `lowLagRecording == null`, returns `false`

- `DisposeAsync_DisposesAllResources`
  - Disposes `MediaPlayer`, `MediaSource`, `LowLagMediaRecording`, `MediaCapture`
  - Sets all fields to `null`
  - Sets `initialized = false`
  - Does not throw even if some fields are `null`

- `Dispose_SynchronousCleanup`
  - Implements synchronous cleanup via `Dispose()`
  - Uses `.AsTask().GetAwaiter().GetResult()` for async calls
  - Same resource cleanup as `DisposeAsync()`

---

### Integration Tests (Real Dependencies)
These tests use real Windows.Media.Capture APIs to verify end-to-end behavior.

#### Test Class: `MediaCaptureServiceIntegrationTests`

**1. Device Enumeration**
- `InitializeAsync_FindsCameraDevice_IfAvailable`
  - Enumerates real camera devices via `DeviceInformation.FindAllAsync()`
  - Verifies at least one device is found (skip test if no camera)
  - Confirms initialization succeeds

**2. Lifecycle Management**
- `StartPreviewAsync_Then_StopPreviewAsync_NoExceptions`
  - Start preview on real camera (if available; skip otherwise)
  - Stop preview without throwing
  - Verify resources are cleaned up

- `InitializeAndDisposeAsync_Multiple_Cycles`
  - Initialize → Dispose → Initialize → Dispose (3 cycles)
  - Verify no resource leaks or corruption on re-initialization

**3. Recording Concurrency**
- `StartPreviewAsync_Then_StartRecordingAsync_BothActive`
  - Start preview
  - Start recording while preview is active
  - Verify both are active and don't interfere
  - Stop recording while preview continues
  - Verify preview continues without artifacts

---

## Test Infrastructure

### Mocking Strategy
- Mock `DeviceInformation` and device enumeration via constructor or factory pattern (if needed)
- Mock `MediaCapture.FrameSources` dictionary with test frame source
- Mock `MediaPlayerElement.SetMediaPlayer()` to avoid WinUI UI context requirements
- Use `Mock<T>` from Moq for all external dependencies

### Test Fixtures & Helpers
- **MediaCaptureServiceTestFixture**: Provides clean `MediaCaptureService` instance and mock setup
- **MockFrameSourceFactory**: Creates mock `MediaFrameSource` for testing preview binding
- **TestHelpers**: Utility methods for creating mock cameras, permissions, etc.

### Isolation
- Each test creates a fresh `MediaCaptureService` instance
- No shared state between tests
- Use `[TestInitialize]` and `[TestCleanup]` for setup/teardown
- Dispose service after each test to avoid resource leaks

---

## Implementation Plan

1. **Add Moq to test project** — Update `IntVue.Tests.csproj` to include Moq NuGet package
2. **Add project reference** — Reference `IntVue.csproj` from test project
3. **Create test fixtures** — Base classes and helpers for mocking Windows.Media APIs
4. **Implement unit tests** — `MediaCaptureServiceTests` with mocked dependencies (≥80% coverage)
5. **Implement integration tests** — `MediaCaptureServiceIntegrationTests` with real API calls (skip if no camera)
6. **Run and validate** — Execute `dotnet test` and verify all tests pass

---

## Success Criteria

- [ ] All unit tests pass with ≥80% code coverage of `MediaCaptureService`
- [ ] Integration tests pass (or skip gracefully if no camera device)
- [ ] No resource leaks detected during dispose/cleanup cycles
- [ ] All test names follow `MethodName_Scenario_ExpectedResult` pattern
- [ ] Tests are independent (no shared state or execution order dependencies)

---

## Notes

- **WinUI 3 Context:** Integration tests may need to run in a test context that initializes WinUI (e.g., via a test window or dispatcher).
- **Camera Device:** Integration tests should gracefully skip if no camera is available rather than failing.
- **Async/Await:** All async tests must use `async Task` and `await` async methods; never use `.Result` or `.Wait()` on the UI thread.
- **Mock Disposal:** Ensure mocks and real resources are properly disposed after each test to prevent interference.

---

**Status:** Ready for implementation  
**Last Updated:** 2026-06-04
