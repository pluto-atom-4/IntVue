---
description: Template and guidance for creating scoped CLAUDE.md files in subdirectories
---

# Scoped CLAUDE.md Files — Template & Guidance

This file explains how to create and maintain scoped CLAUDE.md files in subdirectories. Scoped files apply progressive disclosure: agents load only the rules relevant to the area they're modifying.

---

## What is a Scoped CLAUDE.md?

A **scoped CLAUDE.md** is a lightweight guidance file placed in a subdirectory (e.g., `Services/CLAUDE.md`, `Views/CLAUDE.md`) that:

1. **Focuses on one area** — Rules specific to that folder and its contents
2. **References detailed guidance** — Links to instruction files in `.github/instructions/` for full context
3. **Is brief** — 50–100 lines; dense with links, not repetition
4. **Is discovered automatically** — AI agents load it when modifying files in that folder

---

## Structure

Each scoped CLAUDE.md should have:

```markdown
---
description: [One-line summary: area, scope, and key focus]
applyTo: [Glob patterns for files this applies to, e.g., **/*.cs, **/*.xaml]
---

# [Folder Name] — Scoped Guidance

[1–2 sentence intro explaining the folder's responsibility]

---

## Key Patterns

[Brief bullets or examples of patterns specific to this folder]

---

## Rules & Constraints

[Brief list of hard rules for this folder]

---

## References

| File | When to consult |
|---|---|
| [instruction file] | [What to read it for] |
| ... | ... |

---

## Quick Checklist

- [ ] [Specific to this folder]
- [ ] [Specific to this folder]
- [ ] [Reference to project-wide rules]
```

---

## Example: Services/CLAUDE.md

```markdown
---
description: Business logic, media capture, file operations, and dependency injection patterns for services
applyTo: Services/**/*.cs
---

# Services — Scoped Guidance

Services implement business logic, media capture, file I/O, and navigation. They are registered in `App.xaml.cs` via dependency injection and injected into ViewModels via constructors.

---

## Key Patterns

- **Interface-first:** Define `IMyService` interface before `MyService` implementation.
- **Single Responsibility:** Each service handles one concern (IMediaCaptureService, IRecordingService, INavigationService).
- **Async by default:** Use `async Task` and `async Task<T>` for I/O operations; never block with `.Result` or `.Wait()`.
- **No UI dependencies:** Services must not reference ViewModels, Views, or `DispatcherQueue` directly. Pass UI logic via callbacks or event handlers if needed.
- **Resource cleanup:** Implement `IDisposable` for services that hold unmanaged resources (MediaCapture, file handles, streams).

---

## Media Capture Rules (Critical)

When implementing media capture (camera, microphone, recording):

- Obtain explicit user consent before starting capture. Show a persistent recording indicator.
- Do not hold `MediaCapture` open while the app is backgrounded or suspended. Release immediately when suspending.
- Use `DeviceAccessInformation` or `MediaCapture` availability APIs to check device access gracefully.
- Initialize `MediaCapture` immediately before preview/recording; dispose as soon as possible.
- Log only high-level events ("RecordingStarted", "RecordingStopped") — never log file paths, metadata, or PII.
- Persist user consent records if your policy requires an audit trail.

See `.github/instructions/security.instructions.md` for full details.

---

## Testing Services

- Mock `IMyService` in ViewModel tests using Moq.
- Use integration tests for services that touch the file system or external APIs.
- Test both success and failure paths (e.g., camera unavailable, file write permission denied).
- Use the AAA pattern: Arrange → Act → Assert.

See `.github/instructions/testing.instructions.md` for full details.

---

## References

| File | When to consult |
|---|---|
| `design-principles.instructions.md` | Before adding a new service; apply SRP and DIP |
| `security.instructions.md` | Media capture, permissions, secrets, PII handling |
| `performance.instructions.md` | Async patterns, blocking calls on UI thread |
| `testing.instructions.md` | Service mocking, test naming, AAA pattern |
| `windows-apis.instructions.md` | When using WinAppSDK or platform APIs for the first time |

---

## Quick Checklist

- [ ] Service has a single responsibility and corresponding interface
- [ ] All I/O operations are async (no blocking `.Result` or `.Wait()`)
- [ ] MediaCapture and file handles are properly disposed
- [ ] Service has >80% test coverage (unit tests + integration tests)
- [ ] No UI dependencies; logic is decoupled from Views/ViewModels
- [ ] Secrets and sensitive data are not hard-coded
```

---

## Example: Views/CLAUDE.md

```markdown
---
description: XAML, WinUI 3 controls, data binding, accessibility, theming, and localization for UI
applyTo: Views/**/*.xaml, Views/**/*.cs, Controls/**/*.xaml, Controls/**/*.cs
---

# Views & Controls — Scoped Guidance

Views (XAML pages and windows) and Controls (custom or reusable UI components) define the visual structure and interaction of the app. All Views use x:Bind data binding and are styled with WinUI 3 theme resources.

---

## Key Patterns

- **x:Bind, not {Binding}:** Compile-time checking, better performance. Use `Mode=OneWay` or `Mode=OneTime` for read-only bindings.
- **Theme Resources:** Never hard-code colors; use `{ThemeResource TextFillColorPrimaryBrush}`, etc.
- **x:Load for deferred content:** Heavy UI sections (advanced options, settings) should be loaded only when needed.
- **Accessibility first:** All interactive controls must have `AutomationProperties.Name`, keyboard navigation must work, and contrast must meet WCAG AA.
- **Localization:** User-facing strings use `x:Uid` in XAML and `ResourceLoader` in code-behind; strings live in `.resw` files.
- **MVVM binding:** DataContext is the ViewModel; never set properties directly from code-behind.

---

## XAML Rules

- One attribute per line for controls with 3+ attributes. Order: `x:Name` → `x:Uid` → `AutomationProperties` → layout → data → style.
- Use `Microsoft.UI.Xaml.Controls`, not `Windows.UI.Xaml.Controls` (WinUI 3).
- Never call `Window.Current`; pass window reference explicitly.
- Use `DispatcherQueue` instead of `CoreDispatcher`.

---

## Accessibility Rules

- Add `AutomationProperties.Name` to all interactive controls.
- Ensure keyboard navigation works (Tab order, Enter/Space to activate, Escape to cancel).
- Test with screen readers (Narrator on Windows).
- Ensure color contrast meets WCAG AA (4.5:1 for text, 3:1 for large text).
- Test light, dark, and high-contrast themes.

See `.github/instructions/accessibility.instructions.md` for full details.

---

## References

| File | When to consult |
|---|---|
| `winui-best-practices.instructions.md` | MVVM architecture, x:Bind, DI, navigation, theming |
| `accessibility.instructions.md` | Keyboard nav, automation properties, screen readers, contrast |
| `globalization.instructions.md` | Localization, x:Uid, .resw files, language testing |
| `performance.instructions.md` | x:Load, XAML depth, virtualization, async patterns |
| `testing.instructions.md` | ViewModel testing, mocking, AAA pattern |
| `design-principles.instructions.md` | UI component design, DRY, KISS, SOLID |

---

## Quick Checklist

- [ ] All bindings use x:Bind with appropriate Mode (OneWay/OneTime)
- [ ] No hard-coded colors; all use `{ThemeResource}`
- [ ] All interactive controls have `AutomationProperties.Name`
- [ ] Keyboard navigation works end-to-end
- [ ] User-facing strings use x:Uid and .resw
- [ ] Heavy UI sections use x:Load for deferred loading
- [ ] Controls use `Microsoft.UI.Xaml`, not `Windows.UI.Xaml`
- [ ] ViewModels have >80% test coverage
- [ ] Light, dark, and high-contrast themes tested
```

---

## When to Create a Scoped CLAUDE.md

Create a scoped CLAUDE.md when a subdirectory has:

1. **Specialized patterns** not covered by project-wide rules (e.g., media capture in Services/)
2. **Constraints or guidelines** unique to that area (e.g., accessibility in Views/)
3. **Common mistakes or pitfalls** agents might encounter (e.g., forgetting to dispose MediaCapture)

### Recommended Scoped Files (by priority)

| Folder | Priority | Reason |
|---|---|---|
| `Services/` | **HIGH** | Media capture, recording, resource disposal, async patterns |
| `Views/` & `Controls/` | **HIGH** | XAML, accessibility, theming, localization, x:Bind |
| `ViewModels/` | **MEDIUM** | MVVM patterns, async commands, ObservableProperty, RelayCommand, testing |
| `Models/` | **LOW** | Data structures; mostly inherit project-wide rules |
| `Converters/` | **LOW** | Similar patterns across all; keep in project-wide rules |
| `Helpers/` | **LOW** | Static utilities; mostly inherit project-wide rules |

---

## Best Practices

- **Keep it brief:** 50–100 lines. Link to detailed guidance, don't repeat it.
- **Link liberally:** Every major topic should link to the relevant instruction file.
- **Use the frontmatter:** Always include `description` and `applyTo` so agents know when this file is relevant.
- **Update as you learn:** If you discover a common mistake in this folder, add it to the "Quick Checklist" or "Rules & Constraints".
- **Don't duplicate:** Reference instruction files rather than copying their content.
- **Use examples sparingly:** A short code snippet is fine; long examples belong in instruction files.

---

## How Agents Use These Files

1. **On startup:** Agent loads the project-level `./CLAUDE.md`
2. **When modifying a file:** Agent automatically loads the scoped CLAUDE.md for that directory (if it exists)
3. **For detailed rules:** Agent follows links in the Rules Router to `.github/instructions/`

**Example flow:**
- Agent modifies `Services/MediaCaptureService.cs`
- Agent loads `./CLAUDE.md` (project-wide rules)
- Agent loads `Services/CLAUDE.md` (scoped rules for media capture)
- Agent references `.github/instructions/security.instructions.md` for detailed media capture rules
- Agent implements the feature with all three layers of guidance in mind
