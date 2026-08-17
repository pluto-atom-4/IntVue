# SKILLS.md — IntVue Skills Catalog

Indexed catalog of IntVue's task-specific skills. Claude Code auto-discovers these from
`.claude/skills/<name>/SKILL.md`; this file is the **explicit fallback index** for tools —
GitHub Copilot Agent Mode included — that don't scan that directory convention on their own.

---

## Auto-Discovery Mechanism (Claude Code)

1. Claude Code scans `.claude/skills/*/SKILL.md` for the project.
2. Each `SKILL.md` starts with YAML frontmatter: `name`, `description`, `trigger` (a
   `|`-separated set of phrases).
3. A skill is invoked **explicitly** (`/skill-name`) or **implicitly** when the user's request
   matches a `trigger` phrase, or via the `PostToolUse` hook pattern described in
   [AGENTS.md § Skill Discovery & Framework](./AGENTS.md#skill-discovery--framework).
4. `.claude/settings.local.json` can turn a skill off per-user via
   `"skillOverrides": { "<name>": "off" }` without deleting the file. That file is git-ignored
   and per-developer — check it for the live on/off state before assuming a skill below is
   active in your session.

## Fallback for Tools That Don't Auto-Discover (GitHub Copilot, etc.)

GitHub Copilot Agent Mode does not scan `.claude/skills/`. When Copilot — or any agent without
directory-skill auto-discovery — is working on a task that matches a row below, it should:

1. Open the linked `SKILL.md` file directly by path.
2. Read and apply its playbook/checklist as if it were inline instructions for the current task.
3. **Not** assume the skill "ran" automatically — there is no hook wiring this catalog into
   Copilot; this is a manual-lookup fallback, not a live integration.

## Skill Catalog

| Skill | Trigger phrases | Purpose | File |
|---|---|---|---|
| `accessibility-review` | "audit accessibility", "check a11y", "review keyboard nav", "verify contrast", "accessibility review" | Audits XAML for WCAG AA compliance, keyboard nav, automation properties, contrast, theme support | [`.claude/skills/accessibility-review/SKILL.md`](.claude/skills/accessibility-review/SKILL.md) |
| `feature-generation` | "scaffold page", "generate feature", "create page", "new winui page", "new feature" | Scaffolds a new Model/Service/ViewModel/View slice following MVVM + DI conventions | [`.claude/skills/feature-generation/SKILL.md`](.claude/skills/feature-generation/SKILL.md) |
| `security-audit` | "audit security", "check media capture", "review permissions", "verify pii handling", "security review" | Audits MediaCapture lifecycle, consent flow, permissions, storage, and PII/secrets handling | [`.claude/skills/security-audit/SKILL.md`](.claude/skills/security-audit/SKILL.md) |

> As of this writing, `accessibility-review` and `security-audit` are disabled in this
> repo's local `.claude/settings.local.json` (`skillOverrides`). That file is per-developer and
> git-ignored, so treat this as a snapshot, not a guarantee — enable the skill locally (remove
> or flip its override) before relying on it running implicitly.

## Skill File Pattern (Frontmatter Schema)

```yaml
---
name: skill-name
description: One-line description of what the skill audits/generates
trigger: "phrase one|phrase two|phrase three"
---
```

Body: a Markdown playbook — numbered sections, ❌ AVOID / ✓ GOOD code pairs, and a submission
checklist. See any file in the table above for the full pattern to copy when adding a new skill.

## Adding a New Skill

1. Create `.claude/skills/<skill-name>/SKILL.md` with the frontmatter schema above.
2. Add a row to the **Skill Catalog** table in this file (Copilot and any non-auto-discovering
   tool only sees what's indexed here).
3. If the skill should run automatically on file-pattern matches, wire a `PostToolUse` hook in
   `.claude/settings.json` — see [AGENTS.md § Hook Configuration Reference](./AGENTS.md#hook-configuration-reference).

## Cross-References

- [`AGENTS.md`](./AGENTS.md) § Skill Discovery & Framework — Claude Code's own skill-invocation rules
- [`.github/copilot-instructions.md`](./.github/copilot-instructions.md) — points Copilot here as its skills fallback
- [`.github/copilot/README.md`](./.github/copilot/README.md) — the equivalent auto-discovery/fallback doc for path-specific *rules* (as opposed to skills)
- [`CLAUDE.md`](./CLAUDE.md) — project-wide entry point
