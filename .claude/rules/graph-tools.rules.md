# Graph Intelligence Rule: code-review-graph + Graphify Routing

**Purpose:** Route codebase-navigation prompts to graph queries instead of expensive Grep/Glob/full-file reads, per [Issue #162](https://github.com/pluto-atom-4/IntVue/issues/162). `code-review-graph` handles micro/AST blast-radius; Graphify handles macro/doc knowledge-graph context. Content below was injected verbatim by each tool's own installer (`code-review-graph install --platform claude-code -y`, `graphify claude install`) and relocated here from CLAUDE.md to stay under its 250-line CI cap (`.github/workflows/config-validation.yml`).

---

## Routing Matrix

| Intent | Primary Tool | Fallback |
|---|---|---|
| "Where is X?", exploring code | `semantic_search_nodes_tool` / `query_graph_tool` (code-review-graph MCP) or `graphify query "<question>"` | Narrow Grep, scoped to the identified files |
| "What breaks if I change X?", blast radius | `get_impact_radius_tool` (code-review-graph) | Targeted Grep on the file + its direct imports |
| Code review of a diff | `detect_changes_tool` + `get_review_context_tool` (code-review-graph) | Read the changed files directly |
| Architecture overview, business logic across docs/schemas | `graphify query`/`graphify explain`, or `graphify-out/GRAPH_REPORT.md` | Read `README.md`/design docs |
| Relationship path between two symbols | `graphify path "<A>" "<B>"` | `query_graph_tool` pattern=callers_of/callees_of |

## Orchestration Rules

1. **Macro → Micro escalation**: check `graphify-out/GRAPH_REPORT.md` (or `graphify query`) to find the subsystem, then switch to `code-review-graph` MCP tools to trace exact call paths inside it.
2. **Single-engine isolation**: don't query both engines in the same reasoning step. If `code-review-graph` fails on an unparsed/broken file, fall back to a plain Grep — don't reach for Graphify's AST layer instead.
3. **Stale-graph handling**: the `PostToolUse` hook (`Edit|Write` → `code-review-graph update --skip-flows`) keeps `.code-review-graph/graph.db` fresh automatically; run `code-review-graph build` yourself after a large structural refactor. For Graphify, run `graphify update .` after modifying code (AST-only, no API cost).
4. **Source is authoritative**: when the graph and the source disagree, the source wins — the graph may be stale or not model a relationship. An empty graph result means "not indexed", not "does not exist". Never change code from graph output alone; verify against the actual file for non-trivial changes (behavior, DB logic, migrations, retries, fallbacks, compatibility code).

## Uninstall note

`code-review-graph uninstall` / `graphify claude uninstall` normally strip their own marker blocks from `CLAUDE.md` automatically. Since those blocks were relocated into this file, running either uninstall will **not** clean up this file — manually delete the corresponding section below if a tool is ever removed.

---

<!-- code-review-graph MCP tools -->
## MCP Tools: code-review-graph

**This project has a knowledge graph. Start with the code-review-graph
MCP tools to narrow scope, then read the source.** The graph is cheaper than scanning files and
gives you structural context (callers, dependents, test coverage) that file search cannot.

### When to use graph tools FIRST

- **Exploring code**: `semantic_search_nodes_tool` or `query_graph_tool` instead of Grep
- **Understanding impact**: `get_impact_radius_tool` instead of manually tracing imports
- **Code review**: `detect_changes_tool` + `get_review_context_tool` instead of reading entire files
- **Finding relationships**: `query_graph_tool` with callers_of/callees_of/imports_of/tests_for
- **Architecture questions**: `get_architecture_overview_tool` + `list_communities_tool`

### Verify in the source

- Narrow scope with the graph, then read the source. Do not change code from graph output alone.
- For any non-trivial change, read the implementation and the relevant tests before concluding.
- Verify the exact source when touching behavior, database logic, migrations, retries, fallbacks,
  recovery, or compatibility code.
- When the graph and the source disagree, the source wins. The graph may be stale or may not
  model that relationship.
- An empty graph result can mean "not indexed" or "not statically visible", not "does not exist".

### Key Tools

| Tool | Use when |
| ------ | ---------- |
| `detect_changes_tool` | Reviewing code changes — gives risk-scored analysis |
| `get_review_context_tool` | Need source snippets for review — token-efficient |
| `get_impact_radius_tool` | Understanding blast radius of a change |
| `get_affected_flows_tool` | Finding which execution paths are impacted |
| `query_graph_tool` | Tracing callers, callees, imports, tests, dependencies |
| `semantic_search_nodes_tool` | Finding functions/classes by name or keyword |
| `get_architecture_overview_tool` | Understanding high-level codebase structure |
| `refactor_tool` | Planning renames, finding dead code |

### Workflow

1. The graph auto-updates on file changes (via hooks).
2. Use `detect_changes_tool` for code review.
3. Use `get_affected_flows_tool` to understand impact.
4. Use `query_graph_tool` pattern="tests_for" to check coverage.
<!-- /code-review-graph MCP tools -->

## graphify

This project has a knowledge graph at graphify-out/ with god nodes, community structure, and cross-file relationships.

Rules:
- For codebase questions, first run `graphify query "<question>"` when graphify-out/graph.json exists. Use `graphify path "<A>" "<B>"` for relationships and `graphify explain "<concept>"` for focused concepts. These return a scoped subgraph, usually much smaller than GRAPH_REPORT.md or raw grep output.
- If graphify-out/wiki/index.md exists, use it for broad navigation instead of raw source browsing.
- Read graphify-out/GRAPH_REPORT.md only for broad architecture review or when query/path/explain do not surface enough context.
- After modifying code, run `graphify update .` to keep the graph current (AST-only, no API cost).

---

## Bash Permissions

`.claude/settings.json` → `permissions.allow` includes `graphify {query,path,explain,affected,update,god-nodes}` and `code-review-graph {query,impact,detect-changes,update,status}` so these run without a per-call prompt. Mutating operations (`install`, `uninstall`, `register`) are intentionally not allow-listed — they require approval per this repo's [destructive-command-governance.rules.md](./destructive-command-governance.rules.md) philosophy of not auto-approving anything that changes shared tool config.

## Cross-References

- **CLAUDE.md:** short pointer + Rules-by-Category row link here
- **AGENTS.md:** "Graph Intelligence Tools" subsection (Tool Matrix area)
- **.mcp.json:** registers `code-review-graph` (stdio, `uvx code-review-graph serve`) for this repo; `graphify` MCP server is registered globally in the user's `~/.claude/settings.json` (`graphify-mcp`)
- **Data locations:** `.code-review-graph/graph.db` (SQLite, gitignored), `graphify-out/` (JSON + reports, gitignored)
