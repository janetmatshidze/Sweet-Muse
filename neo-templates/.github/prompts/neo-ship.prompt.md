---
description: 'Ship a feature — final review, cleanup, and pull request preparation'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal', 'githubRepo']
---

# Neo Ship

Perform the final review before shipping.

## Step 0 — Branch Check

Run `git branch --show-current` to identify the current branch. If the result is `main`, `master`, or `develop`, stop and notify the user:
> "You appear to be on `{branch}`. Neo Ship should run from a feature branch. Check out your feature branch and retry."

Only continue if the current branch is a feature branch.

## Step 0.5 — Security Review

Scan the changed files (from `git diff --name-only main`). If any files handle user input, authentication, sessions, or sensitive data, check whether a security review artifact exists at `artifacts/architecture/security-review-{feature-name}.md` or whether `/neo-verify` was run with security checks. If no evidence of a security review exists, stop and notify the user:
> "This feature appears to handle sensitive data. Run `/neo-verify` before shipping."

If the feature does not touch sensitive paths, continue.

## Step 0.75 — Phase Check

If `artifacts/STATE.md` exists, read it. If Current Phase is not "Implementation",
warn: "STATE.md shows phase is {phase}. /neo-ship expects all stories DONE in
Implementation phase. Continue anyway?" Wait for confirmation.

## Step 1 — Verify Completeness

Read the story index in `artifacts/stories/`. All stories for the feature must have status `DONE`. If any remain `TODO`, `IN_PROGRESS`, or `BLOCKED`, stop and notify the user with the specific stories and their statuses.

After confirming all index entries are `DONE`, also verify each story file:
1. Read each story `.md` file referenced in the index.
2. Check that **all** `- [ ]` items under `## Acceptance Criteria` are `- [x]` (checked off).
3. Check that **all** `- [ ]` items under `## Definition of Done` are `- [x]` (checked off).
4. If any checkboxes are unchecked, stop and list them:
   ```
   ⚠ Story {N} has unchecked items:
   Acceptance Criteria:
   - [ ] {unchecked criterion}
   Definition of Done:
   - [ ] {unchecked item}
   ```
   Do not proceed until all checkboxes are checked or the user explicitly overrides.

## Step 1.5 — Verification Check

Check if `/neo-verify` has already been run by reading `artifacts/STATE.md` for a `## Verification` block. Look for:
- `**Result:** PASS`
- `**Date:**` that is not older than the most recent story completion

If a passing verification block exists and is current, skip re-running checks and note: "Verification already passed via `/neo-verify` on {date}."

If the verification block is missing, shows `FAIL`, or is older than the latest story completion, run built-in checks:
1. **Build** — Run the project build command. Report pass or fail.
2. **Tests** — Run the full test suite. Report pass or fail, and coverage if available.
3. **Lint** — Run the linter. Report errors. Warnings are acceptable.
4. **Type check** — Run type checking if applicable. Report errors.

Inform the user: "No recent passing `/neo-verify` found. Running built-in checks."

If any check fails, diagnose and fix the issue before proceeding.

## Step 2 — Code Review

Scan changed files for:

- Unused imports or dead code
- Unresolved TODO or FIXME comments
- Debug logging that should be removed
- Hardcoded values that should be configuration
- Missing error handling on async operations

Fix minor issues directly. Flag significant concerns to the user.

## Step 3 — Generate PR Description

Produce a pull request description:

```markdown
## Summary
{1-2 sentence description of the feature}

## PRD
artifacts/prds/{feature-name}.md

## Stories Completed
- [x] Story 1: {Title}
- [x] Story 2: {Title}

## Test Coverage
{Number of tests added or modified}. All passing.

## Review Notes
{Specific areas to review, decisions made, or trade-offs}
```

## Step 4 — Report

Tell the user the feature is ready to ship, and present the PR description.

Suggest: "Run `/neo-changelog` to generate a release notes entry."

## Step 4.5 — Update STATE.md

If `artifacts/STATE.md` exists, update it to reflect that the feature has shipped:

1. Set the current phase to `Ship`.
2. Add or update the ship date: `**Shipped:** {YYYY-MM-DD}`
3. If a features table exists (In-Flight features), move this feature from In-Flight to a `## Completed Features` section. If the section doesn't exist, create it.
4. Clear the `## Context Handoff` block (remove it entirely — it's no longer needed).
5. Clear the `## Blockers` section if all blockers belonged to this feature.
