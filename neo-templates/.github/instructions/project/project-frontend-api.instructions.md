---
description: "Use when creating API clients. Covers project-specific folder structure, naming conventions, and registration requirements."
applyTo: "src/**/ApiClients/**/*.ts"
---
# Project API Client Conventions

## Related Skills

- [frontend-create-api-client](../../skills/frontend-create-api-client/SKILL.md) — Create the API client
- [frontend-register-di-service](../../skills/frontend-register-di-service/SKILL.md) — Register it in DI

## Folder Structure

```
src/Domain/ApiClients/
└── {DomainArea}/
    ├── {Entity}QueryApiClient.ts    → GET operations
    └── {Entity}CommandApiClient.ts  → POST/PUT/DELETE operations
```

API clients are grouped by domain area under `src/Domain/ApiClients/`. Match the grouping pattern already established in this project.

## Naming Conventions

See `frontend-api-clients.instructions.md` for Query/Command client naming conventions. This project follows the same pattern.

## Registration

After creating an API client, it must be:

1. **Added to `DomainTypes.ts`** — create a `ServiceIdentifier` typed to the interface
2. **Bound in `DomainModule.ts`** — bind the concrete class in singleton scope

See the `project-di-modules` instruction file for details.
