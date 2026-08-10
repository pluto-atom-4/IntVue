# CLAUDE.md — IntVue WinUI 3

WinUI 3 (Windows App SDK 1.8.x) interview practice app. MVVM + DI. .NET 10.0+.

> **Agents:** Read [AGENTS.md](./AGENTS.md) first (Two-Gate System, Core Workflow).
> **Designers:** Read [DESIGN.md](./DESIGN.md) first (semantic tokens, design rules).
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

Blocked? → [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md)
Full build/run/deploy details: [AGENTS.md § Build, Run & Deploy](./AGENTS.md#build-run--deploy)

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
| **Code Quality** | [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) |
| **Git Hooks** | [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md) |
| **All Rules** | [copilot-instructions.md](.github/copilot-instructions.md) |
