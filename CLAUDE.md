# CLAUDE.md — IntVue WinUI 3

WinUI 3 (Windows App SDK 1.8.x) interview practice app. MVVM + DI. .NET 10.0+.

> **Agents:** Read [AGENTS.md](./AGENTS.md) first (Two-Gate System, Core Workflow).
> **Designers:** Read [DESIGN.md](./DESIGN.md) first (semantic tokens, design rules).
> **Skills:** [SKILLS.md](./SKILLS.md) — indexed catalog + auto-discovery/fallback mechanism.
> **All rules:** [.github/copilot-instructions.md](.github/copilot-instructions.md).
> **Graph tools:** [graph-tools.rules.md](.claude/rules/graph-tools.rules.md) — code-review-graph (blast-radius) + graphify (macro map) routing.

## Quick Start (5 Steps)

1. Clone the repo: `git clone <repo>`
2. Detect platform in every shell session: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
3. Build: `dotnet build -c Debug -p:Platform=$Platform`
4. Test: `dotnet test -c Debug -p:Platform=$Platform`
5. Read [AGENTS.md](./AGENTS.md) § Two-Gate System before implementing features

## Platform Detection (Mandatory)

**Platform detection must run in every PowerShell session before any dotnet command.**

```powershell
# Detect platform (x86, x64, ARM64)
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

Pass `-p:Platform=$Platform` to every `dotnet build`/`test`/`run`/`publish`.

**Why it matters:** Windows supports x86, x64, and ARM64. The platform flag ensures binaries build for your machine architecture. Missing it causes linker errors or silent failures on ARM64 devices.

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

## Configuration Files Overview

IntVue uses layered configuration files to separate concerns and avoid duplication. Here's the map:

| File | Purpose | Audience | Scope |
|---|---|---|---|
| **CLAUDE.md** (this file) | Quick reference for CLI users (Claude Code agents) | Claude Code agents | Build, test, lint, platform detection, core guardrails |
| **[AGENTS.md](./AGENTS.md)** | Comprehensive agent workflow, Two-Gate System, tool matrix | Agents (both Claude Code and GitHub Copilot) | Execution lifecycle, plan mode, verification gates, hook configuration |
| **[DESIGN.md](./DESIGN.md)** | Design system tokens, XAML patterns, accessibility rules | Designers, UI developers | Semantic colors, spacing scale, typography, component patterns |
| **[SKILLS.md](./SKILLS.md)** | Indexed catalog of custom skills + auto-discovery mechanism | All agents | Accessibility audits, feature scaffolding, security reviews |
| **.github/copilot-instructions.md** | GitHub Copilot Agent Mode rules (path-specific, auto-discovered) | GitHub Copilot Agent Mode only | Pattern rules, scope matrix, WRAP workflow |
| **.claude/settings.json** | Hook configuration (PreToolUse, PostToolUse) + permission matrix | Claude Code harness | Destructive command blocking, platform/config warnings, issue closure approval gate |
| **.claude/rules/** | Fine-grained governance rules (design, destructive commands, git hooks, GitHub) | All agents | Specific policies with detailed walkthroughs |

**Key distinction:** `.github/` rules apply to Copilot Agent Mode (auto-discovered via glob patterns); `.claude/` rules apply to Claude Code and enforcement hooks; DESIGN.md is the single source of truth for all color/spacing/type tokens.

## Core Guardrails (10 Critical Rules)

These rules prevent silent failures, security breaches, and architectural debt. Violation blocks build/test hooks or requires explicit approval.

- **XAML Binding:** `x:Bind` only (never `{Binding}`); use `{ThemeResource ...}` for all colors/spacing/fonts (never hard-code hex/px values)
- **Namespaces:** `Microsoft.UI.Xaml` only; no `System.Windows` or legacy WinForms
- **Secrets:** Never hard-code API keys, tokens, or passwords. Use environment variables (dev), PasswordVault (user), or Azure Key Vault (production)
- **MediaCapture:** Don't hold open during app suspend/resume; dispose in `OnSuspending` event
- **Testing:** Minimum 80% code coverage on ViewModels and Services; all public APIs require unit tests
- **YAGNI Principle:** Implement only what's explicitly requested in the issue/PR. Avoid "future-proofing" (see [AGENTS.md § Design Principles](.github/instructions/design-principles.instructions.md))
- **Two-Gate System:** All changes >3 files or >200 LOC require Plan Mode review before implementation (see [AGENTS.md § Two-Gate System](./AGENTS.md#two-gate-system-evidence-based-execution))
- **Platform Detection:** Always pass `-p:Platform=$Platform` to dotnet commands (detected per-session, not per-machine)
- **Error Handling:** Validate at boundaries (user input, file I/O, network, media); trust internal code; log with context
- **Dependencies:** Use constructor injection (DI container) for all service dependencies; avoid service locator pattern

## Rules by Category

| Category | Reference | What You'll Find |
|---|---|---|
| **Design & UI** | [DESIGN.md](./DESIGN.md) | Semantic tokens (colors, spacing, typography), XAML patterns, component accessibility, Fluent Design rules |
| **Agent Workflow** | [AGENTS.md](./AGENTS.md) | Two-Gate System (plan mode, verification), core 14-step workflow, tool matrix, hook configuration, skill discovery |
| **Skills Catalog** | [SKILLS.md](./SKILLS.md) | Auto-discovered skills (accessibility-review, feature-generation, security-audit), per-user skill overrides |
| **Code Quality** | [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) | StyleCop rules (SA*/CA*), naming conventions, static analysis, .editorconfig source of truth |
| **Git Hooks** | [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md) | Commit/push hook logic (formatting, build, tests), bypass env vars, quick fix checklist, prevention strategy |
| **Destructive Commands** | [destructive-command-governance.rules.md](.claude/rules/destructive-command-governance.rules.md) | Approval gate for `rm -rf`, `git reset --hard`, `git push --force`, etc.; allow-list override for recurring use |
| **GitHub Governance** | [github-governance.rules.md](.claude/rules/github-governance.rules.md) | Issue closure approval gate: no auto-close without explicit user approval; scenarios and bypass policy |
| **Copilot Agent Mode** | [.github/copilot-instructions.md](.github/copilot-instructions.md) | GitHub Copilot Agent Mode only: path-specific pattern rules, WRAP workflow, scope matrix |
| **Graph Tools** | [graph-tools.rules.md](.claude/rules/graph-tools.rules.md) | code-review-graph (AST/blast-radius) + graphify (macro knowledge graph) routing, MCP tool table, orchestration/fallback rules |

## Pre-Implementation Checklist

Before starting any feature or fix:

- [ ] **Read the issue** — Understand the task scope, acceptance criteria, and any non-obvious dependencies
- [ ] **Detect platform** — Run `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
- [ ] **Plan if needed** — If >3 files or >200 LOC: invoke `EnterPlanMode`, document impact, then `/exit-plan` (see [AGENTS.md § Two-Gate System](./AGENTS.md#two-gate-system-evidence-based-execution))
- [ ] **Read relevant instruction files** — Design changes? Read [DESIGN.md](./DESIGN.md). XAML? Read [accessibility.instructions.md](.github/instructions/accessibility.instructions.md) and [performance.instructions.md](.github/instructions/performance.instructions.md). Tests? Read [testing.instructions.md](.github/instructions/testing.instructions.md)
- [ ] **Verify environment** — `.csproj` has correct `TargetFramework`, `Platforms`, `RuntimeIdentifiers` (don't hard-code; read from `.csproj`)

## Pre-Commit Verification

Before `git commit`:

```powershell
# Recheck platform (if in a new shell session)
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

# Format
dotnet format IntVue.csproj

# Build (both Debug and Release recommended)
dotnet build -c Debug -p:Platform=$Platform
dotnet build -c Release -p:Platform=$Platform

# Test (with coverage check)
dotnet test -c Debug -p:Platform=$Platform /p:CollectCoverage=true

# Review changes
git diff
git status

# Commit with issue reference
git commit -m "feat: description (Closes #123)"
```

If blocked by hooks (formatting, build, tests), see [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md) § Quick Fix Checklist.

## Troubleshooting Quick Reference

| Problem | Root Cause | Solution | Reference |
|---|---|---|---|
| Build error: CS0246 (type not found) | Missing namespace or assembly reference | Add `using` statement; check `.csproj` PackageReference | [Troubleshooting Build Errors](./AGENTS.md#troubleshooting-build-errors) |
| Test failure after code change | Test expectation mismatch or async timing | Fix code or test assertion; add `await Task.Delay(10)` for timing | [testing.instructions.md](.github/instructions/testing.instructions.md) |
| Commit blocked: "Formatting violations" | Code style doesn't match .editorconfig | Run `dotnet format IntVue.csproj` | [Quick Fix](/.claude/rules/hook-comprehensive.rules.md#section-a-fix-formatting-errors) |
| Command blocked: `rm -rf` / `git reset --hard` | Destructive command protection | Explicit approval required for this turn; or add allow-list entry to `.claude/settings.local.json` | [Destructive Command Governance](./.claude/rules/destructive-command-governance.rules.md) |
| Issue closure blocked | GitHub governance approval gate | Issue closure requires explicit user approval ("Yes, close it") | [GitHub Governance](./.claude/rules/github-governance.rules.md) |
| XAML binding error in IDE | Using `{Binding}` instead of `x:Bind` | Replace `{Binding}` with `x:Bind` throughout the file | [XAML Best Practices](./.github/instructions/winui-best-practices.instructions.md) |
| Hard-coded color in XAML | Color not using `{ThemeResource ...}` | Replace hex values with theme resource (e.g., `{ThemeResource TextFillColorPrimaryBrush}`) | [DESIGN.md](./DESIGN.md) § Color Tokens |

## Related Files & Quick Links

- **Architecture & Data Flow:** [DESIGN.md](./DESIGN.md)
- **Agent Instructions (comprehensive):** [AGENTS.md](./AGENTS.md)
- **Build & Run Procedures:** [agent-build-procedures.md](.github/instructions/agent-build-procedures.md)
- **Security & Secrets:** [security.instructions.md](.github/instructions/security.instructions.md)
- **Windows APIs Reference:** [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md)
- **Project Settings & Hooks:** [.claude/settings.json](.claude/settings.json)
