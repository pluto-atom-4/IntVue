# CLAUDE.md

Quick reference for Claude Code working with the IntVue project.

**IntVue:** WinUI 3 desktop app (Windows App SDK 1.8.x) for interview practice. Platforms: x86, x64, ARM64. Architecture: MVVM + DI (CommunityToolkit.Mvvm). Target: .NET 10.0 on Windows 10.0.26100.0+.

---

## For Agents

**Agents:** Read [AGENTS.md](./AGENTS.md) instead—it has mandatory workflows, file-reading rules, and build procedures. Autonomous agents must follow **[AGENTS.md § Core Agent Workflow](./AGENTS.md#core-agent-workflow)** before writing code.

---

## Platform Detection (Mandatory)

Before ANY build/test command, detect platform:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

Never hardcode—cross-architecture failures result. See **[AGENTS.md § Detect Platform](./AGENTS.md#detect-platform)** for variants.

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
```

**For comprehensive procedures, see [AGENTS.md § Build, Run & Deploy](./AGENTS.md#build-run--deploy).**

---

## Architecture Overview

**Folder structure:** Models (data) → ViewModels (state) → Views (XAML) → Services (logic) → Converters, Helpers, Controls, Strings, Assets

**MVVM pattern:** CommunityToolkit.Mvvm with `[ObservableProperty]` + `[RelayCommand]`. DI in `App.xaml.cs`. Constructor injection preferred.

**Media capture:** `MediaCapture` with `MediaFrameReader` + Win2D `CanvasControl` for preview; `LowLagMediaRecording` for recording; proper init/cleanup on suspend/resume.

**See [DESIGN.md](./DESIGN.md) for full architecture & constraints.**

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

## Before Handing Off to an Agent

1. Document changes with clear commit messages
2. Pass `dotnet test -c Debug -p:Platform=$Platform` (full suite)
3. Verify app runs: `dotnet run -c Debug -p:Platform=$Platform`
4. Update `.github/instructions/` if adding capabilities

**Agents must follow [AGENTS.md § Core Agent Workflow](./AGENTS.md#core-agent-workflow) before writing code (steps 5-8 mandatory).**

---

## Quick Start

- [ ] Enable Windows Developer Mode (Settings → For developers → Developer Mode)
- [ ] Detect platform: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
- [ ] Build: `dotnet build -c Debug -p:Platform=$Platform`
- [ ] Test: `cd Tests/IntVue.Tests && dotnet test -c Debug -p:Platform=$Platform`
- [ ] Run: `dotnet run -c Debug -p:Platform=$Platform`
- [ ] Before changes: Read relevant instruction file from Rules Router above
- [ ] UI work? Read [DESIGN.md](./DESIGN.md) first
- [ ] Before commit: `dotnet format`, `dotnet build`, `dotnet test` (prevents hook blocks)
- [ ] Blocked? See `.claude/rules/hook-quick-fix.rules.md` (2-30 min fix)
