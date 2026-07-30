---
name: frontend-create-model
description: Create a Neo domain entity, command/criteria ValueObject, or read-only lookup
argument-hint: "<ModelName> <entity|command|criteria|lookup>"
---

Use this skill when adding a new TypeScript model that mirrors a C# server-side type. Choose the correct base class based on how the model is used: entities are tracked and persisted, commands/criteria are untracked request payloads, and lookups are read-only query results.

## Decision: which base class?

| Use case | Extend | Call in constructor |
|---|---|---|
| Domain entity (tracked, persisted) | `ModelBase` or project-specific base class | `makeObservable()` |
| Command / criteria (untracked DTO) | `ValueObject` | `makeObservable()` |
| Read-only query result | `LookupBase` | `makeBindable()` |

> For project-specific base class selection, check `src/Domain/Models/Base/` — see `project-frontend-models.instructions.md`.

## Steps

### 1. Entity (`ModelBase`)

```ts
import { ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class MyEntity extends ModelBase {
    static typeName = "MyEntity";   // must match C# class name

    constructor() {
        super();
        this.makeObservable();
    }

    public myEntityId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public name: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<MyEntity>) {
        super.addBusinessRules(rules);
        rules.failWhen(c => !c.name, "Name is required.");
    }

    public toString(): string {
        return this.isNew ? "New entity" : this.name;
    }
}
```

### 2. Command / Criteria (`ValueObject`)

```ts
import { ValueObject, Rules, Validation } from '@singularsystems/neo-core';

export default class UpsertMyEntityCommand extends ValueObject {
    constructor() {
        super();
        this.makeObservable();
    }

    public myEntityId: number = 0;

    @Rules.Required()
    @Rules.StringLength(100)
    public name: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<UpsertMyEntityCommand>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "Upsert My Entity";
    }
}
```

### 3. Lookup (`LookupBase`)

```ts
import { LookupBase } from '@singularsystems/neo-core';

export default class MyEntityLookup extends LookupBase {
    constructor() {
        super();
        this.makeBindable();
    }

    public readonly myEntityId: number = 0;
    public readonly name: string = "";
    public readonly statusId: number = 0;
}
```

### 4. Apply type decorators

Apply decorators for non-string, non-id properties:

| C# type | Decorator |
|---|---|
| `int` (non-id) | `@Attributes.Integer()` |
| `decimal` | `@Attributes.Float()` |
| `DateTime` | `@Attributes.Date()` |
| `bool?` | `@Attributes.NullableBoolean()` |
| `int?` | `@Attributes.Nullable()` + `@Attributes.Integer()` |
| Nested object | `@Attributes.ChildObject(Type)` (or `@Attributes.ChildObject(() => Type)` for forward references) |
| Child list | `@Attributes.ChildObject(ChildType)` (or `@Attributes.ChildObject(() => ChildType)`) above `new List(ChildType)` |
| Client-only property | `@Attributes.NoTracking()` |

### 5. Place in the correct folder

```
src/Domain/
├── Models/           ← domain entities
│   └── {DomainArea}/
└── Contracts/
    └── {DomainArea}/
        └── {SubArea}/
            ├── Commands/   ← UpsertXCommand, DeleteXCommand
            └── Lookups/    ← XLookup
```

### 6. Naming conventions

- Commands: `Upsert{Entity}Command`, `Delete{Entity}Command`
- Lookups: `{Entity}Lookup`
- Criteria: `{BaseLookup}Criteria` (e.g. `FarmLookupCriteria`)
- One class per file; filename matches class name; default export

## Quality checklist

- [ ] Correct base class chosen for the use case
- [ ] `static typeName` present on entities (matches C# class name)
- [ ] `makeObservable()` / `makeBindable()` called in constructor
- [ ] All properties initialized with defaults (no `undefined`)
- [ ] Type decorators applied to non-string/non-id properties
- [ ] Every `new List(ChildType)` property has `@Attributes.ChildObject(() => ChildType)` decorator
- [ ] `addBusinessRules` calls `super.addBusinessRules(rules)` first
- [ ] `toString()` returns a human-readable label
- [ ] Client-only properties decorated with `@Attributes.NoTracking()`
