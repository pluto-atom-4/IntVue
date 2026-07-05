# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

---

## Project Overview

**IntVue** is a WinUI 3 desktop application (Windows App SDK) for interview practice. The MVP focuses on camera preview, video recording, and playback with a countdown/think-time workflow.

- **Framework:** WinUI 3 (Windows App SDK 1.8.x)
- **Target:** .NET 10.0 on Windows 10.0.26100.0+
- **Platforms:** x86, x64, ARM64
- **Package Model:** MSIX (loose-layout for development)
- **Architecture:** MVVM with dependency injection (CommunityToolkit.Mvvm)

---

## Working with Agents & Automation

**This guide is for Claude Code.** If you are an **autonomous agent** or writing instructions for agents, read **[AGENTS.md](./AGENTS.md)** instead. It contains the comprehensive agent onboarding guide with mandatory workflows.

### When to Consult CLAUDE.md vs AGENTS.md

| Scenario | Read... | Why |
|---|---|---|
| **You are Claude Code (claude.ai/code)** using this repo manually | CLAUDE.md (this file) | Quick reference for platform detection, build commands, architecture |
| **You are an autonomous agent or writing agent instructions** | AGENTS.md | Comprehensive workflows, mandatory file-reading rules, build/run procedures |
| **Working on a specific scope** (Services/, Views/, ViewModels/) | Both CLAUDE.md and AGENTS.md, then the scoped CLAUDE.md file | Scope-specific guidance applies to both Claude Code and agents |
| **UI generation, design tokens, theming** | DESIGN.md first, then `.claude/rules/design-*.rules.md` | Visual design system (referenced by both CLAUDE.md and AGENTS.md) |

### Key Differences

**CLAUDE.md** focuses on:
- Quick-start platform detection and build commands
- Architecture overview for manual exploration
- Rules Router for finding the right instruction file
- Immediate troubleshooting steps

**AGENTS.md** includes:
- Detailed agent workflows (Before/While/After Writing Code)
- **Mandatory file reading rules** for instruction files (steps 5-8)
- Comprehensive Build, Run & Deploy procedures with all options
- Troubleshooting escalation order (web search → samples → WinMD)
- Windows AI prerequisites
- **Project.csproj reading guidance** (source of truth for versions)

### Critical: Platform Detection is Mandatory for All Agents

**Before running ANY build, test, or deployment command**, you (and all agents) **must** detect your platform:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

See **[AGENTS.md § Detect Platform](./AGENTS.md#detect-platform)** for detailed platform detection and all build/run variants. This is **not optional** — hardcoding a platform value will cause cross-architecture failures.

### Instruction Files Apply to ALL Agents

All files in `.github/instructions/` contain **mandatory rules** that apply to:
- Claude Code (manual use)
- Autonomous agents
- Any contributor using this repository

See **[AGENTS.md § Instruction Files Index](./AGENTS.md#instruction-files-index)** for the complete index and **[§ Core Agent Workflow](./AGENTS.md#core-agent-workflow)** for when to read each file (steps 5-8 are mandatory).

---

## NEVER DO THIS

**These constraints apply to all agents and contributors.** For additional agent-specific guard rails, see **[AGENTS.md § Key Rules](./AGENTS.md#key-rules-always-enforced)**.

Hard constraints that must never be violated:

- **XAML Binding:** Never use `{Binding}`; always use `x:Bind` with `Mode=OneWay` or `Mode=OneTime`.
- **Hard-coded Colors:** Never hard-code colors in XAML; use `{ThemeResource TextFillColorPrimaryBrush}`.
- **Secrets:** Never commit API keys, passwords, or connection strings — use environment variables, `PasswordVault`, or Azure Key Vault.
- **MediaCapture:** Never hold `MediaCapture` open while the app is backgrounded or suspended.
- **Namespaces:** Never use `Windows.UI.Xaml`; always use `Microsoft.UI.Xaml` (WinUI 3).
- **Testing:** Never add features without unit tests (aim for 80%+ coverage on ViewModels/Services).
- **Speculative Code:** Never add "just in case" features — implement only what is explicitly requested now (YAGNI).

---

## Development Commands

> **For comprehensive build/run/deploy procedures, see [AGENTS.md § Build, Run & Deploy](./AGENTS.md#build-run--deploy).** This section provides quick-reference commands; AGENTS.md contains all variants, troubleshooting, and prerequisites.

### Platform Detection
Always detect your CPU architecture first—never hardcode `x64` or `x86`:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

**Key Point:** Platform detection is mandatory. Hardcoding a platform value causes cross-architecture failures. See **[AGENTS.md § Detect Platform](./AGENTS.md#detect-platform)** for full details and all supported variants.

### Build, Test, Run
```powershell
# Build (Debug or Release)
dotnet build -c Debug -p:Platform=$Platform

# Run all tests
cd Tests/IntVue.Tests
dotnet test -c Debug -p:Platform=$Platform

# Run app (auto-registers loose-layout package)
dotnet run -c Debug -p:Platform=$Platform

# Run specific test class
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"
```

### Troubleshooting
```powershell
# Reset stale package registration
winapp unregister
dotnet run -c Debug -p:Platform=$Platform
```

---

## High-Level Architecture

### Folder Structure
```
IntVue/
  Models/           → Data classes
  ViewModels/       → UI state, commands (no business logic)
  Views/            → XAML pages and windows
  Services/         → Business logic, media capture, file I/O
  Converters/       → IValueConverter implementations
  Helpers/          → Static utility methods
  Controls/         → Custom/reusable controls
  Strings/          → Localized strings (.resw)
  Assets/           → Images, icons
```

### MVVM Pattern
The app strictly follows Model-View-ViewModel:

| Layer | Responsibility |
|---|---|
| **View** (XAML) | Layout, styling, animations |
| **ViewModel** | UI state, commands, data binding (no business logic) |
| **Service** | Business logic, media capture, file I/O, navigation |
| **Model** | Data structures |

**Key:** Use `CommunityToolkit.Mvvm` with `[ObservableProperty]` and `[RelayCommand]`. Register services in `App.xaml.cs` via `Microsoft.Extensions.DependencyInjection`. Inject dependencies via constructor (preferred) or `App.Services.GetService<T>()`.

### Media Capture
The app uses `Windows.Media.Capture.MediaCapture` with:
- **Preview:** Frame-based rendering via `MediaFrameReader` + Win2D `CanvasControl`
- **Recording:** `LowLagMediaRecording` to prevent UI stuttering
- **Resource Management:** Proper init/cleanup on suspend/resume cycles (critical for media resources)

---

## Rules Router

For detailed guidance, consult the relevant section below:

### Design System & Visual Tokens
- **Design Philosophy & Index** → `DESIGN.md`
- **Color & Theme Tokens** → `.claude/rules/design-colors.rules.md`
- **Spacing, Sizing & Layout** → `.claude/rules/design-spacing.rules.md`
- **Typography Tokens** → `.claude/rules/design-typography.rules.md`
- **Component & Pattern Rules** → `.claude/rules/design-components.rules.md`

For UI generation tasks: Read `DESIGN.md` first (philosophy & overview), then reference specific rule files as needed.

### Git Hooks & Automation

**Important:** The project uses Git hooks that block commits/pushes with formatting, build, or test errors.

- **Hook Strategy & Escalation** → `.claude/rules/hook-strategy.rules.md`
- **Quick Fix Checklist** (use when blocked) → `.claude/rules/hook-quick-fix.rules.md`
- **Detailed Error Resolution Guide** → `.claude/rules/hook-resolution.rules.md`

**When you encounter a commit block:** Open `hook-quick-fix.rules.md` for your error type (5-step checklist, 2-30 min resolution).

### Project-Wide Rules (All Code Changes)
- **Design Principles** (DRY, KISS, SOLID, YAGNI) → `.github/instructions/design-principles.instructions.md`
- **Code Quality** (StyleCop, naming, cleanup) → `.github/instructions/code-quality.instructions.md`
- **Testing Standards** (MSTest, Moq, AAA pattern) → `.github/instructions/testing.instructions.md`
- **Security & Permissions** (secrets, validation, PII) → `.github/instructions/security.instructions.md`
- **Performance** (async, x:Bind, virtualization) → `.github/instructions/performance.instructions.md`
- **Windows APIs** (API lookup, sample-first rule) → `.github/instructions/windows-apis.instructions.md`

### Scoped Rules (When Modifying Specific Areas)

**Before starting work on any scope, consult BOTH CLAUDE.md and AGENTS.md**, then the scoped `CLAUDE.md` file (if it exists). All rules in `.github/instructions/` apply equally to Claude Code and all autonomous agents.

- **Services/** (media capture, recording, file operations) → `Services/CLAUDE.md` (planned)
- **Views/** & **Controls/** (XAML, accessibility, theming, localization) → [Views/CLAUDE.md](./Views/CLAUDE.md) (reference available)
- **ViewModels/** (MVVM patterns, async commands, testing) → `ViewModels/CLAUDE.md` (planned)

See **[AGENTS.md § Instruction Files Index](./AGENTS.md#instruction-files-index)** for a complete table of mandatory instruction files and when to read each one (steps 5-8 in **[Core Agent Workflow](./AGENTS.md#core-agent-workflow)**).

### Specialized Guidance
- **WinUI 3 & Architecture** → `.github/instructions/winui-best-practices.instructions.md`
- **Accessibility** (keyboard nav, automation) → `.github/instructions/accessibility.instructions.md`
- **Localization** (x:Uid, .resw, multiple languages) → `.github/instructions/globalization.instructions.md`

---

## Agent Coordination & Multi-Agent Safety

When working with multiple agents or coordinating agent workflows:

### Before Handing Off to Another Agent

1. **Document your changes** — Leave clear commit messages that describe what was implemented and why.
2. **Run the full test suite** — Agents will assume `dotnet test -c Debug -p:Platform=$Platform` passes before they start.
3. **Verify the app runs** — Use `dotnet run -c Debug -p:Platform=$Platform` to confirm the latest changes work in the live app.
4. **Check the instruction files** — If you added a new capability or changed a core system, update the relevant `.github/instructions/` file.

### Mandatory Checks for All Agents

Every autonomous agent **must** follow **[AGENTS.md § Core Agent Workflow](./AGENTS.md#core-agent-workflow)** before writing code:

1. ✓ Review the original goal
2. ✓ Check existing code (DRY)
3. ✓ Find the right API (Windows APIs catalog)
4. ✓ Plan the approach (SOLID)
5. ✓ **Read design-principles.instructions.md** (mandatory)
6. ✓ **Read applicable instruction files** based on scope (mandatory — steps 6a-6d)
7. ✓ **Read code-quality.instructions.md** (mandatory)
8. ✓ **Read winui-best-practices.instructions.md** (mandatory)
9. ✓ Remove unused code
10. ✓ Write unit tests
11. ✓ Build: `dotnet build -c Debug -p:Platform=$Platform`
12. ✓ Run tests: `dotnet test -c Debug -p:Platform=$Platform`
13. ✓ Run the app: `dotnet run -c Debug -p:Platform=$Platform`
14. ✓ Re-review against original goal

**If any mandatory file (steps 5-8) is not read, the implementation is incomplete.**

### Instruction Files are Always Required

- **Never skip reading** `.github/instructions/` files when they apply to your scope.
- **Never assume** you can infer the rules — read the actual file (AGENTS.md steps 5-8 are mandatory, not optional).
- **If a build fails**, follow the **[AGENTS.md § Troubleshooting Build Errors](./AGENTS.md#troubleshooting-build-errors)** escalation order: Web Search → Sample Repos → WinMD/Decompiler.

---

## Quick Start Checklist

- [ ] **Read this file (CLAUDE.md)** for quick reference
- [ ] **If you are an agent, read [AGENTS.md](./AGENTS.md)** for the comprehensive workflow
- [ ] **Bookmark `.claude/rules/hook-quick-fix.rules.md`** — you'll use it if commits are blocked
- [ ] Detect your platform using the command in § Development Commands
- [ ] Enable Windows Developer Mode (Settings → System → For developers → Developer Mode → On)
- [ ] Build: `dotnet build -c Debug -p:Platform=$Platform`
- [ ] Test: `cd Tests/IntVue.Tests && dotnet test -c Debug -p:Platform=$Platform`
- [ ] Run: `dotnet run -c Debug -p:Platform=$Platform`
- [ ] **Before making changes, consult BOTH this file and the relevant `.github/instructions/` file**
- [ ] **See [DESIGN.md](./DESIGN.md) for UI/visual design tasks**
- [ ] **Before committing, run:** `dotnet format`, `dotnet build`, `dotnet test` (prevents hook blocks)
