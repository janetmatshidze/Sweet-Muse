---
description: 'Show status and resume — dashboard of all in-flight features, or pick up where you left off'
mode: 'agent'
tools: ['read', 'search/codebase']
---

# Neo Status

Produce a dashboard of all active features and optionally resume work from where you left off.

## Step 1 — Read State

If `artifacts/STATE.md` exists, read it for:
- Current phase and active feature
- Scale (Small, Medium, Large)
- Blockers
- In-flight features table
- Session notes (last entry)
- Context Handoff block (if present)

## Step 2 — Discover Features

Scan `artifacts/stories/` for all `*-index.md` files. Each file represents one feature.

Also scan `artifacts/stories/` for all `*-epic.md` files. Each epic file represents a multi-phase feature that was split into sub-features.

Also scan `artifacts/stories/quick/` for any `.md` files. These are lightweight tasks created by `/neo-quick` that have no index file. Include them in the dashboard as individual items.

## Step 3 — Parse Each Index

For each index file, read:
- Feature name (from the file heading)
- Total story count
- Count of stories with status `DONE`
- Count of stories with status `IN_PROGRESS`
- Count of stories with status `BLOCKED`
- Count of stories with status `TODO`
- The title of the next actionable story: first any `IN_PROGRESS` story (to resume), then the next `TODO` story
- Wave grouping (if the index uses wave-based format)

## Step 3.5 — Parse Epics

For each epic file (`*-epic.md`), read:
- Epic name (from the file heading)
- Sub-feature list with their index file paths
- For each sub-feature, parse its index (same as Step 3) to get story counts and status

Calculate epic-level totals:
- Total stories across all sub-features
- Total DONE across all sub-features
- Sub-features completed (all stories DONE) vs total sub-features

## Step 4 — Print the Dashboard

Output a table:

```
Neo Status — {date}

| Feature | Phase | Scale | Stories | Done | In Progress | Blocked | TODO | Next Action |
|---------|-------|-------|---------|------|-------------|---------|------|-------------|
| {name}  | {phase} | {scale} | {total} | {n}  | {n}       | {n}     | {n}  | Story {N}: {title} |
```

For epics (multi-phase features), show epic-level progress above the sub-feature rows:
```
Epic: {name} — {sub-features done}/{total sub-features} phases complete, {stories done}/{total stories} stories done

| Sub-Feature | Stories | Done | Remaining | Next Action |
|-------------|---------|------|-----------|-------------|
| {name}-phase-1 | 4 | 4 | 0 | All DONE |
| {name}-phase-2 | 4 | 1 | 3 | Story 2: {title} |
```

If wave-based story indexes exist, show wave progress:
```
Wave 1: 3/3 DONE | Wave 2: 1 IN_PROGRESS, 1 TODO | Wave 3: 0/2 waiting (blocked by Wave 2)
```

If there are blockers in STATE.md, display them prominently:
```
Blockers:
- {blocker description}
```

If `artifacts/stories/` is empty or contains no index files, output:
> No features in flight. Start one with `/neo-plan`.

## Step 5 — Resume Mode

If a Context Handoff block exists in STATE.md, or if the user asked to resume (e.g., "where was I?", "resume", "continue"), present a resume summary:

```
Resuming: {Feature Name}
Phase: {Current Phase}
Scale: {Scale}
Last session: {Summary from session notes}

Next action: {Specific command and context}
```

Determine the next action based on phase:

| Phase | Condition | Action |
|-------|-----------|--------|
| Discovery | No brief exists | "Run `/neo-plan` to create a brief." |
| Discovery | Brief exists, not approved | "Review and approve the brief, then run `/neo-spec`." |
| Requirements | No PRD exists | "Run `/neo-spec` to create a PRD from the brief." |
| Architecture | No architecture doc | "Run `/neo-design` to create an architecture doc." |
| Implementation | IN_PROGRESS story exists | "Run `/neo-implement` to resume story {N}: {title} (in progress)." |
| Implementation | BLOCKED stories exist | "Story {N} is blocked: {reason}. Run `/neo-implement` to work on the next unblocked story, or resolve the blocker first." |
| Implementation | TODO stories remain | "Run `/neo-implement` to pick up story {N}: {title}." |
| Implementation | All stories DONE | "All stories complete. Run `/neo-verify` then `/neo-ship`." |
| Ship | Ready | "Run `/neo-ship` to prepare the PR." |

If a Context Handoff block exists, use its instructions instead of the generic table.

If there are blockers, display them before the next action.

Ask: "Shall I run `{command}` now, or would you like to do something else first?"

## Step 6 — Update State

Update `artifacts/STATE.md` (if it exists) with the current status snapshot.

## Step 7 — Suggest Next Action

After the table, suggest the highest-priority action:
- If blockers exist: address the blocker first
- If any feature has all stories DONE: "Run `/neo-verify` then `/neo-ship` for {feature}."
- If any feature has TODO stories: "Run `/neo-implement` to continue {feature}."
- If all features are shipped: "All features shipped. Nothing in flight."
