---
applyTo: "**/Controllers/Catalogue/*.cs,**/CatalogueController.cs,**/Services/Catalogue/*.cs,**/CatalogueService.cs,**/CatalogueModelService.cs,**/*.Tests/**/*.cs"
---

# Catalogue Backend Implementation

Use these rules when implementing or modifying catalogue backend functionality.

Catalogue entities can be defined as entities that satisfy one or more of the following criteria:
- Simple lookup or configuration entities within the system.
- Used primarily as a lookup for other core system entities.
- Do not have child lists or child lists always loaded with parent, and other entity relationships only point to parent, not child.
- Do not have lifecycles, any field can be changed at any time.

## Related Skills

- [backend-add-catalogue-entity](../skills/backend-add-catalogue-entity/SKILL.md) — Step-by-step: add a catalogue entity end-to-end
- [ef-migrations](../skills/ef-migrations/SKILL.md) — Generate, remove, or roll back an EF Core migration

## Catalogue model setup

When adding a new catalogue type to a codebase (or extending an existing one), start by creating the model and getting it into the EF model.

- Keep catalogue models simple: typically an `{EntityName}Id` plus display/config fields (e.g. `Name`, `Code`) and standard audit fields.
- Add the entity to the main `ModelDbContext` as a `DbSet<TEntity>` with pluralized property name so it participates in migrations.
- Add/update an EF migration in the `*.Models.Migrations` project for the module.
- Prefer using existing base classes (audit/soft-delete/tenant-scoped/ModelBase) used elsewhere in the solution rather than inventing new ones.

## Catalogue API shape

### Basic catalogue entities

- Keep basic catalogue endpoints in `/Controllers/CatalogueController.cs`.
- Routes should use the form:
  - `GET /api/catalogue/{entity-route}`
  - `POST /api/catalogue/{entity-route}`
- Use typed `List<T>` request and response models for write operations.
- Use `CatalogueModelService` directly for basic catalogue reads and writes:
  - `this.catalogueService.GetListAsync<{EntityType}>();`
  - `this.catalogueService.SaveListAsync({ListOfEntitiesToSave});`
- Prefer explicit methods such as `GetCompanies()` and `UpdateCompanies(...)` instead of route-switch dispatch. Example:

```csharp
[Route("api/catalogue")]
[ApiController]
public class CatalogueController(CatalogueModelService catalogueService) : ControllerBase
{
  [HttpGet("companies")]
  public Task<List<Company>> GetCompanies()
  {
    return catalogueService.GetListAsync<Company>();
  }

  [HttpPost("companies")]
  [RequireRole(Roles.Catalogue.EditCompanies)]
  public Task<List<Company>> UpdateCompanies([FromBody] List<Company> companies)
  {
    return catalogueService.SaveListAsync(companies);
  }
}
```

## Authorization

- Catalogue reads are typically available to any authenticated user; only add read roles if there is a clear business requirement.
- Catalogue writes should require the relevant granular edit role, for example `Roles.Catalogue.EditYears` or `Roles.Catalogue.EditDeliveryTeams`.
- Prefer granular edit roles over a single broad edit role for all catalogue entities.

## Testing

- Use the existing `UnitTestHelper`'s `InitContext()` method to create an in-memory DbContext for tests.
- When saving tracked entities in tests, set `TrackingState` explicitly.

## Alignment goals

- Prefer explicit, per-entity action methods on catalogue controllers rather than generic or reflection-based dispatch.
- Prefer the repository CQRS-lite conventions for complex catalogue domains.
- Keep implementations explicit, typed, and easy to trace.
- Avoid reflection-heavy routing, generic endpoint dispatch, and unnecessary controller abstractions.
