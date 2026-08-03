# Skills Index

Agent Skills are granular, step-by-step playbooks invoked on demand. They complement the instruction files, which contain always-applicable rules and conventions.

## Template skills (general patterns)

These skills apply to any project using the Neo stack.

| Skill | Description |
|---|---|
| [backend-add-catalogue-entity](backend-add-catalogue-entity/SKILL.md) | Add a new catalogue entity end-to-end (model → migration → controller → role) |
| [backend-create-roles](backend-create-roles/SKILL.md) | Define new RBAC roles in a backend Roles class |
| [backend-db-script-runner](backend-db-script-runner/SKILL.md) | Create and submit an auditable production SQL data fix |
| [background-worker](background-worker/SKILL.md) | Implement a long-running task as a Neo background worker (offloads slow work from HTTP, optionally pushes results via SignalR) |
| [ef-migrations](ef-migrations/SKILL.md) | Generate, remove, or roll back an EF Core database migration |
| [encrypt-field](encrypt-field/SKILL.md) | Guides implementation of SQL Server Always Encrypted for entity fields |
| [frontend-add-catalogue-entry](frontend-add-catalogue-entry/SKILL.md) | Add a new catalogue entry end-to-end on the frontend (model, API client, data cache, entry class, route) |
| [frontend-add-route](frontend-add-route/SKILL.md) | Add a new page route or sidebar menu item |
| [frontend-create-api-client](frontend-create-api-client/SKILL.md) | Create a typed query or command API client |
| [frontend-create-model](frontend-create-model/SKILL.md) | Create a domain entity, command, criteria, or lookup model |
| [frontend-create-roles](frontend-create-roles/SKILL.md) | Mirror backend roles as TypeScript string enums |
| [frontend-create-view](frontend-create-view/SKILL.md) | Create a new Neo MVVM View and ViewModel pair |
| [frontend-register-di-service](frontend-register-di-service/SKILL.md) | Register a new API client or service in the DI container |
| [grill-me](grill-me/SKILL.md) | Interview the user relentlessly about a plan or design until reaching shared understanding |
| [neo-context](neo-context/SKILL.md) | Locate and load Neo framework source for API lookup and usage pattern research |
| [neo-paged-grid](neo-paged-grid/SKILL.md) | Full vertical slice: paged data grid with search (backend Criteria/Lookup/Service/Controller + frontend Criteria/Lookup/API client/ViewModel/View) |

---

## When to use skills vs instructions

- **Instructions** (`.instructions.md`) contain always-applicable rules, conventions, and guardrails. Copilot applies them automatically based on `applyTo` globs.
- **Skills** (`SKILL.md`) are invoked on demand for specific tasks. Invoke a skill by referencing it: `@workspace /skill frontend-create-view`.
