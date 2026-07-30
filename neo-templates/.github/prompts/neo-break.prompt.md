---
description: 'Break down an approved PRD into self-contained developer stories with full context'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase']
---

# Neo Break

Follow the workflow defined in `.github/agents/neo-planner.agent.md`.

1. If `artifacts/STATE.md` exists, read it to identify the active feature, current phase,
   and scale. Use the Active Feature name to locate the relevant PRD and architecture doc.
   If Current Phase is not "Architecture" and not "Requirements" (for Medium scale), warn:
   "STATE.md shows phase is {phase}. /neo-break typically runs after Architecture (or Requirements for Medium scale). Continue anyway?" Wait for confirmation.
2. Find the relevant PRD in `artifacts/prds/`. If multiple exist, ask the user which one.
3. Check for a matching architecture doc in `artifacts/architecture/`.
4. Shard into self-contained story files.
5. Save each story to `artifacts/stories/{feature-name}-{N}.md`.
6. Create a story index at `artifacts/stories/{feature-name}-index.md`.
7. Update `artifacts/STATE.md`: set Current Phase to "Implementation".
8. Present the story index.
