# Hook Rule: Error Resolution Guide

**Status:** Use this for detailed explanations and prevention strategies

This is the comprehensive guide for understanding and resolving hook errors.

---

## Hook System Overview

The IntVue project uses Git hooks to enforce code quality **before** commits and pushes:

| Hook | Runs Before | What It Checks | Blocks? |
|---|---|---|---|
| `pre-commit` | `git commit` | Format, Build, Tests | ✅ Yes |
| `pre-push` | `git push` | All pre-commit checks + optional full tests | ✅ Yes |

**Location:** `scripts/pre-commit.ps1`, `scripts/pre-push.ps1`

---

## Error Type 1: Formatting Violations

### What's Happening

**Symptom:** `dotnet format detected formatting issues (exit code 2)`

The `dotnet format` tool found code that doesn't match project style rules:
- Missing XML documentation (`/// <summary>`)
- Wrong member ordering (StyleCop SA1201)
- Hard-coded colors in XAML instead of theme resources
- Using `{Binding}` instead of `x:Bind`
- Inconsistent indentation or spacing

### Why It Matters

Consistent formatting ensures:
- Code is readable and maintainable
- All developers follow same style
- Easier code reviews
- Reduced diff noise in Git

### How to Fix

**Method 1: Auto-Fix (Recommended)**

```powershell
# Step 1: Run formatter
dotnet format C:\Users\nobu\RiderProjects\IntVue\IntVue.csproj

# Step 2: Review what changed
git diff

# Step 3: Commit formatting changes
git add .
git commit -m "style: Auto-format code"

# Step 4: Retry your original commit
git commit -m "feat: your feature"
```

**Method 2: Manual Fix**

If auto-fix doesn't work, fix manually:
```
Common issues and fixes:

Missing XML docs:
  ❌ public void MyMethod() { }
  ✅ /// <summary>Does something.</summary>
     public void MyMethod() { }

Wrong member order:
  ❌ Property → Event → Constructor
  ✅ Field → Constructor → Event → Property → Methods

Hard-coded colors:
  ❌ Foreground="#000000"
  ✅ Foreground="{ThemeResource TextFillColorPrimaryBrush}"

Using {Binding}:
  ❌ Text="{Binding Title}"
  ✅ Text="{x:Bind Title, Mode=OneWay}"

Bad spacing:
  ❌ if(x==1){return;}
  ✅ if (x == 1) { return; }
```

### Prevention

**IDE Auto-Format on Save:**

**JetBrains Rider:**
1. Settings → Editor → Code Style → C#
2. Scroll to bottom → Enable "Reformat code"
3. Settings → Tools → Actions on Save
4. Check "Reformat code"

**Visual Studio:**
1. Tools → Options → Text Editor → C#
2. Code Style → Formatting → Enable "Format on save"

**Or manually before commit:**
```powershell
dotnet format C:\Users\nobu\RiderProjects\IntVue\IntVue.csproj
git add .
git commit -m "style: Format code"
git commit -m "feat: your feature"  # Now this works
```

### Common Formatting Rules

| Rule | Enforced | Example |
|---|---|---|
| XML documentation | Yes | `/// <summary>`, `/// <param>` |
| Member ordering (SA1201) | Yes | Fields → Constructor → Properties → Methods |
| No hard-coded colors | Yes | Use `{ThemeResource ...}` in XAML |
| Use `x:Bind` not `{Binding}` | Yes | `x:Bind Title, Mode=OneWay` |
| Consistent indentation | Yes | 4 spaces per level |
| Naming conventions | Yes | PascalCase for public, camelCase for private |

---

## Error Type 2: Build Errors

### What's Happening

**Symptom:** `dotnet build failed (exit code 1)`

The C# compiler found errors that prevent the project from building:
- Type or namespace not found (CS0246)
- Property/method doesn't exist (CS1061)
- Type mismatch (CS0029)
- Syntax error (CS1002)
- Missing reference or using statement

### Why It Matters

Build errors mean:
- Code won't run at all
- Tests can't even start
- Deployment is impossible
- Further commits are blocked

### How to Fix

**Step 1: Get Full Error Details**

```powershell
dotnet build -c Debug -p:Platform=x64
```

Read the error output carefully. Look for:
- **Error code** (CS####)
- **File and line number** (MainViewModel.cs:42)
- **Message** (what went wrong)
- **Suggestion** (how to fix it)

**Step 2: Understand Common Errors**

| Code | Meaning | Solution |
|---|---|---|
| CS0246 | Type not found | Add `using` statement, check spelling |
| CS1061 | Member not found | Verify property/method exists, check typos |
| CS0103 | Name not in scope | Declare variable, add `using` statement |
| CS0029 | Can't convert type | Cast explicitly or change type |
| CS1002 | Unexpected character | Check for syntax errors (missing semicolon, bracket) |

**Step 3: Fix the Error**

Example:
```csharp
// ❌ Error: CS0246 - Type 'ICountdownService' not found
public MainViewModel(ICountdownService service)
{
    // ...
}

// ✅ Fix: Add using statement
using IntVue.Services;  // <- Add this
```

**Step 4: Rebuild to Verify**

```powershell
dotnet build -c Debug -p:Platform=x64
```

Should see: `Build succeeded. 0 Warning(s), 0 Error(s)`

**Step 5: Commit**

```powershell
git commit -m "fix: Resolve compilation error in MainViewModel"
```

### Prevention

**Build frequently during development:**
```powershell
# After making changes
dotnet build -c Debug -p:Platform=x64

# Enable IDE error detection
# Rider: Analyze → Run Inspection by Name
# Visual Studio: Build → Build Solution
```

---

## Error Type 3: Test Failures

### What's Happening

**Symptom:** `dotnet test failed (exit code 1)`

One or more unit tests failed:
- Assertion mismatch (`Expected: 0, Actual: 1`)
- Null reference exception
- Timeout (test took too long)
- Unhandled exception in test code

### Why It Matters

Test failures mean:
- Implementation has a bug
- Test expectations not met
- Or test is wrong/outdated
- Code doesn't do what you think it does

### How to Fix

**Option A: Fix the Implementation (Recommended)**

```powershell
# Step 1: See which tests fail
dotnet test -c Debug -p:Platform=x64

# Example output:
# Failed StartCountdownAsync_UpdatesCountdownSeconds
# Error: Assert.AreEqual failed. Expected: 0, Actual: 1
```

```csharp
// The test expects countdown to end at 0
// But implementation reports 1
// So the implementation is wrong, fix it:

// ❌ Wrong:
for (int i = seconds; i >= 1; i--)
{
    progress.Report(i);  // Reports 3, 2, 1 (never 0)
}

// ✅ Fix:
for (int i = seconds; i >= 0; i--)  // Change >= 1 to >= 0
{
    progress.Report(i);  // Reports 3, 2, 1, 0
}
```

```powershell
# Step 2: Re-run tests
dotnet test -c Debug -p:Platform=x64

# Step 3: Commit
git commit -m "fix: Update countdown to report 0 at completion"
```

**Option B: Fix the Test (If Implementation is Correct)**

```csharp
// If implementation is correct but test has wrong expectation:

// ❌ Wrong test:
Assert.AreEqual(0, countdownValue);  // Test expects 0

// ✅ If implementation only reports 1:
Assert.AreEqual(1, countdownValue);  // Test should expect 1
```

```powershell
# Re-run and commit
dotnet test -c Debug -p:Platform=x64
git commit -m "fix: Correct test expectation for countdown"
```

**Option C: Bypass (Only for Work-in-Progress)**

If test failure is intentional (WIP feature):

```powershell
# Bypass test check (warns but allows commit)
$env:SKIP_TESTS_ON_FAILURE = '1'
git commit -m "feat: Work in progress - tests pending"

# Or one-liner
SKIP_TESTS_ON_FAILURE=1 git commit -m "feat: WIP"
```

⚠️ **Use bypass sparingly** - only during active development of incomplete features.

### Common Test Issues

| Issue | Cause | Fix |
|---|---|---|
| `Assert.AreEqual failed` | Expected ≠ Actual | Fix implementation or assertion |
| `NullReferenceException` | Null object | Set up mock dependencies correctly |
| `Task timeout` | Operation slow | Increase timeout or optimize code |
| `Progress callback not executed` | Async timing | Add `await Task.Delay(10)` in test |

### Prevention

**Run tests frequently:**
```powershell
# After making changes
dotnet test -c Debug -p:Platform=x64

# Before every commit
dotnet test -c Debug -p:Platform=x64  # Should be all passing
git commit
```

---

## Build Warnings (Do NOT Block)

### What's Happening

Build succeeds but shows warnings:
```
CSC : warning CA1016: Mark assemblies with assembly version
```

**Important:** Warnings do NOT block commits. You can commit with warnings.

### Examples of Allowed Warnings

| Code | Message | Action |
|---|---|---|
| CA1016 | Assembly version | Suppress in .csproj (already done) |
| CA1416 | Platform compatibility | Suppress (Windows-only app) |
| CA1001 | Disposable field | Fixed via `IDisposable` pattern |
| CS0618 | Obsolete member | Update to newer API |

### When to Worry

- Warnings are logged but don't prevent commits
- Should still fix warnings during code review phase
- Some warnings can be suppressed if not applicable

---

## Prevention Strategy

### Pre-Commit Workflow

**Do this before EVERY commit:**

```powershell
# 1. Format code
dotnet format C:\Users\nobu\RiderProjects\IntVue\IntVue.csproj

# 2. Build
dotnet build -c Debug -p:Platform=x64

# 3. Test
dotnet test -c Debug -p:Platform=x64

# 4. Review changes
git diff

# 5. Commit (now safe!)
git commit -m "feat: your feature"
```

### IDE Configuration (Automatic)

Enable auto-format on save:
- **Rider:** Settings → Actions on Save → Reformat code
- **Visual Studio:** Tools → Options → Format on save

Then you only need to run build/test before commit.

### Early Error Detection

Check for errors during development:
```powershell
# Check formatting
dotnet format --verify-no-changes IntVue.csproj

# Check build
dotnet build -c Debug -p:Platform=x64

# Check tests
dotnet test -c Debug -p:Platform=x64
```

---

## Troubleshooting

### Hook Not Running

```powershell
# Test hook manually
powershell -File scripts\pre-commit.ps1

# Or
powershell -File scripts\pre-push.ps1
```

### Tests Skip but Should Run

```powershell
# RUN_TESTS=0 disables tests
$env:RUN_TESTS  # Check value

# Clear it
$env:RUN_TESTS = ''
git commit  # Tests will run now
```

### dotnet-format Not Found

```powershell
# Install globally
dotnet tool install -g dotnet-format
```

### Build Succeeds Locally But Fails in Hook

```powershell
# Platform mismatch
[System.Environment]::ProcessorArchitecture

# Clean build from scratch
dotnet clean
dotnet build -c Debug -p:Platform=x64
```

### Different Results Locally vs Hook

```powershell
# Ensure same platform
$Platform = if ([System.Environment]::ProcessorArchitecture -eq 'AMD64') { 'x64' } else { 'x86' }

# Build with explicit platform
dotnet build -c Debug -p:Platform=$Platform
```

---

## Summary Table

| Error | Blocks? | Time | Bypass? | Prevention |
|---|---|---|---|---|
| Formatting | ✅ Yes | 2-5 min | ❌ No | `dotnet format` |
| Build | ✅ Yes | 5-30 min | ❌ No | `dotnet build` |
| Tests | ✅ Yes | 10-45 min | ✅ Yes* | `dotnet test` |
| Warnings | ❌ No | - | - | Fix in review |

*Bypass only for WIP features

---

## Essential Rules

1. **Format before commit** → Use `dotnet format` or IDE auto-format
2. **Build before commit** → Run `dotnet build -c Debug -p:Platform=x64`
3. **Test before commit** → Run `dotnet test -c Debug -p:Platform=x64`
4. **Read error messages** → They tell you exactly what's wrong
5. **Ask for help** → If stuck after 30 minutes, escalate to team

---

## Cross-References

- **Quick checklist:** `.claude/rules/hook-quick-fix.rules.md`
- **Strategy & escalation:** `.claude/rules/hook-strategy.rules.md`
- **Code quality rules:** `.github/instructions/code-quality.instructions.md`
- **Testing standards:** `.github/instructions/testing.instructions.md`
- **Main guidance:** `CLAUDE.md`
