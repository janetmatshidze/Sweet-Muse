---
name: backend-add-catalogue-entity
description: Add a new catalogue entity end-to-end — model, migration, controller endpoint, and roles
argument-hint: "<EntityName> [plural form]"
---

Use this skill when adding a new simple lookup or configuration entity to the catalogue. Catalogue entities are simple (no child lists, no lifecycle constraints), used as lookups, and managed through `CatalogueController` with `CatalogueModelService`.

## When this skill applies

An entity is a catalogue entity if it:
- Is a simple lookup or configuration table (e.g. crop varieties, delivery teams, years)
- Is used as a lookup reference by other entities
- Has no complex child relationships or lifecycle
- Can have any field changed at any time

## Steps

### 1. Create the model

Create the entity class in the appropriate `*.Models` project:

```csharp
// Models/Catalogue/MyEntity.cs
public class MyEntity : /* appropriate audit base class from the project */
{
    public int MyEntityId { get; set; }

    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    // Add display/config fields as needed
}
```

Choose the audit base class consistent with others in the solution (soft-delete, temporal, simple audit, etc.).

### 2. Register in the DbContext

Add a `DbSet` with the pluralized property name to `ModelDbContext`:

```csharp
public DbSet<MyEntity> MyEntities { get; set; }
```

### 3. Add an EF migration

In the `*.Models.Migrations` project, add and apply a new migration:

```bash
dotnet ef migrations add AddMyEntity --project *.Models.Migrations
```

Review the generated migration to confirm it matches the intended schema.

### 4. Add endpoints to `CatalogueController`

Add explicit GET and POST methods — do not use generic dispatch:

```csharp
// Controllers/CatalogueController.cs
[HttpGet("my-entities")]
public Task<List<MyEntity>> GetMyEntities()
{
    return this.catalogueService.GetListAsync<MyEntity>();
}

[HttpPost("my-entities")]
[RequireRole(Roles.Catalogue.EditMyEntities)]
public Task<List<MyEntity>> UpdateMyEntities([FromBody] List<MyEntity> entities)
{
    return this.catalogueService.SaveListAsync(entities);
}
```

- Routes follow `GET /api/catalogue/{entity-route}` and `POST /api/catalogue/{entity-route}`.
- GET endpoints do not require a role unless there is a specific business requirement.

### 5. Define the edit role

Add a new value to the catalogue roles enum:

```csharp
public enum Catalogue
{
    // ... existing roles
    EditMyEntities,
}
```

### 6. Mirror the role on the frontend

See [frontend-create-roles](../frontend-create-roles/SKILL.md).

### 7. Wire up the frontend catalogue entry

See [frontend-add-catalogue-entry](../frontend-add-catalogue-entry/SKILL.md) for the full frontend walkthrough (model, API client, data cache, catalogue entry class, route registration).

## Quality checklist

- [ ] Model extends the appropriate audit base class used elsewhere in the project
- [ ] `DbSet` added with pluralized name
- [ ] Migration generated and reviewed
- [ ] GET endpoint: no role required (unless business reason)
- [ ] POST endpoint: granular edit role applied with `[RequireRole]`
- [ ] Role added to `Roles.cs` with XML doc comment
- [ ] Frontend role enum updated
- [ ] Use `UnitTestHelper.InitContext()` for in-memory DB in tests; set `TrackingState` when saving
