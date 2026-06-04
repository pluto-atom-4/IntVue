# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**IntVue** is a WinUI 3 desktop application (Windows App SDK) for interview practice. The MVP focuses on camera preview, video recording, and playback with a countdown/think-time workflow.

- **Framework:** WinUI 3 (Windows App SDK 1.8.x)
- **Target:** .NET 10.0 on Windows 10.0.26100.0+
- **Platforms:** x86, x64, ARM64
- **Package Model:** MSIX (loose-layout for development)
- **Architecture:** MVVM with dependency injection

### Key Project Files
- **IntVue.csproj** — Project file; source of truth for target framework, platforms, and package versions
- **.github/instructions/** — Detailed rules for design, WinUI patterns, testing, security, and accessibility
- **AGENTS.md** — Build/run/deploy workflows and agent rules
- **Docs/ImplementationPlanning/impl-mvp.md** — MVP scope and phase breakdown

---

## Development Commands

### Platform Detection
Always detect your CPU architecture first—never hardcode `x64` or `x86`:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

### Build
```powershell
# Debug build
dotnet build -c Debug -p:Platform=$Platform

# Release build
dotnet build -c Release -p:Platform=$Platform
```

### Run
```powershell
# Preferred: dotnet run (auto-registers loose-layout package and launches via AUMID)
dotnet run -c Debug -p:Platform=$Platform
```

### Tests
```powershell
# Run all tests
cd Tests/IntVue.Tests
dotnet test -c Debug -p:Platform=$Platform

# Run tests for a specific class
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"

# Run a single test
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests.LoadItemsAsync_OnSuccess_PopulatesItems"

# Run tests in a namespace (e.g., all ViewModel tests)
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~Tests.ViewModels"
```

### Troubleshooting
```powershell
# If package identity is stale or tests fail with permission errors:
winapp unregister

# Then rebuild and run
dotnet run -c Debug -p:Platform=$Platform
```

---

## High-Level Architecture

### Folder Structure
```
IntVue/
  Models/           → Data classes (e.g., RecordingSession, Interview, etc.)
  ViewModels/       → UI state & commands (one per page/dialog)
  Views/            → XAML pages and windows
  Services/         → Business logic, media capture, file operations
  Converters/       → IValueConverter implementations
  Helpers/          → Static utility methods
  Controls/         → Custom/reusable controls
  Strings/          → Localized resource strings (.resw)
  Assets/           → Images, icons, splash screens
```

### MVVM Pattern
The app strictly follows **Model-View-ViewModel (MVVM)**:

| Layer | Responsibility |
|---|---|
| **View** (XAML) | UI layout, styling, animations |
| **ViewModel** | UI state, commands, data transformation (no business logic) |
| **Service** | Business logic, media capture, file I/O, navigation |
| **Model** | Data structures (interview questions, recordings, etc.) |

**Key:** ViewModels use `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]` attributes. Bind ViewModels to Views in XAML via `x:Bind` (never `{Binding}`).

### Dependency Injection (DI)
- All services are registered in `App.xaml.cs` via `Microsoft.Extensions.DependencyInjection`.
- Access services at runtime via `App.Services.GetService<IMyService>()`.
- Constructor-inject dependencies into ViewModels and Services (preferred) or resolve via `App.Services`.

### MediaCapture & Preview
The app uses `Windows.Media.Capture.MediaCapture` for camera access:
- **Preview rendering:** Implements frame-based rendering via `MediaFrameReader` + Win2D `CanvasControl` (see issue #22).
- **Recording:** Uses `LowLagMediaRecording` to avoid UI stuttering.
- **Resource management:** Proper init/cleanup on suspend/resume cycles.

---

## Code Rules & Conventions

### Design Principles (Read `.github/instructions/design-principles.instructions.md`)
Apply **DRY, KISS, SOLID, YAGNI** in every change:

- **DRY:** Search for existing implementations before writing new code. Extract shared logic into helpers/services.
- **KISS:** Choose the simplest solution. Avoid unnecessary abstractions. If a method exceeds ~30 lines, split it.
- **SOLID:**
  - **SRP:** Each class has one reason to change. ViewModels handle UI state; Services handle business logic.
  - **OCP:** Extend via interfaces/inheritance, not modification.
  - **LSP:** Derived classes must honor their base contracts.
  - **ISP:** Keep interfaces small and focused.
  - **DIP:** Depend on abstractions, not concretions. Use DI.
- **YAGNI:** Implement only what is explicitly needed now; avoid speculative code.

### XAML & UI (Read `.github/instructions/winui-best-practices.instructions.md`)
- **Use `x:Bind` (not `{Binding}`):** Compile-time checking, better performance, IntelliSense.
- **Use `x:Load` for deferred content:** Improves startup time for optional UI sections.
- **Theme resources, not hard-coded colors:** Use `{ThemeResource TextFillColorPrimaryBrush}` to support light/dark/high-contrast themes.
- **WinUI 3 controls:** Use `Microsoft.UI.Xaml.Controls`, not the older `Windows.UI.Xaml.Controls`.
- **Accessibility:** Add `AutomationProperties.Name`, `x:Uid` for localization, ensure keyboard navigation works.

### Testing (Read `.github/instructions/testing.instructions.md`)
- **Framework:** MSTest with Moq for mocking.
- **Coverage:** Aim for 80%+ on ViewModels/Services; 100% on helpers.
- **Naming:** `MethodName_Scenario_ExpectedResult` (e.g., `LoadDataAsync_WhenServiceThrows_SetsErrorState`).
- **Structure:** Arrange → Act → Assert (AAA pattern). One logical concept per test.
- **Location:** Mirror the main project folder structure in `Tests/IntVue.Tests/`.

Example:
```csharp
[TestMethod]
public async Task LoadItemsAsync_OnSuccess_PopulatesItems()
{
    // Arrange
    var mockService = new Mock<IDataService>();
    mockService.Setup(s => s.GetItemsAsync()).ReturnsAsync(new List<Item> { new("Test") });
    var viewModel = new MainViewModel(mockService.Object);

    // Act
    await viewModel.LoadItemsAsync();

    // Assert
    Assert.AreEqual(1, viewModel.Items.Count);
}
```

### Performance (Read `.github/instructions/performance.instructions.md`)
- Use `x:Bind` with `Mode=OneWay` or `Mode=OneTime` for read-only bindings (faster than `TwoWay`).
- Use `x:Load` for heavy content; leverage `Visibility.Collapsed` + virtualization for lists.
- Async patterns: Use `Task`-based APIs; avoid blocking `Wait()` or `.Result` on the UI thread.
- Media operations (frame reading, encoding) must run off-UI-thread.

### Security & Privacy (Read `.github/instructions/security.instructions.md`)
- **No hard-coded secrets:** API keys, tokens, and credentials belong in secure storage (e.g., `PasswordVault`) or environment.
- **Input validation:** Validate all user inputs and external data at system boundaries.
- **Least privilege:** Request only necessary capabilities (camera, microphone) in the manifest.
- **PII handling:** Never log file paths, user names, or other PII. Sanitize recording filenames.
- **Resource location:** Recordings are saved to `ApplicationData.LocalFolder` (private per-app).

### Localization (Read `.github/instructions/globalization.instructions.md`)
- User-facing strings go in `.resw` files under `Strings/en-us/`.
- Use `x:Uid` in XAML and `ResourceLoader` in code-behind to load strings.
- Test light/dark/high-contrast themes and multiple languages.

---

## Common Workflows

### Adding a New Feature
1. **Understand the requirement** — What is the user trying to do?
2. **Check existing code** — Search for related implementations to avoid duplication.
3. **Plan the approach** — Identify the Models, ViewModels, Services, and Views you'll touch.
4. **Read the relevant instruction files:**
   - Design? → Read `design-principles.instructions.md`
   - UI/XAML? → Read `winui-best-practices.instructions.md` AND `accessibility.instructions.md` AND `performance.instructions.md`
   - Strings/localization? → Read `globalization.instructions.md`
   - Media/Windows APIs? → Read `windows-apis.instructions.md`
5. **Implement** — Follow SOLID principles and MVVM.
6. **Test** — Write unit tests (AAA pattern, `MethodName_Scenario_ExpectedResult` naming).
7. **Build & run** — Detect platform, build, run tests, run the app.
8. **Review** — Confirm the implementation matches the original requirement.

### Modifying Existing Code
1. **Run existing tests first** — Establish baseline.
2. **Make the change** — Apply design principles.
3. **Run tests again** — Fix any failures. Add new tests if new behavior is introduced.
4. **Build & run the app** — Verify in the real UI.

### Fixing a Build Error
1. **Read `.github/instructions/windows-apis.instructions.md`** — Check if the API is in the catalog.
2. **Web search the error** — Use the [WinAppSDK API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/) or [Platform SDK API Reference](https://learn.microsoft.com/en-us/uwp/api/).
3. **Check samples** — Look up working examples in [WindowsAppSDK-Samples](https://github.com/microsoft/WindowsAppSDK-Samples).
4. **Decompile as last resort** — Only use `.winmd` inspection or decompilers if steps 1-3 fail.

---

## Key References

All detailed rules are in `.github/instructions/`:

| File | When to consult |
|---|---|
| `design-principles.instructions.md` | Before refactoring or adding classes; apply DRY, KISS, SOLID, YAGNI |
| `winui-best-practices.instructions.md` | Adding/changing UI, XAML, navigation, theming, or community toolkit usage |
| `accessibility.instructions.md` | Adding UI controls; ensure keyboard nav, automation, and contrast |
| `performance.instructions.md` | Async patterns, data binding (x:Bind vs {Binding}), virtualization, media I/O |
| `security.instructions.md` | Handling secrets, user input, permissions, and PII |
| `globalization.instructions.md` | Adding user-facing strings; use .resw and x:Uid |
| `code-quality.instructions.md` | Static analysis, StyleCop, naming conventions, cleanup |
| `windows-apis.instructions.md` | Looking up WinAppSDK or platform APIs; check this FIRST before implementing |
| `testing.instructions.md` | Unit test setup, MSTest/Moq, AAA pattern, test naming |

---

## Common Pitfalls

| Issue | Fix |
|---|---|
| Using `{Binding}` in XAML | Switch to `x:Bind` with `Mode=OneWay` or `Mode=OneTime` |
| Hard-coded colors in XAML | Replace with `{ThemeResource TextFillColorPrimaryBrush}` |
| Using `Windows.UI.Xaml` | Switch to `Microsoft.UI.Xaml` (WinUI 3) |
| Calling `Window.Current` | Pass window reference explicitly; `Window.Current` is not available in WinUI 3 |
| Using `CoreDispatcher` | Use `DispatcherQueue` instead |
| `REGDB_E_CLASSNOTREG` error | Enable Developer Mode; run `winapp unregister` then `dotnet run` |
| Test depends on another test's state | Each test must be fully independent; no shared state |
| Mocking too much in tests | Mock only external dependencies, not the class under test |
| Async test without `await` | Always `await` async methods; use `async Task` return type |
| Speculative code ("just in case") | Remove it. If it's not requested, it violates YAGNI. |

---

## Before You Start

- **Read AGENTS.md** for the detailed Core Agent Workflow and platform-specific rules.
- **Detect your platform** with the script in "Development Commands" above.
- **Verify Developer Mode** is enabled on Windows.
- **Build and test** before submitting any change.
- **Never skip instruction files** — they contain rules that must be applied, not just read.
