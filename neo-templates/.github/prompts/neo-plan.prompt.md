---
description: 'Plan a feature — full pipeline from idea to implementation-ready stories in one session'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'fetch', 'runInTerminal', 'githubRepo']
---

# Neo Plan

Run the full Neo pipeline for a new feature. Create `artifacts/` subdirectories as needed.

## Phase 0 — Initialize State

Create or update `artifacts/STATE.md`:
- Set **Current Phase** to "Discovery"
- Set **Active Feature** to the feature name (derive from the user's description)
- Add the feature to the **In-Flight Features** table with phase "Discovery", 0 stories done, 0 total
- Clear any previous session notes for this feature

If `artifacts/STATE.md` already exists, update it rather than overwriting — preserve entries for other in-flight features.

## Phase 0.5 — Scale Assessment

After initializing state, assess the scale of the work:

- **Small** (bug fix, config change, single-file feature): Zero-ceremony path. Go directly to Brief → Single Story → Build. No PRD, no architecture, no waves.
- **Medium** (multi-file feature, clear scope): Brief → PRD → Stories → Build. Skip Architecture unless the user requests it or new infrastructure is involved.
- **Large** (new system, new integration, multi-service): Full pipeline — Brief → PRD → Architecture → Stories → Build.

Ask the user to confirm the scale or override it. Update `artifacts/STATE.md` with the chosen scale.

If the scale is later revealed to be wrong (e.g., Small turns out to be Medium), offer to upgrade: "This looks bigger than initially assessed. Upgrade to Medium scale? This will add a PRD phase but preserve your existing brief."

Each phase header below notes when it can be skipped based on scale.

## Phase 1 — Discovery (all scales)

### GitHub Issue Import

If the user has a GitHub Issue URL, fetch it automatically:
1. Use the `githubRepo` tool to fetch the issue title, body, labels, and checklist items.
2. Extract: Title → feature name, Description → Problem and Proposed Solution, Checklist items → Constraints, Labels → existing related code.
3. Scan the codebase for files related to the issue's subject area.

### Manual Discovery

If no ticket exists, follow the workflow defined in `.github/agents/neo-analyst.agent.md`. Ask clarifying questions, scan the codebase, and produce a project brief.

### Brief Output

Save the brief to `artifacts/briefs/{feature-name}.md`. Present it to the user and wait for explicit approval before continuing.

## Phase 1.5 — Ambiguity Resolution (Medium and Large)

Skip for Small scale.

After the brief is approved, assess whether significant ambiguities remain (unfamiliar integrations, complex API design, multi-service coordination).

If ambiguities exist, resolve them inline:
1. Analyze the brief across: Data Model, API Design, UX/Behavior, Integration, Security, Performance.
2. For each gray area, present options with trade-offs and a recommendation.
3. After the user resolves all items, append a **Resolved Decisions** section to the brief.

If no significant ambiguities remain, continue to the next phase.

## Phase 2 — Requirements (Medium and Large)

Skip this phase for Small scale — go directly to Phase 4.

Follow the workflow defined in `.github/agents/neo-pm.agent.md`. Read the approved brief. Produce a lean PRD. Present it and wait for approval.

## Phase 3 — Architecture (Large only)

Skip this phase for Small and Medium scale — unless the user requests it or the feature introduces new infrastructure.

Follow the workflow defined in `.github/agents/neo-architect.agent.md`. Read the PRD and scan the codebase. Produce an architecture doc. Present it and wait for approval.

## Phase 4 — Stories (all scales)

### Small scale
Create a single lightweight story directly from the brief. Save to `artifacts/stories/{feature-name}-1.md`. Create a minimal index. Done — no PRD, no waves.

### Medium and Large scale
Follow the workflow defined in `.github/agents/neo-planner.agent.md`. Read the PRD and architecture doc (if they exist). Break the feature into self-contained stories. Present the story index.

## Phase 5 — Handoff

After stories are presented and approved, suggest: "Stories are ready. Run `/neo-implement` to start implementing story 1."

Update `artifacts/STATE.md`: set Current Phase to "Implementation".

## Rules

- Do not proceed to the next phase without explicit user approval.
- The entire planning session should complete within one session.
- If any phase reveals the feature is too large for 7 stories, follow the planner's Auto-Split Workflow (`.github/agents/neo-planner.agent.md`). Draft all stories, propose sub-features with rationale, and create an epic index after approval.
- For Small scale, the entire plan should take under 5 minutes. Brief + story + done.

## Context Management

- Keep the orchestrator thin — delegate to agent workflows, don't duplicate their logic.
- After completing Phase 2 (Requirements): suggest "PRD approved. **Start a fresh chat** and run `/neo-design` — it will read STATE.md and your PRD automatically."
- After completing Phase 3 (Architecture): strongly recommend "Architecture approved. **Start a fresh chat** and run `/neo-break` — it reads STATE.md, your PRD, and architecture doc."
- Always update `artifacts/STATE.md` before suggesting a context switch.
