# CLAUDE.md — IntVue WinUI 3

WinUI 3 (Windows App SDK 1.8.x) interview practice app. MVVM + DI. .NET 10.0+.

> **Agents:** Read [AGENTS.md](./AGENTS.md) first (Two-Gate System, Core Workflow).
> **Designers:** Read [DESIGN.md](./DESIGN.md) first (semantic tokens, design rules).
> **Skills:** [SKILLS.md](./SKILLS.md) — indexed catalog + auto-discovery/fallback mechanism.
> **All rules:** [.github/copilot-instructions.md](.github/copilot-instructions.md).

## Platform Detection (Mandatory)

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

Pass `-p:Platform=$Platform` to every `dotnet build`/`test`/`run`.

## Before Committing

```powershell
dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
git commit -m "feat: description"
```

Commit/push blocked (formatting, build, tests)? → [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md)
Command blocked/asked (destructive command, missing `-c`/`-p:Platform`)? → [destructive-command-governance.rules.md](.claude/rules/destructive-command-governance.rules.md)
Full build/run/deploy details: [AGENTS.md § Build, Run & Deploy](./AGENTS.md#build-run--deploy)

## Exact Test & Lint Syntax

```powershell
# Run one test class
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"

# Run one test method
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests.LoadItemsAsync_OnSuccess_PopulatesItems"

# Run all tests in a folder/namespace (e.g. all ViewModel tests)
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~Tests.ViewModels"

# Lint check only, no files rewritten (use before committing to preview diffs)
dotnet format IntVue.csproj --verify-no-changes
```

Full filter syntax & on-demand test scoping: [testing.instructions.md § Running Tests On-Demand](.github/instructions/testing.instructions.md#3-test-organization)
Lint rules source of truth: `.editorconfig` + `stylecop.json` (SA*/CA*/IDE* analyzers) — see [code-quality.instructions.md](.github/instructions/code-quality.instructions.md)

## Core Guardrails

- **XAML:** `x:Bind` (never `{Binding}`), `{ThemeResource ...}` (never hard-code)
- **Namespaces:** `Microsoft.UI.Xaml` only
- **Secrets:** Env vars, PasswordVault, or Azure Key Vault
- **MediaCapture:** Don't hold open during suspend/resume
- **Testing:** 80%+ coverage on ViewModels/Services
- **Code:** YAGNI—implement only what's explicitly requested

## Rules by Category

| Category | Reference |
|---|---|
| **Design & UI** | [DESIGN.md](./DESIGN.md) |
| **Agent Workflow** | [AGENTS.md](./AGENTS.md) |
| **Skills Catalog** | [SKILLS.md](./SKILLS.md) |
| **Code Quality** | [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) |
| **Git Hooks** | [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md) |
| **Destructive Command Governance** | [destructive-command-governance.rules.md](.claude/rules/destructive-command-governance.rules.md) |
| **All Rules** | [copilot-instructions.md](.github/copilot-instructions.md) |
