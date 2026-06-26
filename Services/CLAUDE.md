---
description: Business logic, media capture, file operations, and dependency injection patterns for services
applyTo: Services/**/*.cs
---

# Services — Scoped Guidance

Services implement business logic, media capture, file I/O, and navigation. They are registered in `App.xaml.cs` via dependency injection and injected into ViewModels via constructors.

---

## Key Patterns

- **Interface-first:** Define `IMyService` interface before `MyService` implementation (e.g., `IMediaCaptureService` → `MediaCaptureService`).
- **Single Responsibility:** Each service handles one concern. Do not create "Manager" classes with multiple responsibilities.
- **Async by default:** Use `async Task` and `async Task<T>` for all I/O operations (file access, media operations). Never block with `.Result` or `.Wait()` on the UI thread.
- **No UI dependencies:** Services must not reference ViewModels, Views, or `DispatcherQueue` directly. Pass UI logic via callbacks, events, or return values.
- **Resource cleanup:** Implement `IDisposable` for services that hold unmanaged resources (MediaCapture, file handles, streams). Always use `using` statements or `try/finally` blocks.

---

## Media Capture & Recording Rules (Critical)

When implementing media capture (camera, microphone, recording):

- **User Consent:** Obtain explicit user consent before starting capture. Show a persistent, visible recording indicator whenever a capture session is active.
- **Lifecycle:** Do not hold `MediaCapture` open while the app is backgrounded or suspended. Release/dispose immediately when suspending or navigating away.
- **Device Access:** Use `DeviceAccessInformation` or `MediaCapture` availability APIs to check device access gracefully; never assume capability exists at runtime.
- **Timing:** Initialize `MediaCapture` immediately before preview/recording; dispose as soon as possible.
- **Logging:** Log only high-level events ("RecordingStarted", "RecordingStopped", "UserConsentGranted") — never log file paths, media metadata, raw audio/video data, or any PII.
- **Audit Trail:** If your policy requires it, persist user consent records (timestamp, acknowledgement) securely.
- **Preview Safety:** If displaying preview frames on-screen, ensure they do not leak to screenshots or secondary capture without user awareness.
- **Device Selection:** Allow users to choose alternative devices and confirm the selected device before recording begins.

See `.github/instructions/security.instructions.md` (Media Capture section) for full details.

---

## File Operations & Storage

- **Save to private storage:** Avoid saving files to globally shared or public paths. Use `ApplicationData.LocalFolder` for recordings and app data.
- **Sanitize filenames:** Never use user names, timestamps, or other PII in recording filenames.
- **Handle permissions:** Check write permissions before saving; handle "access denied" and "disk full" scenarios gracefully.
- **Cleanup:** Delete temporary files after use; implement proper error handling to avoid orphaned files.

See `.github/instructions/security.instructions.md` (Recording, Storage & File Handling section) for full details.

---

## Dependency Injection Integration

- **Constructor injection:** All dependencies passed via constructor, not resolved via `App.Services.GetService<T>()`.
- **Registration in App.xaml.cs:** Services registered as `AddSingleton`, `AddTransient`, or `AddScoped` (single responsibility determines lifetime).
- **Testability:** Design services so they can be easily mocked in unit tests via their interfaces.

---

## Testing Services

- **Mock dependencies:** In ViewModel tests, mock `IMyService` using Moq; replace real implementations with test doubles.
- **Integration tests:** For services that touch the file system or external APIs, use integration tests (not just unit tests) to verify real behavior.
- **Success & failure paths:** Test both success (operation completes) and failure (permissions denied, device unavailable, disk full) scenarios.
- **AAA pattern:** Arrange (setup) → Act (execute) → Assert (verify). One logical concept per test.
- **Test naming:** Use `MethodName_Scenario_ExpectedResult` (e.g., `StartRecordingAsync_WhenDeviceUnavailable_ThrowsInvalidOperationException`).

See `.github/instructions/testing.instructions.md` for full details.

---

## Common Pitfalls

| Pitfall | Fix |
|---|---|
| Service holds a long-lived `MediaCapture` after app suspends | Dispose on suspend; re-initialize on resume |
| Service logs file paths or recording metadata | Log only high-level events; never log PII or paths |
| Service blocks UI thread with synchronous I/O | Use `async/await`; never call `.Result` or `.Wait()` |
| Service created without corresponding interface | Define interface first; service implements it |
| Service initialized without checking device availability | Use `DeviceAccessInformation` before initializing |

---

## References

| File | When to consult |
|---|---|
| `security.instructions.md` | Media capture, permissions, secrets, PII handling, file storage |
| `performance.instructions.md` | Async patterns, blocking calls, threading constraints |
| `testing.instructions.md` | Service mocking, test naming, AAA pattern, integration tests |
| `design-principles.instructions.md` | Before adding a new service; apply SRP and DIP |
| `windows-apis.instructions.md` | When using WinAppSDK or platform APIs for the first time |

---

## Quick Checklist

- [ ] Service has a single responsibility and corresponding interface
- [ ] All I/O operations are async (use `async Task` / `async Task<T>`)
- [ ] No blocking calls on UI thread (no `.Result`, `.Wait()`, or `.GetAwaiter().GetResult()`)
- [ ] MediaCapture is disposed on app suspend and re-initialized on resume
- [ ] File operations use `ApplicationData.LocalFolder` (private storage)
- [ ] User consent obtained before starting media capture
- [ ] Recording indicator shown during capture
- [ ] Service has >80% test coverage (unit + integration tests)
- [ ] No UI dependencies (ViewModels, Views, DispatcherQueue)
- [ ] Secrets and sensitive data not hard-coded
- [ ] Logging contains no PII, file paths, or media metadata
