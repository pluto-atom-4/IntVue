# CLAUDE.md

Quick reference for Claude Code working with the IntVue project.

**IntVue:** WinUI 3 desktop app (Windows App SDK 1.8.x) for interview practice. Platforms: x86, x64, ARM64. Architecture: MVVM + DI (CommunityToolkit.Mvvm). Target: .NET 10.0 on Windows 10.0.26100.0+.

---

## 🤖 For Autonomous Agents

Read [AGENTS.md](./AGENTS.md) first—it contains **mandatory Two-Gate System verification** (Plan Mode + Evidence-based checks), workflow rules, and build procedures. Agents must follow **[AGENTS.md § Core Agent Workflow](./AGENTS.md#core-agent-workflow)** (steps 1-14) before writing code.

For isolated research threads: **Use sub-agent delegation** via Agent tool (see [AGENTS.md § Unified Execution Lifecycle](./AGENTS.md#unified-execution-lifecycle) for hook integration).

---

## Platform Detection (Mandatory)

Before ANY build/test command, detect platform:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

Never hardcode—cross-architecture failures result. See **[AGENTS.md § Detect Platform](./AGENTS.md#detect-platform)** for variants.

---

## Context Management

Use `/compact` at **50% tokens** to checkpoint. Use `/rewind` on contradictions. For multi-step tasks: spawn `Agent` tool (inherits AGENTS.md hooks). For specialized tasks: use `Skill` (e.g., `/accessibility-review`). See `.claude/skills/`.

---

## Core Guardrails

**Key constraints:**
- **XAML:** Use `x:Bind` (never `{Binding}`), `{ThemeResource ...}` for colors (never hard-coded)
- **Namespaces:** `Microsoft.UI.Xaml` only (not `Windows.UI.Xaml`)
- **Secrets:** Env vars, PasswordVault, or Azure Key Vault (never hard-code)
- **MediaCapture:** Don't hold open during backgrounding/suspension
- **Testing:** 80%+ coverage on ViewModels/Services; unit tests required for all features
- **Code:** YAGNI—implement only what's explicitly requested now

See **[AGENTS.md § Key Rules](./AGENTS.md#key-rules-always-enforced)** for agent-specific rules.

---

## Quick Commands

```powershell
# Build
dotnet build -c Debug -p:Platform=$Platform

# Test (all)
cd Tests/IntVue.Tests && dotnet test -c Debug -p:Platform=$Platform

# Test (specific)
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"

# Run app
dotnet run -c Debug -p:Platform=$Platform

# Troubleshoot (reset package)
winapp unregister && dotnet run -c Debug -p:Platform=$Platform

# GitHub CLI (repo admin tasks)
gh auth login                                  # Authenticate (do once)
gh repo edit --enable-issues --enable-projects  # Configure repo
gh api repos/pluto-atom-4/IntVue/branches/main/protection  # View branch protection

# Secret scanning
gitleaks detect --source . -v                  # Detect secrets in git history
gitleaks detect --source . -v --log-opts="HEAD~10..HEAD"  # Last 10 commits only
```

**For comprehensive procedures, see [AGENTS.md § Build, Run & Deploy](./AGENTS.md#build-run--deploy).**

---

## GitHub CLI Auth Scopes

When running `gh auth login`, accept the default scopes:
- `repo` — Full repository access (branch protection, labels, security settings)
- `admin:public_key` — SSH key management
- `workflow` — GitHub Actions automation

These are required by `.claude/skills/secure-github-repo/scripts/` and repo hardening automation.

---

## Architecture Overview

**Folder structure:** Models → ViewModels → Views (XAML) → Services → Converters, Helpers, Controls, Strings, Assets

**MVVM:** CommunityToolkit.Mvvm with `[ObservableProperty]` + `[RelayCommand]`. Constructor DI in `App.xaml.cs`.

**Media capture:** `MediaCapture` + `MediaFrameReader` + Win2D for preview; `LowLagMediaRecording` for recording.

See [DESIGN.md](./DESIGN.md) for full details.

---

## Rules Router

| Category | Links |
|---|---|
| **Design & UI** | Read `DESIGN.md` first. Then: `design-colors.rules.md`, `design-spacing.rules.md`, `design-typography.rules.md`, `design-components.rules.md` |
| **Git Hooks** | Blocked? → `hook-quick-fix.rules.md` (5-step checklist). Details: `hook-strategy.rules.md`, `hook-resolution.rules.md` |
| **Code Quality** | `design-principles.instructions.md`, `code-quality.instructions.md`, `winui-best-practices.instructions.md` |
| **Testing** | `testing.instructions.md` (MSTest, Moq, AAA, coverage targets) |
| **Security** | `security.instructions.md` (secrets, validation, PII) |
| **Performance** | `performance.instructions.md` (async, x:Bind, virtualization) |
| **Windows APIs** | `windows-apis.instructions.md` (API lookup, samples-first) |
| **Accessibility** | `accessibility.instructions.md` + `Views/CLAUDE.md` |
| **Localization** | `globalization.instructions.md` + `Views/CLAUDE.md` |
| **Scoped** | `Services/CLAUDE.md`, `Views/CLAUDE.md`, `ViewModels/CLAUDE.md` |

**All rules in `.github/instructions/` apply equally to Claude Code and autonomous agents.**

---

## Unified Configuration (Cross-Tool Synergy)

**Single Source of Truth:** 
- `AGENTS.md` — autonomous agent workflows, Two-Gate System, skill discovery
- `CLAUDE.md` — CLI quick reference (you are here)
- `.claude/settings.json` — hook-driven automation (PreToolUse, PostToolUse, OnAgentLaunch)
- `.github/copilot-instructions.md` — GitHub Copilot Agent Mode boundaries

Both Claude Code and GitHub Copilot CLI read these files without duplication. See [AGENTS.md § Unified Execution Lifecycle](./AGENTS.md#unified-execution-lifecycle).

---

## Before Handing Off to Agent

1. Document changes with clear commit messages
2. Pass full test suite: `dotnet test -c Debug -p:Platform=$Platform`
3. Verify app runs: `dotnet run -c Debug -p:Platform=$Platform`
4. Secret scan: `gitleaks detect --source . -v` (before pushing, catches secrets)
5. Agents follow [AGENTS.md § Two-Gate System](./AGENTS.md#two-gate-system)

---

## Quick Start

### Environment Setup
- [ ] Enable Windows Developer Mode (Settings → For developers → Developer Mode)
- [ ] Detect platform: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`

### Required Tools (First Time)
- [ ] Install GitHub CLI: `winget install GitHub.cli` or https://cli.github.com
- [ ] Authenticate: `gh auth login` (accept default scopes: repo, admin:public_key, workflow)
- [ ] Install gitleaks: `winget install gitleaks` or https://github.com/gitleaks/gitleaks
- [ ] Verify: `gitleaks version` (should show version number)

### Development Workflow
- [ ] Build: `dotnet build -c Debug -p:Platform=$Platform`
- [ ] Test: `cd Tests/IntVue.Tests && dotnet test -c Debug -p:Platform=$Platform`
- [ ] Run: `dotnet run -c Debug -p:Platform=$Platform`
- [ ] Before changes: Read relevant instruction file from Rules Router above
- [ ] UI work? Read [DESIGN.md](./DESIGN.md) first
- [ ] Before commit: `dotnet format`, `dotnet build`, `dotnet test` (prevents hook blocks)
- [ ] Before push: `gitleaks detect --source . -v` (secret scanning blocks push if secrets found)
- [ ] Blocked? See `.claude/rules/hook-quick-fix.rules.md` (2-30 min fix)
