---
description: 'Capture a standalone Architectural Decision Record mid-sprint'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase']
---

# Neo ADR

Capture a single Architectural Decision Record without running the full architect workflow. Use this when a significant decision arises mid-sprint that needs to be documented and preserved.

## When to Use

Use for: technology choices, pattern selections, API design decisions, database schema decisions, security approach choices. For decisions that require full architectural review, use `/neo-architect` instead.

## Step 1 — Capture the Decision

Ask the user:
1. "What decision needs to be recorded?"
2. "What context drove this decision?"
3. "What alternatives were considered and why were they rejected?"
4. "What are the consequences or trade-offs?"

## Step 2 — Generate a Slug

Generate a kebab-cased slug from the decision title (e.g., `use-jwt-for-api-auth`).

## Step 3 — Write the ADR

Save to `artifacts/architecture/adr-{slug}.md` using this structure:

```markdown
# ADR: {Decision Title}

**Date:** {YYYY-MM-DD}
**Status:** Accepted
**Feature:** {feature-name or "cross-cutting"}

## Context
{Why a decision was needed. What forces are at play.}

## Decision
{What was decided. Be specific.}

## Consequences
{What becomes easier or harder. Trade-offs accepted. Follow-on work required.}

## Alternatives Considered
- **{Alternative}:** {Why it was rejected}
```

## Step 4 — Present and Confirm

Present the ADR and ask: "Does this accurately capture the decision and its rationale?"

After approval, note: "This ADR is now part of the project's decision log in `artifacts/architecture/`."
