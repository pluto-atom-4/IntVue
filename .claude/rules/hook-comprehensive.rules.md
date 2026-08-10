# Git Hooks — Comprehensive Guide

All commits and pushes are validated via pre-commit and pre-push hooks. This guide covers quick fixes, detailed resolution, and prevention strategies.

---

## Quick Fix Checklist

**When commit is blocked:**

### Step 1: Identify the Error

| Error | Section | Time |
|---|---|---|
| Formatting violations | A | 2-5 min |
| Build errors | B | 5-30 min |
| Test failures | C | 10-45 min |

### Section A: Fix Formatting Errors

```powershell
# Auto-fix formatting
dotnet format IntVue.csproj

# Review changes
git diff

# Stage and commit fix
git add .
git commit -m "style: Format code"

# Retry original commit
git commit -m "feat: your feature"
```

**Common Issues:**
- Missing XML docs → Auto-adds `/// <summary>`
- Wrong member order → Reorders per StyleCop SA1201
- Hard-coded colors → Must use `{ThemeResource ...}`
- `{Binding}` usage → Change to `x:Bind`

### Section B: Fix Build Errors

```powershell
# See full error
dotnet build -c Debug -p:Platform=x64

# Fix the error (add using, fix typo, etc.)
# Common: CS0246 (type not found), CS1061 (member not found)

# Rebuild to verify
dotnet build -c Debug -p:Platform=x64

# Commit fix
git commit -m "fix: Resolve compilation error"
```

### Section C: Fix Test Failures

**Option 1: Fix Implementation (Recommended)**
```powershell
# See which tests fail
dotnet test -c Debug -p:Platform=x64

# Fix implementation based on error message

# Re-run tests
dotnet test -c Debug -p:Platform=x64

# Commit fix
git commit -m "fix: Resolve failing tests"
```

**Option 2: Bypass (For WIP Only)**
```powershell
# Bypass test check (warns but allows commit)
SKIP_TESTS_ON_FAILURE=1 git commit -m "feat: WIP - tests pending"
```

---

## Hook System Overview

### Mandatory Checks (Always Block)

| Check | Blocks? | Command | Fix |
|---|---|---|---|
| **Formatting** | ✅ YES | `dotnet format` | Run auto-fix |
| **Build** | ✅ YES | `dotnet build -c Debug -p:Platform=$Platform` | Fix compilation errors |

### Conditional Checks (Can Bypass)

| Check | Blocks? | Bypass | Command |
|---|---|---|---|
| **Tests** | ✅ YES | ✅ `SKIP_TESTS_ON_FAILURE=1` | `dotnet test -c Debug -p:Platform=$Platform` |

### Non-Blocking (Logged Only)

- Build warnings (e.g., CA1016, CA1416) — Do not prevent commits

---

## Prevention Strategy

**Run this before EVERY commit:**

```powershell
# Detect platform
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

# Format
dotnet format IntVue.csproj

# Build
dotnet build -c Debug -p:Platform=$Platform

# Test
dotnet test -c Debug -p:Platform=$Platform

# Review & commit
git diff
git commit -m "feat: your feature"
```

**IDE Auto-Format (Automatic Prevention):**
- **Rider:** Settings → Actions on Save → Reformat code
- **Visual Studio:** Tools → Options → Format on save

---

## Detailed Error Resolution

Beyond the Quick Fix steps above, these reference tables help diagnose *which* error you're seeing.

### Build Error Codes

| Code | Meaning | Solution |
|---|---|---|
| CS0246 | Type/namespace not found | Add `using` statement |
| CS1061 | Member doesn't exist | Check spelling, verify inheritance |
| CS0103 | Name not in scope | Declare variable, add `using` |
| CS0029 | Type mismatch | Cast or change type |
| MSIX0001 | Windows App SDK error | Check `.csproj` configuration |
| WinRT001 | WinRT projection error | Check `EnableUnsafeMixedMicrosoftWindowsUIXamlProjections` |

### Test Failures — Fix Code vs. Fix Test

When a test fails, decide which side is wrong before editing:
- **Implementation is wrong** (test expectation is correct) → fix the code, re-run, commit `fix: ...`.
- **Test expectation is wrong** (implementation is correct) → fix the assertion, re-run, commit `fix: Correct test expectation`.
- **Intentional WIP** → bypass with `SKIP_TESTS_ON_FAILURE=1` (Section C above).

| Issue | Cause | Fix |
|---|---|---|
| `Assert.AreEqual failed` | Expected ≠ Actual | Fix code or test |
| `NullReferenceException` | Null object access | Set up mock properly |
| `Task timeout` | Operation takes too long | Increase timeout or optimize |
| `Progress callback not executed` | Async timing issue | Add `await Task.Delay(10)` |

---

## Hook Execution Flow

```
Before Commit:
  ├─ Format check (dotnet format --verify-no-changes)
  ├─ Build (dotnet build -c Debug -p:Platform=$Platform)
  └─ Tests (dotnet test -c Debug -p:Platform=$Platform)

Before Push:
  ├─ Run pre-commit checks
  ├─ Optional: Full test suite (if RUN_FULL_TESTS=1)
  └─ Optional: Secret scanning (gitleaks detect)
```

---

## Environment Variables

| Variable | Effect |
|---|---|
| `RUN_TESTS=0` | Skip test execution entirely |
| `SKIP_TESTS_ON_FAILURE=1` | Warn if tests fail, but allow commit |
| `RUN_FULL_TESTS=1` | Run complete test suite on push |

---

## Troubleshooting

### Hook Not Running
```powershell
# Test hook manually
powershell -File scripts\pre-commit.ps1
powershell -File scripts\pre-push.ps1
```

If tests skip unexpectedly, check `$env:RUN_TESTS` (see Environment Variables above).
If build/test results differ from local, run a clean build (`dotnet clean`) and confirm
`$Platform` matches `[System.Environment]::ProcessorArchitecture`.

**Time estimates:** Formatting 2-5 min · Build 5-30 min · Tests 10-45 min (see Section headers above).

---

## Success Criteria

✅ Error identified via quick fix checklist  
✅ Fix applied per detailed resolution section  
✅ Build: `dotnet build -c Debug -p:Platform=$Platform` succeeds  
✅ Tests: `dotnet test -c Debug -p:Platform=$Platform` pass (or bypassed intentionally)  
✅ Formatting: `dotnet format` completes with no changes  
✅ Commit succeeds without hook blocks

---

## Key Rules

- ✅ **Run format/build/test BEFORE committing** (prevents 90% of blocks)
- ✅ **Use this checklist first** when blocked
- ✅ **Read error messages carefully** (they tell you exactly what's wrong)
- ✅ **Ask team if stuck after 30 minutes** (escalate appropriately)
- ✅ **Bypass tests only for intentional WIP** (rare, documented in commit)
