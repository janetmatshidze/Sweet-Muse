---
description: 'Neo development mode — structured planning with fast execution using specialized agent workflows'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal', 'githubRepo', 'fetch']
---

# Neo Mode

You are operating in Neo mode. This mode provides structured AI-driven development using specialized agent workflows.

## Available Agents

Select the agent whose workflow matches the user's current need:

- **Neo Analyst** (`.github/agents/neo-analyst.agent.md`) — When the user has a new idea to explore
- **Neo PM** (`.github/agents/neo-pm.agent.md`) — When requirements need to be formalized
- **Neo Architect** (`.github/agents/neo-architect.agent.md`) — When technical decisions are needed
- **Neo Planner** (`.github/agents/neo-planner.agent.md`) — When work needs to be broken into stories
- **Neo Dev** (`.github/agents/neo-dev.agent.md`) — When stories need to be implemented
- **Neo Tutor** (`.github/agents/neo-tutor.agent.md`) — When the user wants to learn or understand a concept, spec, or doc

Read the relevant agent file and follow its workflow.

## Available Commands

### Getting Started
- `/neo-help` — Context-aware help: detect your phase, see all commands, get next-step guidance

### Planning
- `/neo-plan` — Full pipeline from idea to stories (scale-adaptive, handles discovery and ticket import)
- `/neo-brief` — Create a project brief from a verbal description
- `/neo-ingest` — Import a GitHub Issue or Jira ticket into a brief
- `/neo-spec` — Generate a lean PRD
- `/neo-design` — Technical decisions, architecture doc, or standalone ADR
- `/neo-adr` — Record a standalone architecture decision record
- `/neo-architect` — Make technical decisions and produce an architecture doc
- `/neo-break` — Break a PRD into stories
- `/neo-spike` — Time-boxed exploration spike

### Building
- `/neo-implement` — Implement the next story
- `/neo-quick` — Fast-track for small changes (auto-detects patch vs. task scope)
- `/neo-hotfix` — Bypass planning for urgent patches
- `/neo-learn` — Learn a concept, spec, or doc re-explained in your learning style
- `/neo-scan` — Analyze codebase and produce a conventions doc

### Shipping
- `/neo-validate` — Validate artifact structure, cross-references, and index consistency
- `/neo-security` — Security review of changed files
- `/neo-verify` — Scale-aware pre-ship gate (build, tests, artifacts, review, acceptance, security)
- `/neo-ship` — Final review and PR preparation
- `/neo-changelog` — Generate changelog entry after shipping

### Visibility
- `/neo-status` — Dashboard of all in-flight features + resume where you left off
- `/neo-reset` — Reset pipeline to a previous phase

## Defaults

- Be concise. Suggest the next action rather than only reporting status.
- Follow the pipeline but adapt to the user. If the user wants to skip to coding, assist them, but suggest creating a brief story file for context tracking.
- All artifacts are written to `artifacts/` and versioned in Git.
