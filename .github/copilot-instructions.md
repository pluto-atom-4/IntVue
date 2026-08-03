# GitHub Copilot Agent Mode — IntVue

**Quick Links:** [CLAUDE.md](../CLAUDE.md) | [AGENTS.md](../AGENTS.md) | [Rules](./copilot/rules/) | [Instructions](../instructions/)

## Authorization Boundaries

**✅ Auto:** Bug fixes, test additions, docs, formatting, config updates (no breaking changes)  
**⚠️ Confirm:** Architecture, APIs, dependencies, manifest, build scripts, storage schemas  
**🚫 Blocked:** Secrets, releases, package identity changes, cross-cutting refactors

---

## Pattern Rules by Layer

**XAML UI** — [Rules](./copilot/rules/xaml-binding.rules.yaml) | [Design](../.claude/rules/) | [Accessibility](../instructions/accessibility.instructions.md)
- Use `x:Bind`, `{ThemeResource ...}` only; add `AutomationProperties.Name`

**ViewModel** — [Rules](./copilot/rules/viewmodel-patterns.rules.yaml) | [Code Quality](../instructions/code-quality.instructions.md)
- Inherit `ObservableObject` (partial); `[ObservableProperty]` + `[RelayCommand]`
- Constructor DI, async I/O, ≥80% coverage

**Service Layer** — [Rules](./copilot/rules/service-patterns.rules.yaml) | [Security](../instructions/security.instructions.md)
- Interface, validate inputs, async I/O, `IDisposable`, no hardcoded secrets

**Tests** — [Rules](./copilot/rules/test-patterns.rules.yaml) | [Testing](../instructions/testing.instructions.md)
- MSTest, `MethodName_Scenario_ExpectedResult`, AAA, ≥80% coverage, mock dependencies

---

## Pre-Commit Checks

1. **Format:** `dotnet format IntVue.csproj`
2. **Build:** `dotnet build -c Debug -p:Platform=$Platform`
3. **Test:** `dotnet test -c Debug -p:Platform=$Platform` (bypass: `SKIP_TESTS_ON_FAILURE=1`)
4. **Secrets:** `gitleaks detect --source . -v`

[Quick Fix](../.claude/rules/hook-quick-fix.rules.md) | [Detailed Guide](../.claude/rules/hook-resolution.rules.md)

---

## Development Workflow

1. Read pattern rules for your file type
2. Follow examples in YAML rule file
3. Enable IDE auto-format
4. Run pre-commit checks locally
5. Commit with issue reference: `git commit -m "feat: description (Closes #123)"`

---

## Acceptance Criteria

✅ Build succeeds (0 errors)  
✅ Tests pass (≥80% coverage)  
✅ Formatting compliant  
✅ No secrets  
✅ Commit references issue  

---
**Last Updated:** 2026-08-02 | **Version:** 2.0
