---
description: 'Fast-path for urgent patches — skip planning, fix immediately'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal', 'githubRepo']
---

# Neo Hotfix

Bypass the planning pipeline for urgent patches. No story files, no index — just fix, test, and ship.

## When to Use

Use this for: bug fixes, security patches, configuration corrections, and small changes where the fix is already understood. If the fix touches **more than 3 files**, stop and suggest: "This looks larger than a hotfix. Consider `/neo-plan` for a proper planning pass."

## Step 1 — Describe the Fix

Ask the user: "Describe the fix in 2–3 sentences: what is broken, what the correct behaviour is, and which file(s) are affected."

Scan the codebase to confirm the affected files and understand the surrounding code.

## Step 2 — Implement

1. Create a branch named `hotfix/{name}` (kebab-cased from the description).
2. Implement the fix.
3. Write or update at least one test covering the broken case.
4. Run the build and test suite. Fix any failures.
5. Run the security checklist (no hardcoded secrets, inputs validated, no PII in logs, auth enforced).

## Step 3 — Report

Report completion:

```
**Hotfix:** {Fix Title}
**Branch:** hotfix/{name}

**Files changed:**
- `{path}` — {what changed}

**Tests:** {count} added/updated, all passing
**Security checklist:** Passed

**Next:** Run `/neo-ship` to review and prepare a PR, or merge directly if pre-approved.
```
