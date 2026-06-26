# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**IntVue** is a WinUI 3 desktop application (Windows App SDK) for interview practice. The MVP focuses on camera preview, video recording, and playback with a countdown/think-time workflow.

- **Framework:** WinUI 3 (Windows App SDK 1.8.x)
- **Target:** .NET 10.0 on Windows 10.0.26100.0+
- **Platforms:** x86, x64, ARM64
- **Package Model:** MSIX (loose-layout for development)
- **Architecture:** MVVM with dependency injection (CommunityToolkit.Mvvm)

---

## NEVER DO THIS

Hard constraints that must never be violated:

- **XAML Binding:** Never use `{Binding}`; always use `x:Bind` with `Mode=OneWay` or `Mode=OneTime`.
- **Hard-coded Colors:** Never hard-code colors in XAML; use `{ThemeResource TextFillColorPrimaryBrush}`.
- **Secrets:** Never commit API keys, passwords, or connection strings — use environment variables, `PasswordVault`, or Azure Key Vault.
- **MediaCapture:** Never hold `MediaCapture` open while the app is backgrounded or suspended.
- **Namespaces:** Never use `Windows.UI.Xaml`; always use `Microsoft.UI.Xaml` (WinUI 3).
- **Testing:** Never add features without unit tests (aim for 80%+ coverage on ViewModels/Services).
- **Speculative Code:** Never add "just in case" features — implement only what is explicitly requested now (YAGNI).

---

## Development Commands

### Platform Detection
Always detect your CPU architecture first—never hardcode `x64` or `x86`:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

### Build, Test, Run
```powershell
# Build (Debug or Release)
dotnet build -c Debug -p:Platform=$Platform

# Run all tests
cd Tests/IntVue.Tests
dotnet test -c Debug -p:Platform=$Platform

# Run app (auto-registers loose-layout package)
dotnet run -c Debug -p:Platform=$Platform

# Run specific test class
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"
```

### Troubleshooting
```powershell
# Reset stale package registration
winapp unregister
dotnet run -c Debug -p:Platform=$Platform
```

---

## High-Level Architecture

### Folder Structure
```
IntVue/
  Models/           → Data classes
  ViewModels/       → UI state, commands (no business logic)
  Views/            → XAML pages and windows
  Services/         → Business logic, media capture, file I/O
  Converters/       → IValueConverter implementations
  Helpers/          → Static utility methods
  Controls/         → Custom/reusable controls
  Strings/          → Localized strings (.resw)
  Assets/           → Images, icons
```

### MVVM Pattern
The app strictly follows Model-View-ViewModel:

| Layer | Responsibility |
|---|---|
| **View** (XAML) | Layout, styling, animations |
| **ViewModel** | UI state, commands, data binding (no business logic) |
| **Service** | Business logic, media capture, file I/O, navigation |
| **Model** | Data structures |

**Key:** Use `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]`. Register services in `App.xaml.cs` via `Microsoft.Extensions.DependencyInjection`. Inject dependencies via constructor (preferred) or `App.Services.GetService<T>()`.

### Media Capture
The app uses `Windows.Media.Capture.MediaCapture` with:
- **Preview:** Frame-based rendering via `MediaFrameReader` + Win2D `CanvasControl`
- **Recording:** `LowLagMediaRecording` to prevent UI stuttering
- **Resource Management:** Proper init/cleanup on suspend/resume cycles (critical for media resources)

---

## Rules Router

For detailed guidance, consult the relevant section below:

### Project-Wide Rules (All Code Changes)
- **Design Principles** (DRY, KISS, SOLID, YAGNI) → `.github/instructions/design-principles.instructions.md`
- **Code Quality** (StyleCop, naming, cleanup) → `.github/instructions/code-quality.instructions.md`
- **Testing Standards** (MSTest, Moq, AAA pattern) → `.github/instructions/testing.instructions.md`
- **Security & Permissions** (secrets, validation, PII) → `.github/instructions/security.instructions.md`
- **Performance** (async, x:Bind, virtualization) → `.github/instructions/performance.instructions.md`
- **Windows APIs** (API lookup, sample-first rule) → `.github/instructions/windows-apis.instructions.md`

### Scoped Rules (When Modifying Specific Areas)
- **Services/** (media capture, recording, file operations) → `Services/CLAUDE.md` (planned)
- **Views/** & **Controls/** (XAML, accessibility, theming, localization) → `Views/CLAUDE.md` (planned)
- **ViewModels/** (MVVM patterns, async commands, testing) → `ViewModels/CLAUDE.md` (planned)

### Specialized Guidance
- **WinUI 3 & Architecture** → `.github/instructions/winui-best-practices.instructions.md`
- **Accessibility** (keyboard nav, automation) → `.github/instructions/accessibility.instructions.md`
- **Localization** (x:Uid, .resw, multiple languages) → `.github/instructions/globalization.instructions.md`

---

## Quick Start Checklist

- [ ] Detect your platform (see Platform Detection command above)
- [ ] Read AGENTS.md for detailed agent workflows
- [ ] Enable Windows Developer Mode
- [ ] Build: `dotnet build -c Debug -p:Platform=$Platform`
- [ ] Test: `cd Tests/IntVue.Tests && dotnet test -c Debug -p:Platform=$Platform`
- [ ] Run: `dotnet run -c Debug -p:Platform=$Platform`
- [ ] Always consult the relevant instruction files before making changes
