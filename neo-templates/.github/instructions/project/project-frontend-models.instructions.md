---
description: "Use when creating or editing domain models, value objects, lookups, or contracts. Covers the project base class hierarchy, file organisation, and naming conventions."
applyTo: ["src/Domain/Models/**/*.ts", "src/Domain/Contracts/**/*.ts", "src/Identity/Models/**/*.ts"]
---
# Project Models & Contracts

## Related Skills

- [frontend-create-model](../../skills/frontend-create-model/SKILL.md) — Create a model using the correct project base class

## Project Base Class Hierarchy

This project defines custom audit base classes in `src/Domain/Models/Base/`. Before creating a new entity, read the files in that folder to understand the available base classes and their intended use cases (the class names and JSDoc/comments describe when each should be used).

> Do not extend `ModelBase` directly for domain entities — always use the appropriate project base class from `src/Domain/Models/Base/`.

## Domain Entity Example

Entities extend the appropriate project base class:

```ts
import { Rules, Validation } from '@singularsystems/neo-core';
import {ProjectBaseClass} from '../Base/{ProjectBaseClass}';

export default class {EntityName} extends {ProjectBaseClass} {
    static typeName = "{EntityName}";

    constructor() {
        super();
        this.makeObservable();
    }

    public {entityName}Id: number = 0;

    @Rules.StringLength(100)
    @Rules.Required()
    public name: string = "";

    protected static addBusinessRules(rules: Validation.Rules<{EntityName}>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return this.isNew ? "New {entity name}" : this.name;
    }
}
```

## File Organisation

```
src/Domain/
├── Models/           → Domain entities (tracked by server)
│   ├── Base/         → Project-specific audit base classes
│   ├── {DomainArea}/ → Entity models grouped by domain area
│   └── ...
├── Contracts/        → Commands, criteria, lookups (DTOs)
│   └── {DomainArea}/
│       └── {SubArea}/
│           ├── Commands/   → UpsertXCommand, DeleteXCommand, etc.
│           └── Lookups/    → XLookup (read-only)
```

Match the folder depth and grouping already used in this project.

## Naming Conventions

- Commands: `Upsert{Entity}Command`, `Delete{Entity}Command`
- Lookups: `{Entity}Lookup`
- Criteria: `{BaseLookup}Criteria` (e.g., `OrderLookupCriteria` for `OrderLookup`)
- Enums: PascalCase, numeric values starting at 1
