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

---

## Phase 2: Create Detailed GitHub Issue Descriptions

### Step 2.1 — Identify Issues to Create

Based on gap analysis, determine which issues are needed:
- Missing prerequisites (infrastructure, setup, mocks)
- Blocking dependencies (must fix before downstream work)
- Feature work (phases 4-6)
- Documentation/research tasks

### Step 2.2 — Write Issue Content

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

### Step 2.3 — Create GitHub Issues

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

## Must Read & Research

When following this workflow, consult:

| Reference | When | Purpose |
|-----------|------|---------|
| `.github/instructions/design-principles.instructions.md` | Before issue analysis | Validate architecture decisions |
| `Docs/ImplementationPlanning/impl-mvp.md` | During analysis phase | Understand current scope & decisions |
| [GitHub CLI docs](https://cli.github.com/manual/) | When creating issues | Correct `gh` syntax |
| Git commit best practices | During commit phase | Write clear, linked commit messages |
| [Microsoft Learn SQL](https://learn.microsoft.com/en-us/sql/) | When tracking in SQL | Query existing data for patterns |

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
