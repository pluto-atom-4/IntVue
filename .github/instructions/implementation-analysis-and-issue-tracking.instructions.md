---
description: 'Implementation analysis, status reporting, GitHub issue creation, and documentation workflow'
applyTo: 'Implementation planning, status reviews, GitHub issue management'
---

# Implementation Analysis & GitHub Issue Tracking Workflow

This instruction documents the systematic workflow for analyzing implementation status, identifying gaps and blockers, creating GitHub issues, updating project documentation, and managing temporary files during implementation planning sessions.

---

## Workflow Overview

**Purpose:** Maintain clear visibility into implementation progress, identify blocking dependencies, create granular tracked work items, and keep documentation synchronized with actual work.

**When to use:** At major planning checkpoints, after completing a development phase, or when creating an implementation roadmap.

---

## Phase 1: Analyze Current Implementation Status

### Step 1.1 — Gather Baseline Data

1. **Review MVP plan:** Read the current `Docs/ImplementationPlanning/impl-mvp.md` thoroughly
   - Note which phases are complete vs. planned
   - Identify any deferred work or architectural decisions
   - Document security/privacy decisions that affect remaining work

2. **Audit code state:** Inspect the main implementation files
   - Check current classes, interfaces, and services
   - Note which components are fully implemented vs. stubs
   - Identify any incomplete patterns or missing methods

3. **Query implementation tracking:** If available, use SQL to track completed vs. pending work
   ```sql
   SELECT phase, status, COUNT(*) FROM implementation_phases GROUP BY phase, status;
   SELECT * FROM remote_debug_blockers WHERE severity = 'CRITICAL';
   ```

### Step 1.2 — Document Current State

Create a summary table covering:
- ✅ Completed phases (with effort invested)
- 📄 Documented phases (with issue count)
- ⏳ Pending phases (with estimated effort)
- 🔴 Identified blockers (severity & impact)

**Example:**
```
Phases 0-3: ✅ COMPLETE (28 hours invested)
  - Phase 0: Discovery
  - Phase 1: Foundation (DI, manifest)
  - Phase 2: Core MediaCapture
  - Phase 3: ViewModel & Commands

Phases 4-6: 📄 DOCUMENTED (31.5 hours estimated)
  - Phase 4: UI & Accessibility (3 issues)
  - Phase 5: Interview Features (4 issues)
  - Phase 6: Tests & Validation (3 issues)

Blockers: CRITICAL (1 identified)
  - #13: Device-not-found crash (blocks all downstream work)
```

### Step 1.3 — Identify Blocking Dependencies

Analyze which issues/features depend on others:

1. **CRITICAL blockers:** Issues that prevent ALL downstream work
   - Mark as 🔴 CRITICAL
   - Estimate time to fix
   - Document solution if known

2. **HIGH blockers:** Issues that prevent entire phases
   - Mark as 🟡 HIGH
   - Map which phases/issues are blocked

3. **Dependency chains:** Document the order of work
   - Use this to create execution priority

**SQL query pattern:**
```sql
SELECT blocker, severity, affected_by, status FROM remote_debug_blockers;
```

### Step 1.4 — Identify Infrastructure & Debugging Prerequisites

For projects requiring cross-device debugging or hardware-dependent testing, analyze infrastructure blockers separately:

1. **Device Handling Issues**
   - Does the app crash or fail gracefully when hardware is missing?
   - Are there null device checks before accessing camera, microphone, or sensors?
   - Can the app run on desktop (no hardware) for UI testing?
   - Mark as 🔴 CRITICAL if blocking all downstream phases

2. **Remote Debugging Infrastructure**
   - Can the app be debugged from a development machine to a target device?
   - Are build deployment options documented (fast inner-loop vs. production MSIX)?
   - Is the remote debugger configured and tested?
   - Mark as 🟡 HIGH if prerequisite for hardware testing phases

3. **Mock/Offline Testing Support**
   - Can unit tests run without hardware dependencies?
   - Are mock service implementations available?
   - Can the app fall back gracefully to mock services in offline mode?
   - Mark as 🟡 HIGH if blocking parallel offline development

**Checklist for cross-device development:**
- [ ] Device-not-found handling implemented (graceful vs. crash)
- [ ] Mock services created for offline unit testing
- [ ] Remote debugging infrastructure documented (VS Remote Tools, network setup)
- [ ] Build/deploy workflow established for target device
- [ ] Rider or IDE remote debugging configuration step-by-step
- [ ] Troubleshooting guide for common remote debugging issues
- [ ] Three-phase workflow documented: desktop-only → hardware-focused → validation

**Example blocker chain:**
```
Issue #13 (CRITICAL): Device-not-found crash
  └─ BLOCKS Issue #14 (remote debugging setup) and Issue #15 (mock services)
     └─ BLOCKS Phase 4 (UI testing on Surface)
        └─ BLOCKS Phase 5 (interview features)
```

---

## Phase 2: Create Detailed GitHub Issue Descriptions

### Step 2.1 — Identify Issues to Create

Based on gap analysis, determine which issues are needed:
- Missing prerequisites (infrastructure, setup, mocks)
- Blocking dependencies (must fix before downstream work)
- Feature work (phases 4-6)
- Documentation/research tasks

### Step 2.2 — Write Issue Content

#### Generic Content Template

For each issue, create a markdown file in `Docs/ImplementationPlanning/` with:

**File naming:** `issue-{NUMBER}-{KEBAB-CASE-TITLE}.md`

**Content template:**
```markdown
## Problem
[Clear description of the gap, blocker, or feature request]

## Scenario / Impact
[Specific use case or consequences of not doing this]

## Solution
[Detailed solution with code examples if applicable]

## Code Location
- **File:** Path to relevant code
- **Method/Class:** Specific location
- **Line numbers:** Where changes go

## Acceptance Criteria
- [ ] Item 1
- [ ] Item 2
- [ ] Item 3

## Related Issues
- Blocks: #X (if this unblocks downstream work)
- Depends on: #Y (if this depends on other work)

## Effort Estimate
**X hours/minutes**

## Priority
[🔴 CRITICAL / 🟡 HIGH / 🟢 MEDIUM]

## Reference
[Links to documentation, samples, or related guidance]
```

**Key guidelines:**
- Keep problem statements concise and specific
- Provide working code examples (not just concepts)
- Include acceptance criteria that are testable
- Mark blocking relationships clearly
- Estimate effort realistically (30 min to 3 hours per issue)

### Step 2.2.1 — Remote Debugging Issue Template (for infrastructure blockers)

For device handling, mock services, and remote debugging setup issues:

```markdown
## Problem
[Specific failure mode: crash on missing device, cannot deploy to Surface, tests fail without hardware, etc.]

## Scenario / Impact
- **Environment:** Desktop PC (no hardware) vs. Surface Tablet (with camera/mic)
- **Blocked workflow:** [e.g., "Cannot test UI on Surface from Desktop PC"]
- **Impact on phases:** [e.g., "Blocks Phase 4 (UI testing) and Phase 5 (interview features)"]

## Solution

### For Device Handling Issues:
Include working C# code with:
- Device availability check (`DeviceInformation.FindAllAsync`)
- Graceful null/empty device handling (no crash)
- Fallback behavior (mock, disabled preview, etc.)

### For Remote Debugging Infrastructure:
Include step-by-step setup:
1. Prerequisites (Developer Mode, VS Remote Tools version, network)
2. Build/deploy process (dotnet build, winapp run with IP/architecture)
3. Rider configuration (remote connection, breakpoint setup)
4. Troubleshooting (common failures and solutions)

### For Mock Services:
Include mock implementation with:
- Interface implementation (all public methods)
- Task-returning methods (async without actual work)
- Property setters for test assertions
- No external dependencies

## Code Location
- **File:** Path to affected code (e.g., Services/MediaCaptureService.cs)
- **Method/Class:** Specific entry point (e.g., InitializeAsync)
- **Related files:** Tests/Mocks/MockMediaCaptureService.cs

## Acceptance Criteria
- [ ] App runs on Desktop without hardware (graceful device-not-found)
- [ ] Mock service passes all unit tests
- [ ] Remote debugger connects to Surface successfully
- [ ] Build/deploy workflow documented with examples
- [ ] Troubleshooting guide includes 5+ common issues
- [ ] No crashes or unhandled exceptions

## Testing Strategy
1. **Desktop (no hardware):** Unit tests with mocks; UI runs with preview disabled
2. **Surface Tablet:** Deploy app; verify camera/mic access; remote debug from Desktop
3. **Both environments:** Validate graceful fallbacks and error messages

## Related Issues
- Blocks: #X, #Y (downstream phases/features)
- Depends on: #Z (if prerequisites exist)

## Effort Estimate
**X hours (device handling: 0.5h, infrastructure: 1-2h, mocks: 1-1.5h)**

## Priority
[🔴 CRITICAL / 🟡 HIGH]

## Reference
- [WinAppSDK MediaCapture API](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.mediaelement)
- [Visual Studio Remote Debugging](https://learn.microsoft.com/en-us/visualstudio/debugger/remote-debugging)
- [Rider Remote Debugging Guide](https://www.jetbrains.com/help/rider/Debugging-Code.html)
```

### Step 2.2.2 — Generic Issue Template

For each markdown file created:

1. **Use `gh` CLI for creation:**
   ```powershell
   cd C:\{repo-path}
   gh issue create `
     --title "{TITLE}" `
     --body-file "Docs/ImplementationPlanning/issue-{N}-{SLUG}.md" `
     --label "{label1},{label2}"
   ```

2. **Record the issue number:** Note the GitHub issue # returned
3. **Link dependent issues:** Update related issues with blocking/blocked relationships

**Label guidance (use existing labels):**
- `bug` — For fixes/corrections
- `enhancement` — For new features
- `documentation` — For docs/research
- `help wanted` — For infrastructure/setup tasks
- `good first issue` — For well-scoped, accessible tasks

---

## Phase 3: Update MVP Plan Documentation

### Step 3.1 — Add Prerequisites Section

At the end of `Docs/ImplementationPlanning/impl-mvp.md`, add a new section:

```markdown
---

## Prerequisites: Critical Issues & Infrastructure (DATE)

### Issue #X — TITLE

**Problem:** [One sentence describing the blocker]

**Impact:** [Bullet list of what's blocked]

**Solution:** [Code snippet or brief steps]

**Status:** Ready to implement (GitHub Issue #X)
**Effort:** [X hours/minutes]
**Priority:** [🔴 CRITICAL / 🟡 HIGH]
```

### Step 3.2 — Document Execution Priority

Add a workflow diagram:

```markdown
### Execution Priority

\`\`\`
1. FIX #X (blocker)
   └─ UNBLOCKS #Y-#Z, Phases A-B

2. PARALLEL #A & #B (infrastructure)
   └─ Enables hardware testing / offline testing

3. START Phase N
   └─ #M, #O, #P
\`\`\`
```

### Step 3.3 — Add Next Steps Section

```markdown
### Next Steps

1. **Immediate (X hours):** Implement Issue #X (blocker)
2. **Then (X hour):** Setup infrastructure (Issue #Y)
3. **Then (X hours):** Begin Phase N work

All N GitHub issues created and ready. See issues #X-#Y for detailed requirements.
```

---

## Phase 4: Clean Up Temporary Files

### Step 4.1 — Identify Temporary Files

Files to remove after creating GitHub issues:
- Issue description markdown files (`issue-{N}-*.md`)
- Any draft analysis or planning notes not committed
- Temporary SQL dumps or debugging logs

**Location:** `Docs/ImplementationPlanning/`

### Step 4.2 — Remove Temporary Files

```powershell
cd C:\{repo-path}\Docs\ImplementationPlanning

# Remove temporary issue markdown files
Remove-Item issue-*.md

# Verify only permanent files remain
Get-ChildItem
```

**Files to keep:**
- `impl-mvp.md` (always keep, update instead)
- `GITHUB-ISSUES-CREATED.md` (optional summary)
- Any reference documentation

---

## Phase 5: Commit Changes to Main

### Step 5.1 — Stage Documentation Updates

```powershell
cd C:\{repo-path}

# Stage only the MVP plan update
git add Docs/ImplementationPlanning/impl-mvp.md

# Verify staged files
git status
```

### Step 5.2 — Create Commit Message

**Format:**
```
docs: Add critical prerequisites (issues #X-#Y) to MVP plan

- Issue #X: TITLE (effort)
- Issue #Y: TITLE (effort)
- Updated impl-mvp.md with prerequisites section
- All issues created on GitHub with detailed requirements

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>
```

### Step 5.3 — Commit and Verify

```powershell
git commit -m "docs: Add critical prerequisites to MVP plan

- Issue #13: CRITICAL fix (30 min)
- Issue #14: Infrastructure setup (1 hour)
- Issue #15: Mock service (1 hour)
- Updated impl-mvp.md with prerequisites
- All issues created on GitHub

Co-authored-by: Copilot <223556219+Copilot@users.noreply.github.com>"

# Verify commit
git log --oneline -3
```

---

## Phase 6: Track in SQL Database

### Step 6.1 — Record GitHub Activities

```sql
INSERT INTO github_activities (activity_type, item_id, title, status, created_at) VALUES
  ('issue', '#13', 'CRITICAL: Fix device-not-found crash', 'created', datetime('now')),
  ('issue', '#14', 'Setup: Remote debugging infrastructure', 'created', datetime('now')),
  ('issue', '#15', 'TEST: MockMediaCaptureService', 'created', datetime('now')),
  ('commit', 'abc123', 'docs: Add prerequisites to MVP plan', 'complete', datetime('now'));
```

### Step 6.2 — Update Implementation Phases

```sql
UPDATE implementation_phases SET status = 'documented' 
WHERE phase IN ('Phase 4', 'Phase 5', 'Phase 6')
AND status = 'pending';

INSERT INTO implementation_phases (phase, status, issues_count, estimated_hours, notes)
VALUES ('Prerequisites', 'created', 3, 2.5, 'Blockers #13-15, unblocks Phases 4-6');
```

---

## Validation Checklist

- [ ] Implementation status analyzed and documented
- [ ] Blocking dependencies identified with severity levels
- [ ] Issue markdown files created with complete details
- [ ] All GitHub issues created successfully via `gh` CLI
- [ ] MVP plan updated with prerequisites section
- [ ] Temporary issue markdown files removed
- [ ] Changes committed to main branch
- [ ] SQL database updated with new activities
- [ ] Commit message includes all issue numbers
- [ ] git log shows clean commit history

---

## Anti-Patterns to Avoid

❌ **Creating issues without markdown:** Write detailed descriptions first, then create issues  
❌ **Leaving temporary files:** Always clean up issue-*.md files after creation  
❌ **Not documenting blocking relationships:** Mark all #Blocks and #Depends-on relationships  
❌ **Incomplete acceptance criteria:** Make criteria testable and exhaustive  
❌ **Skipping MVP plan update:** Always update impl-mvp.md with new prerequisites/changes  
❌ **Vague effort estimates:** Be realistic; use 30-min buckets (0.5h, 1h, 1.5h, etc.)  
❌ **Forgotten commits:** Always commit with Co-authored-by trailer  

### Remote Debugging Anti-Patterns

❌ **Ignoring device-not-found crashes:** Assume all devices exist — crashes on Desktop testing  
❌ **Missing mock services:** No offline testing capability — blocks parallel development  
❌ **Skipping infrastructure blocker analysis:** Discover remote debugging issues late in project  
❌ **Incomplete deployment workflow:** No documented steps for build/deploy to target device  
❌ **Vague troubleshooting guides:** List problems without solutions — wasted debugging time  
❌ **No graceful fallbacks:** App disabled entirely when hardware missing — cannot test UI  
❌ **Untested on both environments:** Desktop-only testing masks hardware-dependent bugs  
❌ **Missing three-phase workflow:** No clear strategy for desktop → remote → validation phases  

---

## Reusable SQL Queries

### Query 1: Implementation Progress
```sql
SELECT phase, status, COUNT(*) as count, SUM(estimated_hours) as total_hours
FROM implementation_phases
GROUP BY phase, status
ORDER BY phase;
```

### Query 2: Blocking Dependencies
```sql
SELECT issue_title, blocker, severity, affected_by, status
FROM remote_debug_blockers
WHERE severity IN ('CRITICAL', 'HIGH')
ORDER BY severity DESC;
```

### Query 3: GitHub Activity Summary
```sql
SELECT activity_type, COUNT(*) as count, status
FROM github_activities
WHERE created_at >= date('now', '-1 day')
GROUP BY activity_type, status;
```

---

## Example Workflow (Complete Session)

1. **Analyze:** Read impl-mvp.md, check code state, identify gaps
2. **Identify blockers:** Find #13 (device-not-found), #14 (remote debugging), #15 (mocks)
3. **Create issues:** Write detailed markdown → `gh issue create` → 3 issues on GitHub
4. **Update docs:** Add prerequisites section to impl-mvp.md
5. **Clean up:** Remove issue-*.md temporary files
6. **Commit:** `git add`, `git commit` with co-authored-by trailer
7. **Track:** Insert activities into SQL database
8. **Verify:** `git log`, `gh issue list`, confirm working tree clean

**Total time:** ~2 hours (analysis, writing, creation, documentation, commit)

---

## Example Workflow: Cross-Device Remote Debugging (Surface Tablet Scenario)

This example applies the workflow to identify and document infrastructure blockers for remote debugging from Desktop PC to Surface Tablet.

### Phase 1: Analyze Implementation Status

**Step 1.1 — Gather Baseline Data**
- Review `impl-mvp.md`: Phase 0-3 complete, Phase 4 (UI testing) requires Surface Tablet
- Audit `Services/MediaCaptureService.cs`: No device-not-found handling (will crash on Desktop)
- Identify: Cannot test UI on Desktop (no camera); cannot debug on Surface (no remote setup)

**Step 1.2 — Document Current State**
```
Phases 0-3: ✅ COMPLETE (28 hours)
Phases 4-6: 📄 PLANNED (31.5 hours) — BLOCKED by infrastructure

Blockers: CRITICAL (3 identified)
  - #13: Device-not-found crash (crashes on Desktop without camera)
  - #14: Remote debugging not configured (no Surface connectivity)
  - #15: No mock services (tests fail without hardware)
```

**Step 1.4 — Identify Infrastructure & Debugging Prerequisites**
- **Device handling:** App crashes on Desktop (no camera) → cannot test UI
- **Remote debugging:** No infrastructure for Surface debugging → cannot reach hardware-dependent code
- **Mock services:** Unit tests require real MediaCapture → cannot run offline

### Phase 2: Create Detailed GitHub Issue Descriptions

Create three issue markdown files in `Docs/ImplementationPlanning/`:

**issue-13-device-not-found-crash.md**
```markdown
## Problem
MediaCaptureService crashes on Desktop PC when DeviceInformation.FindAllAsync() returns 0 devices.

## Scenario / Impact
- **Environment:** Desktop PC without camera/microphone
- **Blocked workflow:** Cannot test UI (app crashes during initialization)
- **Impact:** Blocks Phase 4 (UI testing), Phase 5 (interview features), remote debugging setup

## Solution
Add graceful device-not-found check in MediaCaptureService.InitializeAsync():
- If no devices found, log warning and return (don't crash)
- Set initialized = true so ViewModel can proceed
- Preview will be disabled but UI remains functional

[Include C# code example from impl-mvp.md section "Handling Device-Not-Found Gracefully"]

## Code Location
- **File:** Services/MediaCaptureService.cs
- **Method:** InitializeAsync (line 49)
- **Change:** Add `if (devices.Count == 0)` check before accessing devices[0]

## Acceptance Criteria
- [ ] App runs on Desktop without camera (no crash)
- [ ] InitializeAsync sets initialized = true gracefully
- [ ] Debug output shows "Warning: No camera device found"
- [ ] Unit tests pass with mock service

## Effort Estimate
**30 minutes**

## Priority
🔴 CRITICAL (unblocks all downstream work)
```

**issue-14-remote-debugging-setup.md**
```markdown
## Problem
Infrastructure for remote debugging from Desktop PC to Surface Tablet not configured.

## Scenario / Impact
- Cannot deploy app to Surface from Desktop
- Cannot attach debugger to app running on Surface
- Cannot test camera/microphone functionality without manual device access

## Solution
Document and implement:
1. Developer Mode on both Desktop and Surface
2. Visual Studio Remote Tools (ARM64 version for Surface)
3. Network connectivity verification (ipconfig)
4. Rider remote debugging configuration (host, port, breakpoints)
5. Build/deploy options: fast inner-loop (dotnet run) and production MSIX

[Include step-by-step setup from impl-mvp.md section "Configure Rider for Remote Debugging"]

## Acceptance Criteria
- [ ] Developer Mode enabled on both machines
- [ ] Visual Studio Remote Tools installed on Surface
- [ ] Rider remote connection configured (Surface IP, port 4026)
- [ ] Build succeeds for ARM64 architecture
- [ ] App deploys to Surface and runs
- [ ] Breakpoints hit during remote debugging
- [ ] Troubleshooting guide documents 7 common issues

## Effort Estimate
**1 hour (one-time setup)**

## Priority
🟡 HIGH (prerequisite for hardware testing)
```

**issue-15-mock-media-capture-service.md**
```markdown
## Problem
Unit tests crash on Desktop without hardware. Need mock implementation for offline testing.

## Solution
Create Tests/Mocks/MockMediaCaptureService.cs implementing IMediaCaptureService:
- All async methods return Task.CompletedTask
- Properties have getters/setters for test assertions
- IsRecording and other state can be verified

[Include mock implementation from impl-mvp.md section "Creating Mock Services for Unit Testing"]

## Acceptance Criteria
- [ ] Mock service implements all IMediaCaptureService methods
- [ ] Mock tests pass without hardware
- [ ] ViewModel tests use mock service via DI
- [ ] No external dependencies

## Effort Estimate
**1 hour**

## Priority
🟡 HIGH (enables offline unit testing)
```

### Phase 3 & Beyond

- Create GitHub issues via `gh issue create --body-file`
- Update `impl-mvp.md` with prerequisites section
- Clean up temporary issue-*.md files
- Commit with documented blocker chain
- Execute fixes in priority order: #13 → #14 & #15 → Phase 4

---

## Must Read & Research

When following this workflow, consult:

| Reference | When | Purpose |
|-----------|------|---------|
| `.github/instructions/design-principles.instructions.md` | Before issue analysis | Validate architecture decisions |
| `Docs/ImplementationPlanning/impl-mvp.md` | During analysis phase | Understand current scope & decisions |
| [GitHub CLI docs](https://cli.github.com/manual/) | When creating issues | Correct `gh` syntax |
| Git commit best practices | During commit phase | Write clear, linked commit messages |
| [Microsoft Learn SQL](https://learn.microsoft.com/en-us/sql/) | When tracking in SQL | Query existing data for patterns |
| **Remote Debugging Resources** | When analyzing cross-device issues | See table below |

### Remote Debugging: Research & References

For projects requiring hardware-dependent testing or remote debugging:

| Resource | Reference | When to consult |
|----------|-----------|-----------------|
| WinAppSDK MediaCapture | [Windows.Media.Capture API Docs](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/microsoft.ui.xaml.controls.mediaelement) | Implementing graceful device-not-found handling |
| Visual Studio Remote Tools | [VS Remote Debugging Guide](https://learn.microsoft.com/en-us/visualstudio/debugger/remote-debugging) | Setting up remote debugger for target device |
| Rider Debugging | [Rider Debugger Documentation](https://www.jetbrains.com/help/rider/Debugging-Code.html) | Configuring IDE for remote debugging |
| WinAppSDK Deployment | [Set Up Dev Environment](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/set-up-your-development-environment) | Build & deploy to target device (winapp CLI) |
| winapp CLI | [winapp CLI Usage Docs](https://github.com/microsoft/WinAppCli/blob/main/docs/usage.md) | Deploying packages and managing identity |
| Device Info API | [DeviceInformation Class Docs](https://learn.microsoft.com/en-us/uwp/api/windows.devices.enumeration.deviceinformation) | Detecting and handling missing hardware |

---

## Next Session Continuation

If this workflow is interrupted:

1. **Check SQL tables:** Query `github_activities` to see what was created
2. **Check git status:** Verify uncommitted work
3. **Check temp files:** Look for remaining `issue-*.md` files in `Docs/ImplementationPlanning/`
4. **Resume from appropriate step:** See Phase numbers above

**Key files to watch:**
- `Docs/ImplementationPlanning/impl-mvp.md` (always committed)
- `Docs/ImplementationPlanning/issue-*.md` (remove before commit)
- Latest commits in main branch (should include Co-authored-by trailer)
