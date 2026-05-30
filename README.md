IntVue
======

WinUI 3 desktop application (Windows App SDK).

Build & run (developer loop):

- Detect platform in PowerShell: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
- Build: `dotnet build -c Debug -p:Platform=$Platform`
- Run (registers package identity via winapp): `dotnet run -c Debug -p:Platform=$Platform`

Run tests:

- `dotnet test -c Debug -p:Platform=$Platform`

See .csproj for target framework and package versions.

MVP: Video Interview Practice

- Objective: minimal WinUI 3 app to preview the front camera, record timed responses, and allow immediate playback.
- Scope: camera preview, start/stop recording to ApplicationData.LocalFolder, countdown/think-time, immediate in-app playback, and unit tests for ViewModel and service abstractions.
- Security & Privacy (MVP): recordings saved to ApplicationData.LocalFolder (private); show a concise privacy notice before first camera/microphone access; sanitize filenames; avoid logging PII or file paths.

For full implementation plan and phase breakdown, see: Docs/ImplementationPlanning/impl-mvp.md
