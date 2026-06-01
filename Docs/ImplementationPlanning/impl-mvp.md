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

---

## Remote Debugging with JetBrains Rider (Surface Tablet from Desktop PC)

This section describes how to debug the IntVue app on a Surface Tablet from a Desktop PC without native hardware.

### Scenario

- **Development Machine:** Desktop PC (Windows 11+, no camera/microphone)
- **Target Device:** Surface Tablet (Windows 11+, has camera/microphone)
- **Goal:** Debug app on Surface using Rider on Desktop

### Prerequisites

1. **Developer Mode enabled on both machines**
   - Desktop PC: Settings → System → For developers → Developer Mode: ON
   - Surface Tablet: Settings → System → For developers → Developer Mode: ON

2. **Visual Studio Remote Tools (must match Rider version)**
   - Download from: https://visualstudio.microsoft.com/downloads/#remote-tools-for-visual-studio-2026
   - Choose architecture matching Surface Tablet (typically ARM64 for Surface devices)
   - Install on Surface Tablet (target device)
   - Start the Remote Debugging Monitor (`msvsmon.exe`) on Surface

3. **Network connectivity**
   - Both machines on same LAN (Wi-Fi or Ethernet)
   - Note Surface IP address: `ipconfig` in PowerShell on Surface

4. **Rider & .NET SDK**
   - Rider 2024.3+ with .NET debugging support
   - .NET 8+ SDK on both machines

### Build & Deploy to Surface

#### Option 1: Deploy via `dotnet run` (Recommended for single debug session)

```powershell
# On Desktop PC in project directory

$Platform = "ARM64"  # Adjust if Surface uses x64
$TargetIP = "192.168.1.100"  # Surface tablet IP address

# Build for target architecture
dotnet build -c Debug -p:Platform=$Platform

# Deploy loose-layout package to Surface (requires Developer Mode + network discovery)
# Use winapp CLI to register and launch
winapp run --architecture $Platform --device "192.168.1.100"
```

#### Option 2: Package & Deploy (For persistent installation)

```powershell
# On Desktop PC

$Platform = "ARM64"

# Build release package
dotnet build -c Release -p:Platform=$Platform

# Sign the package
winapp sign ".\bin\$Platform\Release\net10.0-windows10.0.26100.0\win-$Platform\*" `
    --cert ".\dev-cert.pfx"

# Transfer MSIX to Surface and install
# On Surface: Add-AppxPackage "C:\path\to\IntVue.msix"
```

### Configure Rider for Remote Debugging

#### Step 1: Set Remote Connection in Rider

1. Open **Rider** on Desktop PC
2. Go to **Run → Edit Configurations**
3. Select/create **.NET Executable** configuration
4. Set:
   - **Target framework:** `net10.0-windows10.0.26100.0`
   - **Executable:** Path to built app or AUMID
   - **Host:** Surface Tablet IP (e.g., `192.168.1.100`)
   - **Port:** `4026` (default for Remote Debugging)

#### Step 2: Attach Debugger to Running App

Alternative to pre-configured run config:

1. Deploy/run app on Surface Tablet
2. In Rider: **Run → Attach to Process**
3. Set connection type: **Remote (TCP/IP)**
4. Enter Surface IP and port `4026`
5. Select the IntVue process from list
6. Click **Attach**

#### Step 3: Set Breakpoints and Debug

1. Open source files in Rider (same codebase)
2. Click to set breakpoints (red circles)
3. Interact with app on Surface; breakpoints trigger in Rider
4. Use **Variables**, **Call Stack**, **Threads** panels to inspect state
5. Step through code using standard debugger controls

### Handling Device-Not-Found Gracefully (Critical for Cross-Device Debug)

Since Desktop PC has no camera, the app **must gracefully handle missing devices** to reach the UI for testing.

#### Required Implementation

In `Services/MediaCaptureService.cs`:

```csharp
public async Task InitializeAsync(CancellationToken cancellationToken = default)
{
    if (this.initialized) return;

    this.mediaCapture = new MediaCapture();

    try
    {
        var devices = await DeviceInformation.FindAllAsync(
            DeviceClass.VideoCapture, 
            cancellationToken
        );

        // ✅ Graceful fallback if no devices found
        if (devices.Count == 0)
        {
            System.Diagnostics.Debug.WriteLine(
                "Warning: No camera device found. Preview will be unavailable."
            );
            this.initialized = true;
            return;  // Exit gracefully, don't crash
        }

        // Select front camera, fallback to first device
        var front = devices.FirstOrDefault(
            d => d.EnclosureLocation?.Panel == Panel.Front
        ) ?? devices[0];

        var settings = new MediaCaptureInitializationSettings
        {
            VideoDeviceId = front.Id,  // Now guaranteed non-null
            StreamingCaptureMode = StreamingCaptureMode.AudioAndVideo,
        };

        await this.mediaCapture.InitializeAsync(settings);
        this.initialized = true;
    }
    catch (Exception ex)
    {
        System.Diagnostics.Debug.WriteLine($"MediaCapture init error: {ex.Message}");
        this.initialized = false;
        throw;  // Caller handles
    }
}
```

**Benefit:** App runs on Desktop (preview disabled), full functionality on Surface.

### Creating Mock Services for Unit Testing

For unit tests on Desktop PC without hardware, create mock implementation:

```csharp
// Tests/Mocks/MockMediaCaptureService.cs
public class MockMediaCaptureService : IMediaCaptureService
{
    public bool IsRecording { get; private set; }

    public Task InitializeAsync(CancellationToken ct = default) => Task.CompletedTask;

    public Task<bool> RequestPermissionsAsync() => Task.FromResult(true);

    public Task StartPreviewAsync(object previewHost) => Task.CompletedTask;

    public Task StopPreviewAsync() => Task.CompletedTask;

    public async Task<string> StartRecordingAsync(string baseFileName)
    {
        this.IsRecording = true;
        return $"mock-recording-{Guid.NewGuid()}.mp4";
    }

    public async Task StopRecordingAsync()
    {
        this.IsRecording = false;
    }

    public Task DisposeAsync() => Task.CompletedTask;
}

// Usage in tests:
[TestMethod]
public async Task RecordingFlow_Success()
{
    var mockService = new MockMediaCaptureService();
    var viewModel = new InterviewViewModel(mockService);

    await viewModel.StartPreviewAsync(previewControl);
    await viewModel.StartRecordingAsync("test");

    Assert.IsTrue(mockService.IsRecording);

    await viewModel.StopRecordingAsync();
    Assert.IsFalse(mockService.IsRecording);
}
```

### Debugging Common Issues

| Issue | Cause | Solution |
|-------|-------|----------|
| **Cannot connect to Surface** | Developer Mode off, firewall blocking, wrong IP | Enable Dev Mode, check Windows Firewall inbound rules, verify IP with `ipconfig` |
| **Remote debugger not starting** | Remote Tools not installed or wrong architecture | Download matching VS Remote Tools for ARM64/x64, run `msvsmon.exe` on Surface |
| **App crashes on init (Desktop)** | No camera device found | Implement graceful device-not-found handling (see above) |
| **Breakpoints not hitting** | Debug symbols not loaded | Verify `.pdb` files in bin folder, check Rider symbol loading in debugger options |
| **App runs differently on Surface vs Desktop** | Environmental differences (device features) | Use mock services for unit tests, remote debug actual hardware-dependent code |

### Recommended Workflow

1. **Develop & test UI logic on Desktop** (no hardware needed)
   - Use mock `IMediaCaptureService`
   - Run unit tests: `dotnet test`
   - Build & run: `dotnet run`

2. **Debug hardware-dependent code on Surface**
   - Deploy app: `winapp run --device 192.168.1.100`
   - Set breakpoints in Rider
   - Attach debugger: **Run → Attach to Process** (Remote TCP/IP)
   - Step through real camera/microphone flows

3. **Validate on both environments**
   - Desktop PC: UI layout, navigation, accessibility
   - Surface: Camera preview, recording, playback, permissions

### Further Reading

- [Rider Debugger Documentation](https://www.jetbrains.com/help/rider/Debugging-Code.html)
- [Visual Studio Remote Debugging](https://learn.microsoft.com/en-us/visualstudio/debugger/remote-debugging)
- [Windows App SDK Deployment](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment)
- [winapp CLI Documentation](https://github.com/microsoft/WinAppCli/blob/main/docs/usage.md)

---

---

## Prerequisites: Critical Issues & Infrastructure (June 1, 2026)

### Issue #13 — CRITICAL: Fix device-not-found crash in MediaCaptureService

**Problem:** MediaCaptureService crashes on Desktop PC (no camera) when `DeviceInformation.FindAllAsync()` returns 0 devices.

**Impact:** ❌ Blocks all UI testing, Phases 4-6, and remote debugging setup.

**Solution:** Add graceful device-not-found handling in `MediaCaptureService.InitializeAsync()` (line 49):

```csharp
// After: var devices = await DeviceInformation.FindAllAsync(...)
if (devices.Count == 0)
{
    Debug.WriteLine("Warning: No camera device found. Preview mode disabled.");
    this.initialized = true;
    return;  // Exit gracefully without crash
}
```

**Status:** Ready to implement (GitHub Issue #13)  
**Effort:** 30 minutes  
**Priority:** 🔴 CRITICAL (unblocks all downstream work)

### Issue #14 — Setup: Configure JetBrains Rider for Remote Debugging

**Problem:** Remote debugging from Desktop PC to Surface Tablet requires infrastructure not yet configured.

**Solution:** One-time setup:
1. Download Visual Studio Remote Tools (ARM64 version for Surface)
2. Enable Developer Mode on both Desktop and Surface
3. Rider → Run → Edit Configurations → Remote Machine
4. Set host: Surface IP, port: 4026
5. Deploy via `winapp run`

**Status:** Ready to implement (GitHub Issue #14)  
**Effort:** 1 hour (one-time)  
**Priority:** 🟡 HIGH (prerequisite for hardware testing)

### Issue #15 — TEST: Implement MockMediaCaptureService for Offline Testing

**Problem:** Unit tests crash on Desktop PC without hardware mocking.

**Solution:** Create `Tests/Mocks/MockMediaCaptureService.cs` implementing `IMediaCaptureService`:
- All methods return `Task.CompletedTask`
- Properties have getters/setters for assertions
- No external dependencies

**Status:** Ready to implement (GitHub Issue #15)  
**Effort:** 1 hour  
**Priority:** 🟡 HIGH (enables offline unit testing)

### Execution Priority

```
1. FIX #13 (30 min) → Unblocks all work
2. PARALLEL #14 & #15 (1 hour each)
3. START Phase 4 → #2, #3, #4
```

---

## Next Steps

1. **Immediate (30 minutes):** Implement Issue #13 device-not-found fix
2. **Then (1 hour):** Setup remote debugging infrastructure (Issue #14)
3. **Parallel (1 hour):** Create MockMediaCaptureService (Issue #15)
4. **Then (4 hours):** Begin Phase 4 UI & Accessibility work

All 15 GitHub issues created and ready. See issues #13-#15 for detailed requirements, acceptance criteria, and effort estimates.

