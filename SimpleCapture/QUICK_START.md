# SimpleCapture - Quick Start (Self-Contained Executable)

## The Solution

You now have **SimpleCapture** — a minimal camera capture app that publishes as a **completely standalone executable** with all .NET runtime included.

## What You Get

After publishing, you get a folder containing:
- **SimpleCapture.exe** (292 KB)
- All .NET runtime DLLs
- All WinUI libraries
- All required system libraries

**Total: 218 MB** — but it requires **NO .NET installation** on the target machine.

## How to Create the Standalone Executable

### Option 1: Use the Script (Easiest)

```powershell
cd C:\Users\nobu\RiderProjects\IntVue\SimpleCapture
.\build-and-run.ps1
```

This will:
1. ✅ Publish the app with self-contained runtime
2. ✅ Display the full path to SimpleCapture.exe
3. ✅ Launch the app

### Option 2: Manual Command

```powershell
cd C:\Users\nobu\RiderProjects\IntVue\SimpleCapture
dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64
```

The executable appears at:
```
bin/x64/Debug/net10.0-windows10.0.26100.0/win-x64/publish/SimpleCapture.exe
```

## How to Deploy to Surface Pro 7

### Step 1: Publish on Your Dev Machine
```powershell
dotnet publish -c Debug -p:Platform=x64 --self-contained true --runtime win-x64
```

### Step 2: Copy the Entire `publish` Folder
```
From: C:\Users\nobu\RiderProjects\IntVue\SimpleCapture\bin\x64\Debug\net10.0-windows10.0.26100.0\win-x64\publish\
To:   [USB drive] or [network share]
```

### Step 3: Copy to Surface Pro 7
- Insert USB or map network share
- Copy the `publish` folder to Surface Pro 7 (e.g., `C:\Users\{user}\Desktop\SimpleCapture`)

### Step 4: Run on Surface Pro 7
Simply double-click `SimpleCapture.exe` — **no .NET installation needed!**

## Key Files

| File | Purpose |
|------|---------|
| `build-and-run.ps1` | Publishes and runs the app automatically |
| `README.md` | Build/run instructions |
| `SETUP_GUIDE.md` | Detailed setup and troubleshooting |
| `DEPLOYMENT.md` | Deployment to Surface Pro 7 |
| `bin/.../publish/SimpleCapture.exe` | The final standalone executable |

## What Makes It Self-Contained

The `--self-contained true --runtime win-x64` flags tell .NET to:
1. Include the entire .NET runtime (coreclr.dll, clrjit.dll, etc.)
2. Include all WinUI libraries
3. Include all system dependencies
4. Bundle everything in the `publish` folder

This creates a **complete, independent application** that doesn't need any external .NET installation.

## File Sizes

```
SimpleCapture.exe           292 KB
Total publish folder        218 MB (includes all runtime)
```

## Testing Checklist

**On your dev machine:**
- [ ] Run `.\build-and-run.ps1`
- [ ] App launches and displays UI
- [ ] Click "Capture Video" — camera dialog appears
- [ ] Record a short video
- [ ] Video plays back in the app

**On Surface Pro 7:**
- [ ] Copy `publish` folder to device
- [ ] Double-click `SimpleCapture.exe`
- [ ] App launches without .NET installation
- [ ] Test camera functionality same as above
- [ ] If works: camera hardware is OK
- [ ] If fails: camera driver/hardware issue

## Why This Matters for IntVue Debugging

✅ **If SimpleCapture works on Surface Pro 7:**
- Camera hardware is functional
- Windows API access is working
- **Issue is IntVue-specific** (MediaCapture, state management, preview/recording logic)

❌ **If SimpleCapture fails on Surface Pro 7:**
- Camera driver or hardware problem
- Windows system-level issue
- **Issue is platform-level** (not app-related)

## Next Steps

1. **Publish on your machine:** `.\build-and-run.ps1`
2. **Test locally:** Verify video capture works
3. **Copy to USB drive:** The entire `publish` folder
4. **Test on Surface Pro 7:** Copy and run `SimpleCapture.exe`
5. **Report findings:** Success/failure of camera capture

## Common Questions

**Q: Why is the publish folder so large (218 MB)?**  
A: It includes the entire .NET 10 runtime plus WinUI libraries. This is normal and necessary for standalone deployment.

**Q: Can I delete language folders to make it smaller?**  
A: Yes. See [DEPLOYMENT.md](DEPLOYMENT.md) for instructions on removing non-English language packs (saves ~40 MB).

**Q: Will it run on older Windows versions?**  
A: No. Requires Windows 10.0.26100.0 or later (same as IntVue).

**Q: Can I ship this to users?**  
A: Yes! The `publish` folder is the final distribution package. Copy it to users' machines and they can run it directly.

**Q: What's the difference between `build` and `publish`?**  
A: `build` creates DLLs (needs .NET installed). `publish` creates a standalone EXE (self-contained).

## Documentation

- [README.md](README.md) — Build instructions
- [SETUP_GUIDE.md](SETUP_GUIDE.md) — Detailed setup
- [DEPLOYMENT.md](DEPLOYMENT.md) — Deploy to Surface Pro 7

---

**You're ready to go!** Run `.\build-and-run.ps1` to create and test the executable.
