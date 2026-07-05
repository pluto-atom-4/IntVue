# Hook Rule: Quick Fix Checklist

**Status:** Use this first when your commit is blocked by hooks

Keep this checklist handy: `.claude/rules/hook-quick-fix.rules.md`

---

## 🛑 Commit Blocked? Use This Checklist

### Step 1: Identify the Error

Read the error message from `git commit` output. Match it below:

```
❌ "dotnet format detected formatting issues"  → Section A (2-5 min)
❌ "dotnet build failed"                       → Section B (5-30 min)  
❌ "dotnet test failed"                        → Section C (10-45 min)
```

---

## Section A: Fix Formatting Errors

**Error:** `dotnet format detected formatting issues (exit code 2)`

### Checklist

- [ ] **Step 1:** Run auto-fixer
  ```powershell
  dotnet format C:\Users\nobu\RiderProjects\IntVue\IntVue.csproj
  ```

- [ ] **Step 2:** Review changes
  ```powershell
  git diff
  ```

- [ ] **Step 3:** Stage formatting fix
  ```powershell
  git add .
  ```

- [ ] **Step 4:** Commit formatting fix
  ```powershell
  git commit -m "style: Format code"
  ```

- [ ] **Step 5:** Retry your original commit
  ```powershell
  git commit -m "your actual feature message"
  ```

### Common Formatting Issues (Fixed by `dotnet format`)

| Issue | Example | Fix |
|---|---|---|
| Missing XML docs | Method without `/// <summary>` | Auto-adds comments |
| Wrong member order | Constructor after event | Reorders to correct position |
| Hard-coded colors | `Foreground="#000000"` | Use `{ThemeResource ...}` |
| `{Binding}` usage | `{Binding Property}` | Change to `x:Bind` |
| Bad spacing/indentation | Inconsistent whitespace | Auto-corrects |

### Time Estimate: 2-5 minutes

---

## Section B: Fix Build Errors

**Error:** `dotnet build failed (exit code 1) - Fix build errors before committing.`

### Checklist

- [ ] **Step 1:** Build locally to see full error
  ```powershell
  dotnet build -c Debug -p:Platform=x64
  ```

- [ ] **Step 2:** Read error message carefully
  ```
  Example: "CS0246: The type or namespace name 'X' could not be found"
  Scroll back in terminal to see full error
  ```

- [ ] **Step 3:** Fix the error
  ```
  Common fixes:
    - Add missing 'using' statement
    - Fix typo in class/method name
    - Add missing NuGet package reference
    - Fix type mismatch
  ```

- [ ] **Step 4:** Rebuild to verify fix
  ```powershell
  dotnet build -c Debug -p:Platform=x64
  ```

- [ ] **Step 5:** Commit the fix
  ```powershell
  git commit -m "fix: Resolve compilation error"
  ```

### Common Build Errors

| Error Code | Meaning | Solution |
|---|---|---|
| `CS0246` | Type/namespace not found | Add `using` statement |
| `CS1061` | Member doesn't exist | Check spelling, verify inheritance |
| `CS0103` | Name doesn't exist in context | Declare variable, import namespace |
| `MSIX0001` | Windows App SDK error | Check `.csproj` configuration |
| `WinRT001` | WinRT projection error | Check `EnableUnsafeMixedMicrosoftWindowsUIXamlProjections` |

### Time Estimate: 5-30 minutes

---

## Section C: Fix Test Failures

**Error:** `dotnet test failed (exit code 1) - Fix failing tests before committing. Set SKIP_TESTS_ON_FAILURE=1 to bypass.`

### Option 1: Fix the Code (Recommended)

- [ ] **Step 1:** Run tests to see which fail
  ```powershell
  dotnet test -c Debug -p:Platform=x64
  ```

- [ ] **Step 2:** Read failure message
  ```
  Look for:
    - Test method name
    - Expected value
    - Actual value
    - Stack trace (scroll back)
  ```

- [ ] **Step 3:** Determine what's wrong
  ```
  Option A: Test expectation is correct → Fix implementation
  Option B: Implementation is correct → Fix test assertion
  ```

- [ ] **Step 4:** Fix the issue
  ```powershell
  # Edit code or test file
  # Example: Change countdown loop to report 0
  ```

- [ ] **Step 5:** Re-run tests
  ```powershell
  dotnet test -c Debug -p:Platform=x64
  ```

- [ ] **Step 6:** Commit
  ```powershell
  git commit -m "fix: Resolve failing tests"
  ```

### Option 2: Bypass Test Failure (When Intentional)

Use **only** when tests are expected to fail temporarily:

```powershell
# One-liner
SKIP_TESTS_ON_FAILURE=1 git commit -m "feat: work in progress"

# Or set environment variable first
$env:SKIP_TESTS_ON_FAILURE = '1'
git commit -m "feat: work in progress (tests failing)"
```

⚠️ **Bypass should be rare** - only for work-in-progress features.

### Common Test Failures

| Error | Cause | Solution |
|---|---|---|
| `Assert.AreEqual failed` | Expected ≠ Actual | Fix code or test |
| `NullReferenceException` | Null object access | Set up mock properly |
| `Task timeout` | Operation takes too long | Increase timeout or optimize |
| `Progress callback not executed` | Async timing issue | Add `await Task.Delay(10)` |

### Time Estimate: 10-45 minutes

---

## Prevention Checklist

**Do this BEFORE every commit to avoid hook blocks:**

```powershell
# ✅ Step 1: Format
dotnet format C:\Users\nobu\RiderProjects\IntVue\IntVue.csproj

# ✅ Step 2: Build
dotnet build -c Debug -p:Platform=x64

# ✅ Step 3: Test
dotnet test -c Debug -p:Platform=x64

# ✅ Step 4: Review
git diff

# ✅ Step 5: Commit
git commit -m "feat: your feature"  # ✅ Safe now!
```

**IDE Auto-Format (Recommended):**
- **Rider:** Settings → Actions on Save → Reformat code
- **Visual Studio:** Tools → Options → Format on save

---

## Environment Variables

### Skip Test Execution Entirely
```powershell
$env:RUN_TESTS = '0'
git commit  # Tests won't run
```

### Bypass Test Failures (Warn but Allow)
```powershell
$env:SKIP_TESTS_ON_FAILURE = '1'
git commit  # Warns if tests fail, but allows commit
```

### Run Full Test Suite on Push
```powershell
$env:RUN_FULL_TESTS = '1'
git push  # Runs all tests before pushing
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

## When You Get Stuck

### Immediate Help
1. **Read full error output** - Scroll back in terminal
2. **Google the error code** - e.g., "CS0246 C#"
3. **Check if similar issue was fixed recently** - `git log -p -S "error keyword"`

### Detailed Reference
- See `hook-resolution.rules.md` for step-by-step guides
- See `CLAUDE.md` for project architecture

### Team Help
- Ask code review team with full error output
- Post error message + context in team chat

---

## Quick Links

| Need | File |
|---|---|
| This checklist | `.claude/rules/hook-quick-fix.rules.md` |
| Detailed guides | `.claude/rules/hook-resolution.rules.md` |
| Strategy docs | `.claude/rules/hook-strategy.rules.md` |
| Project rules | `CLAUDE.md` |
| Code quality rules | `.github/instructions/code-quality.instructions.md` |

---

## Essential Rules

- ✅ **Run format/build/test BEFORE committing** (prevents 90% of blocks)
- ✅ **Use this checklist first** when blocked
- ✅ **Follow 5 steps in your error section** (usually resolves in 30 min)
- ✅ **Read error messages carefully** (they tell you exactly what's wrong)
- ✅ **Ask team if stuck after 30 minutes** (escalate appropriately)

---

## Success Criteria

✅ Error blocks commit  
✅ You follow checklist in your section  
✅ Error is resolved  
✅ Commit succeeds  

**Expected outcome:** Most errors resolved in 2-30 minutes.
