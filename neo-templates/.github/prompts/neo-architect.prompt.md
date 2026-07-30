---
description: 'Make technical decisions and produce an architecture doc from a PRD'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal']
---

# Neo Architect

Follow the workflow defined in `.github/agents/neo-architect.agent.md`.

1. If `artifacts/STATE.md` exists, read it to identify the active feature and current phase.
   Use the Active Feature name to locate the relevant PRD in `artifacts/prds/`.
   If Current Phase is not "Requirements", warn: "STATE.md shows phase is {phase}. /neo-architect typically runs after Requirements. Continue anyway?" Wait for confirmation.
2. Read the PRD from `artifacts/prds/`. If no PRD exists, stop and tell the user:
   "No PRD found. Run `/neo-spec` or `/neo-plan` first."
3. If `artifacts/architecture/conventions.md` exists, read it for established codebase conventions. Otherwise scan the codebase for patterns.
4. Make key technical decisions and document them as ADRs.
5. Write the architecture doc to `artifacts/architecture/{feature-name}.md`.
6. Update `artifacts/STATE.md`: set Current Phase to "Architecture".
7. Present the doc and wait for approval.
8. After approval, suggest: "Ready to break this into stories with `/neo-break`."
