---
description: "Use when working with Neo framework model types. Covers ModelBase, ValueObject, LookupBase, type decorators, validation rules, and data binding conventions."
applyTo: ["src/**/Models/**/*.ts", "src/**/Contracts/**/*.ts"]
---
# Neo Models & Data Types

## ModelBase (Domain Entities)

Tracked entities persisted to the server extend `ModelBase`.

```ts
import { ModelBase, Rules, Validation } from '@singularsystems/neo-core';

export default class MyEntity extends ModelBase {
    static typeName = "MyEntity";

    constructor() {
        super();
        this.makeObservable();
    }

    public myEntityId: number = 0;

    @Rules.StringLength(50)
    @Rules.Required()
    public name: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<MyEntity>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.name) {
            return "New entity";
        }
        return this.name;
    }
}
```

### Rules

- `static typeName` — must match the server-side type name
- Constructor calls `super()` then `this.makeObservable()`
- Always override `addBusinessRules` calling `super.addBusinessRules(rules)` first
- Always override `toString()` with a user-friendly label
- Any properties that will only be used client side (i.e. are not part of the c# class) should be added after the "client only properties" comment. These properties should also be decorated with `@Attributes.NoTracking()`.

## ValueObject (Commands / Criteria)

Untracked data objects with no primary key tracking. Commands and search criteria extend `ValueObject`.

```ts
import { Rules, Validation, ValueObject } from '@singularsystems/neo-core';

export default class UpsertMyEntityCommand extends ValueObject {
    constructor() {
        super();
        this.makeObservable();
    }

    public myEntityId: number = 0;

    @Rules.StringLength(50)
    @Rules.Required()
    public name: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<UpsertMyEntityCommand>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return "Upsert My Entity Command";
    }
}
```

## LookupBase (Read-Only Query Results)

Lookups extend `LookupBase` and use `readonly` properties. Call `this.makeBindable()` instead of `makeObservable()`.

```ts
import { List, LookupBase } from '@singularsystems/neo-core';

export default class MyEntityLookup extends LookupBase {
    constructor() {
        super();
        this.makeBindable();
    }

    public readonly myEntityId: number = 0;
    public readonly name: string = "";
    public readonly status: string = "";
    public childItems = new List(ChildItemLookup);
}
```

### Rules

- All scalar properties are `readonly`
- Child lists use `new List(LookupType)` (not readonly)
- Call `this.makeBindable()` in constructor (not `makeObservable`)
- No `addBusinessRules`, `toString`, or `static typeName` needed
- If writable properties are added to a lookup (e.g. an IsExpanded property), decorate them with `@Attributes.Observable()`.

### List is already an array

`List<T>` extends `Array<T>`. **Never call `.toArray()` on a `List` property** — it does not exist and will cause a runtime error. Iterate, map, filter, and spread `List` properties directly:

```ts
// ✅ Correct
this.items.map(i => i.name)
this.items.filter(i => i.isActive)
this.items.length

// ❌ Wrong — .toArray() does not exist on List
this.items.toArray().map(i => i.name)
```

## Enums

Simple TypeScript enums with numeric values:

```ts
export enum Priority { Low = 0, Medium = 1, High = 2 }
```

Decorate with display names using `EnumHelper`:

```ts
import { EnumHelper } from '@singularsystems/neo-core';

EnumHelper.decorateEnum(Priority, e => {
    e.describe(Priority.Low, "Low", "Not urgent");
    e.describe(Priority.Medium, "Medium", "Normal priority");
    e.describe(Priority.High, "High", "Requires immediate attention");
});
```

## Business Rules

Use `addBusinessRules` for validation:

```ts
protected static addBusinessRules(rules: Validation.Rules<MyEntity>) {
    super.addBusinessRules(rules);

    // Fail rule (blocks save)
    rules.failWhen(c => c.quantity <= 0, "Quantity must be greater than zero.");

    // Warning rule (does not block save)
    rules.warnWhen(c => c.quantity > 1000, "Quantity seems unusually high.");

    // Info rule
    rules.infoWhen(c => !c.description, "Consider adding a description.");

    // Rule on multiple properties (shows error on all listed properties)
    rules.failWhen(c => c.endDate < c.startDate, "End date must be after start date.")
        .onProperties(c => [c.startDate, c.endDate]);
}
```

## Type Decorators

Neo requires type decorators on properties for serialisation and UI component behaviour.

| C# Type             | TypeScript        | Decorator |
| ---                 | ---               | ---       |
| `int` (id property) | `number = 0`      | None required |
| `string`            | `string = ""`     | None required |
| `int`               | `number = 0`      | `@Attributes.Integer()` |
| `decimal`           | `number = 0`      | `@Attributes.Float()` |
| `DateTime`          | `Date`            | `@Attributes.Date()` |
| `date` (no time)    | `Date`            | `@Attributes.Date(Misc.TimeZoneFormat.None)` |
| Nested type         | `Type`            | `@Attributes.ChildObject(() => Type)` |
| Child list          | `new List(ChildType)` | `@Attributes.ChildObject(() => ChildType)` |
| `bool?`             | `boolean \| null` | `@Attributes.NullableBoolean()` |
| `int?`              | `number \| null`  | `@Attributes.Nullable()` + `@Attributes.Integer()` |

## Common Decorators

| Decorator                                  | Purpose |
| ---                                        | ---     |
| `@Rules.Required()`                        | Field is mandatory |
| `@Rules.StringLength(n)`                   | Max string length |
| `@Attributes.Date()`                       | Date-type property |
| `@Attributes.Nullable()`                   | Nullable numeric/enum |
| `@Attributes.Integer()`                    | Integer constraint |
| `@Attributes.Float()`                      | Float constraint |
| `@Attributes.ChildObject(() => Type)`            | Nested object (auto-deserialised) |
| `@Attributes.ChildObject(() => Type, true)`      | Nested object, always instantiated |
| `@Attributes.NullableBoolean()`            | Tri-state boolean |
| `@Attributes.Display("Label")`             | Custom display label |
| `@Attributes.NoTracking()`                 | Excluded from change tracking / serialisation |
| `@Attributes.OnChanged<T>(c => c.handler)` | Callback when value changes |
| `@NeoModel`                                | Class decorator for auto-observability (alternative to `makeObservable()`) |

## Related Skills

- [frontend-create-model](../skills/frontend-create-model/SKILL.md) — Create a new entity, command, or lookup model

## Key Conventions

- Initialise all properties with defaults (never leave `undefined`)
- Use `number | null = null` or `number | null = 0` depending on whether the field starts populated
- Use `string | null = null` for optional strings, `string = ""` for required
- `List(Type)` for collections (from `@singularsystems/neo-core`); when the collection contains nested models that must be deserialised, add `@Attributes.ChildObject(() => ChildType)` above the `new List(ChildType)` property.
