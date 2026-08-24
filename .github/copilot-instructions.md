# Copilot Agent Mode - IntVue

## Tech Stack Anchor

Architecture, tech boundaries, and data flow are defined in [DESIGN.md](../DESIGN.md)
(§ System Architecture, § Technology Boundaries). Read it before generating code — it is the
single source of truth for the stack: WinUI 3 / Windows App SDK, MVVM + DI, `x:Bind`-only XAML,
and semantic design tokens (never hard-coded colors/spacing/type sizes).

## Scope

**Auto:** Bug fixes, tests, docs, formatting, config
**Confirm:** Architecture, APIs, dependencies, build scripts
**Blocked:** Secrets, releases, refactors

## Pattern Rules

Copilot Agent Mode auto-appends path-specific rules (XAML → x:Bind, ViewModels → MVVM, Services → validation/async, Tests → MSTest/AAA). See [.github/copilot/README.md](copilot/README.md) for full glob precedence and [.github/instructions/](../instructions/) for detailed patterns.

## Skills Catalog

Copilot does not auto-discover skills; use [SKILLS.md](../SKILLS.md) as the indexed fallback catalog for accessibility-review, feature-generation, security-audit.

## Checks & Build Procedure

**Before commit:** Format → Build → Test → Secrets scan.

```powershell
dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=$Platform
dotnet test -c Debug -p:Platform=$Platform
gitleaks detect --source . -v
```

Details: [agent-build-procedures.md](../instructions/agent-build-procedures.md) · Troubleshooting: [hook-comprehensive.rules.md](../.claude/rules/hook-comprehensive.rules.md)

## WRAP Workflow

**W**rite issues before coding · **R**efine instructions per path · **A**tomic tasks (one concern per PR) · **P**air with agent (run checks after each pass, treat output as draft). Reference issues in commits: `git commit -m "feat: description (Closes #123)"`. See [AGENTS.md § Core Agent Workflow](../AGENTS.md#core-agent-workflow) for details.

**v2.1** | Updated 2026-08-23
