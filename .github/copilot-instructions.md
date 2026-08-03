# Copilot Agent Mode - IntVue

## Scope

**Auto:** Bug fixes, tests, docs, formatting, config  
**Confirm:** Architecture, APIs, dependencies, build scripts  
**Blocked:** Secrets, releases, refactors

## Pattern Rules

**XAML:** Use `x:Bind`, `{ThemeResource}`, `AutomationProperties.Name`  
[Details](../.claude/rules/)

**ViewModel:** `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`, DI  
[Details](../instructions/code-quality.instructions.md)

**Services:** Interfaces, validation, async, `IDisposable`  
[Details](../instructions/security.instructions.md)

**Tests:** MSTest, AAA, ≥80% coverage, mocked dependencies  
[Details](../instructions/testing.instructions.md)

## Checks

1. Format: `dotnet format IntVue.csproj`
2. Build: `dotnet build -c Debug -p:Platform=$Platform`
3. Test: `dotnet test -c Debug -p:Platform=$Platform`
4. Secrets: `gitleaks detect --source . -v`

[Quick Fix](../.claude/rules/hook-quick-fix.rules.md)

## Workflow

1. Read pattern rules
2. Follow examples in rules
3. Run checks before commit
4. Reference issue: `git commit -m "feat: x (Closes #123)"`

**v2.0** | Updated 2026-08-02
