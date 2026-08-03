---
description: 'Spike an unknown — time-boxed exploration to answer a question before committing'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'fetch', 'runInTerminal']
---

# Neo Spike

Run a time-boxed exploration spike. Use this when a question must be answered before a feature can be designed or estimated. Spikes do not ship code — they produce a learning artifact.

## When to Use

Use for: technology evaluation, proof-of-concept, performance benchmarking, API compatibility checks, architectural unknowns. For anything that ships production code, use `/neo-plan` instead.

## Step 1 — Define the Spike

Ask the user:
1. "What is the question this spike must answer?"
2. "What is the time box? (e.g., 2 hours, 1 day)"
3. "What does a useful answer look like?"

## Step 2 — Write the Spike Story

Generate a kebab-cased name from the question (e.g., `redis-cache-throughput`).

Save to `artifacts/stories/spike-{name}-1.md`:

```markdown
# Story: Spike — {Question}

**Feature:** spike-{name}
**Story:** 1 of 1
**Type:** spike
**Depends on:** none

## Objective
{The question to answer and why it matters.}

## Time Box
{Duration}

## Definition of Done
- [ ] Question answered with evidence
- [ ] Learning artifact saved
- [ ] Recommendation documented

## Investigation Steps
1. {Approach step}
2. {Approach step}

## Expected Output
{What a useful answer looks like — prototype code, benchmark numbers, compatibility verdict, etc.}
```

## Step 3 — Run the Investigation

Carry out the investigation within the time box. Document findings inline.

## Step 4 — Write the Learning Artifact

Save findings to `artifacts/briefs/spike-{name}.md`:

```markdown
# Spike: {Question}

**Date:** {today}
**Time spent:** {actual duration}

## Question
{The question being answered}

## Finding
{The answer, with evidence}

## Recommendation
{What to do next based on this finding}

## Artifacts
{Links to any prototype code, benchmark output, or reference material}
```

## Step 5 — Report

Present the learning artifact and suggest the next action based on the finding (e.g., "Run `/neo-plan` to start the feature" or "The spike revealed a blocker — add to Open Questions").
