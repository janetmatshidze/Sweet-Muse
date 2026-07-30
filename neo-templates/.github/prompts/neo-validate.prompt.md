---
description: 'Validate project artifacts — verify PRD requirement IDs, story references, required sections, and index consistency'
mode: 'agent'
tools: ['read', 'search/codebase', 'runInTerminal']
---

# Neo Validate

Validate the structural integrity of project artifacts. This checks that artifacts follow Neo conventions and that cross-references are consistent.

## Step 1 — Discover Artifacts

Scan the `artifacts/` directory for PRDs, architecture docs, story files, and story indexes. If no artifacts exist, stop and notify the user:
> "No artifacts found in `artifacts/`. Run `/neo-plan` to start a feature."

## Step 1.5 — Validate Briefs

For each file in `artifacts/briefs/`:

1. **Required sections** — Verify: Problem, Proposed Solution, Business Value.
2. **Stakeholders** — Warn if missing.

## Step 2 — Validate PRDs

For each file in `artifacts/prds/`:

1. **Requirement IDs** — Every Must Have requirement must have an `R-XX` identifier (e.g., `R-01`, `R-02`). Flag any requirement without an ID.
2. **Required sections** — Verify these sections exist: Problem, Solution, Requirements (with Must Have and Won't Have subsections), Non-Functional Requirements, Done Criteria.
3. **BDD format** — Must Have requirements should follow `Given / When / Then` format. Warn if any do not.

## Step 3 — Validate Stories

For each story file in `artifacts/stories/` (excluding index files):

1. **Requirement reference** — The story must reference at least one PRD requirement ID (`R-XX`). Flag stories with no traceability.
2. **Required sections** — Verify these sections exist: Description (or Summary or Overview or Objective), Acceptance Criteria, Technical Context, Implementation Steps, Testing Requirements, Definition of Done.

## Step 4 — Validate Story Index

For each story index file in `artifacts/stories/` (files containing "index" in the name or listing multiple stories):

1. **File references** — Every story file listed in the index must exist on disk. Flag missing files.
2. **Completeness** — Every story file in `artifacts/stories/` should appear in the index. Flag orphaned story files.
3. **Status values** — Every story in the index must have a valid status: `TODO`, `IN_PROGRESS`, `BLOCKED`, or `DONE`. Flag any other values.

## Step 5 — Validate Architecture Docs

For each file in `artifacts/architecture/` (excluding security review files):

1. **PRD reference** — The doc should reference a PRD file path. Warn if missing.
2. **Required sections** — Verify these sections exist: Approach, Key Decisions, Components.

## Step 6 — Report

Present a summary table:

```markdown
## Artifact Validation Report

| Artifact | Checks | Passed | Warnings | Errors |
|----------|--------|--------|----------|--------|
| PRDs     | {n}    | {n}    | {n}      | {n}    |
| Stories  | {n}    | {n}    | {n}      | {n}    |
| Index    | {n}    | {n}    | {n}      | {n}    |
| Arch     | {n}    | {n}    | {n}      | {n}    |

**Overall:** PASS / FAIL
```

List each error and warning with the file path and specific issue. Suggest fixes for each error.
