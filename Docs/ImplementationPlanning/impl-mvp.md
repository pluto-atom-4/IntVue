# Implementation Plan — MVP: Video Interview Practice (Security-enhanced)

## Objective

- Deliver a minimal WinUI 3 MVP that previews the front camera, records a timed
  response to a question, and allows immediate review. Built with DI,
  testable services, accessible UI, and security-first defaults.

## Scope (MVP)

- Camera preview (front camera preferred)
- Start/Stop recording to a local file
- Countdown before recording and optional think-time
- Immediate playback in-app after recording
- Unit tests for ViewModel behaviour and service abstraction
- Manifest permissions and developer-run instructions

## Security & Privacy Decisions (chosen for MVP)

- Default storage: `ApplicationData.LocalFolder` (private) — used for MVP.
- `KnownFolders.VideosLibrary` will NOT be used by default. It may be added
  later only with explicit user opt-in.
- Encryption: defer implementation. MVP will NOT encrypt files at-rest, but
  the design will include an abstraction so encryption (DataProtectionProvider
  or DPAPI) can be added later without a major refactor.
- Consent: show a concise privacy notice before first camera/microphone access
  and persist the user's consent choice (timestamp + decision) in
  `ApplicationData.LocalSettings`.
- Logging: log only high-level events (e.g., "RecordingStarted", "RecordingStopped");
  never log file paths, file contents, or PII.

## Phases & Deliverables

### Phase 0 — Discovery (status)

- Reviewed `Docs/start-from-here.md` and current code stubs.
- Current stubs found in repo:
  - `Services/IMediaCaptureService.cs` (empty)
  - `Services/MediaCaptureService.cs` (stub)
  - `ViewModels/MainViewModels.cs` (Title property)
  - `Views/MainPage.xaml` (empty Grid)

### Phase 1 — Foundation (interface, DI, manifest)

Deliverables:
- `Services/IMediaCaptureService.cs` — define minimal service API:
  - `Task InitializeAsync(CancellationToken)`
  - `Task<bool> RequestPermissionsAsync()`
  - `Task StartPreviewAsync(object previewHost)`
  - `Task StopPreviewAsync()`
  - `Task<string> StartRecordingAsync(string baseFileName)`
  - `Task StopRecordingAsync()`
  - `Task DisposeAsync()`
  - `bool IsRecording { get; }`
- `Services/MediaCaptureService.cs` — scaffolding for implementation (no encryption)
- Register service in `App.xaml.cs` DI container:
  `services.AddSingleton<IMediaCaptureService, MediaCaptureService>();`
- Manifest: ensure only the required capabilities are present:
  - `DeviceCapability Name="webcam"`
  - `DeviceCapability Name="microphone"`

Security tasks (Phase 1):
- Default storage target is `ApplicationData.LocalFolder`.
- Implement filename sanitization helper (no path separators, allowed charset,
  max length) in `Helpers/FileHelpers.cs`.
- Implement permission check and expose friendly error paths when permission is denied.

### Phase 2 — Core MediaCapture implementation

Deliverables:
- Implement `MediaCaptureService` using `Windows.Media.Capture.MediaCapture`:
  - Select front camera via `DeviceInformation.FindAllAsync(DeviceClass.VideoCapture)`
	and `EnclosureLocation.Panel == Panel.Front`.
  - Initialize with `MediaCaptureInitializationSettings{ VideoDeviceId = id, StreamingCaptureMode = AudioAndVideo }`.
  - Start/Stop preview to a `MediaPlayerElement` or other preview host.
  - Prepare low-lag recording and save to `ApplicationData.LocalFolder` using
	`CreationCollisionOption.GenerateUniqueName`.
  - Ensure proper Dispose/Release on app suspend/navigation.

Security tasks (Phase 2):
- `RequestPermissionsAsync()` must verify device access and return false if denied.
- Do not log file paths or sensitive metadata.
- Use sanitized filenames and generation to avoid collisions and path traversal.

### Phase 3 — ViewModel & Commands

Deliverables:
- `ViewModels/InterviewViewModel.cs` (recommend CommunityToolkit.Mvvm):
  - Properties: `QuestionText`, `IsPreviewing`, `IsRecording`, `RecordedFilePath`, `ConsentGiven`
  - Commands: `StartPreviewCommand`, `StopPreviewCommand`, `StartRecordingCommand`, `StopRecordingCommand`, `StartCountdownCommand`
- Wire ViewModel to `Views/MainPage.xaml` via `x:Bind` or DI.

Security tasks (Phase 3):
- ViewModel checks `RequestPermissionsAsync()` before starting preview/recording.
- Persist consent metadata (timestamp + choice) in `ApplicationData.LocalSettings` (no PII).

### Phase 4 — UI & Accessibility

Deliverables:
- `Views/MainPage.xaml`:
  - `MediaPlayerElement` preview control (`x:Name="PreviewControl"`).
  - Overlay question panel with `x:Uid` keys for localization.
  - Start/Stop buttons with `AutomationProperties.Name` and `AutomationProperties.AutomationId`.
  - Countdown/Think-time display.
- `Views/MainPage.xaml.cs` to forward the preview control handle to the service.

Security tasks (Phase 4):
- UI must present the privacy notice before first camera open.
- Provide settings UI to list and delete recordings (delete from LocalFolder).

### Phase 5 — Interview features & UX

Deliverables:
- Countdown (3s) and optional think-time (30s) before recording.
- Immediate playback in the same preview control after Stop.
- Simple recordings list UI to playback/delete files.

Security tasks (Phase 5):
- Deletion must remove file from LocalFolder and clear in-memory references.
- If upload is added later, require explicit opt-in and enforce TLS + authenticated endpoints.

### Phase 6 — Tests & Validation

Tests:
- `Tests/ViewModels/InterviewViewModelTests.cs`:
  - Verify countdown → StartRecording → StopRecording sets `RecordedFilePath`.
  - Permission denied path surfaces error and prevents recording.
- `Tests/Services/MediaCaptureServiceTest.cs`:
  - Filename sanitizer tests.
  - Permission denied and device-not-found handled gracefully (use adapter/wrapper to allow mocking).

Build & run commands (PowerShell snippet for developers):

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
dotnet build -c Debug -p:Platform=$Platform
dotnet run -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
```

CI / PR checks:
- Ensure manifest capabilities do not exceed the approved list.
- Run unit tests on PRs.

## Implementation Notes & Decisions (applied)

1. Storage default: `ApplicationData.LocalFolder` (private) — chosen for MVP.
2. Encryption: deferred; design planner includes `IFileProtector` abstraction so
   encryption can be added later (recommended: `DataProtectionProvider` for
   user-scoped protection).
3. MVP exclusion: do not implement `VideosLibrary` saving or at-rest encryption
   in the MVP release.

## Files to create or edit (phase-ordered)

- Docs:
  - `Docs/ImplementationPlanning/impl-mvp.md` (this file)
- Services:
  - `Services/IMediaCaptureService.cs` (add interface members)
  - `Services/MediaCaptureService.cs` (implement basic MediaCapture flows; no encryption)
- Helpers:
  - `Helpers/FileHelpers.cs` (filename sanitization, LocalFolder save helper)
- ViewModels:
  - `ViewModels/InterviewViewModel.cs` (new)
  - `ViewModels/MainViewModels.cs` (no change required yet)
- Views:
  - `Views/MainPage.xaml` (add preview and controls)
  - `Views/MainPage.xaml.cs` (wire to ViewModel/service)
- App:
  - `App.xaml.cs` (DI registration: `services.AddSingleton<IMediaCaptureService, MediaCaptureService>();`)
- Tests:
  - `Tests/ViewModels/InterviewViewModelTests.cs`
  - `Tests/Services/MediaCaptureServiceTest.cs`

## Test Plan (high level)

- Unit tests for ViewModel command flows and service contract behavior.
- Sanity build + run on dev machine using `dotnet run` (winapp integration).
- Manual checks for: privacy prompt, preview starts, recording file exists under LocalFolder, playback works, deletion works.

## Next steps

If you confirm, I will produce Phase 1 detailed per-file skeletons and ready-to-paste code
for `IMediaCaptureService`, DI registration in `App.xaml.cs`, and a small filename
sanitizer helper in `Helpers/FileHelpers.cs`. After Phase 1 is applied we will run
the unit tests and proceed to Phase 2.

