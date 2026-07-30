---
description: 'Help with Neo — detect your phase, see all commands, get next-step guidance'
mode: 'agent'
tools: ['read', 'search/codebase']
---

# Neo Help

Detect the user's current pipeline phase, list all available commands and agents, and suggest the most relevant next action.

## Step 1 — Read Project State

Scan the project for existing artifacts:

- `artifacts/STATE.md` — authoritative phase tracker (if present)
- `artifacts/briefs/` — discovery briefs
- `artifacts/prds/` — product requirement docs
- `artifacts/architecture/` — architecture docs and conventions
- `artifacts/stories/` — story files and index files

Note which directories contain files (ignore `.gitkeep`).

## Step 2 — Determine Current Phase

Infer the pipeline phase from artifacts found. Use `STATE.md` as the authoritative source when present. Otherwise, determine phase from the highest artifact tier that exists:

| Artifacts Present | Phase |
|-------------------|-------|
| None | Not Started |
| Briefs only | Discovery |
| Briefs + PRDs | Requirements |
| Briefs + PRDs + Architecture | Architecture |
| Stories exist (any status) | Implementation |
| All stories DONE + shipped | Ship |

## Step 3 — Handle User Question

If the user asked a specific question (e.g., "what do I do after stories?", "how do I start?", "I just finished the architecture"), answer that question directly using knowledge of the pipeline and available commands. Skip the full listing unless the user asked for it or asked a general "help" question.

## Step 4 — Decision Tree

Present a quick decision tree to help the user find the right command:

```
What do you want to do?

New feature (any size)     → /neo-plan
Small change (≤10 files)   → /neo-quick
Have stories to implement  → /neo-implement
Ready to ship              → /neo-verify → /neo-ship
Where was I?               → /neo-status
Need help deciding         → Keep reading ↓
```

## Step 5 — Display Help

Print the following sections:

### Current Status

> **Phase:** {detected phase} | **Feature:** {active feature name or "none detected"}

### Pipeline

```
Idea → Brief → PRD → Architecture → Stories → Implementation → Ship
      [analyst] [pm]  [architect]    [planner]      [dev]         [ship]
      ├──────────── PLAN ────────────┤├────────── BUILD ──────────┤
                                                          ▲ You are here
```

Place the "▲ You are here" indicator under the detected phase.

### Commands (22)

**Getting Started**
- `/neo-help` — Context-aware help (this command)

**Planning**
- `/neo-plan` — Full pipeline: idea → stories in one session (scale-adaptive, handles discovery and ticket import)
- `/neo-brief` — Create a project brief from a verbal description
- `/neo-ingest` — Import a GitHub Issue or Jira ticket into a brief
- `/neo-spec` — Generate a lean PRD from a brief
- `/neo-design` — Technical decisions, architecture doc, or standalone ADR
- `/neo-adr` — Record a standalone architecture decision record
- `/neo-architect` — Make technical decisions and produce an architecture doc
- `/neo-break` — Break a PRD into implementation-ready stories
- `/neo-spike` — Time-boxed exploration spike

**Building**
- `/neo-implement` — Implement the next story (wave-aware)
- `/neo-quick` — Fast-track for small changes (auto-detects patch vs. task scope)
- `/neo-hotfix` — Bypass planning for urgent patches
- `/neo-learn` — Learn a concept, spec, or doc re-explained in your personal learning style
- `/neo-scan` — Analyze codebase and produce a conventions doc

**Shipping**
- `/neo-validate` — Validate artifact structure, cross-references, and index consistency
- `/neo-security` — Security review of changed files
- `/neo-verify` — Scale-aware pre-ship gate (build, tests, artifacts, code review, acceptance, security)
- `/neo-ship` — Final review + PR description
- `/neo-changelog` — Generate changelog entry after shipping

**Visibility**
- `/neo-status` — Dashboard of all in-flight features + resume where you left off
- `/neo-reset` — Reset pipeline to a previous phase

### Agents

| Agent | Role |
|-------|------|
| `@neo-analyst` | Requirements discovery → project brief |
| `@neo-pm` | Brief → lean PRD with explicit scope |
| `@neo-architect` | PRD → technical decisions + architecture doc |
| `@neo-planner` | PRD + architecture → self-contained story files |
| `@neo-dev` | Story file → tested, working code |
| `@neo-tutor` | Concept, spec, or doc → re-explained in your learning style |

## Step 6 — Suggest Next Action

Based on the detected phase, recommend the single most relevant command:

| Phase | Suggestion |
|-------|-----------|
| Not Started | "Run `/neo-plan` to start a new feature." |
| Discovery | "Brief exists. Run `/neo-spec` to produce a lean PRD." |
| Requirements | "PRD exists. Run `/neo-design` to make technical decisions, or `/neo-break` to skip straight to stories (small/medium features)." |
| Architecture | "Architecture doc exists. Run `/neo-break` to break it into implementation-ready stories." |
| Implementation | "Stories exist. Run `/neo-implement` to implement the next TODO story, or `/neo-status` to see progress." |
| Ship | "All stories done. Run `/neo-verify` for pre-ship checks, then `/neo-ship` to prepare the PR." |

## Step 7 — Offer Follow-Up

End with:

> Have a specific question? Ask me anything about the pipeline, commands, or what to do next.
