---
description: 'Pre-ship security review — scan for vulnerabilities, validate auth, produce sign-off checklist'
mode: 'agent'
tools: ['read', 'search/codebase', 'runInTerminal']
---

# Neo Security

Perform a security review before shipping. This is a required step for any feature that handles user input, authentication, or sensitive data.

## Step 1 — Scope

Identify changed files: run `git diff --name-only main` (or the appropriate base branch). If no feature is specified, ask the user which feature to review.

## Step 2 — Static Scan

Run the security checklist below. Additionally scan changed files for:

- **Unsafe operations** — SQL string concatenation, `eval()`, `exec()`, `shell=True`, `innerHTML`, etc.

### SAST Tool Scan (if available)

If Semgrep is installed, run it against changed files for automated static analysis:

```bash
# Run Semgrep with auto-detected rules for the project's languages
semgrep scan --config auto --error --changed-files $(git diff --name-only main)
```

If Semgrep is not installed, note this in the report:
> "Automated SAST scan skipped — Semgrep not installed. Install with `pip install semgrep` or `brew install semgrep` for automated vulnerability detection."

Report any Semgrep findings alongside the manual review. Semgrep complements but does not replace the manual checklist review.

## Step 3 — Architecture Review

Read the architecture doc (`artifacts/architecture/{feature-name}.md`) if it exists. Verify:
- The security architecture section was followed
- Auth/authz mechanism matches what was designed
- Data classification is respected (Confidential/Restricted data is encrypted in transit and at rest)

## Step 4 — Produce Sign-Off Checklist

Save the sign-off checklist to `artifacts/architecture/security-review-{feature-name}.md` and present it to the user:

```markdown
## Security Review: {Feature Name}

**Date:** {today}
**Reviewer:** Neo Security Agent

| Check | Status | Notes |
|-------|--------|-------|
| No hardcoded secrets | ✓ / ✗ | {finding or "clear"} |
| User inputs validated | ✓ / ✗ | {finding or "clear"} |
| Output encoding applied | ✓ / ✗ | {finding or "clear"} |
| No PII in logs | ✓ / ✗ | {finding or "clear"} |
| Auth/authz enforced | ✓ / ✗ | {finding or "clear"} |
| Session management secure | ✓ / ✗ | {finding or "clear"} |
| CSRF protection enabled | ✓ / ✗ | {finding or "clear"} |
| Rate limiting applied | ✓ / ✗ | {finding or "clear"} |
| CORS configured correctly | ✓ / ✗ | {finding or "clear"} |
| Security headers set | ✓ / ✗ | {finding or "clear"} |
| Dependencies audited | ✓ / ✗ | {finding or "clear"} |
| No unsafe operations | ✓ / ✗ | {finding or "clear"} |
| SAST scan (Semgrep) | ✓ / ✗ / skipped | {finding or "clear" or "not installed"} |
| Architecture followed | ✓ / ✗ | {finding or "clear"} |

**Overall:** PASS / FAIL — {1-sentence summary}
```

If any check fails, describe the finding and suggest the fix. Do not proceed to ship until all checks pass.

## Step 5 — Handoff

After all checks pass, say: "Security review passed. Run `/neo-ship` to complete the pre-ship checklist."
