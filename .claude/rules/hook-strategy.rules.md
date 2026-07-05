# Hook Rule: Error Resolution Strategy

**Project:** IntVue  
**Status:** ✅ All hooks operational - 0 blocking errors

This file documents the project-level strategy for handling pre-commit and pre-push hook errors.

---

## Hook Overview

The project uses **two automated hooks** that block commits and pushes when code quality checks fail:

| Hook | Triggers When | Exit Code | Blocks? |
|---|---|---|---|
| **pre-commit** | Before `git commit` | Non-zero | ✅ Yes |
| **pre-push** | Before `git push` | Non-zero | ✅ Yes |

### Hook Execution Order

```
pre-commit.ps1
  ├─ Formatting check (dotnet format --verify-no-changes)
  ├─ Build (dotnet build -c Debug -p:Platform=x64)
  └─ Tests (dotnet test -c Debug -p:Platform=x64)

pre-push.ps1
  ├─ Runs pre-commit checks first
  └─ (Optional) Full test suite if RUN_FULL_TESTS=1
```

---

## Blocking Behavior

### 🔴 ALWAYS Blocks (No Bypass)

| Check | Blocks? | Bypass? |
|---|---|---|
| Formatting violations | ✅ YES | ❌ No |
| Build errors | ✅ YES | ❌ No |

**Example:** Commit is rejected until code is formatted and compiles.

### 🟡 BLOCKS But Can Bypass

| Check | Blocks? | Bypass? | Env Var |
|---|---|---|---|
| Test failures | ✅ YES | ✅ Yes | `SKIP_TESTS_ON_FAILURE=1` |

**Example:** 
```powershell
# Default: Block on test failure
git commit  # ❌ Blocked

# Bypass: Allow commit despite test failure
SKIP_TESTS_ON_FAILURE=1 git commit  # ✅ Allowed
```

### ✅ Does NOT Block

| Check | Blocks? | Notes |
|---|---|---|
| Build warnings | ❌ No | Logged but allowed (e.g., CA1016, CA1416) |
| Code analysis warnings | ❌ No | StyleCop informational warnings |

**Example:** Commit succeeds even if build has warnings.

---

## Error Resolution Strategy

### Phase 1: Prevention (Ongoing)

**Goal:** Prevent errors from blocking commits in the first place

**Best Practice:**
```powershell
# Before every commit, run:
dotnet format IntVue.csproj
dotnet build -c Debug -p:Platform=x64
dotnet test -c Debug -p:Platform=x64
git commit -m "feature message"
```

**IDE Configuration (Automatic):**
- **Rider:** Settings → Actions on Save → Reformat code
- **Visual Studio:** Tools → Options → Format on save

---

### Phase 2: Quick Resolution (When Errors Occur)

**Goal:** Resolve blocking errors in 2-30 minutes using documented procedures

**Procedure:**

| Error Type | Resolution | Time | Owner |
|---|---|---|---|
| Formatting | Run `dotnet format` | 2-5 min | Developer |
| Build | Fix compilation errors | 5-30 min | Developer |
| Tests | Fix failing test(s) | 10-45 min | Developer |

**Reference:** See `hook-quick-fix.rules.md` for step-by-step checklist.

---

### Phase 3: Escalation (Persistent Issues)

**Trigger:** Error persists after 2+ resolution attempts

**Escalation Path:**
1. Clean build: `dotnet clean && dotnet build`
2. Check environment: `[System.Environment]::ProcessorArchitecture`
3. Review recent changes: `git log -p --follow -S "error keyword"`
4. Ask team for code review
5. Update hook documentation if new error pattern found

**Owner:** Code review team / Lead developer  
**Timeline:** Within 24 hours

---

## Error Categories

### 🔴 High Priority (Blocks Immediately)

**Formatting Violations**
- Impact: Cannot commit
- Response: Auto-fix with `dotnet format`
- Prevention: IDE auto-format on save
- Time: 2-5 minutes

**Build Errors**
- Impact: Cannot commit
- Response: Fix compilation errors
- Prevention: Build locally before commit
- Time: 5-30 minutes

**Test Failures**
- Impact: Cannot commit (bypass available)
- Response: Fix test or implementation
- Prevention: Run tests locally before commit
- Time: 10-45 minutes

### 🟡 Medium Priority (Logged, Not Blocking)

**Build Warnings**
- Impact: Logged but don't prevent commit
- Examples: CA1016, deprecated APIs
- Response: Fix during code review
- Owner: Individual developer

**Code Analysis Warnings**
- Impact: Logged but don't prevent commit
- Examples: StyleCop informational
- Response: Fix during development
- Owner: Individual developer

---

## Success Metrics

### Current Status (✅ All Green)

```
Formatting:  ✅ PASS (no StyleCop violations)
Build:       ✅ PASS (0 errors, 0 warnings in enforced rules)
Tests:       ✅ PASS (18/18 tests passing)
Code:        ✅ PASS (all linting rules satisfied)
```

### Monitoring Goals

- **Commit Success Rate:** >95% (errors resolved quickly)
- **Average Resolution Time:** <30 minutes per error
- **False Positive Rate:** <5% (legitimate blocks)
- **Developer Satisfaction:** Guides solve issue first time

---

## Key Rules

1. **Formatting is required** - Cannot bypass
2. **Build must succeed** - Cannot bypass
3. **Tests should pass** - Can bypass if intentional
4. **Warnings are logged** - Do not block commits
5. **Prevention is preferred** - Run checks before commit
6. **Documentation guides resolution** - See hook-quick-fix.rules.md

---

## Implementation Details

### Hook Scripts Location

```
scripts/
├── pre-commit.ps1      ← Formatting, build, tests
└── pre-push.ps1        ← Calls pre-commit + optional full suite
```

### Hook Activation

Hooks auto-activate when you clone the repo (installed via `.git/hooks/`).

To manually test:
```powershell
powershell -File scripts\pre-commit.ps1
powershell -File scripts\pre-push.ps1
```

### Environment Variables

```powershell
# Skip tests (entire test suite)
$env:RUN_TESTS = '0'

# Bypass test failures (warns but allows commit)
$env:SKIP_TESTS_ON_FAILURE = '1'

# Run full test suite during push
$env:RUN_FULL_TESTS = '1'
```

---

## References

| Document | Purpose |
|---|---|
| `hook-quick-fix.rules.md` | 5-step checklist for each error type |
| `hook-resolution.rules.md` | Detailed guides + prevention strategies |
| `CLAUDE.md` | Main project guidance (references hooks) |
| `scripts/pre-commit.ps1` | Hook implementation |
| `scripts/pre-push.ps1` | Hook implementation |

---

## Related Rules

- **Code Quality:** `.github/instructions/code-quality.instructions.md`
- **Testing:** `.github/instructions/testing.instructions.md`
- **WinUI Best Practices:** `.github/instructions/winui-best-practices.instructions.md`
