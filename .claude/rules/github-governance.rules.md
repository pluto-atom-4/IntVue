# GitHub Governance Rule: Issue Closure Approval Gate

**Purpose:** Prevent any agent (Claude Code, skill, or workflow) from closing GitHub issues without explicit user authorization.

---

## Rule: No Automatic Issue Closure

### Policy

**Issue closure is a manual-only operation.** Agents (Claude Code, subagents, skills, workflows) must NOT call GitHub APIs to close, mark as resolved, or transition issues to closed states without explicit user approval in the current turn.

| Operation | Agent Can Do? | Requires Approval? | Who Approves? |
|---|---|---|---|
| **Analyze issue** | ✅ Yes | ❌ No | - |
| **Create PR linked to issue** | ✅ Yes | ❌ No | - |
| **Comment on issue** | ✅ Yes | ❌ No (unless sensitive) | - |
| **Close issue** | ❌ No | ✅ YES (explicit request) | User |
| **Mark issue as duplicate** | ❌ No | ✅ YES (explicit request) | User |
| **Change issue state** | ❌ No | ✅ YES (explicit request) | User |

---

## When an Agent Can Close an Issue

Only in these exact scenarios:

### Scenario 1: User Explicitly Requests Closure
```
User: "Close issue #42"
Agent: ✅ Can close (explicit request)
```

### Scenario 2: User Approves in Same Turn
```
User: "Fix issue #42. When done, you can close it."
Agent: ✅ Can close after completing the fix (approval given in same turn)
```

### Scenario 3: User Confirms Closure After Preview
```
Agent: "Issue #42 is fixed. Ready to close?"
User: "Yes, close it."
Agent: ✅ Can close (user confirmed)
```

---

## When an Agent MUST NOT Close an Issue

In all other cases, agents must report completion and ask for approval:

### ❌ NOT Allowed: Automatic Closure
```
Agent: "Implemented fix for issue #42. Closing now..."
❌ BLOCKED - No user approval
```

### ❌ NOT Allowed: Closure After Fix Alone
```
Agent: "Issue #42 is fixed [creates PR]. Closing issue..."
❌ BLOCKED - Fix != approval to close
```

### ❌ NOT Allowed: Closure Without User Confirmation
```
Agent: "Tests pass. Closing issue #42."
❌ BLOCKED - No explicit user request
```

### ✅ CORRECT: Report and Ask
```
Agent: "Issue #42 is resolved. The fix is in PR #X. Ready to close the issue? (Requires your approval)"
User: "Yes, close it."
Agent: ✅ Closes (user approved)
```

---

## Implementation Rules

### For Claude Code (This Agent)

**Before closing an issue, I MUST:**

1. ✅ Complete the fix/implementation
2. ✅ Verify tests pass
3. ✅ Create/link a PR
4. ✅ **Ask for explicit approval** — "Shall I close issue #42?"
5. ✅ Wait for user confirmation
6. ✅ Only then call GitHub API to close

**I will NOT:**
- Close issues autonomously
- Assume approval from context ("they asked for a fix")
- Use environment variables or settings to bypass this rule
- Delegate closure to subagents without explicit user approval

### For Skills (fix-github-issues, etc.)

**Skills must NOT:**
- Auto-close issues via GitHub API
- Include closure in their automation without explicit parameter
- Bypass this rule via environment variables

**Skills SHOULD:**
- Report when issue is resolved
- Provide summary of what was fixed
- Pause and ask for approval before closure
- Document in skill output when closure requires manual step

### For Subagents

**Subagents MUST:**
- Inherit this rule from parent context
- Never close issues without explicit user request in their input
- Return to parent if closure is needed (escalate)
- Document all attempted closures

---

## Configuration

### Settings.json Hook

This rule is enforced via a PreToolUse hook on GitHub API calls:

```json
{
  "hooks": {
    "PreToolUse": [
      {
        "matcher": ".*GitHub.*",
        "hooks": [
          {
            "type": "approval",
            "id": "github-issue-closure-gate",
            "operation": "close|closeIssue|updateIssueState",
            "if": "IssueOperation(close|resolve|duplicate|wontfix)",
            "requireApproval": true,
            "approvalMessage": "⚠️ Issue closure requires explicit user approval. Proceed? (Y/N)"
          }
        ]
      }
    ]
  }
}
```

---

## Exceptions

### When This Rule Can Be Waived

In rare cases, the user can opt-in to automatic closure:

**One-time waiver (current session):**
```
User: "Use fix-github-issues with auto-close enabled"
Agent: ✅ Can auto-close for this session only
```

**Permanent waiver (update settings.json):**
```json
{
  "governance": {
    "github": {
      "issueClosureRequiresApproval": false,
      "reason": "User opted out of approval gate"
    }
  }
}
```

**When you might waive this:**
- Trusted automation workflow
- Bulk issue processing (many low-risk issues)
- Scheduled jobs (nightly cleanup)

**When you should NOT waive this:**
- Critical issues
- Issues with stakeholders
- Issues that affect production

---

## Audit Trail

All issue closures are logged for compliance:

| Date | Issue | Closed By | Reason | Approval |
|---|---|---|---|---|
| 2026-08-05 | #42 | Claude Code | User request | "Yes, close it" |
| 2026-08-05 | #43 | User (manual) | Duplicate | N/A |
| 2026-08-06 | #44 | fix-github-issues skill | Auto-close (waived) | Settings.json override |

---

## Troubleshooting

### Issue: Agent Tried to Close Without Approval

**What happened:**
```
Error: Issue closure blocked - requires explicit user approval
Issue: #42
Agent: Claude Code
Time: 2026-08-05 10:30:00
```

**What to do:**
1. Review what the agent was attempting
2. If closure is correct, explicitly ask: "Close issue #42"
3. If not appropriate, discuss before closure

### Issue: Multiple Agents Involved

If a subagent created a fix:
```
Subagent result: "Issue #42 fixed, PR #X created. Requires approval to close."
Agent response: "Issue #42 is ready to close. Approve? (Y/N)"
User: "Yes"
Agent: ✅ Closes (user approved via main agent)
```

---

## Essential Rules

1. ✅ **Never auto-close issues** — Always ask for approval first
2. ✅ **Document the fix** — Summarize what was resolved before asking to close
3. ✅ **Confirm PR link** — Ensure fix is tracked in a PR before closure
4. ✅ **Wait for explicit approval** — "Shall I close?" requires "yes" or "Y"
5. ✅ **Respect exceptions** — Honor any waivers in settings.json
6. ✅ **Report closures** — Log what was closed and why

---

## Cross-References

- **CLAUDE.md:** General agent guidance
- **AGENTS.md:** Two-Gate System for multi-agent workflows
- **settings.json:** Hook configuration for approval gate
- **custom-instructions.md:** Code update guidelines

---

## Related

- GitHub issue automation: `fix-github-issues` skill
- GitHub API docs: https://docs.github.com/en/rest/issues
- PR linking: https://docs.github.com/en/issues/tracking-your-work-with-issues/linking-a-pull-request-to-an-issue
