---
description: 'Quick change — lightweight flow for small features, chores, and patches with automatic scope detection'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal', 'githubRepo']
---

# Neo Quick

Fast-track for small changes that need traceability without a full PRD. Automatically detects whether the change is a patch (1-3 files) or a task (4-10 files) and adjusts ceremony accordingly.

## Step 1 — Describe the Change

Ask the user: "Describe the change in 2-3 sentences: what needs to change and why."

Scan the codebase to confirm affected files and understand the surrounding code. Count the likely files to change.

## Step 2 — Auto-Detect Scope

Based on the file count and description:

- **Patch** (1-3 files): Urgent fix, config change, or tiny feature. No story artifact — just fix, test, and ship. Branch: `hotfix/{name}`.
- **Task** (4-10 files): Small feature, chore, or refactor. Creates a minimal story artifact for traceability. Branch: `quick/{name}`.
- **Too big** (>10 files): Stop and suggest: "This looks larger than a quick change. Consider `/neo-plan` for a proper planning pass."

Tell the user which scope was detected and let them override.

## Step 3 — Create Story Artifact (Task scope only)

Skip this step for Patch scope.

Create a minimal story file at `artifacts/stories/quick/{name}.md`:

```markdown
# Quick: {Title}

**Status:** TODO
**Branch:** quick/{name}
**Created:** {YYYY-MM-DD}

## Objective
{1-2 sentences describing what changes and why}

## Files
- `{path}` — {what changes}

## Acceptance Criteria
- [ ] {criterion 1}
- [ ] {criterion 2}

## Testing
- {what to test}

## Definition of Done
- [ ] All acceptance criteria met
- [ ] Tests pass
- [ ] Build succeeds
- [ ] No lint errors introduced
```

Present the story to the user and wait for approval before implementing.

## Step 4 — Implement

1. Create the branch (`hotfix/{name}` for Patch, `quick/{name}` for Task).
2. Implement the changes.
3. Write or update tests covering the change.
4. Run the build and test suite. Fix any failures.
5. Run the security checklist if any changes touch user input, auth, or external APIs.

## Step 5 — Complete

1. If a story artifact exists (Task scope): update it — set Status to DONE, check off acceptance criteria and DoD items.
2. If `artifacts/STATE.md` exists, add a session note.
3. Add a Context Handoff block to STATE.md if the work spans multiple sessions:
   ```
   ## Context Handoff
   **Date:** {today}
   **Task completed:** {title}
   **Next action:** {what to do next}
   ```

Report completion:

```
**Quick change completed:** {Title}
**Scope:** {Patch / Task}
**Branch:** {branch name}

**Files changed:**
- `{path}` — {what changed}

**Tests:** {count} added/updated, all passing
**Security checklist:** {Passed / N/A}

**Next:** Run `/neo-ship` to review and prepare a PR, or merge directly if pre-approved.
```
