# GitHub Copilot Agent Mode — IntVue Configuration

**For Claude Code:** See [CLAUDE.md](../CLAUDE.md) | **For Agents:** See [AGENTS.md](../AGENTS.md)

This file configures GitHub Copilot's Agent Mode with repo-wide boundaries, path-specific overrides, and verification gates.

---

## Repo-Wide Boundaries

### ✅ Automatic Changes (No Human Confirmation)
- **Bug fixes:** Typos, null checks, obvious logic errors in existing functions
- **Test additions:** New test methods in existing test files (MSTest, AAA pattern)
- **Documentation:** README.md, Docs/, inline comments, .md files (no semantic changes)
- **Config updates:** .editorconfig, stylecop.json, .gitignore (no breaking changes)
- **Formatting:** Running `dotnet format` across modified files

### ⚠️ Changes Requiring Human Confirmation
- **Architecture changes:** New folders, new layers (Services, Converters, etc.)
- **API additions:** New public methods, interface signatures, property changes
- **Dependencies:** NuGet package additions, version upgrades
- **Manifest changes:** Package.appxmanifest capabilities, publisher, AUMID
- **Build scripts:** build.sh, publish.sh, .csproj modifications
- **Database/Storage:** New models, schema changes, file format changes

### 🚫 Blocked Changes (Manual Only)
- **Secrets:** Any API keys, connection strings, passwords, credentials
- **Production release:** Version bumps, release tagging, MSIX signing
- **Package identity reset:** Changes to app publisher, AUMID, or certificate
- **Cross-cutting refactors:** Affecting >50% of codebase, breaking changes

---

## Path-Specific Rules

See [.github/copilot/rules-detailed.md](./copilot/rules-detailed.md) for complete YAML examples and code snippets.

### XAML Files (`**/*.xaml`)
- **Binding:** `x:Bind` only; never `{Binding}`
- **Colors:** `{ThemeResource ...Brush}` only; never hard-coded hex
- **Spacing:** 8px grid: `Spacing="8"`, `Padding="12"`, `Margin="16"`
- **Accessibility:** Every interactive control has `AutomationProperties.Name`
- **Namespaces:** `Microsoft.UI.Xaml` only
- **Performance:** Use `x:Load` for conditionals; avoid deep nesting
- **Structure:** Views contain layout/styling only; no business logic
- **Ref:** [accessibility.instructions.md](../instructions/accessibility.instructions.md)

### C# ViewModels (`ViewModels/**/*.cs`)
- **Base:** Inherit `ObservableObject` (CommunityToolkit.Mvvm)
- **Properties:** Use `[ObservableProperty]` attribute
- **Commands:** Use `[RelayCommand]` attribute
- **Logic:** Services only; ViewModel exposes state
- **Syntax:** Modern C# 11+, nullable reference types, switch expressions
- **Async:** All I/O must be async; never block UI thread
- **DI:** Constructor inject; no `App.Services.GetService<T>()`
- **Tests:** Every public method testable with MSTest + Moq
- **Ref:** [winui-best-practices.instructions.md](../instructions/winui-best-practices.instructions.md)

### Services (`Services/**/*.cs`)
- **Stateless:** Singletons only; no mutable state
- **Validation:** Validate all boundary inputs
- **Async:** All I/O/network/file ops must be async
- **Secrets:** Use env vars, PasswordVault, or Azure Key Vault
- **Errors:** Catch specific exceptions; log with context
- **MediaCapture:** Dispose on suspend/resume
- **Naming:** Descriptive imperative (e.g., `StartRecordingAsync()`)
- **Ref:** [security.instructions.md](../instructions/security.instructions.md)

### Test Files (`**/*Tests.cs`)
- **Framework:** MSTest only
- **Naming:** `MethodName_Scenario_ExpectedResult`
- **Pattern:** Arrange-Act-Assert (see detailed rules)
- **Mocking:** Use Moq for external deps
- **Coverage:** 80%+ target for ViewModels/Services
- **Data:** Use fixtures or builders
- **Async:** Use `async Task` for async tests
- **Ref:** [testing.instructions.md](../instructions/testing.instructions.md)

---

## Pre-Commit Verification Checklist

- [ ] Detect platform: `$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
- [ ] Build: `dotnet build -c Debug -p:Platform=$Platform` (0 errors)
- [ ] Test: `dotnet test -c Debug -p:Platform=$Platform` (all pass, 80%+ coverage)
- [ ] Format: `dotnet format --verify-no-changes`
- [ ] No secrets, API keys, or PII
- [ ] Commit message references issue (`Closes #123`)
- [ ] New public methods have unit tests
- [ ] BREAKING changes documented

**Bypass:** `RUN_TESTS=0 git commit` for docs-only changes only.

---

## C# Code Style

- **Syntax:** Modern C# 11+, nullable reference types, switch expressions
- **Names:** Explicit camelCase locals (`isRecording`, `currentTime`)
- **MVVM:** `ObservableObject` base; `[ObservableProperty]` + `[RelayCommand]`
- **Async:** `async/await` for all I/O; never `Wait()` or `Result`
- **DI:** Constructor inject; no `ServiceLocator`

---

## Quick Links

- [CLAUDE.md](../CLAUDE.md) — CLI quick start
- [AGENTS.md](../AGENTS.md) — Agent framework & Two-Gate System
- [DESIGN.md](../DESIGN.md) — Architecture & design system
- [.github/instructions/](../instructions/) — 11 scoped rule files
- [.claude/settings.json](../.claude/settings.json) — Hook configuration
- [.github/copilot/rules-detailed.md](./copilot/rules-detailed.md) — Detailed examples

---

## Acceptance Criteria (for Copilot PRs)

A PR from Copilot Agent Mode is ready for merge when:

✅ All verification checks pass (build, tests, formatting)  
✅ Changes respect path-specific rules  
✅ New public APIs have unit tests  
✅ No secrets or hard-coded values  
✅ Commit message references issue  
✅ PR description links to this file if boundaries modified  

---

**Last Updated:** 2026-07-25 | **Config Version:** 1.0
