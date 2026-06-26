---
name: security-audit
description: Audits media capture implementation, permissions, storage, and PII handling for security compliance
trigger: "audit security|check media capture|review permissions|verify pii handling|security review"
---

# Security Audit Playbook

This skill audits media capture, permissions, and data handling for security and privacy compliance. Use this when implementing or modifying camera/microphone capture, recording storage, or PII-sensitive features.

---

## When to Use This Skill

- Implementing media capture (camera, microphone, recording)
- Adding user consent flows for sensitive capabilities
- Modifying file storage or data handling
- Reviewing code for secrets (API keys, passwords) or PII leaks
- Testing permissions and error handling for missing capabilities
- Preparing for security review or compliance audit

---

## Playbook

### 1. Media Capture Initialization

**Checklist:**

- [ ] `MediaCapture` initialized **immediately before** use (not in constructor)
- [ ] `MediaCapture` disposed **immediately after** use (in `finally` block or `using` statement)
- [ ] Device availability checked before initialization:

```csharp
// GOOD: Check device availability first
var availability = await DeviceAccessInformation.CreateFromIdAsync(videoDeviceId).AsTask();
if (availability.CurrentStatus == DeviceAccessStatus.Denied)
{
    // Handle permission denied gracefully
    return;
}

// GOOD: Initialize immediately before use
var mediaCapture = new MediaCapture();
try
{
    var settings = new MediaCaptureInitializationSettings
    {
        VideoDeviceId = videoDeviceId,
        // ...
    };
    await mediaCapture.InitializeAsync(settings);
    // Use mediaCapture here
}
finally
{
    mediaCapture?.Dispose();
}
```

- [ ] No long-lived `MediaCapture` instances that persist across page navigation or app suspend

**Verify:**
```csharp
// Search for: MediaCapture { ... } (long-lived fields)
// These should be local variables or properties that are disposed on suspend
```

### 2. User Consent & Recording Indicator

**Checklist:**

- [ ] Explicit user consent obtained **before starting capture** (e.g., button click, checkbox)
- [ ] Persistent, visible recording indicator shown **during capture** (red dot, text label, or both)
- [ ] Recording indicator **disabled/hidden** immediately after stopping capture
- [ ] Consent flow is clear and user-friendly (not hidden in settings)

```csharp
// GOOD: Show clear recording indicator
<Grid Background="{ThemeResource SystemFillColorCriticalBrush}">
    <TextBlock Text="● Recording" Foreground="White" />
</Grid>

// When user clicks "Stop Recording"
IsRecording = false; // This disables/hides the indicator
```

- [ ] Consent state persisted (optional, but good for UX):

```csharp
// Example: Remember user consent for session
private bool _userConsentedToRecord = false;

[RelayCommand]
private async Task StartRecordingAsync()
{
    if (!_userConsentedToRecord)
    {
        // Show consent dialog
        var dialog = new ContentDialog { /* ... */ };
        var result = await dialog.ShowAsync();
        if (result == ContentDialogResult.Primary)
        {
            _userConsentedToRecord = true;
        }
        else
        {
            return; // User declined
        }
    }

    // Start recording
}
```

### 3. Permissions & Manifest

**Checklist:**

- [ ] Camera capability declared in `Package.appxmanifest` (if using camera):

```xml
<Capabilities>
    <DeviceCapability Name="webcam" />
</Capabilities>
```

- [ ] Microphone capability declared (if using audio):

```xml
<Capabilities>
    <DeviceCapability Name="microphone" />
</Capabilities>
```

- [ ] **No unnecessary capabilities:** Don't request camera if only using microphone
- [ ] **Principle of least privilege:** Request only what the feature actually needs
- [ ] App handles gracefully if permission is denied at runtime:

```csharp
// GOOD: Handle permission denied
try
{
    await mediaCapture.InitializeAsync(settings);
}
catch (UnauthorizedAccessException)
{
    StatusMessage = "Camera permission denied. Please enable it in Settings.";
    return;
}
```

**Verify:**
```xml
<!-- Review Package.appxmanifest for DeviceCapability -->
<!-- Ensure only necessary capabilities are present -->
```

### 4. Suspend/Resume Lifecycle

**Checklist:**

- [ ] `MediaCapture` is **disposed when app suspends**:

```csharp
// In App.xaml.cs or page code-behind
private async void OnSuspending(object sender, SuspendingEventArgs e)
{
    var deferral = e.SuspendingOperation.GetDeferral();
    try
    {
        if (_mediaCapture != null)
        {
            _mediaCapture.Dispose();
            _mediaCapture = null;
        }
    }
    finally
    {
        deferral.Complete();
    }
}
```

- [ ] `MediaCapture` is **re-initialized when app resumes**:

```csharp
private async Task OnResuming()
{
    // Re-initialize MediaCapture if needed
    await InitializeMediaCaptureAsync();
}
```

- [ ] No crashes or resource leaks during suspend/resume cycle

**Test:**
1. Start recording
2. Press Windows + X or trigger suspend
3. Resume the app
4. Verify app doesn't crash and resources are cleaned up

### 5. Recording Storage & File Handling

**Checklist:**

- [ ] Recordings saved to **private app storage** (`ApplicationData.LocalFolder`), not public/shared folders:

```csharp
// GOOD: Private storage
var folder = ApplicationData.Current.LocalFolder;
var file = await folder.CreateFileAsync("recording.mp4", CreationCollisionOption.GenerateUniqueName);

// AVOID: Public storage without explicit user consent
// var folder = KnownFolders.VideosLibrary; // Only if user explicitly chose this location
```

- [ ] **No PII in file paths or filenames:**

```csharp
// GOOD: Sanitized filename
string filename = $"recording_{DateTime.Now:yyyyMMdd_HHmmss}.mp4";

// AVOID: PII in filename
// string filename = $"recording_{userName}_{userId}.mp4"; // Contains PII!
```

- [ ] **Proper error handling for storage:**

```csharp
try
{
    var file = await folder.CreateFileAsync(filename, CreationCollisionOption.GenerateUniqueName);
    // Save recording to file
}
catch (UnauthorizedAccessException)
{
    StatusMessage = "Permission denied. Cannot save recording.";
}
catch (Exception ex) when (ex.HResult == -2147024784) // Disk full
{
    StatusMessage = "Disk space full. Cannot save recording.";
}
```

- [ ] **Cleanup temporary files:**

```csharp
// After saving recording to final location, delete temporary files
if (temporaryFile != null)
{
    await temporaryFile.DeleteAsync();
}
```

### 6. Logging & PII Protection

**Checklist:**

- [ ] **Only high-level events logged**, not sensitive data:

```csharp
// GOOD: High-level events only
Debug.WriteLine("RecordingStarted");
Debug.WriteLine("RecordingStopped");
Debug.WriteLine("UserConsentGranted");

// AVOID: Logging PII or media metadata
// Debug.WriteLine($"Recording to {filePath}"); // Exposes file path!
// Debug.WriteLine($"User {userName} started recording"); // Exposes username!
// Debug.WriteLine($"Recording duration: {duration.TotalSeconds}"); // Media metadata
```

- [ ] **No file paths in logs:**

```csharp
// GOOD: Generic message
Debug.WriteLine("Recording saved successfully");

// AVOID: File path exposure
// Debug.WriteLine($"Recording saved to {file.Path}");
```

- [ ] **No credentials, tokens, or API keys in logs:**

```csharp
// GOOD: No secrets
Debug.WriteLine("Authentication successful");

// AVOID: Logging secrets
// Debug.WriteLine($"Token: {accessToken}"); // Never!
```

**Verify:**
```csharp
// Search for Debug.WriteLine, Console.WriteLine, Logger
// Ensure no file paths, usernames, tokens, or sensitive metadata are logged
```

### 7. Device Selection

**Checklist:**

- [ ] **Multiple device support:** If there are multiple cameras/microphones, allow user to choose:

```csharp
// GOOD: Enumerate devices and let user choose
var videoCaptureDevices = await DeviceInformation.FindAllAsync(DeviceClass.VideoCapture);
var audioCaptureDevices = await DeviceInformation.FindAllAsync(DeviceClass.AudioCapture);

// Show list to user; let them select
var selectedCamera = videoCaptureDevices[userSelectedIndex];
```

- [ ] **Confirm device selection before recording:**

```csharp
// GOOD: Show selected device to user before starting
StatusMessage = $"Recording from: {selectedCamera.Name}";
// User clicks "Start" to confirm
```

### 8. Compliance & Audit Trail (Optional)

**Checklist:**

- [ ] **If audit trail required:** Persist user consent records securely:

```csharp
// Example: Store consent timestamp
var consentRecord = new
{
    Timestamp = DateTime.UtcNow,
    Feature = "MediaCapture",
    Action = "UserConsentGranted",
    DeviceId = selectedCamera.Id
};

// Save to secure storage (not plain text file)
// Use PasswordVault or encrypted storage
```

- [ ] **No sensitive audit data:** Don't log usernames, email addresses, or media content

### 9. Testing

**Checklist:**

- [ ] **Happy path:** Start recording → Record → Stop → Verify file saved
- [ ] **Permission denied:** Deny camera permission → App handles gracefully (no crash, clear error message)
- [ ] **Device unavailable:** Unplug camera → App handles gracefully
- [ ] **Suspend/resume:** Start recording → Suspend app → Resume → App doesn't crash
- [ ] **Disk full:** Fill disk → Try to save recording → Graceful error handling
- [ ] **No PII leakage:** Search code for file paths, usernames, emails in logs or UI
- [ ] **Resource cleanup:** Check that MediaCapture is disposed; no orphaned file handles

**Test code example:**
```csharp
[TestMethod]
public async Task StartRecordingAsync_WhenPermissionDenied_SetsErrorMessage()
{
    // Arrange
    var mockMediaCapture = new Mock<MediaCapture>();
    mockMediaCapture.Setup(m => m.InitializeAsync(It.IsAny<MediaCaptureInitializationSettings>()))
        .ThrowsAsync(new UnauthorizedAccessException());
    var viewModel = new RecordingViewModel(/* ... */);

    // Act
    await viewModel.StartRecordingCommand.ExecuteAsync(null);

    // Assert
    Assert.IsTrue(viewModel.StatusMessage.Contains("permission"));
}
```

### 10. Code Review Checklist

Before submitting media capture changes:

```csharp
// 1. Search for: MediaCapture
//    Ensure all instances are disposed in finally/using block

// 2. Search for: Debug.WriteLine, Console.WriteLine, Logger
//    Ensure no file paths, usernames, tokens, or sensitive data logged

// 3. Search for: filePath, fileName
//    Ensure no PII in filenames or paths

// 4. Search for: KnownFolders.VideosLibrary
//    Ensure user consented or feature explicitly requires public storage

// 5. Check: Package.appxmanifest
//    Ensure only necessary capabilities (webcam, microphone) declared

// 6. Check: OnSuspending, OnResuming handlers
//    Ensure MediaCapture disposed/re-initialized

// 7. Check: Exception handling
//    Ensure UnauthorizedAccessException and other permission errors handled
```

---

## References

- **Media Capture Details:** `.github/instructions/security.instructions.md` (Media Capture section)
- **Recording & Storage:** `.github/instructions/security.instructions.md` (Recording, Storage & File Handling section)
- **Services Implementation:** `Services/CLAUDE.md`
- **Testing:** `.github/instructions/testing.instructions.md`
- **Windows APIs:** `.github/instructions/windows-apis.instructions.md`

---

## Checklist for Submission

- [ ] MediaCapture initialized/disposed properly (not long-lived)
- [ ] User consent obtained before starting capture
- [ ] Recording indicator shown during capture
- [ ] Permissions checked before initialization
- [ ] Proper exception handling for permission denied, device unavailable
- [ ] MediaCapture disposed on app suspend
- [ ] Recordings saved to private storage (ApplicationData.LocalFolder)
- [ ] No PII in filenames or file paths
- [ ] Logging contains no file paths, usernames, tokens, or metadata
- [ ] Multiple devices supported (if applicable)
- [ ] Suspend/resume cycle tested (no crashes)
- [ ] Permission denial tested (graceful error handling)
- [ ] Tests cover happy path and error paths
- [ ] Code review checklist completed
