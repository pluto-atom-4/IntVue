---
title: "GitHub Copilot Agent Mode — IntVue WinUI 3"
version: "2.2"
updated: "2026-08-30"
audience: "GitHub Copilot Agent Mode (auto-discovered via glob patterns)"
---

# Copilot Agent Mode — IntVue WinUI 3

## Tech Stack

WinUI 3 / Windows App SDK 1.8.x, MVVM + DI, .NET 10.0+. **Read [DESIGN.md](../DESIGN.md) first** — it's the single source of truth for architecture, tech boundaries, and design tokens.

---

## Scope

| Task | Auto? | Confirm? | Block? |
|---|---|---|---|
| Bug fixes, tests, docs, formatting, config | ✅ | — | — |
| Architecture, APIs, dependencies, build scripts | — | ✅ | — |
| Secrets, releases, refactors >3 files | — | — | ✅ |

Path-specific rules auto-apply: `**/*.xaml` → x:Bind/ThemeResource; `**/*ViewModel.cs` → MVVM/DI; `**/*Service.cs` → stateless/validation; `**/*Tests.cs` → MSTest/AAA/coverage.

---

## Quick Rules

| Category | DO ✅ | DO NOT ❌ |
|---|---|---|
| **XAML** | `x:Bind`, `{ThemeResource ...}`, `AutomationProperties.Name`, spacing scale (8px grid), CornerRadius (2/8/16) | `{Binding}`, hard-coded colors/spacing/fonts, omit accessibility labels |
| **ViewModel** | `ObservableObject`, `[ObservableProperty]`, `RelayCommand`, DI via constructor, boundary validation | Business logic in ViewModel, auto-properties without INPC, `new Service()`, hold state across views |
| **Service** | Stateless, validate inputs, dispose resources, `ConfigureAwait(false)`, meaningful errors | State across calls, bare exceptions, `Task.Result`, lazy disposal |
| **Tests** | MSTest + Moq, AAA pattern, name `Method_Scenario_Result`, ≥80% coverage, mock dependencies | Generic names, mixed concerns, complex `[TestInitialize]`, assert on error text |
| **Git** | Reference issues (Closes #123), conventional commits, ≤50 char subject | Commit without issue ref, generic subjects, secrets/certs, force-push main |

**Detailed patterns & examples:** [.claude/rules/copilot-patterns.rules.md](../../.claude/rules/copilot-patterns.rules.md)

---

## Build Commands

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
```

Run specific test: `dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~TestClassName"`

---

## WRAP Workflow

**W**rite issues · **R**efine instructions · **A**tomic tasks · **P**air with agent (run checks after each pass).

Reference issues in commits: `git commit -m "feat: description (Closes #123)"`.

---

## Skills & Reference

- **Accessibility audit:** `/accessibility-review` ([SKILLS.md](../SKILLS.md))
- **Feature scaffold:** `/feature-generation` ([SKILLS.md](../SKILLS.md))
- **Security audit:** `/security-audit` ([SKILLS.md](../SKILLS.md))

Detailed guidance: [AGENTS.md](../AGENTS.md) (14-step workflow, Two-Gate System, hook config, conflict resolution).

---

## File Precedence (Single Source of Truth)

1. `.claude/settings.json` — Hook config (enforcement)
2. `AGENTS.md` — Agent workflow (overrides all)
3. `CLAUDE.md` — CLI reference
4. `.github/copilot-instructions.md` — Copilot-only patterns
5. `.claude/rules/` — Governance details

