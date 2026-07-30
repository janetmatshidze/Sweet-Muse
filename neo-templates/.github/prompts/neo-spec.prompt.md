---
description: 'Specify requirements — generate a lean PRD from a project brief or verbal description'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase']
---

# Neo Spec

Follow the workflow defined in `.github/agents/neo-pm.agent.md`.

1. If `artifacts/STATE.md` exists, read it to identify the active feature and current phase.
   Use the Active Feature name to locate the relevant brief in `artifacts/briefs/`.
   If Current Phase is not "Discovery" and not "Idle", warn: "STATE.md shows phase is {phase}. /neo-spec typically runs after Discovery. Continue anyway?" Wait for confirmation.
2. Check `artifacts/briefs/` for an existing brief. If found, read it.
3. If no brief exists, ask the user to describe the feature.
4. Before writing the PRD, scan the brief for ambiguities (vague scope, undefined user types, missing constraints, unclear success criteria). If any are found, list the ambiguities and ask the user to resolve them inline, or proceed with reasonable assumptions.
5. Scan the codebase for context.
6. Write the PRD and save to `artifacts/prds/{feature-name}.md`.
7. Update `artifacts/STATE.md`: set Current Phase to "Requirements".
8. Present the PRD and ask for approval.
