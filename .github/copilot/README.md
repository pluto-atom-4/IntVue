# .github/copilot/ — Path-Specific Rule Auto-Discovery

This directory holds GitHub Copilot Agent Mode's path-specific architecture rules for IntVue.
Rules are scoped to the app's real layers — `ViewModels/`, `Services/`, `Views/` (XAML), and
test files — not generic categories like "auth/infra/data-access" that don't exist in this
codebase. See [Why These Categories](#why-these-categories-not-authinfradata-access) below.

> **Confidence caveat:** it is not independently confirmed that GitHub Copilot Agent Mode
> ingests this custom YAML schema natively (see the `NOTE` at the top of each `.rules.yaml`
> file and of [`rules-detailed.md`](rules-detailed.md)). Treat these files as the authoritative,
> version-controlled source of truth for the rules regardless — Copilot (if it reads them
> natively), Claude Code, and any human reviewer should all apply them when editing a matching
> path.

## Auto-Discovery Mechanism

1. Copilot Agent Mode (or an agent following this repo's conventions) inspects the path of the
   file it is about to edit.
2. It matches that path against the `applyTo` glob in each `.github/copilot/rules/*.rules.yaml`
   file (see table below).
3. It fetches every `references:` entry in the matched rule file(s) — these point to
   `.github/instructions/*.md` and `.claude/rules/*.md` for the full canonical rule text.
4. It applies the `rules:` list in the matched YAML (pattern → fix → example) when generating
   or reviewing code for that path.

**Fallback for tools that don't auto-discover directory rules:** if a tool cannot glob-scan
`.github/copilot/rules/` on its own, read [`copilot-instructions.md`](../copilot-instructions.md)'s
"Pattern Rules" table directly — it lists every rule file next to the glob it covers, so a
single-file read gives the same map without directory scanning. The equivalent fallback for
*skills* (not rules) is [`SKILLS.md`](../../SKILLS.md) at the repo root.

## Rule Files & Glob Patterns

| Rule file | `applyTo` glob | Domain boundary |
|---|---|---|
| [`rules/xaml-binding.rules.yaml`](rules/xaml-binding.rules.yaml) | `**/*.xaml` | Views/XAML — `x:Bind`, theme tokens, accessibility, spacing/typography scale |
| [`rules/viewmodel-patterns.rules.yaml`](rules/viewmodel-patterns.rules.yaml) | `ViewModels/**/*.cs` | MVVM — `ObservableObject`, `[ObservableProperty]`, `[RelayCommand]`, DI, no UI refs |
| [`rules/service-patterns.rules.yaml`](rules/service-patterns.rules.yaml) | `Services/**/*.cs` | Business logic — interfaces, stateless, async I/O, no hardcoded secrets/PII |
| [`rules/test-patterns.rules.yaml`](rules/test-patterns.rules.yaml) | `**/*Tests.cs`, `**/*Test.cs` | MSTest, AAA pattern, naming convention, ≥80% coverage |

Detailed worked examples (correct vs. incorrect code) for all four files:
[`rules-detailed.md`](rules-detailed.md).

## Rule Precedence

When a file matches more than one rule — e.g.
`Tests/IntVue.Tests/ViewModels/MainViewModelTests.cs` matches both the ViewModel glob
(`ViewModels/**/*.cs` does *not* match it, but a test that exercises a ViewModel often sits
next to `**/*Tests.cs`, which does) — resolve in this order:

1. **Most-specific-glob wins for conflicting guidance.** `**/*Tests.cs` is more specific than
   `ViewModels/**/*.cs` for a *test file* — the test-patterns rules (MSTest, AAA, naming) govern
   the test file's own structure. The viewmodel-patterns rules still govern the *production*
   `ViewModels/**/*.cs` file the test exercises, not the test file itself.
2. **Non-conflicting rules stack.** If two matched rule files don't contradict each other (e.g.
   a XAML code-behind file reasonably read against both XAML rules and general code-quality
   rules), apply both.
3. **`.claude/rules/*.md` and `.github/instructions/*.md` are the canonical rule text.** The
   YAML `rules:` entries in this directory are a machine-checkable summary; when a YAML pattern
   and its linked canonical doc disagree, the canonical doc wins.
4. **`.claude/settings.json` `PreToolUse` hooks outrank all of the above for command
   execution** (destructive-shell blocking, issue-closure gate) — see
   [destructive-command-governance.rules.md](../../.claude/rules/destructive-command-governance.rules.md)
   and [github-governance.rules.md](../../.claude/rules/github-governance.rules.md). Path-specific
   rules never override those approval gates.

## Why These Categories (Not "auth/infra/data-access")

IntVue is a single-user WinUI 3 desktop app with MVVM + DI — it has no auth layer, no
server-side infrastructure, and no data-access layer in the traditional sense. Its real
architectural boundaries are `ViewModels/` (UI state), `Services/` (business logic, media
capture, file I/O), and `Views/` (XAML), plus a cross-cutting test layer. The rule files above
are named after those actual folders instead of generic categories that don't map to anything
in this codebase.

## Cross-References

- [`copilot-instructions.md`](../copilot-instructions.md) — top-level Copilot Agent Mode entry point (Scope, Checks, WRAP workflow)
- [`SKILLS.md`](../../SKILLS.md) — indexed skills catalog (fallback for tools that don't auto-discover `.claude/skills/`)
- [`AGENTS.md`](../../AGENTS.md) — Two-Gate System, full hook reference, cross-tool conflict resolution
- [`DESIGN.md`](../../DESIGN.md) — tech stack & architecture anchor
