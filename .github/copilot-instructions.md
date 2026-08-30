---
title: Copilot Agent Mode — IntVue
version: "2.2"
---

# Copilot Agent Mode

**Tech Stack:** WinUI 3, MVVM + DI, .NET 10.0+. Read [DESIGN.md](../DESIGN.md) for architecture.

## Scope

| Task | Auto? | Confirm? | Block? |
|---|---|---|---|
| Bug fixes, tests, docs, formatting, config | ✅ | — | — |
| Architecture, APIs, dependencies, build scripts | — | ✅ | — |
| Secrets, releases, refactors >3 files | — | — | ✅ |

## Quick Rules

| Category | DO ✅ | DO NOT ❌ |
|---|---|---|
| **XAML** | `x:Bind`, `{ThemeResource ...}`, `AutomationProperties.Name` | `{Binding}`, hard-coded colors, omit labels |
| **ViewModel** | `ObservableObject`, `[ObservableProperty]`, `RelayCommand`, DI | Business logic, `new Service()` |
| **Service** | Stateless, validate inputs, dispose resources, `ConfigureAwait(false)` | State across calls, bare exceptions |
| **Tests** | MSTest + Moq, AAA, `Method_Scenario_Result`, ≥80% coverage | Generic names, mixed concerns |
| **Git** | `Closes #123`, conventional commits, ≤50 chars | No issue ref, generic subjects |

**Detailed patterns:** [copilot-patterns.rules.md](../../.claude/rules/copilot-patterns.rules.md)

## Build

```powershell
$Platform = if ($env:PROCESSOR_ARCHITECTURE -eq 'AMD64') { 'x64' } else { $env:PROCESSOR_ARCHITECTURE }
dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
```

## Workflow

**WRAP:** Write issues · Refine instructions · Atomic tasks · Pair with agent.

## Skills

- `/accessibility-review`, `/feature-generation`, `/security-audit` ([SKILLS.md](../SKILLS.md))

See [AGENTS.md](../AGENTS.md) for full workflow, Two-Gate System, hook config.

