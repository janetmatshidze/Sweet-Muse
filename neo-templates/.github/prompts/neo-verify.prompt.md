---
description: 'Verify before shipping — scale-aware checks covering build, tests, artifacts, code review, acceptance, and security'
mode: 'agent'
tools: ['read', 'edit', 'search/codebase', 'runInTerminal']
---

# Neo Verify

Single pre-ship gate that runs all relevant checks based on feature scale. Replaces the need to run separate check, validate, review, accept, and audit commands.

## Step 1 — Determine Scope and Scale

Read `artifacts/STATE.md` if it exists. Detect:
- **Active feature** and its **scale** (Small, Medium, Large)
- **Current phase** (should be Implementation or Ship)
- Whether story artifacts exist

If no STATE.md, infer from the current branch and artifacts present. Ask the user what to verify if ambiguous.

Determine what to run based on scale:

| Check | Small | Medium | Large |
|-------|-------|--------|-------|
| Build + Tests + Lint | Yes | Yes | Yes |
| Security audit | If sensitive | Yes | Yes |
| Artifact validation | Skip | Structural only | Full (structural + semantic) |
| Code review | Quick scan | Standard | Adversarial |
| Acceptance testing | Manual checklist | UAT walkthrough | Full UAT |
| Security review | Skip | If sensitive | Required |

## Step 2 — Build, Tests, Lint

Detect the build system:
- **Node.js** — `package.json` → `npm run build`, `npm test`, `npm run lint`
- **.NET** — `*.csproj` or `*.sln` → `dotnet build`, `dotnet test`, `dotnet format --verify-no-changes`
- **Other** — `Makefile`, `build.gradle`, etc.

If `artifacts/architecture/conventions.md` exists, read the Build & Run section for commands.

Run each check:
1. **Build** — compile/transpile the project
2. **Tests** — run the full test suite, capture pass/fail count and coverage if available
3. **Lint** — run the linter, capture errors (warnings are acceptable)
4. **Type check** — run type checking if applicable (e.g., `tsc --noEmit`)
5. **Security audit** — run `npm audit --audit-level=high` or equivalent; flag HIGH/CRITICAL

## Step 3 — Artifact Validation (Medium and Large only)

Skip for Small scale or if no artifacts exist.

### Medium — Structural checks only

For each artifact type, verify:
- **Briefs:** Required sections (Problem, Proposed Solution, Business Value)
- **PRDs:** Requirement IDs (`R-XX`), required sections, BDD format
- **Stories:** Requirement references, required sections
- **Story index:** File references exist, completeness, valid status values (`TODO`, `IN_PROGRESS`, `BLOCKED`, `DONE`)

### Large — Full validation (structural + semantic)

All Medium checks plus:
- **Must Have coverage** — every `R-XX` maps to at least one story
- **Won't Have enforcement** — Read the PRD's "Won't Have" section. For each Won't Have item:
  1. Extract key terms and concepts from the Won't Have description.
  2. Search story files for those terms (title, objective, acceptance criteria, implementation steps).
  3. Search changed code (`git diff --name-only main`) for filenames, class names, or comments matching those terms.
  4. If a Won't Have item appears to be implemented (in stories or code), flag it as a **MAJOR** finding:
     ```
     MAJOR: Won't Have item "{item}" appears to be implemented in {story/file}.
     ```
- **NFR traceability** — NFRs appear in architecture doc decisions
- **Story-Architecture coherence** — component alignment, API consistency, dependency alignment
- **Requirement coverage matrix** — build traceability showing coverage levels (FULL, PARTIAL, MISSING)

## Step 4 — Code Review

Identify changed files via `git diff --name-only main`.

### Small — Quick scan
Scan for: unused imports, debug logging, hardcoded secrets, unresolved TODOs.

### Medium — Standard review
Quick scan checks plus: error handling gaps, missing input validation, performance concerns (N+1 queries, unbounded collections).

### Large — Adversarial review
Standard review checks plus:
1. **Acceptance criteria coverage** — cross-reference story criteria against implementation
2. **Concurrency issues** — race conditions, missing locks, shared mutable state
3. **Security** — privilege escalation, insecure defaults, information leakage
4. **Cross-cutting concerns** — logging, observability, backward compatibility, deployment impact
5. **Architecture compliance** — read the architecture doc from `artifacts/architecture/`. Cross-reference key decisions against the implementation:
   - Database choice matches what's used in code (e.g., if architecture says PostgreSQL, code shouldn't use SQLite)
   - API patterns match (REST vs GraphQL, naming conventions, auth approach)
   - Dependencies used in code are listed in the architecture doc
   - Security approach follows architecture decisions (auth mechanism, encryption, etc.)
   - Flag deviations as **MAJOR** findings with the specific architecture decision that was violated

Report findings classified as:
- **CRITICAL** — Will cause a bug, security vulnerability, or data loss in production
- **MAJOR** — Significant gap that should be fixed before shipping
- **MINOR** — Improvement opportunity, acceptable to ship without fixing

## Step 5 — Acceptance Testing (Medium and Large)

Skip for Small scale.

Read story files and extract acceptance criteria. Present a test checklist:

```markdown
## UAT Checklist — {Feature Name}

### Story 1: {Title}
- [ ] **UAT-1:** {Testable criterion} — **Steps:** {How to verify}
- [ ] **UAT-2:** {Testable criterion} — **Steps:** {How to verify}
```

Walk the user through each test case. For failures, diagnose and offer to fix.

## Step 6 — Security Review (Large always, Medium if sensitive)

Skip for Small scale unless the feature handles auth, user input, or sensitive data.

Scan changed files for:
- Unsafe operations (SQL concatenation, `eval()`, `innerHTML`, etc.)
- Hardcoded secrets
- Auth/authz enforcement
- Input validation at system boundaries

If Semgrep is installed, run: `semgrep scan --config auto --error` against changed files.

If an architecture doc exists, verify security decisions were followed.

Produce a security sign-off checklist and save to `artifacts/architecture/security-review-{feature-name}.md`.

## Step 7 — Report

Present a unified report:

```markdown
## Verification Report — {Feature Name}

**Scale:** {Small / Medium / Large}
**Date:** {today}

| Check | Status | Details |
|-------|--------|---------|
| Build | PASS/FAIL | {summary} |
| Tests | PASS/FAIL | {X passed, Y failed, Z% coverage} |
| Lint | PASS/FAIL | {error count} |
| Type check | PASS/FAIL | {error count} |
| Security audit | PASS/FAIL | {HIGH/CRITICAL count} |
| Artifacts | PASS/SKIP | {error/warning count} |
| Code review | PASS/FAIL | {critical} critical, {major} major, {minor} minor |
| Acceptance | PASS/SKIP | {passed}/{total} test cases |
| Security review | PASS/SKIP | {finding count} |

**Overall: PASS / FAIL**
```

## Step 8 — Fix Plan

If any check failed, produce a numbered fix plan:
1. {What failed and why}
2. {Suggested fix}
3. {Command to re-verify}

Offer to implement fixes automatically.

## Step 9 — Update State and Suggest Next Action

If `artifacts/STATE.md` exists, record the verification result using this exact format:

```markdown
## Verification
**Date:** {YYYY-MM-DD}
**Result:** PASS / FAIL
**Scale:** {scale}
**Checks:** Build {✓/✗}, Tests {✓/✗}, Lint {✓/✗}, Security {✓/✗}, Artifacts {✓/SKIP}, Code Review {✓/✗}, Acceptance {✓/SKIP}, Security Review {✓/SKIP}
```

This block is read by `/neo-ship` to confirm verification was completed. If a `## Verification` block already exists in STATE.md, replace it with the new result.

- If all checks pass: "Verification passed. Run `/neo-ship` to prepare the PR."
- If checks failed: "Fix the issues above, then re-run `/neo-verify`."
