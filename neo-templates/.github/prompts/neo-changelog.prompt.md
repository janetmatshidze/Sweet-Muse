---
description: 'Generate a Keep a Changelog entry from completed stories and PR descriptions'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal', 'githubRepo']
---

# Neo Changelog

Generate a changelog entry for a shipped feature. Run this after `/neo-ship`.

## Step 1 — Identify the Feature

If the user did not specify a feature, list all story indexes in `artifacts/stories/` with status DONE and ask which one to document.

## Step 2 — Gather Information

Read:
- The story index: `artifacts/stories/{feature-name}-index.md`
- The PRD: `artifacts/prds/{feature-name}.md` (for the Solution summary)
- Run `git log --oneline main..HEAD` to list commits since branching (or equivalent)

## Step 3 — Determine Version Bump

Ask the user (or infer from the PRD):
- **Breaking change to a public API?** → MAJOR bump
- **New feature, backward compatible?** → MINOR bump
- **Bug fix or chore?** → PATCH bump

## Step 4 — Write the Changelog Entry

Following [Keep a Changelog](https://keepachangelog.com) format, prepend to `CHANGELOG.md` (create the file if it does not exist):

```markdown
## [{version}] — {YYYY-MM-DD}

### Added
- {User-facing description of new functionality} (#{story-number})

### Changed
- {Description of changed behavior} (#{story-number})

### Fixed
- {Description of bug fix} (#{story-number})

### Security
- {Security fix description, if any}
```

Only include sections that apply. Write from the user's perspective — what they can now do, not what the code does.

## Step 5 — Report

Present the changelog entry and confirm: "Does this accurately describe what shipped?"

After approval, commit the changelog update: `docs(changelog): add {version} release notes`.
