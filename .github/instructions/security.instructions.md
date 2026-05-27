
---
description: 'Security requirements for secrets management, input validation, permissions, and secure coding'
applyTo: '**/*.cs, **/*.appxmanifest'
---

# Security

These rules apply to **every feature and change**. They are not optional add-ons.

---

## Rules (core)

- **Never hard-code secrets** (API keys, passwords, connection strings) — use environment variables, Windows Credential Manager, or Azure Key Vault.
- Validate and sanitize **all external input** (user input, file content, network responses).
- Use `SecureString` or `PasswordVault` for sensitive data in memory when practical.
- Follow the **principle of least privilege** — request only the permissions the app actually needs in `Package.appxmanifest`.
- Keep NuGet packages up to date — run `dotnet list package --outdated` regularly.
- Enable **code signing** for published MSIX packages. Use the `winapp` CLI rather than hand-rolling `signtool`:
  - Generate a development certificate matching the manifest publisher: `winapp cert generate --manifest .\Package.appxmanifest --install`.
  - Inspect a cert before signing: `winapp cert info .\devcert.pfx`.
  - Sign an existing file: `winapp sign .\MyApp.msix --cert .\devcert.pfx`.
  - Build + sign in one step: `winapp pack .\bin\<Platform>\Release\<TFM>\win-<rid> --cert .\devcert.pfx`.
  - Production releases must be signed by a trusted certificate authority -- never ship the development cert.
- When using `HttpClient`, always validate TLS certificates and use HTTPS.
- Never log sensitive data (PII, tokens, passwords).
- Prefer `IHttpClientFactory` or a single `HttpClient` instance; do not create one per request.

---

## Rules (media capture, camera & microphone — NEW)

These rules apply when your change uses `Windows.Media.Capture`, device cameras, microphones, or writes recorded media to disk or the network.

- Obtain explicit user consent before starting camera or microphone capture. Show a prominent, persistent recording indicator (visual UI element) whenever a capture session is active.
- Do not hold the camera or microphone open while the app is backgrounded or inactive. Stop/Dispose the `MediaCapture` instance when the app is suspended or when the view is navigated away from.
- Use `DeviceAccessInformation` or `MediaCapture` availability APIs to check device access and handle denied/unavailable scenarios gracefully; do not assume capability exists at runtime.
- Only initialize `MediaCapture` immediately before preview/recording; release it as soon as possible.
- Log only high-level events (e.g., "RecordingStarted", "RecordingStopped", "ConsentGranted") — do not log file paths, media metadata, raw audio/video data, or any PII.
- Persist user consent records (timestamp, user-acknowledgement) if your policy requires an audit trail; store audit data securely.
- If you display preview frames on-screen, ensure preview does not inadvertently leak to screenshots or secondary capture without user awareness (document risk and mitigation).
- For camera selection, ensure users can choose alternative devices and confirm selected device before recording begins.

---

## Rules (recording, storage & file handling — NEW)

- Avoid saving recordings to globally shared or public file paths. Prefer per-user locations:
  - If user expects files in their Videos library, use `KnownFolders.VideosLibrary` with manifest capability and user consent.
  - For app-private storage use `ApplicationData.LocalFolder`.
- Never accept unsanitized filenames from user input. Validate and sanitize any filename:
  - Forbid path separators, normalize the filename, and enforce an allowed character/length policy.
- Prevent path traversal: always create files using the Storage APIs (`StorageFile`, `CreateFileAsync`) rather than concatenating path strings.
- Encrypt recordings at rest if media contains PII, sensitive interview content, or if policy requires it. Use Windows DPAPI/`DataProtectionProvider` (`Windows.Security.Cryptography.DataProtection.DataProtectionProvider`) or platform-appropriate protection.
- When storing temporary files, keep them in `ApplicationData.TemporaryFolder` and delete them promptly after use.
- Minimize metadata exposure: clear or sanitize EXIF/metadata that may reveal device identifiers or geolocation unless explicitly needed and consented.
- Maintain a retention policy: document how long recordings are kept, and provide an easy way to delete them.

---

## Rules (network & upload — NEW)

- Do not upload recordings to external servers without explicit user consent.
- When uploading, require authenticated, authorized endpoints and TLS (HTTPS). Validate server certificates and consider certificate pinning for critical endpoints.
- Use chunked uploads with integrity checks (hash) if large files are transmitted, and confirm successful upload before deleting local copies.
- Transmit only the minimum needed metadata. Avoid sending device identifiers unless necessary and documented in privacy policy.
- Use OAuth or another secure token-based scheme for server authentication. Never embed or hard-code upload credentials.

---

## Rules (privacy & UI behavior — NEW)

- Display a concise privacy notice on first-run (or first use of camera/microphone) explaining:
  - Why camera/microphone access is needed.
  - Where recordings are stored.
  - Whether recordings may be uploaded or shared and how to opt out.
- Provide a clear on/off control for camera and microphone as well as a visible recording indicator while capturing.
- If the app has automated transcripts or analytics on recordings, require separate user consent for those features.
- Provide an in-app mechanism for users to delete their recordings and associated metadata.

---

## Rules (secure coding & exception handling — expanded)

- Do not swallow exceptions silently. When catching exceptions, handle them specifically and log a non-sensitive message with context for diagnostics.
- Validate all inputs from files, network responses, and external devices. For media files, validate file type, expected container, and fail gracefully for corrupted inputs.
- Avoid P/Invoke or Launching external processes with unsanitized content. If needed, sanitize input strictly.
- When using any native interop, validate pointers and manage buffers carefully to avoid overflows.

---

## Anti-patterns

- Storing secrets in `appsettings.json` committed to source control.
- Disabling TLS validation for debugging and forgetting to re-enable it.
- Using `Process.Start` with unsanitized user input.
- Broad `try { } catch (Exception) { }` that swallows errors silently without any logging.
- Holding camera/mic open while the app is backgrounded or when not in foreground use.
- Saving recordings to public, world-readable locations or temporary folders and never cleaning them up.

---

## Validation

- Build & register the MSIX package — see **Build, Run & Deploy** in `.github/agents/Agents.md`.
- Check for hard-coded secrets: search for `password`, `apikey`, `secret`, `connectionstring` in `.cs` files.
- Manifest verification: ensure listed capabilities in `Package.appxmanifest` match actual runtime requirements and are documented.
- Media checks: validate that `MediaCapture` is released appropriately and that user consent is requested before use.

### Verification Checklist (expanded)

- [ ] No secrets are hard-coded
- [ ] Camera & microphone access only occurs after explicit user consent
- [ ] App shows a persistent recording indicator while capturing
- [ ] MediaCapture is disposed when not needed and on suspend/navigation
- [ ] Recordings saved only to user-expected locations (`KnownFolders.VideosLibrary`) or `ApplicationData.LocalFolder`
- [ ] Filenames are sanitized and path traversal is prevented
- [ ] Recordings containing PII are encrypted at rest or stored in private app storage
- [ ] Uploads require explicit user consent and use TLS + validated certificates
- [ ] Logs do not contain PII, file paths to recordings, or raw media identifiers
- [ ] Manifest capabilities are minimal and documented (why each capability is needed)
- [ ] NuGet packages are up-to-date
- [ ] Runtime tests exist for permission-denied and device-unavailable scenarios

---

## Must Read & Research

> **Agent Rule:** Before any security-related change (auth, input handling, permissions, HTTP, camera/mic), you **must** fetch and review these references using `fetch_webpage`. Apply what you learn.

| # | Reference | When to consult |
|---|---|---|
| 1 | [.NET Security Best Practices](https://learn.microsoft.com/en-us/dotnet/standard/security/) | Any code handling credentials, tokens, or sensitive data |
| 2 | [Secure coding guidelines for .NET](https://learn.microsoft.com/en-us/dotnet/standard/security/secure-coding-guidelines) | Input validation, exception handling, type safety |
| 3 | [MSIX Security](https://learn.microsoft.com/en-us/windows/msix/msix-container) | Packaging, signing, or distribution changes |
| 4 | [Package.appxmanifest capabilities](https://learn.microsoft.com/en-us/windows/uwp/packaging/app-capability-declarations) | Adding or modifying app capabilities/permissions |
| 5 | [WinAppSDK MediaCapture guidance](https://learn.microsoft.com/) | Check MediaCapture availability patterns and permission handling (search Windows.Media.Capture docs) |
| 6 | [Windows Data Protection API (DPAPI) & DataProtectionProvider](https://learn.microsoft.com/) | Encrypting sensitive files at rest |
| 7 | [Privacy guidance for camera and microphone apps](https://learn.microsoft.com/) | User consent and privacy UI patterns |
