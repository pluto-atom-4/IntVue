# Custom Instructions for Claude

These directives guide Claude Code and Claude API when working on the IntVue project.

---

## Code Update Guidelines

- **YAGNI:** Implement only what's explicitly requested. No "just in case" features or premature abstractions.
- **DRY:** Before adding code, check if similar logic already exists. Link/refactor rather than duplicate.
- **SOLID Principles:** Especially Single Responsibility. Services handle domain logic; ViewModels handle state. Never mix.
- **Speculative Code:** Never add features "for future extensibility" without a concrete use case.

---

## Refactoring Guards

**Before changing architecture:**
1. Read `DESIGN.md` (technology boundaries, hard constraints)
2. Check if the change violates MVVM pattern (Views → ViewModels → Services → Models)
3. Verify no MediaCapture resource leaks on suspend/resume
4. Ensure test coverage stays ≥80% on affected ViewModels/Services

**Disallowed without explicit request:**
- Changing folder structure (Views/, Services/, etc.)
- Replacing MVVM with alternate pattern
- Modifying DI registration without updating App.xaml.cs documentation

---

## Testing Expectations

- All public ViewModels and Services require unit tests
- Target: 80%+ coverage on business logic
- Use MSTest + Moq (see `testing.instructions.md`)
- AAA pattern: Arrange → Act → Assert
- Async tests: Use `async Task` + `await Task.Delay(10)` for timing issues

---

## Error Handling

- **Validate at boundaries:** User input, file I/O, network, media capture
- **Trust internal code:** Don't validate between internal classes (preconditions exist)
- **Graceful degradation:** Null checks → safe defaults (not exceptions)
- **Log failures:** Include context (operation name, inputs, error code)

---

## Secrets & Security

- **Never hard-code:** API keys, passwords, connection strings, tokens
- **Use environment variables:** For local development (set in IDE)
- **Use PasswordVault:** For sensitive UI state (optional, not required for MVP)
- **Use Azure Key Vault:** For production secrets (future)
- **Never commit .env files**

---

## Code Responses

- **Show complete functions,** not truncated snippets
- **Include context:** Surrounding code if refactoring large sections
- **Explain why,** not just what (non-obvious decisions)
- **Link to guidance:** Point to relevant instruction files for detailed rules

---

## Build & Deploy Prerequisites

- Always detect platform: `$arch = $env:PROCESSOR_ARCHITECTURE; $Platform = if ($arch -eq 'AMD64') { 'x64' } else { $arch }`
- Never hardcode platform (x64, x86, ARM64)
- Check `Project.csproj` for true source of versions (not package.json or .nuspec)
- Run tests before committing: `dotnet test -c Debug -p:Platform=$Platform`

---

## When Stuck

**Follow this escalation:**
1. Check `DESIGN.md` (architecture, constraints)
2. Search codebase (DRY principle)
3. Read relevant `.github/instructions/` file
4. Web search for Windows API samples
5. Check WinUI 3 documentation or decompiler
6. Ask team for context/clarification

---

## Essential Rules

1. ✅ Read instruction files when they apply to your scope
2. ✅ Validate at system boundaries only
3. ✅ Write tests for public API (80%+ coverage)
4. ✅ Use `x:Bind` in XAML (never `{Binding}`)
5. ✅ Use `{ThemeResource ...}` for colors (never hard-code)
6. ✅ Dispose MediaCapture properly
7. ✅ Keep ViewModels free of business logic
8. ✅ Use constructor injection for dependencies

---

## Cross-References

- Code quality rules: `code-quality.instructions.md`
- WinUI 3 patterns: `winui-best-practices.instructions.md`
- Architecture & boundaries: `DESIGN.md`
- Design tokens & theming: `design-*.rules.md`
- Error resolution: `hook-*.rules.md`
