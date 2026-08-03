---
description: 'Reset pipeline to a previous phase — archive current artifacts and update state'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal']
---

# Neo Reset

Revert the pipeline to a previous phase, archiving current artifacts so nothing is lost.

## Step 1 — Read Current State

Read `artifacts/STATE.md`. If it does not exist, stop and suggest:
> "No STATE.md found. Nothing to reset."

Display the current state:

```
Current state:
  Feature: {name}
  Phase: {current phase}
  Stories: {done}/{total}
```

## Step 2 — Choose Target Phase

Ask the user which phase to revert to:

```
Which phase should I reset to?

1. Discovery    — Start fresh from the brief
2. Requirements — Keep the brief, redo the PRD
3. Architecture — Keep brief + PRD, redo architecture
4. Implementation — Keep all planning artifacts, reset story progress

Current phase: {current phase}
```

Only show phases earlier than the current one. If the user is in Discovery, there is nothing to reset — inform them.

## Step 3 — Archive Current Artifacts

Create an archive directory: `artifacts/.archive/{YYYY-MM-DD-HHmmss}/`

Based on the target phase, move (not delete) artifacts that will be regenerated:

| Reset to | Archive these |
|----------|--------------|
| Discovery | `artifacts/briefs/{feature}*`, `artifacts/prds/{feature}*`, `artifacts/architecture/{feature}*`, `artifacts/stories/{feature}*` |
| Requirements | `artifacts/prds/{feature}*`, `artifacts/architecture/{feature}*`, `artifacts/stories/{feature}*` |
| Architecture | `artifacts/architecture/{feature}*`, `artifacts/stories/{feature}*` |
| Implementation | `artifacts/stories/{feature}*` (reset story statuses to TODO) |

For Implementation reset: instead of archiving stories, reset all story statuses from DONE/IN_PROGRESS back to TODO in the story index.

## Step 4 — Update STATE.md

Update `artifacts/STATE.md`:
- Set **Current Phase** to the target phase
- Reset story counts if applicable
- Add a session note: `"Reset to {phase} on {date}. Previous artifacts archived to .archive/{timestamp}/"`

## Step 5 — Confirm

```
Reset complete.

  Feature: {name}
  New phase: {target phase}
  Archived to: artifacts/.archive/{timestamp}/

Next: Run `{suggested command}` to continue from here.
```

Suggest the appropriate command for the target phase (e.g., `/neo-plan` for Discovery, `/neo-spec` for Requirements).
