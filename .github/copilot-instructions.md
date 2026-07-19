# Copilot Instructions

Style preferences for GitHub Copilot (autocomplete) in IntVue.

---

## C# Code Style

- Use modern C# 11+ syntax: switch expressions, nullable reference types, `!` operator
- Explicit variable names: `isRecording`, `currentTime` (not `is`, `ct`)
- MVVM pattern: ViewModels inherit `ObservableObject`, use `[ObservableProperty]` + `[RelayCommand]`
- Async/await for all I/O operations; never block UI thread
- Constructor injection for dependencies; no `App.Services.GetService<T>()` except in App.xaml.cs

---

## XAML

- **Binding:** Always use `x:Bind` with `Mode=OneWay` or `Mode=OneTime` (never `{Binding}`)
- **Colors:** `{ThemeResource TextFillColorPrimaryBrush}` (never `Foreground="#000000"`)
- **Spacing:** Use 8px base grid: `Spacing="8"`, `Padding="12"`, `Margin="16"`
- **Accessibility:** Every button/input has `AutomationProperties.Name="Clear description"`
- **Namespaces:** `Microsoft.UI.Xaml` only (never `Windows.UI.Xaml`)

---

## Project Structure

- **Views/** → XAML pages/windows (layout & styling only)
- **ViewModels/** → UI state & commands (no business logic)
- **Services/** → Business logic, media capture, file I/O
- **Models/** → Data classes (POCOs, no logic)
- **Converters/** → IValueConverter implementations
- **Helpers/** → Static utility methods
- **Controls/** → Custom/reusable XAML controls
- **Assets/** → Images, icons
- **Strings/** → Localized strings (.resw files)

---

## Patterns

- MVVM: Services → ViewModels → Views; never skip layers
- DI: Register in `App.xaml.cs` via `Microsoft.Extensions.DependencyInjection`
- Error handling: Validate at boundaries; trust internal code
- Testing: MSTest + Moq; AAA pattern; 80%+ coverage on ViewModels/Services
- Resource management: Dispose MediaCapture on suspend; use `using` statements

---

## More Guidance

See **[CLAUDE.md](../CLAUDE.md)** for full project rules, architecture, and build commands.
