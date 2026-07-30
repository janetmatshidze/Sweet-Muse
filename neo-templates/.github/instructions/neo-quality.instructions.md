---
applyTo: '**/*.cs,**/*.ts,**/*.tsx,**/*.js,**/*.jsx'
description: 'Quality gates for source code — lean but non-negotiable standards'
---

# Quality Gates

All source code must meet these standards before a story is marked as done.

## Required

1. **Functional.** The code does what the story specifies. Happy path and documented edge cases are handled.
2. **Tested.** New functions have unit tests covering the happy path and at least one error case. Test names describe the expected behavior. External calls are mocked.
3. **Builds cleanly.** No compilation errors, no type errors, no unresolved imports.
4. **Lint clean.** No linter errors. Warnings are acceptable but should be addressed when practical.
5. **No artifacts left behind.** No debug logging, no commented-out code, no unused imports, no hardcoded secrets or credentials, no unresolved `TODO` or `FIXME` comments.
6. **Follows conventions.** Matches the existing codebase in naming, file structure, error handling, and formatting. New dependencies must be listed in the architecture doc.
7. **Security.** No hardcoded secrets. All user inputs validated. No PII in logs. Auth/authz enforced on protected paths. New dependencies audited (`dotnet list package --vulnerable` for C#, `npm audit` for JS/TS) — HIGH/CRITICAL issues resolved before merge.
8. **Documentation.** Public APIs and exported functions have JSDoc or docstrings. If a public interface changed, its documentation is updated.
9. **Observability.** Structured logging on all error paths. No silent failures. New user-facing features have at least one instrumented metric or trace point.

## Strongly Recommended (not blockers)

- 80% coverage for new code
- Contract tests for external APIs
- Performance baseline verified if the story touches a hot path

## Not Required to Ship

- 100% test coverage
- Comprehensive prose documentation
- Performance optimization of adjacent code
- Refactoring of unrelated code
