# Destructive Command Governance Rule: PreToolUse Approval Gate

**Purpose:** Prevent any agent (Claude Code, skill, or workflow) from running destructive shell commands — commands that can irreversibly delete files, rewrite history, or wipe volumes — without explicit human approval in the current turn.

---

## Rule: No Automatic Destructive Command Execution

### Policy

**Destructive commands are blocked by default.** Agents (Claude Code, subagents, skills, workflows) must NOT run a command matched by the `block-destructive-shell` PreToolUse hook (see Configuration below) without a human explicitly approving that specific command in the current turn, or a pre-existing allow-list entry documented in `.claude/settings.local.json` (see Exceptions below).

| Operation | Agent Can Do? | Requires Approval? | Who Approves? |
|---|---|---|---|
| **Read-only shell commands** (`ls`, `git status`, `git diff`, etc.) | ✅ Yes | ❌ No | - |
| **Non-destructive writes** (`git add`, `git commit`, edit files) | ✅ Yes | ❌ No | - |
| **`rm -rf` / `del /s` / `rd /s`** | ❌ No | ✅ YES (explicit approval) | Human (this turn) |
| **`Remove-Item ... -Recurse` / `-Force`** | ❌ No | ✅ YES (explicit approval) | Human (this turn) |
| **`git reset --hard`** | ❌ No | ✅ YES (explicit approval) | Human (this turn) |
| **`git push --force` / `git push -f`** | ❌ No | ✅ YES (explicit approval) | Human (this turn) |
| **`Format-Volume`** | ❌ No | ✅ YES (explicit approval) | Human (this turn) |
| **Recurring legitimate destructive command** | ❌ No (agent cannot self-grant) | ✅ YES (documented allow-list entry) | Human (adds entry to `.claude/settings.local.json`) |

The hook matches these patterns **anywhere in the command string**, not just at the start — so chained commands such as `cd build && rm -rf bin` are also caught.

---

## When a Destructive Command Can Proceed

Only in these exact scenarios:

### Scenario 1: User Explicitly Approves the Prompted Command
```
Agent attempts: rm -rf ./bin
Hook: permissionDecision "ask" — prompts the user
User: "Yes, approve"
Agent: ✅ Command proceeds (explicit approval this turn)
```

### Scenario 2: User Pre-Approves in the Same Turn
```
User: "Clean the bin/obj folders with rm -rf, that's fine."
Agent: ✅ Can run rm -rf after the hook prompt is approved (approval already given in context)
```

### Scenario 3: Documented Allow-List Entry (Recurring Legitimate Use)
```
Human adds to .claude/settings.local.json:
  "permissions": { "allow": ["Bash(rm -rf ./bin/*)"] }
  with a comment/commit message documenting the reason.
Agent: ✅ Can run the exact allow-listed command form without a per-turn prompt.
```

---

## When a Destructive Command MUST NOT Proceed

### ❌ NOT Allowed: Silent Execution
```
Agent: "Cleaning up old build artifacts..." [runs rm -rf without prompting]
❌ BLOCKED - No human approval; hook must fire and ask
```

### ❌ NOT Allowed: Self-Granted Approval
```
Agent: "This rm -rf is clearly safe, proceeding without asking."
❌ BLOCKED - Agents cannot waive the gate for themselves
```

### ❌ NOT Allowed: Inferred Approval From an Unrelated Request
```
User: "Fix the failing build."
Agent: "I'll run git reset --hard to fix it."
❌ BLOCKED - Fixing a build is not approval for a destructive git operation
```

### ✅ CORRECT: Hook Fires, Agent Waits
```
Agent attempts a destructive command → hook returns permissionDecision "ask" with
a permissionDecisionReason pointing here → agent surfaces the prompt and waits for
the human's explicit yes/no before proceeding.
```

---

## Implementation Rules

### For Claude Code (This Agent)

**Before a destructive command can run, I MUST:**

1. ✅ Let the `block-destructive-shell` PreToolUse hook evaluate the command
2. ✅ If it returns `permissionDecision: "ask"`, surface the reason to the user and wait
3. ✅ Only proceed once the human gives explicit approval for that specific command in this turn
4. ✅ For recurring needs, tell the user how to add a documented allow-list entry in `.claude/settings.local.json` instead of repeatedly prompting

**I will NOT:**
- Run destructive commands without triggering/honoring the hook
- Assume approval from unrelated context (e.g., "fix the build" ≠ "run git reset --hard")
- Add allow-list entries to `.claude/settings.local.json` on my own initiative to bypass the gate
- Reword or split a destructive command specifically to dodge the regex match

### For Skills and Subagents

**Skills and subagents MUST:**
- Inherit this rule from parent context
- Never bypass the `ask` gate via alternate invocation paths (e.g., calling a shell function that wraps `rm -rf`)
- Escalate to the parent/user if a destructive operation seems necessary rather than working around the gate

---

## Configuration

### Settings.json Hook

This rule is enforced via a PreToolUse hook on `Bash` tool calls in `.claude/settings.json`:

```json
{
  "type": "command",
  "id": "block-destructive-shell",
  "if": "Bash((rm -rf|del /s|git reset --hard|Format-Volume|git push --force|git push -f|rd /s|Remove-Item[^\\r\\n]*-Recurse|Remove-Item[^\\r\\n]*-Force))",
  "command": "echo '{\"hookSpecificOutput\": {\"hookEventName\": \"PreToolUse\", \"permissionDecision\": \"ask\", \"permissionDecisionReason\": \"GOVERNANCE: Destructive command (rm -rf / del /s / rd /s / Remove-Item -Recurse|-Force / git reset --hard / git push --force|-f / Format-Volume) blocked by default for safety. Requires explicit human approval in this turn, or a documented allow-list entry in .claude/settings.local.json for recurring legitimate use. See .claude/rules/destructive-command-governance.rules.md\"}}'",
  "statusMessage": "Checking for destructive commands..."
}
```

**Key differences from the previous implementation:**
- The regex is no longer anchored to the start of the command (`^` removed), so it also catches destructive commands chained after other commands (e.g., `cd x && rm -rf y`).
- PowerShell-native destructive commands are now covered: `Remove-Item ... -Recurse`, `Remove-Item ... -Force`, `rd /s` — in addition to the existing `rm -rf`, `del /s`, `git reset --hard`, `Format-Volume`, `git push --force`, `git push -f`.
- The response uses the `hookSpecificOutput.permissionDecision: "ask"` schema (same shape as `github-issue-closure-approval-gate` in the same file) instead of a hard `"continue": false` stop, so a human can review and approve the specific command instead of the agent being forced to abandon the task entirely.

This file must keep the literal substrings `rm -rf` and `git reset --hard` present in `.claude/settings.json`, because `.github/workflows/config-validation.yml` greps for these exact strings and fails CI if either is missing.

---

## Exceptions

### When This Gate Can Be Overridden

**Recurring legitimate destructive command (documented allow-list entry):**

A human — not an agent — adds an explicit entry to `.claude/settings.local.json` (which is git-ignored / local-only, not the shared `.claude/settings.json`) permitting the exact command form, along with a documented reason (in a code comment, commit message, or an accompanying note):

```json
{
  "permissions": {
    "allow": [
      "Bash(rm -rf ./bin/* ./obj/*)"
    ]
  }
}
```

**When you might add an override:**
- A specific, narrowly-scoped cleanup command (e.g., clearing build output directories) that the human runs routinely and has reviewed
- A trusted, repeatable local workflow step

**When you should NOT add an override:**
- Broad/unscoped destructive patterns (e.g., allow-listing bare `rm -rf` with no path constraint)
- Anything touching source control history (`git reset --hard`, force pushes) on shared branches
- Anything that could affect production data or shared infrastructure

**Approval is never automatic and never self-granted by an agent.** An agent may explain how to add an allow-list entry, but must not add one itself to bypass a block it just hit.

---

## Audit Trail

All approved destructive command executions should be noted for traceability:

| Date | Command | Approved By | Reason | Approval Method |
|---|---|---|---|---|
| _(none yet)_ | | | | |

---

## Troubleshooting

### Issue: Agent's Command Was Blocked

**What happened:**
```
Hook: block-destructive-shell
Decision: ask
Reason: GOVERNANCE: Destructive command blocked by default for safety...
```

**What to do:**
1. Review what the agent was attempting and why
2. If the command is appropriate, approve it explicitly for this turn
3. If the command will recur, consider adding a scoped allow-list entry to `.claude/settings.local.json` (see Exceptions above)
4. If the command is not appropriate, ask the agent for a non-destructive alternative

---

## Essential Rules

1. ✅ **Destructive commands are blocked by default** — the hook asks, it does not silently allow or silently kill the turn
2. ✅ **Approval must be explicit and current-turn** — prior unrelated approval does not count
3. ✅ **Agents never self-approve** — only a human can approve the prompt or add an allow-list entry
4. ✅ **Recurring needs go through `.claude/settings.local.json`** — narrowly scoped, documented, human-added
5. ✅ **Keep literal `rm -rf` / `git reset --hard` strings in `.claude/settings.json`** — required by `config-validation.yml` CI

---

## Cross-References

- **CLAUDE.md:** "Blocked?" pointer for command-hook blocks
- **AGENTS.md:** PreToolUse hook sub-table (Hook Configuration Reference)
- **hook-comprehensive.rules.md:** Git commit/push hook guidance (pre-commit.ps1/pre-push.ps1) — a separate hook system from this one
- **settings.json:** `block-destructive-shell`, `warn-missing-platform`, `warn-missing-config-flag` hook definitions
- **github-governance.rules.md:** Sibling approval-gate pattern for GitHub issue closure (same `ask` schema)

---

## Related

- GitHub issue governance (same approval-gate pattern): `.claude/rules/github-governance.rules.md`
- CI validation of these literal strings: `.github/workflows/config-validation.yml`
