---
description: 'Scan the codebase to extract conventions, patterns, and architecture'
mode: 'agent'
tools: ['read', 'search/codebase', 'runInTerminal']
---

# Neo Scan

Analyze the codebase and produce a conventions document that agents can reference for consistent implementation.

## Step 1 — Project Structure

Map the top-level directory structure. Identify:
- Source code locations (src/, lib/, app/, etc.)
- Test locations (tests/, __tests__/, *.test.*, *.spec.*)
- Configuration files (package.json, .csproj, tsconfig.json, etc.)
- Build system (npm, dotnet, make, etc.)

## Step 2 — Naming Conventions

Sample 5–10 representative files and extract:
- File naming (PascalCase, kebab-case, camelCase)
- Class/function naming patterns
- Test file naming and organization
- Directory naming patterns

## Step 3 — Code Patterns

Identify recurring patterns:
- Error handling style (try/catch, Result types, error codes)
- Dependency injection approach
- API endpoint structure and middleware
- State management approach (if frontend)
- Logging and observability patterns

## Step 4 — Dependencies

Read package manifests and note:
- Framework and runtime versions
- Key libraries and their roles
- Test framework in use
- Linter and formatter configuration

## Step 5 — Produce Conventions Doc

Save findings to `artifacts/architecture/conventions.md`:

```markdown
# Codebase Conventions

**Scanned:** {date}
**Project:** {name from package manifest}

## Project Structure
{Directory layout summary}

## Naming Conventions
- **Files:** {pattern}
- **Classes/Components:** {pattern}
- **Functions/Methods:** {pattern}
- **Tests:** {pattern}

## Code Patterns
- **Error handling:** {pattern}
- **DI/IoC:** {pattern}
- **API structure:** {pattern}
- **Logging:** {pattern}

## Tech Stack
- **Runtime:** {e.g., .NET 8, Node 20}
- **Framework:** {e.g., ASP.NET Core, Next.js}
- **Test framework:** {e.g., xUnit, Jest}
- **Linter:** {e.g., ESLint, dotnet format}

## Key Dependencies
| Package | Version | Role |
|---------|---------|------|
| {name}  | {ver}   | {purpose} |

## Build & Run
- **Build:** {command}
- **Test:** {command}
- **Lint:** {command}
```

## Step 6 — Report

Present a summary of the conventions found. Suggest: "Agents will now read `artifacts/architecture/conventions.md` when implementing stories to follow these patterns."
