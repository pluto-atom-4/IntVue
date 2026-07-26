# GitHub Copilot Agent Mode — IntVue

**Quick Links:** [CLAUDE.md](../CLAUDE.md) | [AGENTS.md](../AGENTS.md) | [Detailed Rules](./copilot/rules-detailed.md) | [Instructions](../instructions/)

---

## Boundaries

**✅ Auto:** Bug fixes, test additions, docs, formatting, config updates (no breaking changes)  
**⚠️ Confirm:** Architecture, APIs, dependencies, manifest, build scripts, storage schemas  
**🚫 Blocked:** Secrets, releases, package identity changes, cross-cutting refactors

---

## Path-Specific Rules

See [rules-detailed.md](./copilot/rules-detailed.md) for examples.

- **XAML** (`**/*.xaml`): x:Bind only, theme colors, 8px grid, accessibility properties, no logic
- **ViewModels** (`ViewModels/**/*.cs`): ObservableObject, [ObservableProperty], [RelayCommand], async/await
- **Services** (`Services/**/*.cs`): Stateless, validate inputs, async I/O, no secrets hardcoded
- **Tests** (`**/*Tests.cs`): MSTest, MethodName_Scenario_ExpectedResult naming, AAA pattern, 80%+ coverage

---

## Pre-Commit Checks

- [ ] Build: `dotnet build -c Debug -p:Platform=$Platform` → 0 errors
- [ ] Test: `dotnet test -c Debug -p:Platform=$Platform` → all pass, 80%+ coverage
- [ ] Format: `dotnet format --verify-no-changes`
- [ ] No secrets, API keys, or PII
- [ ] Commit message references issue (`Closes #123`)
- [ ] New public methods tested

---

## Acceptance Criteria

✅ Build/tests pass | ✅ Rules respected | ✅ No secrets | ✅ Issue referenced

---

**Last Updated:** 2026-07-25 | **Version:** 1.0
