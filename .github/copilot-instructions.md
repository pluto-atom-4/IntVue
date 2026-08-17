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

## Pattern Rules (Path-Specific, Auto-Discovered)

Copilot Agent Mode auto-appends the matching rule file(s) below when it touches a path. Full
glob-matching mechanics and precedence order: [.github/copilot/README.md](copilot/README.md).

| Path glob | Rule file | Domain |
|---|---|---|
| `**/*.xaml` | [xaml-binding.rules.yaml](copilot/rules/xaml-binding.rules.yaml) | Views/XAML — `x:Bind`, `{ThemeResource}`, `AutomationProperties.Name` |
| `ViewModels/**/*.cs` | [viewmodel-patterns.rules.yaml](copilot/rules/viewmodel-patterns.rules.yaml) | `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`, DI |
| `Services/**/*.cs` | [service-patterns.rules.yaml](copilot/rules/service-patterns.rules.yaml) | Interfaces, validation, async, `IDisposable`, no hardcoded secrets |
| `**/*Tests.cs`, `**/*Test.cs` | [test-patterns.rules.yaml](copilot/rules/test-patterns.rules.yaml) | MSTest, AAA, ≥80% coverage, mocked dependencies |

Detailed worked examples: [rules-detailed.md](copilot/rules-detailed.md) · Full instruction
files: [.github/instructions/](../instructions/) · Design tokens: [.claude/rules/](../.claude/rules/)

## Skills Catalog (Fallback for Non-Auto-Discovering Tools)

Copilot Agent Mode does not auto-discover `.claude/skills/*/SKILL.md` the way Claude Code
does. Use [SKILLS.md](../SKILLS.md) at the repo root as the indexed fallback catalog — open the
linked `SKILL.md` file directly when a task matches one of its trigger phrases (accessibility
review, feature scaffolding, security/media-capture audit).

## Checks

1. Format: `dotnet format IntVue.csproj`
2. Build: `dotnet build -c Debug -p:Platform=$Platform`
3. Test: `dotnet test -c Debug -p:Platform=$Platform`
4. Secrets: `gitleaks detect --source . -v`

[Quick Fix](../.claude/rules/hook-comprehensive.rules.md)

## WRAP Workflow

- **W**rite issues — capture the task as a GitHub issue or atomic request before generating
  code; reference it in the commit (`Closes #123`).
- **R**efine instructions — read the pattern rule(s) for the path(s) you're touching (table
  above) before writing code, not after.
- **A**tomic tasks — one concern per PR/commit (one bug fix, one feature slice); keep diffs
  small enough to review against the Scope table above.
- **P**air with agent — run the Checks above after every generation pass; treat Copilot output
  as a draft the human/agent verifies, not a final answer.

Reference issue in commits: `git commit -m "feat: x (Closes #123)"`

**v2.1** | Updated 2026-08-16
