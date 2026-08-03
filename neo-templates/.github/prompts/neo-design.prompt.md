---
description: 'Design the architecture — make technical decisions, produce architecture docs, and record ADRs'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal']
---

# Neo Design

Follow the workflow defined in `.github/agents/neo-architect.agent.md`.

## Full Architecture Workflow

1. If `artifacts/STATE.md` exists, read it to identify the active feature and current phase.
   Use the Active Feature name to locate the relevant PRD in `artifacts/prds/`.
   If Current Phase is not "Requirements", warn: "STATE.md shows phase is {phase}. /neo-design typically runs after Requirements. Continue anyway?" Wait for confirmation.
2. Read the PRD from `artifacts/prds/`. If no PRD exists, stop and tell the user:
   "No PRD found. Run `/neo-spec` or `/neo-plan` first."
3. If `artifacts/architecture/conventions.md` exists, read it for established codebase conventions. Otherwise scan the codebase for patterns.
4. Make key technical decisions and document them as ADRs.
5. Write the architecture doc to `artifacts/architecture/{feature-name}.md`.
6. Update `artifacts/STATE.md`: set Current Phase to "Architecture".
7. Present the doc and wait for approval.
8. After approval, suggest: "Ready to break this into stories with `/neo-break`."

## Standalone ADR Mode

If the user asks to record a single decision (e.g., "record a decision about...", "ADR for..."), capture it without the full architecture workflow:

1. Ask the user:
   - "What decision needs to be recorded?"
   - "What context drove this decision?"
   - "What alternatives were considered and why were they rejected?"
   - "What are the consequences or trade-offs?"

2. Generate a kebab-cased slug from the decision title.

3. Save to `artifacts/architecture/adr-{slug}.md`:

```markdown
# ADR: {Decision Title}

**Date:** {YYYY-MM-DD}
**Status:** Accepted
**Feature:** {feature-name or "cross-cutting"}

## Context
{Why a decision was needed. What forces are at play.}

## Decision
{What was decided. Be specific.}

## Consequences
{What becomes easier or harder. Trade-offs accepted. Follow-on work required.}

## Alternatives Considered
- **{Alternative}:** {Why it was rejected}
```

4. Present the ADR and confirm: "Does this accurately capture the decision and its rationale?"
