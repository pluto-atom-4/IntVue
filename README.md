IntVue
======

WinUI 3 desktop application (Windows App SDK).

Build & run (developer loop):

- Detect platform in PowerShell: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
- Build: `dotnet build -c Debug -p:Platform=$Platform`
- Run (registers package identity via winapp): `dotnet run -c Debug -p:Platform=$Platform`

Run tests:

- `dotnet test -c Debug -p:Platform=$Platform`

See .csproj for target framework and package versions.

MVP: Video Interview Practice

- Objective: minimal WinUI 3 app to preview the front camera, record timed responses, and allow immediate playback.
- Scope: camera preview, start/stop recording to ApplicationData.LocalFolder, countdown/think-time, immediate in-app playback, and unit tests for ViewModel and service abstractions.
- Security & Privacy (MVP): recordings saved to ApplicationData.LocalFolder (private); show a concise privacy notice before first camera/microphone access; sanitize filenames; avoid logging PII or file paths.

For full implementation plan and phase breakdown, see: Docs/ImplementationPlanning/impl-mvp.md

---

## For AI Agents (Claude Code, Cursor, etc.)

This project uses **Progressive Disclosure** architecture to provide AI agents with focused, layer-specific guidance that reduces context pollution and improves decision-making.

### How It Works

The project has **three layers of guidance** that agents load automatically:

1. **Root `./CLAUDE.md`** (Project Overview)
   - System overview, NEVER DO THIS (hard constraints), development commands
   - Rules Router directing agents to detailed guidance
   - ~137 lines; loaded when working on any part of the project

2. **Scoped `<folder>/CLAUDE.md`** (Folder-Specific Rules)
   - **`Services/CLAUDE.md`** — Media capture, resource disposal, security, file operations
   - **`Views/CLAUDE.md`** — XAML, accessibility, theming, localization
   - **`ViewModels/CLAUDE.md`** — MVVM patterns, async commands, testing
   - Loaded automatically when modifying files in that folder
   - ~100–300 lines each; focused on that area only

3. **Instruction Files** (`.github/instructions/`)
   - Detailed reference material (design principles, security, testing, accessibility, etc.)
   - Linked from scoped files for full context
   - Referenced via Rules Router in root `CLAUDE.md`

### Example Workflow

**Agent modifies `Services/MediaCaptureService.cs`:**

1. Agent loads `./CLAUDE.md` (project overview, NEVER DO THIS)
2. Agent loads `Services/CLAUDE.md` (media capture rules, resource disposal, testing)
3. Agent references `.github/instructions/security.instructions.md` for detailed requirements
4. Result: Agent has exactly the guidance needed; no distraction from Views or ViewModels rules

### Local Skills

Reusable workflows in `./.claude/skills/`:

- **`feature-generation/`** — Scaffolds new WinUI pages, ViewModels, and services
- **`security-audit/`** — Audits media capture implementation for security compliance
- **`accessibility-review/`** — Audits XAML UI for accessibility (keyboard nav, contrast, automation)

Trigger examples:
- "Scaffold a new page"
- "Audit security for media capture"
- "Review this page for accessibility"

### Unified Rule Files

All rule files are mirrored in `.ai/rules/` for centralized management:

```
.ai/rules/                           (Single source of truth)
  ├── design-principles.instructions.md
  ├── security.instructions.md
  ├── winui-best-practices.instructions.md
  └── ... (all instruction files)
```

**Cross-platform symlinks:**
- `.claude/rules/` → `.ai/rules/` (Claude Code reads here)
- `.cursor/rules/` → `.ai/rules/` (Cursor reads here, if configured)

To set up symlinks (Windows PowerShell as admin):
```powershell
New-Item -ItemType SymbolicLink -Path .\.claude\rules -Target ..\.ai\rules -Force
New-Item -ItemType SymbolicLink -Path .\.cursor\rules -Target ..\.ai\rules -Force
```

See `.ai/setup-symlinks.md` for macOS/Linux instructions.

### Key Guidance

**NEVER DO THIS** (hard constraints):
- Never use `{Binding}` in XAML; always use `x:Bind`
- Never hard-code colors; use `{ThemeResource TextFillColorPrimaryBrush}`
- Never commit secrets (API keys, passwords); use environment or `PasswordVault`
- Never hold `MediaCapture` open while app is backgrounded
- Never use `Windows.UI.Xaml`; always use `Microsoft.UI.Xaml` (WinUI 3)
- Never add features without unit tests
- Never add speculative code (YAGNI principle)

**Development Commands:**
```powershell
# Platform detection
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

# Build
dotnet build -c Debug -p:Platform=$Platform

# Test
cd Tests/IntVue.Tests
dotnet test -c Debug -p:Platform=$Platform

# Run
dotnet run -c Debug -p:Platform=$Platform
```

**MVVM Architecture:**
- **View** (XAML) → Layout, styling, animations
- **ViewModel** → UI state, commands, data transformation (no business logic)
- **Service** → Business logic, media capture, file I/O
- **Model** → Data structures

### Discovery & Integration

Agents automatically discover and load:
- Root `./CLAUDE.md` on project entry
- Scoped `<folder>/CLAUDE.md` when modifying that folder's files
- Linked instruction files via Rules Router as needed

To trigger local skills:
- Claude Code: Use `/feature-generation`, `/security-audit`, `/accessibility-review` (if available)
- Cursor: Reference the skill in your prompt (e.g., "Using the security-audit skill, check...")

### For Developers

The three-layer guidance system ensures:
- **Context reduction:** Agents load only relevant rules (no "noise" from unrelated guidance)
- **Accessibility first:** Accessibility is a core section in `Views/CLAUDE.md`, not an afterthought
- **Testing integration:** Each layer includes testing patterns and examples
- **Security focus:** Media capture rules are prominent in `Services/CLAUDE.md`
- **Cross-platform support:** Symlinks enable unified configuration for Claude Code and Cursor

For questions about the AI guidance system, see `.github/instructions/CLAUDE-scoped.md` (template and meta-guidance).
