# Copilot Agent Instructions -- WinUI 3 / WinAppSDK

## Project Overview

**IntVue** is a WinUI 3 desktop application for interview practice. Built on Windows App SDK 1.8.x, MVVM + Dependency Injection, MSIX packaging, .NET 10.0+.

**Architecture:**
- **UI:** WinUI 3 (Windows App SDK), x:Bind data binding, semantic design tokens (no hard-coded colors)
- **State:** MVVM with `CommunityToolkit.Mvvm` (ObservableObject, RelayCommand, ObservableProperty)
- **Services:** Dependency injection via DI container; stateless with validation at boundaries
- **Data:** MSTest + Moq for ≥80% coverage on ViewModels and Services
- **Platforms:** x86, x64, ARM64 (detected at build time via `-p:Platform=$Platform`)
- **Packaging:** MSIX (`.msixbundle` for multi-architecture release)

> **Source of truth:** Always read the `.csproj` for current `TargetFramework`, `Platforms`, `RuntimeIdentifiers`, `RootNamespace`, and `Microsoft.WindowsAppSDK` version. Never hard-code these values in instruction files — reference the `.csproj` instead.

---

## Agent Responsibilities & Tool Matrix

| Role | Responsibility | Scope | Escalation |
|---|---|---|---|
| **Claude Code** | Full-context workflows, multi-file refactors, complex fixes | Any; 14-step process | >3 files or >200 LOC → Plan Mode |
| **GitHub Copilot** | Single-file generation, path-specific rules (XAML, ViewModel, Service) | Auto-discovered via glob patterns | Falls back to `SKILLS.md` catalog for skills |
| **Skills (Tools)** | Domain audits (accessibility, security), feature scaffolding | Triggered on matching paths or explicit invocation | Inherits CLAUDE.md + instruction files from parent |
| **Hooks (Enforcement)** | Block destructive commands, warn missing flags, gate issue closure | PreToolUse validates; PostToolUse suggests | Explicit approval required for `ask` gates |

**Escalation path:** 
1. Task >3 files or >200 LOC? → Invoke Plan Mode (§ Two-Gate System)
2. Destructive command needed (rm -rf, git reset --hard)? → Request explicit user approval
3. GitHub issue closure needed? → Request explicit user confirmation
4. Unknown API / build error? → Web search first (§ Troubleshooting Build Errors)

---

## Graph Intelligence Tools

`code-review-graph` (micro/AST blast-radius) and `graphify` (macro/doc knowledge graph) replace expensive Grep/Glob with structured graph queries. Registered as MCP servers for this project (`.mcp.json` + user-level `~/.claude/settings.json`); `PostToolUse`/`SessionStart`/`PreToolUse` hooks in `.claude/settings.json` keep the graph fresh and steer broad searches toward it automatically.

| Need | Use | Fallback |
|---|---|---|
| Macro assessment (subsystem/architecture, business logic across docs) | Graphify — `graphify query "<question>"`, `graphify-out/GRAPH_REPORT.md` | `README.md`/design docs |
| Micro/blast-radius (callers, callees, impact of a change) | code-review-graph MCP tools (`query_graph_tool`, `get_impact_radius_tool`, `detect_changes_tool`) | Targeted Grep on the file + direct imports |
| Graph tool errors/empty result | — | Fall back to Grep/Glob rather than looping on the graph |

Full routing matrix, orchestration rules (macro→micro escalation, single-engine isolation, stale-graph handling), and the MCP tool reference table: [graph-tools.rules.md](.claude/rules/graph-tools.rules.md).

---

## Core Agent Workflow (14 Steps)

1. **Review goal** — Understand the issue/request scope and acceptance criteria
2. **Check existing code** — Search for related implementations (DRY principle)
3. **Find the right API** — Use [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) and [WinUI API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/)
4. **Plan approach** — Identify classes/interfaces and SOLID implications
5. **Read applicable instruction files** — Mandatory per domain:
   - XAML? → [accessibility](.github/instructions/accessibility.instructions.md) + [performance](.github/instructions/performance.instructions.md)
   - Secrets/Security? → [security](.github/instructions/security.instructions.md)
   - Strings/Localization? → [globalization](.github/instructions/globalization.instructions.md)
   - Code quality? → [code-quality](.github/instructions/code-quality.instructions.md)
6. **Apply design principles** — Read [design-principles](.github/instructions/design-principles.instructions.md) (DRY, KISS, SOLID, YAGNI)
7. **Follow WinUI patterns** — Read [winui-best-practices](.github/instructions/winui-best-practices.instructions.md)
8. **Remove unused code** — Delete unused `using`, dead code, comments
9. **Write unit tests** — ≥80% coverage. See [testing](.github/instructions/testing.instructions.md)
10. **Build** — `dotnet build -c Debug -p:Platform=$Platform`; fix all errors/warnings
11. **Run tests** — Use `--filter` for scoped runs; full suite for cross-cutting changes
12. **Run app** — `dotnet run` with package identity verification
13. **Verify against goal** — Confirm implementation matches the original request
14. **Provide Gate 2 evidence** — Build logs, test results, LSP verification (see § Two-Gate System below)

### Troubleshooting Build Errors

**Mandatory escalation order:** Web search first (always), sample repos second, WinMD/decompilers last. Never jump straight to decompilers.

1. **Web search** — Read [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md), translate unknown type to search keywords, use web_search on [WinAppSDK API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/), verify API availability in `.csproj` SDK version
2. **Sample repos** — Check repositories listed in windows-apis.instructions.md for working examples
3. **Decompiler** — WinMD/ildasm only if Steps 1-2 fail (rare edge cases)

Common errors → common solutions table: see [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) and [winui-best-practices.instructions.md](.github/instructions/winui-best-practices.instructions.md)

## Build, Run & Deploy

Complete procedures: [agent-build-procedures.md](.github/instructions/agent-build-procedures.md)

**Quick reference:**
- Platform: Detect via `$env:PROCESSOR_ARCHITECTURE` (x64, x86, ARM64) — never hard-code
- Build: `dotnet build -c Debug -p:Platform=$Platform` (check `.csproj` for TargetFramework/Platforms)
- Run: `dotnet run -c Debug -p:Platform=$Platform` (provides package identity automatically)
- Test: `dotnet test -c Debug -p:Platform=$Platform` (before committing; see Testing Expectations below)
- Deploy: See [agent-build-procedures.md](.github/instructions/agent-build-procedures.md) for MSIX packaging and signing

## Testing Expectations

**Minimum coverage:** 80% on all public ViewModels and Services. Framework: MSTest + Moq.

**Pattern:** AAA (Arrange → Act → Assert). Async tests use `async Task` and `await Task.Delay(10)` for timing.

**Naming:** `MethodName_Scenario_ExpectedResult` (e.g., `LoadItems_OnSuccess_PopulatesCollection`)

**Run tests:** `dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~..."` (filter by class/method to scope runs)

See [testing.instructions.md](.github/instructions/testing.instructions.md) for detailed framework setup and examples.

## Error Handling & Validation

**Boundary validation** — Validate at entry points: user input, file I/O, network calls, media capture. Throw meaningful exceptions with context (operation name, inputs, error codes).

**Internal trust** — Between internal classes, assume preconditions already hold. Don't re-validate within the application layer.

**Graceful degradation** — Prefer null checks + safe defaults over exceptions. Example: "Camera not available? → Use default, display message" rather than crashing.

See [design-principles.instructions.md](.github/instructions/design-principles.instructions.md) and [security.instructions.md](.github/instructions/security.instructions.md) for full error handling and validation strategies.

## Secrets & Security

**Never hard-code** API keys, passwords, connection strings, tokens, or certificates. Use:
- **Local dev:** Environment variables (set in IDE launch settings)
- **Sensitive runtime state:** PasswordVault API (optional for MVP)
- **Production:** Azure Key Vault (future)

**Never commit** `.env` files, `.pfx` certificates, or local secrets to git. Leverage [gitleaks](https://github.com/gitleaks/gitleaks) for pre-commit scanning.

See [security.instructions.md](.github/instructions/security.instructions.md) for full policy, input validation, and PII handling rules.

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

## Instruction Files Index

All detailed agent guidance is organized under `.github/instructions/`:

| File | Scope | Referenced in |
|---|---|---|
| [design-principles.instructions.md](.github/instructions/design-principles.instructions.md) | DRY, KISS, SOLID, YAGNI principles | Step 6 of Core Workflow |
| [accessibility.instructions.md](.github/instructions/accessibility.instructions.md) | XAML accessibility, keyboard nav, screen reader, automation properties | Step 5 of Core Workflow (XAML changes) |
| [performance.instructions.md](.github/instructions/performance.instructions.md) | x:Bind, x:Load, virtualization, async patterns | Step 5 of Core Workflow (data binding/layout) |
| [security.instructions.md](.github/instructions/security.instructions.md) | Secrets, input validation, PII, least privilege, permissions | Step 5 of Core Workflow (secrets/security changes) |
| [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) | StyleCop (SA*/CA*/IDE* analyzers), naming conventions, .editorconfig | Step 7 of Core Workflow |
| [winui-best-practices.instructions.md](.github/instructions/winui-best-practices.instructions.md) | MVVM patterns, x:Bind, community toolkit, API verification | Step 8 of Core Workflow |
| [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) | Windows API namespace catalog, lookup, availability probing | § Troubleshooting Build Errors |
| [testing.instructions.md](.github/instructions/testing.instructions.md) | MSTest framework, AAA pattern, naming conventions, coverage | Step 9-11 of Core Workflow |
| [agent-build-procedures.md](.github/instructions/agent-build-procedures.md) | Platform detection, build/test/run commands, package identity, troubleshooting | § Build, Run & Deploy |
| [globalization.instructions.md](.github/instructions/globalization.instructions.md) | Localization, `.resw` files, `x:Uid`, ResourceLoader | Step 5 of Core Workflow (user-facing strings) |

---

## Skill Discovery & Framework

Auto-discovered skills in `.claude/skills/` provide specialized audits and scaffolding. See [SKILLS.md](./SKILLS.md) for full indexed catalog and per-user overrides (`.claude/settings.local.json`).

**Available skills:**

| Skill | Purpose | Usage | Trigger |
|---|---|---|---|
| `accessibility-review` | Audit XAML for WCAG compliance, keyboard navigation, screen reader support, automation properties | `/accessibility-review` or manual file review | Explicit invocation; PostToolUse on `**/*.xaml` edits |
| `feature-generation` | Scaffold new WinUI pages, ViewModels, services following MVVM+DI template | `/feature-generation --name PageName` | Explicit invocation with page name |
| `security-audit` | Review services for secrets, input validation, PII handling, permission scoping | `/security-audit` or on Services/* edits | Explicit invocation; PostToolUse on `Services/**/*.cs` edits |

**Invocation modes:** (1) Explicit user command `/skill-name`, (2) PostToolUse hook on file pattern match, (3) OnAgentLaunch (planned future feature)

---

## Unified Execution Lifecycle & Hook Configuration

Both Claude Code and GitHub Copilot follow the same execution lifecycle via `.claude/settings.json` hooks.

**Implemented:** `PreToolUse` (block/warn before execution), `PostToolUse` (suggest after execution)  
**Aspirational:** `OnFileSave`, `OnContextDrift`, `OnAgentLaunch` (planned future additions)

### `PreToolUse` Hooks (4 Rules)

| Hook `id` | Trigger | Action | Notes |
|---|---|---|---|
| `block-destructive-shell` | `rm -rf`, `del /s`, `git reset --hard`, `git push --force` (+ 3 more) | `ask` — explicit approval required | See [destructive-command-governance.rules.md](.claude/rules/destructive-command-governance.rules.md) |
| `warn-missing-platform` | `dotnet build\|test\|run` without `-p:Platform=` | `systemMessage` — non-blocking warning | Shows corrected command with `$Platform` |
| `warn-missing-config-flag` | `dotnet build\|test` without `-c` | `systemMessage` — non-blocking warning | Shows corrected command with `-c Debug` or `-c Release` |
| `github-issue-closure-approval-gate` | `gh issue close`, `gh issue delete` | `ask` — explicit approval required | See [github-governance.rules.md](.claude/rules/github-governance.rules.md) |

### `PostToolUse` Hooks (3 Rules)

| Hook `id` | Trigger | Action | Purpose |
|---|---|---|---|
| `suggest-format-on-edit` | Edit `**/*.cs` | `systemMessage` — suggest `dotnet format <file>` | Enforce code style consistency |
| `suggest-xaml-validation` | Edit `**/*.xaml` | `systemMessage` — suggest `dotnet build` | Validate XAML binding syntax |
| `test-coverage-validation` | Edit `**/*Tests.cs` | `systemMessage` — remind 80%+ coverage check | Enforce test coverage standards |

### Execution Lifecycle Summary

```
1. File Modified → 2. PreToolUse (block/warn) → 3. Tool Execution → 4. PostToolUse (suggest) 
→ 5. Gate 2 Evidence (build + test + LSP + app run) → 6. Commit with PR reference
```

Every cycle enforces the Two-Gate System: Plan Mode for scope >3 files/200 LOC; Evidence-Based Verification before task completion.

### Conflict Resolution (GitHub Copilot ↔ Claude Code)

**File precedence (when both tools are active and disagree):**
1. `.claude/settings.json` — Harness configuration (hooks, permissions, env vars) is authoritative for enforcement
2. AGENTS.md — This document supersedes all tool-specific instructions (single source of truth for agent workflow)
3. CLAUDE.md — CLI-only reference; doesn't affect Copilot Agent Mode
4. `.github/copilot-instructions.md` — Copilot Agent Mode specifics only (path patterns, scope matrix)
5. `.claude/rules/` — Domain-specific governance rules (design, destructive commands, git hooks, GitHub)

**Principle:** No conflicts should arise because each file has a distinct audience and purpose. Hooks in `.claude/settings.json` keep them synchronized.

---

## Key Rules (Always Enforced)

**Build & Test (Non-Negotiable):**
- Every change must build (`dotnet build -c Debug -p:Platform=$Platform` succeeds with 0 errors/warnings) and pass tests (`dotnet test -c Debug -p:Platform=$Platform` 100% pass rate) before task completion
- Gate 2 requires evidence: build logs, test results, LSP verification, app execution

**Instruction Files (Mandatory Reading):**
- Read applicable `.github/instructions/*.md` files **before writing code** in that domain (see Instruction Files Index above for trigger conditions)
- These files are authoritative; do not assume or skip them based on prior knowledge

**Two-Gate System (Mandatory for Scope >3 Files or >200 LOC):**
- Gate 1: Plan Mode review + architecture documentation before coding
- Gate 2: Build + Test + LSP + App Run evidence before declaring task complete

**API Lookup (Mandatory Escalation Order):**
- Web search API docs first (always)
- Check sample repos second
- Use WinMD/decompilers only as last resort (see [Troubleshooting Build Errors](#troubleshooting-build-errors))

**Code Architecture:**
- XAML: Always `x:Bind` (never `{Binding}`), always `{ThemeResource ...}` for colors (never hard-code hex values)
- ViewModels: Free of business logic; use Services for domain logic; state management only
- Services: Stateless, validation at boundaries, constructor injection only, dispose MediaCapture properly
- Dependencies: Use DI container; never service locator pattern

**Packaging & Identity:**
- Use `winapp` CLI for app identity, packaging, and signing (never hand-roll `MakeAppx`/`SignTool`/`Add-AppxPackage`)
- Platform: Always pass `-p:Platform=$Platform` (detected per session, not per machine)
- Tests: 80%+ coverage minimum on ViewModels and Services; MSTest + Moq framework

## Windows AI Prerequisites

When integrating Windows AI APIs (Phi Silica on Copilot+ PC, Windows Vision APIs like ImageDescription, TextRecognizer, ImageScaler, etc.), follow these 4 steps. See [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) for complete catalog and sample code.

1. **Package identity is mandatory.** All Windows AI APIs require app package identity.
   - `dotnet run` provides this automatically
   - If testing outside `dotnet run`, register identity: `winapp create-debug-identity` or `winapp run`
   - Verify with: `dotnet run -c Debug -p:Platform=$Platform` (watch for package identity confirmation in output)

2. **Update Package.appxmanifest capabilities.** Each API requires specific capabilities declared in the manifest:
   - Common: `runFullTrust` (most AI APIs)
   - Also check: `internetClient` (if API needs network access)
   - Always: Read the API's documentation page for the exact capability list
   - Re-run `dotnet run` after manifest changes so the registered identity reflects the updates

3. **Hardware / OS requirements.** Some APIs are gated by hardware (Copilot+ PC, NPU) or OS version:
   - Always use `IsAvailable` / `EnsureReadyAsync` patterns to probe availability at runtime
   - Provide graceful fallback for unsupported devices (e.g., "AI features unavailable on this device")
   - Test on both supported and unsupported hardware if possible

4. **Test locally before committing:**
   - After manifest changes, re-run `dotnet run` to refresh the registered identity
   - Stale registrations silently use the old capability set and cause hard-to-debug failures
   - Verify app still runs and feature still works after each manifest edit

## Cross-References

**Configuration & Governance:**
- Quick reference (CLI users): [CLAUDE.md](./CLAUDE.md) — Platform detection, Core Guardrails, Quick Start Checklist
- Copilot Agent Mode only: [.github/copilot-instructions.md](.github/copilot-instructions.md) — Path patterns, WRAP workflow, scope matrix
- Hook configuration & enforcement: [.claude/settings.json](.claude/settings.json) — PreToolUse/PostToolUse hook definitions, permission matrix
- Destructive commands policy: [destructive-command-governance.rules.md](.claude/rules/destructive-command-governance.rules.md) — Approval gate for rm -rf, git reset --hard, git push --force
- GitHub issue closure policy: [github-governance.rules.md](.claude/rules/github-governance.rules.md) — No auto-close without explicit user approval

**Design & Architecture:**
- System architecture & design: [DESIGN.md](./DESIGN.md) — System architecture, technology boundaries, data flow
- Design tokens (authoritative): `.claude/rules/design-*.rules.md` — Colors, spacing, typography, components, accessibility

**Instruction Files (Detailed Guidance):**
- Build procedures: [agent-build-procedures.md](.github/instructions/agent-build-procedures.md) — Platform detection, build/test/run commands, winapp CLI, troubleshooting
- Code quality: [code-quality.instructions.md](.github/instructions/code-quality.instructions.md) — StyleCop, naming conventions, static analysis
- WinUI patterns: [winui-best-practices.instructions.md](.github/instructions/winui-best-practices.instructions.md) — MVVM, x:Bind, community toolkit, API verification
- Testing framework: [testing.instructions.md](.github/instructions/testing.instructions.md) — MSTest, AAA pattern, naming, coverage, on-demand test filtering
- Accessibility: [accessibility.instructions.md](.github/instructions/accessibility.instructions.md) — WCAG compliance, keyboard navigation, automation properties
- Performance: [performance.instructions.md](.github/instructions/performance.instructions.md) — x:Bind, x:Load, virtualization, async patterns
- Security: [security.instructions.md](.github/instructions/security.instructions.md) — Secrets, input validation, permissions, PII handling
- Windows APIs: [windows-apis.instructions.md](.github/instructions/windows-apis.instructions.md) — API namespace catalog, sample repos, lookup guidance

**Skills & Tools:**
- Indexed skill catalog: [SKILLS.md](./SKILLS.md) — Auto-discovery, per-user overrides, skill invocation modes
- Git hooks troubleshooting: [hook-comprehensive.rules.md](.claude/rules/hook-comprehensive.rules.md) — Formatting, build, test, secret-scan quick fixes
