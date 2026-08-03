# GitHub Copilot Agent Mode & Architecture Index — IntVue

**Quick Links:** [CLAUDE.md](../CLAUDE.md) | [AGENTS.md](../AGENTS.md) | [DESIGN.md](../DESIGN.md) | [Rules](./copilot/rules/) | [Instructions](../instructions/) | [Design Rules](./../.claude/rules/)

This is the comprehensive architecture and pattern reference. For quick start, see [CLAUDE.md](../CLAUDE.md). For agent workflows, see [AGENTS.md](../AGENTS.md).

---

## Authorization Boundaries

**✅ Auto:** Bug fixes, test additions, docs, formatting, config updates (no breaking changes)  
**⚠️ Confirm:** Architecture, APIs, dependencies, manifest, build scripts, storage schemas  
**🚫 Blocked:** Secrets, releases, package identity changes, cross-cutting refactors

---

## System Architecture

**Data Flow:**
```
User Input (Views/XAML) → ViewModel (State + Commands) → Services (Business Logic) → Models (Data)
```

**Key Technologies:**
- **UI:** WinUI 3 with `x:Bind` data binding, no code-behind logic
- **State:** CommunityToolkit.Mvvm with `[ObservableProperty]` + `[RelayCommand]`
- **DI:** Constructor injection via `App.xaml.cs`
- **Media:** `MediaCapture` + `MediaFrameReader` + Win2D preview
- **Storage:** `.mp4` recordings to Documents/IntVue

**Platforms:** x86, x64, ARM64 (always detect via `$Platform` variable)

---

## Pattern Reference & Rule Mapping

### XAML UI Layer (`**/*.xaml`)

**Apply Rules:** [`.github/copilot/rules/xaml-binding.rules.yaml`](./copilot/rules/xaml-binding.rules.yaml)

**Design Guidance:**
- Colors: [`.claude/rules/design-colors.rules.md`](../.claude/rules/design-colors.rules.md) — Use only `{ThemeResource ...Brush}`
- Components: [`.claude/rules/design-components.rules.md`](../.claude/rules/design-components.rules.md) — Button states, form controls, accessibility
- Spacing: [`.claude/rules/design-spacing.rules.md`](../.claude/rules/design-spacing.rules.md) — 8px base grid (4, 8, 12, 16, 24)
- Typography: [`.claude/rules/design-typography.rules.md`](../.claude/rules/design-typography.rules.md) — Font sizes (12, 14, 16, 20, 32, 72), weights (Regular, SemiBold, Bold)

**Implementation Standards:**
- Use `x:Bind` (compile-time safe), never `{Binding}` (runtime-resolved)
- All interactive controls: add `AutomationProperties.Name`
- No code-behind logic; use binding to ViewModel
- Reference: [WinUI Best Practices](../instructions/winui-best-practices.instructions.md) | [Accessibility](../instructions/accessibility.instructions.md)

**Enforcement:** OnFileSave hook (2s delay) validates XAML syntax and theme resource usage

---

### ViewModel Layer (`ViewModels/**/*.cs`)

**Apply Rules:** [`.github/copilot/rules/viewmodel-patterns.rules.yaml`](./copilot/rules/viewmodel-patterns.rules.yaml)

**MVVM Requirements:**
- Inherit from `CommunityToolkit.Mvvm.ComponentModel.ObservableObject`
- Mark class `partial` for source generation
- Use `[ObservableProperty]` for properties (not traditional properties)
- Use `[RelayCommand]` for commands (not manual `ICommand`)
- Constructor dependency injection only

**Async/Await:**
- All I/O and long-running operations must be `async`
- No `.Result`, `.Wait()`, or `Thread.Sleep()`
- Command methods: `public async Task MethodNameAsync()`

**Reference:** [WinUI Best Practices](../instructions/winui-best-practices.instructions.md) | [Code Quality](../instructions/code-quality.instructions.md) | [Performance](../instructions/performance.instructions.md)

**Enforcement:**
- OnFileSave hook (3s delay) validates MVVM patterns
- StyleCop SA* rules (pre-commit) enforce XML docs, proper using statements
- Pre-commit tests require ≥80% coverage on all public methods

---

### Service & Business Logic Layer (`Services/**/*.cs`)

**Apply Rules:** [`.github/copilot/rules/service-patterns.rules.yaml`](./copilot/rules/service-patterns.rules.yaml)

**Service Requirements:**
- Define interface (`IServiceName`) for all services
- Stateless design (state via dependency injection)
- Validate all inputs at method boundary (throw `ArgumentNullException`, `ArgumentException`)
- Use async I/O exclusively (no `.Result`, `.Wait()`, blocking operations)
- Implement `IDisposable` for resources (file handles, media captures)

**Security & Safety:**
- Never hardcode secrets, API keys, or connection strings
- Use `Environment.GetEnvironmentVariable()`, `PasswordVault`, or `Azure.Security.KeyVault`
- Do NOT log PII (emails, phone numbers, addresses)
- Sanitize external input; validate before use

**Advanced:**
- Support `CancellationToken` for cancellable operations
- Graceful error handling (catch specific exceptions, return meaningful results)
- Async method naming: `MethodNameAsync()` suffix required

**Reference:** [Security](../instructions/security.instructions.md) | [Performance](../instructions/performance.instructions.md) | [Windows APIs](../instructions/windows-apis.instructions.md)

**Enforcement:**
- OnFileSave hook (3s delay) validates service patterns and security rules
- Security-audit skill audits media capture, permissions, PII handling
- Pre-commit build validates no hardcoded secrets

---

### Test Layer (`**/*Tests.cs`)

**Apply Rules:** [`.github/copilot/rules/test-patterns.rules.yaml`](./copilot/rules/test-patterns.rules.yaml)

**MSTest Standards:**
- Use `[TestClass]` and `[TestMethod]` attributes only
- Naming: `MethodName_Scenario_ExpectedResult` (e.g., `StartCountdownAsync_WithThreeSeconds_ReportsAllValues`)
- AAA pattern: **Arrange** (setup) → **Act** (call) → **Assert** (verify)
- One logical assertion per test; split different scenarios into separate tests

**Mocking & Dependencies:**
- Mock all external dependencies via `Moq`
- Use `[TestInitialize]` for setup, `[TestCleanup]` for teardown
- Inject mocked dependencies via constructor
- Never create real Service instances in tests

**Coverage & Edge Cases:**
- Target: ≥80% coverage for all public methods
- Test edge cases: null input, empty collections, boundary values
- Test error paths: exceptions, invalid state transitions

**Async Tests:**
- Mark test `public async Task MethodName()`
- Use `await` when calling async methods (no `.Result`, `.Wait()`)
- Minimal delays: `await Task.Delay(10-100)` only

**Reference:** [Testing](../instructions/testing.instructions.md) | [Code Quality](../instructions/code-quality.instructions.md)

**Enforcement:**
- OnFileSave hook (2s delay) validates MSTest syntax
- PostToolUse hook validates test coverage ≥80%
- Pre-commit hook runs all tests; must pass before commit
- Pre-push hook runs full test suite with coverage report

---

## Pre-Commit & Pre-Push Hooks

### Mandatory Checks (Block Commits)

1. **Formatting** → `dotnet format --verify-no-changes`
   - Fix: `dotnet format IntVue.csproj`

2. **Build** → `dotnet build -c Debug -p:Platform=$Platform`
   - Platform detection: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
   - Fix: Read compiler error, add missing `using`, fix type mismatch, etc.

3. **Tests** → `dotnet test -c Debug -p:Platform=$Platform`
   - Coverage: ≥80% on changed code
   - All tests must pass (bypass available: `SKIP_TESTS_ON_FAILURE=1 git commit`)

4. **Secret Scanning** → `gitleaks detect --source . -v`
   - Never commit API keys, passwords, connection strings

### Reference

See **[`.claude/rules/hook-comprehensive.rules.md`](../.claude/rules/hook-comprehensive.rules.md)** for quick fixes, detailed resolution, and prevention strategies.

---

## Copilot Configuration

**Hook Triggers:**
- `OnFileSave` — Validates file patterns against rule YAML (XAML, ViewModel, Service, Test)
- `PostToolUse` — Validates coverage after edits
- `PreToolUse` — Warns if `dotnet` commands missing platform/config flags

**Rule References:**
- YAML rules in `.github/copilot/rules/` reference instruction files and design rules
- Design rules in `.claude/rules/design-*.rules.md` provide low-level tokens and patterns
- Instruction files in `.github/instructions/` provide comprehensive domain guidance

---

## Development Workflow

1. **Read rules for your file type** (XAML, ViewModel, Service, or Test)
2. **Follow the pattern examples** in the corresponding YAML file
3. **Use IDE auto-format** (Rider/VS: Tools → Settings → Format on Save)
4. **Run pre-commit checks locally:**
   ```powershell
   dotnet format IntVue.csproj
   dotnet build -c Debug -p:Platform=x64
   dotnet test -c Debug -p:Platform=x64
   ```
5. **Commit with issue reference:** `git commit -m "feat: description (Closes #123)"`

---

## Acceptance Criteria

✅ Build succeeds (0 errors)  
✅ Tests pass (≥80% coverage)  
✅ Formatting compliant  
✅ No secrets in code  
✅ Commit references issue  
✅ All public methods tested  
✅ Rules respected (XAML, ViewModel, Service, Test patterns)

---

**Last Updated:** 2026-08-02 | **Version:** 2.0 | **Phase:** 1 (Foundation)
