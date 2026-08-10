# Copilot Agent Instructions -- WinUI 3 / WinAppSDK

## Project Overview

This is a **WinUI 3** desktop application built on the **Windows App SDK**. It uses MSIX packaging and supports x86, x64, and ARM64 architectures.

> **Source of truth for versions & names:** Always read the project `.csproj` to determine the current `TargetFramework`, `RuntimeIdentifiers`, `Platforms`, `RootNamespace`, and `Microsoft.WindowsAppSDK` package version. Never hard-code project names or version numbers in instruction files.
>
> Throughout this document and the instruction files, `<ProjectName>` is a placeholder -- replace it with the actual project folder/assembly name (derived from the `.csproj` filename).

| Property | How to determine |
|---|---|
| UI Framework | WinUI 3 (`Microsoft.UI.Xaml`) -- always used |
| App SDK | Read `Microsoft.WindowsAppSDK` version from `.csproj` `<PackageReference>` |
| Runtime / TFM | Read `<TargetFramework>` from `.csproj` (e.g., `net10.0-windows10.0.26100.0`) |
| Target OS | Derived from `<TargetFramework>` and `<TargetPlatformMinVersion>` in `.csproj` |
| Platforms | Read `<Platforms>` from `.csproj` (e.g., `x86;x64;ARM64`) |
| Packaging | MSIX (`<EnableMsixTooling>true</EnableMsixTooling>`) |
| Namespace | Read `<RootNamespace>` from `.csproj` |
| Nullable | Read `<Nullable>` from `.csproj` |

> **Default TFM:** Templates ship with `net10.0` by default. Pass
> `--dotnet-version <tfm>` (for example `net10.0`) when running `dotnet new ...`
> or edit `<TargetFramework>` inside the generated `.csproj` before the first
> build if you need a newer framework. Keep `<RuntimeIdentifiers>` synchronized
> with the framework you pick.

## Instruction Files Index

All detailed agent instructions are organized under `.github/instructions/`:

| File | Scope |
|---|---|
| [design-principles.instructions.md](.github/instructions/design-principles.instructions.md) | DRY, KISS, SOLID, YAGNI |
| [globalization.instructions.md](.github/instructions/globalization.instructions.md) | Globalization & Localization |
| [accessibility.instructions.md](.github/instructions/accessibility.instructions.md) | Accessibility |
| [security.instructions.md](.github/instructions/security.instructions.md) | Security |
| [performance.instructions.md](.github/instructions/performance.instructions.md) | Performance |
| [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) | Static Analysis, StyleCop, Code Cleanup |
| [winui-best-practices.instructions.md](.github/instructions/winui-best-practices.instructions.md) | WinUI 3 / WinAppSDK patterns & references |
| [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) | WinAppSDK & Platform SDK API namespace catalog & lookup guidance |
| [testing.instructions.md](.github/instructions/testing.instructions.md) | Unit Testing, Build & Run |

## Core Agent Workflow

Every time you work on this codebase, follow this checklist:

### Before Writing Code
1. **Review the original goal** -- Re-read the user's request and confirm you understand the intent.
2. **Check existing code** -- Search for related implementations to avoid duplication (DRY).
3. **Find the right API** -- If the task involves a platform capability (AI, UI controls, file access, notifications, windowing, widgets, sensors, etc.), first check the [Windows APIs catalog](.github/instructions/windows-apis.instructions.md) and then look up the correct API in the [WinUI 3 API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/) before writing code.
4. **Plan the approach** -- Consider SOLID principles and identify which classes/interfaces are involved.

### While Writing Code

> **Agent Rule -- MANDATORY:** Steps 5-8 are **not** passive references. You **must** actually open and read the linked instruction file before writing code that falls within its scope. Do not skip this -- these files contain rules, anti-patterns, and checklists that must be applied.

5. **Apply Design Principles** -- **Read** [design-principles](.github/instructions/design-principles.instructions.md) before adding/refactoring classes or logic. Apply DRY, KISS, SOLID, YAGNI.
6. **Follow Fundamentals** -- **Read the applicable instruction files** based on what you're changing:
   - Adding or changing **UI controls / XAML**? -> Read [accessibility](.github/instructions/accessibility.instructions.md) (AutomationProperties, keyboard nav, contrast) AND [performance](.github/instructions/performance.instructions.md) (x:Bind, x:Load, virtualization).
   - Adding or changing **user-facing strings** (labels, messages, tooltips)? -> Read [globalization](.github/instructions/globalization.instructions.md) (`.resw` files, `x:Uid`, `ResourceLoader`).
   - Handling **secrets, user input, HTTP, or permissions**? -> Read [security](.github/instructions/security.instructions.md) (no hard-coded secrets, input validation, least privilege).
   - Working on **data binding, collections, async/IO, or layout**? -> Read [performance](.github/instructions/performance.instructions.md) (x:Bind, virtualization, async patterns).
7. **Respect Code Quality Rules** -- **Read** [code-quality](.github/instructions/code-quality.instructions.md) before writing code. Follow all CA*/SA*/IDE* analyzer rules and naming conventions.
8. **Follow WinUI Patterns** -- **Read** [winui-best-practices](.github/instructions/winui-best-practices.instructions.md) for MVVM, x:Bind, community toolkit, and API verification.

### After Writing Code
9. **Remove unused code** -- Delete unused `using` statements, dead code, commented-out blocks.
10. **Write unit tests** -- Every new public method/class needs tests. **Read** [testing](.github/instructions/testing.instructions.md) for framework setup, naming conventions (`MethodName_Scenario_ExpectedResult`), AAA pattern, and `dotnet test` commands.
11. **Build the project** -- Detect the platform first (`$Platform = $env:PROCESSOR_ARCHITECTURE`), then run `dotnet build -c Debug -p:Platform=$Platform` from the project folder and fix all warnings/errors. **If build errors occur, follow the Troubleshooting Build Errors workflow below.**
12. **Run tests** -- Run tests related to the change using `--filter` (see [testing](.github/instructions/testing.instructions.md)). Run the full suite only when the change is cross-cutting.
13. **Run the app with package identity** -- Use `dotnet run` (preferred). See [agent-build-procedures.md](.github/instructions/agent-build-procedures.md) for advanced scenarios.
14. **Re-review against original goal** -- Confirm the implementation matches the user's request.

### Troubleshooting Build Errors

> **Agent Rule -- MANDATORY:** When a build fails due to an unknown type, missing namespace, unresolved API, or similar definition error, follow this escalation order. **Do NOT jump straight to reading `.winmd` files or using `ildasm`/decompilers** -- always try web search first.

**Step 1 -- Web Search (ALWAYS try first):**
1. Open and read [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) -- it contains the API namespace catalog and lookup guidance.
2. Translate the unknown type/namespace into search keywords (e.g., `ImageDescription` -> "WinAppSDK ImageDescription API").
3. Use `web_search` or `web_fetch` to search the [WinAppSDK API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/) and the [Platform SDK API Reference](https://learn.microsoft.com/en-us/uwp/api/) for the correct namespace, class name, and method signatures.
4. Check the [release notes](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel) to verify the API is available in the project's SDK version (read from `.csproj`).

**Step 2 -- Sample Repos:**
If web search finds the API but usage is unclear, search the sample repositories listed in [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) for working examples.

**Step 3 -- WinMD / Decompiler (last resort only):**
Only if Steps 1-2 fail to resolve the issue, then inspect `.winmd` metadata files or use decompilation tools to discover the exact type definitions. This is a fallback, not the default approach.

## Build, Run & Deploy

See [agent-build-procedures.md](.github/instructions/agent-build-procedures.md) for detailed build, run, and deployment procedures including:
- Platform detection
- Build command reference
- Running with package identity (`dotnet run`)
- Test execution
- `winapp` CLI reference
- Troubleshooting build errors

**Prerequisites:**
- Never hardcode a platform identifier (x64, x86, ARM64) -- always detect it via `$env:PROCESSOR_ARCHITECTURE`.
- Check the project `.csproj` for the true source of versions and target names -- never `package.json` or a `.nuspec`.
- Run tests before committing: `dotnet test -c Debug -p:Platform=$Platform`.

## Testing Expectations

- All public ViewModels and Services require unit tests; target 80%+ coverage on business logic (see [testing.instructions.md](.github/instructions/testing.instructions.md)).
- Framework: MSTest + Moq. Follow the AAA pattern (Arrange -> Act -> Assert).
- Async tests: use `async Task` and `await Task.Delay(10)` to resolve timing issues around progress/callback assertions.

## Error Handling

- **Validate at boundaries:** user input, file I/O, network calls, media capture.
- **Trust internal code:** don't re-validate between internal classes -- preconditions already hold.
- **Graceful degradation:** prefer null checks -> safe defaults over throwing exceptions.
- **Log failures with context:** include operation name, inputs, and error code.

## Secrets & Security

- **Never hard-code** API keys, passwords, connection strings, or tokens.
- **Local development:** use environment variables (set in the IDE).
- **Sensitive UI state:** PasswordVault is optional, not required for MVP.
- **Production secrets:** Azure Key Vault (future).
- **Never commit `.env` files.**
- See [security.instructions.md](.github/instructions/security.instructions.md) for full secure-coding rules.

## Two-Gate System (Evidence-Based Execution)

The project enforces **two verification gates** before any task is considered complete. These gates ensure agents make decisions based on evidence (build logs, test output, LSP checks) rather than predictions.

### Gate 1: Plan Mode (Before Writing Code)

**When to Use:** Before implementing changes that affect >3 files OR introduce >200 lines of new code.

**Process:**
1. Invoke `EnterPlanMode` with the task description
2. Read relevant instruction files (steps 5-8 above)
3. Document multi-file architecture impact
4. Identify breaking changes or dependencies
5. Exit Plan Mode with `/exit-plan` to finalize approach

**Example Gate 1 Decisions:**
```
Task: Add countdown recording feature
Files affected: MainViewModel, RecordingService, MainPage.xaml, CountdownService (new)
Breaking changes: RecordingService.StartRecording() signature changes
Dependencies: Uses new CountdownConverter in UI binding
Plan approved? → Yes → Proceed to implementation
```

### Gate 2: Evidence-Based Verification (Before Task Completion)

**Before declaring any task complete, provide evidence:**

1. **Build Evidence** (Required)
   - Output: `dotnet build -c Debug -p:Platform=$Platform`
   - Evidence: "Build succeeded. 0 Error(s), 0 Warning(s)"
   - Failure: Cannot proceed; fix errors first

2. **Test Evidence** (Required)
   - Output: `dotnet test -c Debug -p:Platform=$Platform`
   - Evidence: "Test Run Successful. All tests passed"
   - Coverage: Show >80% coverage on changed code
   - Failure: Cannot proceed; fix failing tests first

3. **LSP Verification** (Required)
   - No unresolved symbols in IDE
   - No "red squiggly" errors in changed files
   - IntelliSense works for new APIs

4. **App Execution (Required)**
   - Output: `dotnet run -c Debug -p:Platform=$Platform`
   - Evidence: App launches and feature works as described
   - Failure: Debug and re-test

**Example Gate 2 Evidence:**
```
✅ Build: 0 errors, 0 warnings
✅ Tests: 18/18 passing (86% coverage on countdown feature)
✅ LSP: No unresolved symbols in MainViewModel or CountdownService
✅ App: Countdown displays correctly, cancellation works, recording starts

Task complete ✓
```

### Bypass Policy (Rare)

Bypass Gate 2 only for:
- Work-in-progress (WIP) features with `SKIP_TESTS_ON_FAILURE=1`
- Documentation-only changes
- Configuration file updates (.gitignore, .editorconfig)

**Bypass is rare and documented in commit message.**

---

## Skill Discovery & Framework

### Available Skills (in `.claude/skills/`)

The following specialized skills are auto-discovered and ready to invoke:

| Skill | Purpose | Usage |
|---|---|---|
| `accessibility-review` | Audit XAML for WCAG compliance, keyboard nav, screen reader support | `/accessibility-review` or via PostToolUse hook |
| `feature-generation` | Scaffold new WinUI pages, ViewModels, services following MVVM | `/feature-generation --name PageName` |
| `security-audit` | Review services for secrets, validation, PII handling | `/security-audit` or triggered on Services/* files |

### Skill Metadata Format

Each skill has YAML frontmatter with:
```yaml
---
skill_name: accessibility-review
description: Audit XAML UI for accessibility compliance
tools: [grep, view, edit, bash]
applyTo: "**/*.xaml"
tags: [qa, accessibility]
refersTo: accessibility.instructions.md
---
```

### Skill Invocation

Skills are triggered:
1. **Explicit:** User invokes `/skill-name`
2. **Implicit:** PostToolUse hook triggers on file pattern match
3. **Agent Launch (aspirational):** an `OnAgentLaunch` hook that auto-inherits skill paths is planned but not yet implemented in `.claude/settings.json`

---

## .github/copilot/rules/ Pattern Reference System

### Glob Pattern Discovery

Copilot Agent Mode discovers path-specific rules via glob patterns:

```yaml
# Example: XAML files inherit accessibility rules
---
applyTo: "**/*.xaml"
references:
  - .github/instructions/accessibility.instructions.md
  - .github/instructions/performance.instructions.md
  - .claude/rules/design-components.rules.md
---
```

### Pattern Matching (Most-Specific-Wins)

| Pattern | Examples | Rules Applied |
|---|---|---|
| `**/*.xaml` | MainPage.xaml, Views/RecordingPage.xaml | XAML-specific rules (x:Bind, colors, spacing, accessibility) |
| `ViewModels/**/*.cs` | MainViewModel.cs, CountdownViewModel.cs | MVVM pattern rules (ObservableObject, [ObservableProperty], [RelayCommand]) |
| `Services/**/*.cs` | RecordingService.cs, MediaService.cs | Service rules (stateless, validation, async, secrets) |
| `**/*Tests.cs` | MainViewModelTests.cs, RecordingServiceTests.cs | Test rules (MSTest, AAA pattern, naming convention) |
| `.github/instructions/**/*.md` | code-quality.instructions.md | Apply to all instruction files themselves |

### Cross-Reference Lookup

When Copilot encounters a file:
1. **Match glob pattern** (e.g., `**/*.xaml` matches `Views/MainPage.xaml`)
2. **Fetch rule references** (accessibility.instructions.md, performance.instructions.md)
3. **Apply rules** in order of specificity (most specific first)
4. **Inherit hooks** from `.claude/settings.json` (currently `PreToolUse` and `PostToolUse`; `OnFileSave` is planned but not yet implemented)

---

## Unified Execution Lifecycle (Cross-Tool Synergy)

Both Claude Code and GitHub Copilot CLI follow the same execution lifecycle via `.claude/settings.json` hooks.
**Only `PreToolUse` and `PostToolUse` are implemented today.** The remaining stages below (file-save
background analysis, context-drift monitoring, agent-launch inheritance) describe the intended future
lifecycle and are aspirational until added to `.claude/settings.json`.

### Execution Flow

```
File Modified
    ↓
PreToolUse Hook (implemented)
  ├─ Validate tool (e.g., block rm -rf)
  ├─ Warn platform detection (add $Platform variable)
  ├─ Warn configuration flag (add -c Debug)
    ↓
Tool Execution
  ├─ Edit file, build project, run tests
    ↓
PostToolUse Hook (implemented)
  ├─ Auto-format C# files (suggest `dotnet format`)
  ├─ Multi-file build verification (>3 files changed)
  ├─ XAML validation (suggest `dotnet build`)
    ↓
[Aspirational — not yet implemented]
  ├─ OnFileSave: StyleCop/test-syntax background analysis
  ├─ OnContextDrift: suggest `/compact` at 50% token usage, `/rewind` on contradiction
  ├─ OnAgentLaunch: auto-inherit AGENTS.md + instructions + skill paths on subagent spawn
    ↓
Task Complete
  ├─ Gate 1: Plan Mode review ✓
  ├─ Gate 2: Build + Test + LSP ✓
  ├─ Result: Commit with evidence
```

### Hook Configuration Reference

Hooks are configured in `.claude/settings.json`:

| Hook | Status | Trigger | Action | Example |
|---|---|---|---|---|
| `PreToolUse` | ✅ Implemented | Before any tool runs | block/warn (see sub-table below) | Block destructive commands, warn missing platform/config flag |
| `PostToolUse` | ✅ Implemented | After tool completes | suggest/auto | Suggest `dotnet format`, suggest build verification |
| `OnFileSave` | 🚧 Aspirational | After file save | background | Run StyleCop analysis after 3s delay |
| `OnContextDrift` | 🚧 Aspirational | At 50% token usage | suggest | Suggest `/compact` to checkpoint |
| `OnAgentLaunch` | 🚧 Aspirational | When spawning subagent | inherit | Auto-inherit AGENTS.md + instructions |

#### `PreToolUse` Hooks (Detail)

The `PreToolUse` row above covers four individual hooks on the `Bash` matcher in `.claude/settings.json`:

| Hook `id` | Trigger | Behavior | Notes |
|---|---|---|---|
| `block-destructive-shell` | `rm -rf`, `del /s`, `rd /s`, `Remove-Item ... -Recurse`\|`-Force`, `git reset --hard`, `git push --force`\|`-f`, `Format-Volume` — matched anywhere in the command (chained commands like `cd x && rm -rf y` are also caught) | `ask` — prompts for explicit human approval before the command runs | See [destructive-command-governance.rules.md](.claude/rules/destructive-command-governance.rules.md) for the full policy, approval scenarios, and the `.claude/settings.local.json` allow-list override for recurring legitimate commands |
| `warn-missing-platform` | `dotnet build\|test\|run` without `-p:Platform=` | Non-blocking `systemMessage` showing the full corrected command, e.g. `dotnet build -c Debug -p:Platform=$Platform` | Does not escalate to `ask` |
| `warn-missing-config-flag` | `dotnet build\|test` without `-c` | Non-blocking `systemMessage` showing the full corrected command, e.g. `dotnet build -c Debug -p:Platform=<arch>` | Does not escalate to `ask` |
| `github-issue-closure-approval-gate` | `gh issue close`, `gh issue delete` | `ask` — prompts for explicit human approval before the command runs | See [github-governance.rules.md](.claude/rules/github-governance.rules.md) |

### Conflict Resolution (GitHub Copilot ↔ Claude Code)

**If both tools are active and disagree:**
1. `.claude/settings.json` takes precedence (it's the harness configuration)
2. AGENTS.md supersedes tool-specific instructions (source of truth)
3. CLAUDE.md is for CLI users; doesn't affect Copilot Agent Mode
4. `.github/copilot-instructions.md` is for Copilot Agent Mode only

**No conflicts should arise** because files have distinct audiences and hooks keep them in sync.

---

## Key Rules (Always Enforced)

- **Every change must build and pass tests** -- Run `dotnet build` and `dotnet test` (see [Build, Run & Deploy](#build-run--deploy)) before considering any task complete.
- **Follow all instruction files** -- The detailed rules in `.github/instructions/` are authoritative. **You must actually open and read them** (not just acknowledge they exist) when working within their scope. See the trigger conditions in steps 5-8 above.
- **Two-Gate System is mandatory** -- Plan Mode for >3 files/200 LOC; Evidence-based verification (build + test + LSP + app run) before task completion.
- **Web search before decompilation** -- When facing unknown types or build errors, always search the web / API docs first. Only use WinMD/ILDASM as a last resort (see [Troubleshooting Build Errors](#troubleshooting-build-errors)).
- **Use `winapp` for app-identity / packaging / signing** -- Don't hand-roll `MakeAppx`/`SignTool`/`Add-AppxPackage` invocations. The CLI keeps the manifest, certificate, and registration steps in sync.
- **Use `x:Bind` in XAML** -- never `{Binding}`.
- **Use `{ThemeResource ...}` for colors** -- never hard-code hex values.
- **Dispose MediaCapture properly** -- never hold it open across suspend/resume.
- **Keep ViewModels free of business logic** -- domain logic belongs in Services; ViewModels handle state.
- **Use constructor injection for dependencies.**

## Windows AI Prerequisites

When integrating Windows AI APIs (Phi Silica, Windows Vision -- ImageDescription,
TextRecognizer, ImageScaler, etc.) -- see
[windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md):

1. **Package identity is required.** All Windows AI APIs require the app to
   run with package identity. The `dotnet run` flow described above already
   provides this. If you're testing outside `dotnet run`, register identity
   first with `winapp run` or `winapp create-debug-identity`.
2. **Manifest capabilities.** Add the capabilities each API requires to
   `Package.appxmanifest` (commonly `runFullTrust`; some scenarios additionally
   need `internetClient`). Check the API's docs page for the exact list.
3. **Hardware / OS gating.** Some APIs require a Copilot+ PC (NPU) or a
   minimum Windows build. Always probe availability with the API's
   `IsAvailable` / `EnsureReadyAsync` pattern (or equivalent) and provide a
   graceful fallback for unsupported devices.
4. **Verify locally before checking in.** After capability or manifest
   changes, re-run `dotnet run` (or `winapp run`) so the registered identity
   reflects the updated manifest -- a stale registration will silently use
   the old capability set.

## Cross-References

- Code quality rules: [code-quality.instructions.md](.github/instructions/code-quality.instructions.md)
- WinUI 3 patterns: [winui-best-practices.instructions.md](.github/instructions/winui-best-practices.instructions.md)
- Architecture & boundaries: [DESIGN.md](./DESIGN.md)
- Design tokens & theming: `.claude/rules/design-*.rules.md`
- Hook / error resolution: [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md)
- GitHub issue closure approval gate: [github-governance.rules.md](.claude/rules/github-governance.rules.md) -- issue closure is a manual-only operation requiring explicit user approval.
