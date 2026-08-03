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

### Formatting Violations

**What's Happening:** `dotnet format` detected code not matching style rules

**Common Issues:**
- Missing XML docs: `/// <summary>`
- Wrong member order: Constructor after Property (should be Constructor → Property → Methods)
- Hard-coded colors in XAML: Must use `{ThemeResource ...Brush}`
- Using `{Binding}`: Change to compile-time safe `x:Bind`
- Inconsistent spacing/indentation

**Fix:** Auto-formatter handles most issues:
```powershell
dotnet format IntVue.csproj
git add .
git commit -m "style: Auto-format code"
```

---

### Build Errors

**Error Examples:**

| Code | Meaning | Solution |
|---|---|---|
| CS0246 | Type/namespace not found | Add `using` statement |
| CS1061 | Member doesn't exist | Check spelling, verify inheritance |
| CS0103 | Name not in scope | Declare variable, add `using` |
| CS0029 | Type mismatch | Cast or change type |
| MSIX0001 | Windows App SDK error | Check `.csproj` configuration |

**Fix Process:**
1. Read error output carefully (file + line number + message)
2. Identify the issue (missing using, typo, wrong type)
3. Fix the code
4. Rebuild: `dotnet build -c Debug -p:Platform=$Platform`
5. Commit: `git commit -m "fix: Resolve compilation error"`

---

### Test Failures

**Option A: Fix Implementation (Recommended)**

```powershell
# Example: Test expects countdown to reach 0, but code reports 1
# Expected: 0, Actual: 1

# Fix the code
for (int i = seconds; i >= 0; i--)  # Change >= 1 to >= 0
{
    progress.Report(i);
}

# Re-run: dotnet test -c Debug -p:Platform=$Platform
# Commit: git commit -m "fix: Update countdown to report 0"
```

**Option B: Fix Test (If Implementation is Correct)**

```csharp
// If implementation only reports 1, fix test assertion
Assert.AreEqual(1, countdownValue);  // Correct expectation
```

**Option C: Bypass (For WIP Only)**

```powershell
$env:SKIP_TESTS_ON_FAILURE = '1'
git commit -m "feat: Work in progress - tests pending"
```

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

### Tests Skip but Should Run
```powershell
# Check environment variable
$env:RUN_TESTS

# Clear it
$env:RUN_TESTS = ''
git commit  # Tests will run now
```

### Different Results Locally vs Hook
```powershell
# Ensure platform consistency
[System.Environment]::ProcessorArchitecture

# Clean build
dotnet clean
dotnet build -c Debug -p:Platform=$Platform
```

---

## Time Estimates

| Error Type | Time | Difficulty |
|---|---|---|
| Formatting | 2-5 min | Easy (auto-fix) |
| Build | 5-30 min | Medium |
| Tests | 10-45 min | Medium-Hard |
| **Total** | **20-80 min** | Varies |

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
