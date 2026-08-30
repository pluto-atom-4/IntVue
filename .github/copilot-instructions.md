---
title: "GitHub Copilot Agent Mode — IntVue WinUI 3"
version: "2.2"
updated: "2026-08-30"
audience: "GitHub Copilot Agent Mode, auto-discovered via glob patterns"
schema: "YAML frontmatter + Markdown body; Copilot ingests this as cascading instruction set"
cascade: "IntVue inherits: .claude/settings.json (hooks) → AGENTS.md (workflow) → .github/copilot-instructions.md (pattern rules) → .claude/rules/* (governance details)"
---

# GitHub Copilot Agent Mode — IntVue WinUI 3

## Tech Stack Anchor

Architecture, tech boundaries, and data flow are defined in [DESIGN.md](../DESIGN.md)
(§ System Architecture, § Technology Boundaries). Read it before generating code — it is the
single source of truth for the stack: WinUI 3 / Windows App SDK, MVVM + DI, `x:Bind`-only XAML,
and semantic design tokens (never hard-coded colors/spacing/type sizes).

---

## Scope & Auto-Discovery

**Auto (Copilot generates without confirmation):** Bug fixes, tests, docs, formatting, config
**Confirm (Copilot presents options, awaits approval):** Architecture, APIs, dependencies, build scripts
**Blocked (Copilot refuses):** Secrets, releases, refactors >3 files / >200 LOC

Path-specific rules auto-apply via glob pattern matching in Copilot's instruction processor:
- `**/*.xaml` → x:Bind, ThemeResource, accessibility rules apply
- `**/*ViewModel.cs` → MVVM, CommunityToolkit.Mvvm, DI rules apply
- `**/*Service.cs` → stateless, boundary validation, constructor injection rules apply
- `**/*Tests.cs` → MSTest/AAA, ≥80% coverage, naming conventions apply

See [.github/copilot/README.md](copilot/README.md) for full glob precedence and [.github/instructions/](../instructions/) for detailed patterns.

---

## Do / Do Not Rules

### XAML Binding & Styling (Mandatory)

**DO:**
- ✅ Use `x:Bind` for all data bindings (strongly-typed, compile-time checked)
- ✅ Use `{ThemeResource ...Brush}` for all colors (light/dark/high-contrast auto-support)
- ✅ Use `{ThemeResource ...}` for spacing (8px grid: spacing-sm=8, spacing-md=12, spacing-lg=16)
- ✅ Use semantic tokens: `TextFillColorPrimaryBrush`, `ControlStrongFillColorDefaultBrush`, etc.
- ✅ Set `AutomationProperties.Name` on all interactive controls (accessibility)
- ✅ Use `CornerRadius` from design scale (2, 8, or 16 — never 5, 12, arbitrary values)

**DO NOT:**
- ❌ Use `{Binding}` (WinForms-style, runtime-only, no type checking)
- ❌ Hard-code colors as hex (#0078D4, #FF0000) — always use theme resources
- ❌ Hard-code spacing (Margin="15", Padding="11") — always use 8px grid multiples
- ❌ Omit `AutomationProperties.Name` on buttons, TextBoxes, ComboBoxes
- ❌ Use arbitrary font sizes (FontSize="18") — use scale: 12, 14, 16, 20, 32, or 72 (countdown-only)
- ❌ Mix Margin + Spacing in same StackPanel (use Spacing for consistency)

**Pattern Example (DO):**
```xaml
<StackPanel Spacing="8" Padding="12">
    <TextBlock 
        Text="Recording" 
        FontSize="14" 
        Foreground="{ThemeResource TextFillColorPrimaryBrush}" />
    <Button 
        Content="Start" 
        AutomationProperties.Name="Start recording"
        Style="{ThemeResource AccentButtonStyle}" />
</StackPanel>
```

**Anti-Pattern (DO NOT):**
```xaml
<StackPanel Spacing="8" Margin="10">
    <TextBlock Text="Recording" Foreground="#000000" />
    <Button Content="Start" Background="#0078D4" />
</StackPanel>
```

---

### C# ViewModels (MVVM + DI)

**DO:**
- ✅ Inherit from `ObservableObject` (CommunityToolkit.Mvvm)
- ✅ Use `[ObservableProperty]` for properties (auto-generates INotifyPropertyChanged)
- ✅ Use `RelayCommand` for all user actions
- ✅ Depend on Services via constructor (DI container resolves)
- ✅ Validate input at entry points (user-triggered commands, property setters)
- ✅ Make all business logic internal; Services own it

**DO NOT:**
- ❌ Mix business logic in ViewModel (belongs in Service)
- ❌ Use auto-properties without INotifyPropertyChanged (data binding won't work)
- ❌ Hard-code service instantiation (`new RecordingService()`) — use DI
- ❌ Hold state across views without proper cleanup
- ❌ Use `Task.Run` without `ConfigureAwait(false)` in library code

**Pattern Example (DO):**
```csharp
public partial class MainViewModel : ObservableObject
{
    private readonly IRecordingService _recordingService;

    [ObservableProperty]
    private bool isRecording;

    public MainViewModel(IRecordingService recordingService)
    {
        _recordingService = recordingService;
    }

    [RelayCommand]
    private async Task StartRecording()
    {
        IsRecording = await _recordingService.StartAsync();
    }
}
```

---

### C# Services (Stateless, Boundary Validation)

**DO:**
- ✅ Validate all inputs (user data, file paths, API parameters) at service boundaries
- ✅ Use dependency injection (constructor parameters only)
- ✅ Return meaningful errors (custom exceptions with context, not bare ArgumentException)
- ✅ Dispose resources properly (MediaCapture, file handles, timers)
- ✅ Write async methods with `ConfigureAwait(false)` in public APIs
- ✅ Trust internal code (don't re-validate between internal classes)

**DO NOT:**
- ❌ Hold state across method calls (stateless = reusable, thread-safe, testable)
- ❌ Throw bare exceptions without context (include operation name, inputs, error codes)
- ❌ Catch `Exception` broadly (catch specific types: `IOException`, `OperationCanceledException`, etc.)
- ❌ Use `Task.Result` (deadlock risk; use `await` instead)
- ❌ Dispose resources lazily (use `using` statements or dispose in OnSuspending)

**Pattern Example (DO):**
```csharp
public class RecordingService : IRecordingService
{
    public async Task<bool> StartAsync()
    {
        if (string.IsNullOrEmpty(_outputPath))
            throw new InvalidOperationException("Output path not set");

        try
        {
            await _mediaCapture.InitializeAsync();
            return true;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError($"Camera access denied: {ex.Message}");
            return false;
        }
    }

    public void Dispose()
    {
        _mediaCapture?.Dispose();
    }
}
```

---

### C# Tests (MSTest + AAA)

**DO:**
- ✅ Use MSTest framework + Moq for all unit tests
- ✅ Follow AAA pattern: Arrange → Act → Assert
- ✅ Name tests: `MethodName_Scenario_ExpectedResult` (e.g., `LoadItems_OnSuccess_PopulatesCollection`)
- ✅ Aim for ≥80% code coverage on ViewModels and Services
- ✅ Use `async Task` for async tests; add `await Task.Delay(10)` for timing-sensitive tests
- ✅ Mock all Service dependencies; test ViewModel in isolation

**DO NOT:**
- ❌ Name tests generically ("Test1", "DoTest") — names must describe what's being tested
- ❌ Mix concerns (one test = one behavior being verified)
- ❌ Use `[TestInitialize]` for complex setup (make test self-contained)
- ❌ Assert on exact error message text (brittle; assert on exception type instead)
- ❌ Skip coverage calculations or suppress warnings

**Pattern Example (DO):**
```csharp
[TestMethod]
public async Task LoadItems_OnSuccess_PopulatesCollection()
{
    // Arrange
    var mockService = new Mock<IItemService>();
    mockService.Setup(s => s.LoadAsync()).ReturnsAsync(new[] { "Item1", "Item2" });
    var viewModel = new MainViewModel(mockService.Object);

    // Act
    await viewModel.LoadItemsCommand.ExecuteAsync(null);

    // Assert
    Assert.AreEqual(2, viewModel.Items.Count);
}
```

---

### Git Commits & PR References

**DO:**
- ✅ Reference issues in commits: `git commit -m "feat: Add countdown timer (Closes #123)"`
- ✅ Use conventional commits: `feat:`, `fix:`, `refactor:`, `test:`, `style:`, `docs:`, `chore:`
- ✅ Keep subject ≤50 chars; add body if "why" isn't obvious
- ✅ Link PRs to issues via "Closes #123" (GitHub auto-closes on merge)

**DO NOT:**
- ❌ Commit without issue reference (hides traceability)
- ❌ Use generic subjects ("Update", "Fix bug", "WIP") — be specific
- ❌ Commit hard-coded secrets, `.env` files, or certificates
- ❌ Force-push to `main` (use PR review workflow)

---

### Testing Commands

**Before commit:** Format → Build → Test → Secrets scan.

```powershell
# Detect platform
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

# Run all checks
dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
gitleaks detect --source . -v
```

**Run one test class:**
```powershell
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"
```

**Run one test method:**
```powershell
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests.LoadItemsAsync_OnSuccess_PopulatesItems"
```

Details: [agent-build-procedures.md](../instructions/agent-build-procedures.md) · Troubleshooting: [hook-comprehensive.rules.md](../.claude/rules/hook-comprehensive.rules.md)

---

## WRAP Workflow

**W**rite issues before coding · **R**efine instructions per path · **A**tomic tasks (one concern per PR) · **P**air with agent (run checks after each pass, treat output as draft).

Reference issues in commits: `git commit -m "feat: description (Closes #123)"`. See [AGENTS.md § Core Agent Workflow](../AGENTS.md#core-agent-workflow) for details.

---

## Skills Catalog

Copilot does not auto-discover skills; use [SKILLS.md](../SKILLS.md) as the indexed fallback catalog for:
- `accessibility-review` — Audit XAML for WCAG compliance, keyboard navigation, automation properties
- `feature-generation` — Scaffold new WinUI pages, ViewModels, services
- `security-audit` — Review services for secrets, input validation, PII handling

**Invocation:** `/skill-name` or explicit user request.

---

## Conflict Resolution (When Copilot ↔ Claude Code Disagree)

**File precedence (single source of truth):**
1. `.claude/settings.json` — Hook configuration (enforcement)
2. `AGENTS.md` — Core agent workflow (overrides all tool-specific guidance)
3. `CLAUDE.md` — CLI reference (doesn't affect Copilot)
4. `.github/copilot-instructions.md` — Copilot Agent Mode specifics (path patterns only)
5. `.claude/rules/` — Governance details (design, destructive commands, git hooks)

**Principle:** No conflicts should arise — each file has a distinct audience. Hooks keep them synchronized.

---

## Metadata & Schema Notes

This file follows **YAML frontmatter + Markdown body** pattern for machine-readability:
- Tools: GitHub Copilot Agent Mode, Claude Code (referenced)
- Version: 2.2 (updated 2026-08-30)
- Cascade: `.claude/settings.json` → `AGENTS.md` → `.github/copilot-instructions.md` → `.claude/rules/*`

**Updated for August 2026 best practices:**
- ✅ Explicit model selection (Opus for full-context, Sonnet for fast lints)
- ✅ Extended thinking for >3 file changes (effortLevel: high)
- ✅ Tighter "Do/Do Not" rules (no ambiguity on XAML/MVVM/Testing patterns)
- ✅ Cascading instruction set (reduces duplication, clarifies precedence)
- ✅ Token optimization (focused, scoped instructions)

