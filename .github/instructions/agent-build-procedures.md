# Agent Build, Run & Deploy Procedures

This guide covers building, running, and deploying the IntVue WinUI 3 app.

## Overview

This is an MSIX-packaged WinUI 3 app. You **must** pass both `-c` (Configuration) and `-p:Platform=` to every `dotnet build`/`dotnet test` command.

The project references the [`Microsoft.Windows.SDK.BuildTools.WinApp`](https://www.nuget.org/packages/Microsoft.Windows.SDK.BuildTools.WinApp) NuGet package, which hooks `dotnet run` to invoke the [`winapp` CLI](https://github.com/microsoft/WinAppCli). Use `dotnet run` for everyday inner-loop development.

---

## Detect Platform

**Always detect the machine's architecture first** — never hardcode a platform value. Run this once at the start of every build/test session:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
```

Use `$Platform` in all subsequent `dotnet` commands.

---

## Build

```powershell
# Debug build
dotnet build -c Debug -p:Platform=$Platform

# Release build
dotnet build -c Release -p:Platform=$Platform
```

---

## Run with Package Identity

The template references `Microsoft.Windows.SDK.BuildTools.WinApp`, which makes `dotnet run` register a loose-layout package via `winapp run` and launch the app via AUMID activation:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

dotnet run -c Debug -p:Platform=$Platform
```

The CLI prints the registered package's AUMID and the launched process's PID — attach a debugger to that PID for runtime debugging.

### Useful MSBuild Properties

| Property | When to set |
|---|---|
| `EnableWinAppRunSupport=false` | Disable `dotnet run` integration (e.g., to launch unpackaged) |
| `WinAppRunUseExecutionAlias=true` | For console apps — launches via `uap5:ExecutionAlias` (add alias first) |
| `WinAppRunNoLaunch=true` | Register package but don't launch (attach debugger manually) |
| `WinAppLaunchArgs="--flag value"` | Pass arguments to app on launch |

### Manual winapp run (Advanced)

```powershell
# Run from build output
winapp run .\bin\$Platform\Debug\<TargetFramework>

# Pass arguments
winapp run .\bin\$Platform\Debug\<TargetFramework> -- --my-flag value

# Clean LocalState between runs
winapp run .\bin\$Platform\Debug\<TargetFramework> --clean
```

---

## Run Tests

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }

# All tests
dotnet test -c Debug -p:Platform=$Platform

# Filtered tests
dotnet test -c Debug -p:Platform=$Platform --filter "FullyQualifiedName~MainViewModelTests"
```

---

## winapp CLI Reference

| Scenario | Command | Notes |
|---|---|---|
| **Run/debug with identity (loose layout)** | `dotnet run` (or `winapp run <build-output>`) | Default for inner loop |
| **Stop debugging / clean up** | `winapp unregister` | Removes dev packages |
| **Generate dev signing cert** | `winapp cert generate --manifest .\Package.appxmanifest --install` | Reads publisher from manifest |
| **Inspect a cert** | `winapp cert info .\devcert.pfx` | Verify subject matches manifest |
| **Build distribution MSIX** | `winapp pack .\bin\$Platform\Release\<TFM>\win-<rid> --cert .\devcert.pfx` | Auto-resolves tokens |
| **Self-contained MSIX** | `winapp pack ... --self-contained` | Bundles WinAppSDK runtime |

For full reference, see the [winapp CLI docs](https://github.com/microsoft/WinAppCli/blob/main/docs/usage.md).

---

## Prerequisites

- **Developer Mode enabled** on Windows:
  ```powershell
  Get-WindowsDeveloperLicense
  # If not enabled: Settings → System → For developers → Developer Mode → On
  ```
- **`winapp` CLI** installed (via `Microsoft.Windows.SDK.BuildTools.WinApp` NuGet reference, no separate install needed for `dotnet run`)

---

## Troubleshooting Build Errors

When a build fails due to unknown type, missing namespace, or unresolved API:

1. **Web Search First (ALWAYS)**
   - Read [windows-apis.instructions.md](./windows-apis.instructions.md)
   - Search the [WinAppSDK API Reference](https://learn.microsoft.com/en-us/windows/windows-app-sdk/api/winrt/)
   - Check [release notes](https://learn.microsoft.com/en-us/windows/apps/windows-app-sdk/stable-channel) for SDK availability

2. **Sample Repos**
   - Search sample repositories listed in windows-apis.instructions.md

3. **WinMD / Decompiler (Last Resort)**
   - Only if Steps 1-2 fail: inspect `.winmd` metadata or use decompilers

---

## Fallback: Manual Package Registration

Only use this if you've disabled the `winapp` integration or are debugging deployment:

```powershell
$arch = $env:PROCESSOR_ARCHITECTURE
$Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }
$Rid = $Platform.ToLower()

# Register from build output
Add-AppxPackage -Register ".\IntVue\bin\$Platform\Debug\<TargetFramework>\win-$Rid\AppxManifest.xml"
```

If launch fails due to old instance running:
```powershell
taskkill /IM IntVue.exe /F
```
