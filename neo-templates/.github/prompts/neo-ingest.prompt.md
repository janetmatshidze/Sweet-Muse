---
description: 'Ingest a GitHub Issue (or Jira ticket) and produce a populated Neo brief'
mode: 'agent'
tools: ['read', 'edit', 'fetch', 'githubRepo']
---

# Neo Ingest

Turn an existing ticket into a Neo project brief. This skips manual discovery when requirements already exist in your issue tracker.

## Step 1 — Identify the Source

Ask the user: "Paste a GitHub Issue URL (e.g. `https://github.com/org/repo/issues/42`)."

Detect the source:
- **GitHub Issue** (primary): URL matches `github.com/{owner}/{repo}/issues/{number}` — use the `githubRepo` tool to fetch the issue title, body, labels, and any checklist items.
- **Jira ticket** (if provided): use `fetch` to call `{baseUrl}/rest/api/3/issue/{ticketId}`. If credentials are missing, instruct the user: "Set a `JIRA_TOKEN` environment variable as a Bearer token, then retry."

## Step 2 — Extract Content

From the ticket, extract:
- **Title** → becomes the feature name (kebab-cased for file naming)
- **Description** → maps to Problem and Proposed Solution
- **Acceptance criteria / checklist items** → map to Constraints
- **Labels / components** → check for existing related code
- **Assignee / reporter** → maps to Target Users if relevant

## Step 3 — Scan the Codebase

Scan the repository for files and patterns related to the ticket's subject area. Note anything relevant under Existing Context.

## Step 4 — Write the Brief

Save to `artifacts/briefs/{feature-name}.md`:

```markdown
# Brief: {Feature Name}

**Date:** {today}
**Source:** {GitHub Issue URL or Jira ticket ID}

## Problem
{1–2 sentences from the ticket description.}

## Proposed Solution
{2–3 sentences. High-level approach inferred from the ticket.}

## Business Value
{Inferred from the ticket description, or add to Open Questions if unclear.}

## Target Users
{From ticket assignee, reporter, or description.}

## Stakeholders
- **Approver:** {From ticket assignee or add to Open Questions}
- **Affected teams:** {Inferred from labels/components}

## Constraints
- {Acceptance criteria item or label-derived constraint}

## NFR Signals
- **Performance:** {From ticket if mentioned, or "none identified"}
- **Security tier:** {Inferred from ticket content, default to Internal}
- **Compliance:** {GDPR: check if PII involved | Accessibility: check if UI}

## Initial Risks
- {Risks identified from the ticket or codebase scan}

## Existing Context
- Stack: {Technologies detected in codebase}
- Related: {Relevant files or modules found}
- Dependencies: {External services referenced in the ticket}

## Open Questions
- {Anything unclear in the ticket that the PM should resolve}
```

## Step 5 — Present and Confirm

Present the brief and ask: "Does this capture the ticket accurately? Any corrections before we move to PRD?"

After approval, suggest: "Run `/neo-spec` to produce a PRD from this brief."
