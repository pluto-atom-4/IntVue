# GitHub Issue Draft: AI Configuration Maintenance & Tuning

## Title
🤖 Maintain and Tune AI Configuration Context Architecture

## Description

### Overview
Establish a decentralized, modular AI context architecture for the IntVue project to optimize token consumption, prevent context window overflow, and ensure consistent AI-assisted development across all agent types (IDE autocomplete, CLI agents, and specialized workers). This issue tracks the implementation and ongoing maintenance of the context engineering strategy.

### Rationale
The current approach relies on monolithic instruction files (`.github/instructions/`) and custom instructions that may grow unbounded, causing:
- **Context Window Degradation:** Long files reduce model attention quality and increase latency
- **Token Tax:** Every instruction file is a permanent tax on every tool invocation and chat turn
- **Inconsistent Context:** No unified strategy for IDE (Copilot/Claude), autonomous agents, or specialized workers
- **Scaling Friction:** Adding new features or instructions requires careful balance to avoid overflow

This issue implements a **token-aware, decentralized architecture** that matches the modern best practices outlined in the [chore-idea.md](../chore-idea.md) guide.

---

## Solution: Decentralized Context Architecture

### Phase 1: Foundation Setup (Token Budgets & Config Structure)

Establish the modular context file hierarchy:

```
IntVue/
├── .github/
│   ├── copilot-instructions.md       # IDE Autocomplete (< 60 lines / 1,000 words)
│   └── instructions/                 # Existing instruction files (refactor into scoped directories)
│
├── .claude/
│   ├── profiles.json                 # Profile routing & feature flags
│   └── custom-instructions.md        # Claude Desktop/CLI defaults
│
├── CLAUDE.md                         # Quick-ref guardrails & commands (< 200 lines / ~1.5K tokens)
├── DESIGN.md                         # Architecture & data flow (200–400 lines / ~3K tokens)
├── SKILLS.md                         # Custom tools, MCP servers, API schemas (< 500 lines / 32 KiB)
├── AGENTS.md                         # Multi-agent choreography (already exists, maintain < 150 lines)
└── INTVUE-CONTEXT-MANIFEST.json      # NEW: Index of all context files + token budgets
```

### Phase 2: Context File Optimization

#### 2a. Establish CLAUDE.md (Project Guardrails & Commands)
**Acceptance Criteria:**
- [ ] File is < 200 lines (~1.5K tokens)
- [ ] Covers technology stack, build/test/run commands, code style essentials
- [ ] Eliminates redundancy with existing instruction files (link instead)
- [ ] Uses "Two-Strikes Rule": only document failures after the model fails twice
- [ ] For multi-agent contexts, use scoped nested `CLAUDE.md` files in subfolders (e.g., `Services/CLAUDE.md`)

**Key Sections:**
- Technology Stack & Environment (TFM, .NET version, platforms)
- Quick Build & Run Commands (with platform detection snippet)
- Code Style Essentials (DRY, KISS, SOLID, YAGNI — link to full guidance)
- Command Index (dev, test, build, run, debugger attach)

#### 2b. Establish DESIGN.md (Architecture & Structural Boundaries)
**Acceptance Criteria:**
- [ ] File is 200–400 lines (~3K tokens)
- [ ] On-demand reference (not injected into every turn)
- [ ] Includes: High-level architecture diagram (Mermaid), data models, state machines, technology boundaries
- [ ] Uses compact bullet fragments + ASCII/Mermaid diagrams (30% fewer tokens than prose)
- [ ] Prevents "hallucinated refactoring" by establishing hard structural boundaries

**Key Sections:**
- High-Level Architecture (Mermaid diagram: Views → ViewModels → Services → Models)
- Data Models & State Machines (MediaCapture lifecycle, recording states)
- Technology Boundaries & Constraints (MVVM pattern, x:Bind rules, disposal patterns)
- Integration Points (file I/O, media capture, localization)

#### 2c. Establish .claude/profiles.json (IDE Persona Layering)
**Acceptance Criteria:**
- [ ] File defines 2–3 profiles: `local-dev`, `ai-audit`, `security-review`
- [ ] Each profile specifies: `instructionsPath`, `allowedTools`, `contextBudget`
- [ ] Profiles are selectable by the user or auto-detected by agent type

**Example Profile Structure:**
```json
{
  "currentProfile": "local-dev",
  "profiles": {
    "local-dev": {
      "instructionsPath": ".claude/custom-instructions.md",
      "allowedTools": ["filesystem", "powershell", "git"],
      "contextBudget": 50000
    },
    "ai-audit": {
      "instructionsPath": ".claude/audit-instructions.md",
      "allowedTools": ["filesystem", "grep", "view"],
      "contextBudget": 80000,
      "readOnlyMode": true
    },
    "security-review": {
      "instructionsPath": ".claude/security-instructions.md",
      "allowedTools": ["filesystem", "grep"],
      "contextBudget": 60000,
      "readOnlyMode": true
    }
  }
}
```

#### 2d. Establish .claude/custom-instructions.md (Claude Desktop/CLI Defaults)
**Acceptance Criteria:**
- [ ] File specifies behavioral directives for Claude when used with this project
- [ ] Includes: code update guidelines, refactoring guards, error handling expectations
- [ ] Complements CLAUDE.md (guardrails) without duplication

**Example Directives:**
- Direct answers; skip pleasantries
- Check DESIGN.md before altering architectural boundaries
- Show complete functions in code responses, not truncated snippets
- Never hard-code secrets; link to security instruction file

#### 2e. Establish .github/copilot-instructions.md (Autocomplete Optimizer)
**Acceptance Criteria:**
- [ ] File is < 60 lines / 1,000 words (GitHub Copilot limit)
- [ ] Focuses on style assertions, not code explanations
- [ ] Covers: syntax preferences, naming conventions, functional patterns
- [ ] Prevents autocomplete bloat and ghost-text latency

**Example Content:**
- Use modern C# syntax (switch expressions, nullable reference types)
- Prefer explicit variable names (`isRecording`) over generics (`is`)
- Always use x:Bind in XAML, not {Binding}
- Link to CLAUDE.md for extended code style guidelines

---

### Phase 3: Token Accounting & Monitoring

#### 3a. Create INTVUE-CONTEXT-MANIFEST.json
**Acceptance Criteria:**
- [ ] File lists all context files, token counts, and insertion points
- [ ] Auto-updated by a CI/CD job (see Phase 4)
- [ ] Enables agents to make informed decisions about which files to load

**Example Structure:**
```json
{
  "version": "1.0",
  "generatedAt": "2026-07-19T12:00:00Z",
  "contexts": [
    {
      "name": "CLAUDE.md",
      "path": "CLAUDE.md",
      "tokenCount": 1500,
      "lineCount": 180,
      "insertionType": "every-turn-tax",
      "priority": 1
    },
    {
      "name": "copilot-instructions.md",
      "path": ".github/copilot-instructions.md",
      "tokenCount": 800,
      "lineCount": 55,
      "insertionType": "ide-autocomplete-trigger",
      "priority": 1
    },
    {
      "name": "DESIGN.md",
      "path": "DESIGN.md",
      "tokenCount": 3000,
      "lineCount": 320,
      "insertionType": "on-demand",
      "priority": 2
    }
  ],
  "totalAlwaysOnTokens": 2300,
  "budget": {
    "maxAlwaysOnTokens": 3000,
    "maxPerFile": 2000
  }
}
```

#### 3b. Add Token Count Script
**Acceptance Criteria:**
- [ ] PowerShell script that counts tokens in context files (approximate line/word-based heuristic)
- [ ] Script validates files against token budgets
- [ ] Runs on every commit (pre-commit hook or CI/CD)

---

### Phase 4: CI/CD Validation (Prevent Context Drift)

#### 4a. Add `.github/workflows/context-drift-check.yml`
**Acceptance Criteria:**
- [ ] Validates that all required context files exist on every push
- [ ] Checks token budgets (file size / line count heuristics)
- [ ] Fails CI if files exceed their hard limits or are deleted
- [ ] Produces a summary report of context health

**Validation Steps:**
```yaml
- name: Verify Context File Presence
  run: |
    for file in CLAUDE.md DESIGN.md AGENTS.md .github/copilot-instructions.md .claude/profiles.json; do
      if [ ! -f "$file" ]; then
        echo "::error::Missing context file: $file"
        exit 1
      fi
    done

- name: Check Token Budgets
  run: |
    pwsh scripts/validate-context-tokens.ps1
```

---

### Phase 5: Ongoing Maintenance & Tuning

#### 5a. Monthly Context Health Review
**Acceptance Criteria:**
- [ ] Quarterly (or monthly) review of context manifest
- [ ] Update token counts based on actual LLM usage metrics
- [ ] Measure context drift (files growing beyond budgets)
- [ ] Refactor oversized files into scoped nested files

**Review Checklist:**
- [ ] All files within their token budgets?
- [ ] Any files deleted or moved without updating CI/CD?
- [ ] New instructions added — do they belong in existing files or new scoped files?
- [ ] Are CI/CD validations catching drift early?

#### 5b. Agent-Specific Context Tuning
**Acceptance Criteria:**
- [ ] Update profiles.json when new agent types are introduced
- [ ] Measure agent success rates (how often do agents fail due to missing context?)
- [ ] Adjust `instructionsPath` and `allowedTools` based on real-world agent behavior

---

## Acceptance Criteria (Global)

- [ ] All context files created and token budgets documented
- [ ] `.github/copilot-instructions.md` < 60 lines, verified by CI/CD
- [ ] `CLAUDE.md` < 200 lines, verified by CI/CD
- [ ] `DESIGN.md` 200–400 lines, verified by CI/CD
- [ ] `AGENTS.md` < 150 lines, verified by CI/CD
- [ ] CI/CD workflow runs and validates context integrity on every push
- [ ] No duplicate guidance between context files (link instead of repeat)
- [ ] All instruction files in `.github/instructions/` have clear ownership and insertion triggers
- [ ] Project builds successfully with current context configuration
- [ ] Documentation updated to reference the new context architecture

---

## Related Files & References

- **Source Guide:** [chore-idea.md](../chore-idea.md) — Comprehensive context engineering blueprint
- **Current AGENTS.md:** [.github/AGENTS.md](./.github/AGENTS.md)
- **Current Instructions:** [.github/instructions/](./github/instructions/)
- **WinUI Best Practices:** [.github/instructions/winui-best-practices.instructions.md](./github/instructions/winui-best-practices.instructions.md)

---

## Success Metrics

1. **Token Efficiency:** "Always-on" context (CLAUDE.md + copilot-instructions.md) stays below 3,000 tokens
2. **Context Clarity:** Agent success rate improves by 20% (fewer failures due to missing context)
3. **Drift Prevention:** CI/CD catches all context file violations before merge
4. **Maintainability:** New contributors can understand context architecture within 15 minutes

---

## Implementation Timeline

- **Week 1:** Phase 1 & 2a–2b (CLAUDE.md, DESIGN.md setup)
- **Week 2:** Phase 2c–2e (.claude/ profiles, copilot-instructions.md)
- **Week 3:** Phase 3 & 4 (Token accounting, CI/CD validation)
- **Week 4+:** Phase 5 (Ongoing reviews, tuning, and agent profiling)

---

## Notes

- This issue is **not** about deleting existing instruction files, but organizing them into a coherent hierarchy with clear ownership
- The "Two-Strikes Rule" prevents premature optimization — only document failures after observing them twice
- Use Git commits with labels like `[context]` or `[ctx-tuning]` to track context-related changes
- Consider automating token counting via pre-commit hooks to catch drift early

---

## Labels
`chore` `documentation` `architecture` `ai-config` `context-engineering`

## Assignees
(To be determined)

## Milestone
(Suggest: Q3 2026 - Project Infrastructure)
