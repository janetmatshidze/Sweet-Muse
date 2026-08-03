---
name: backend-create-roles
description: Define new RBAC roles in a backend C# Roles class and wire them to endpoints
argument-hint: "<ResourceName> [category] [roles...]"
---

Use this skill when adding new authorization roles to a backend project. Roles are defined in `*Roles.cs` classes, auto-registered via reflection, and enforced via `[RequireRole]` attributes or `IAuthorisationService`.

## Steps

### 1. Open or create the roles file

Roles files live in `Models/Security/` and implement `IRoles` from `Neo.AuthorisationServer.Client`:

```csharp
// Models/Security/Roles.cs
using Neo.AuthorisationServer.Client;

public class Roles : IRoles
{
    /// <inheritdoc/>
    public string ResourceName => "Application";

    /// <inheritdoc/>
    public string DisplayName => "Application";

    /// <summary>
    /// Roles for My Entity management.
    /// </summary>
    public enum MyEntity
    {
        View,
        Edit,
    }
}
```

**Structure rules:**
- `ResourceName` groups roles in the authorisation server UI.
- Each enum is a **category**; each enum value is a **role**.
- XML doc comments on each enum are displayed in the authorisation UI — always include them.

### 2. Add new categories or values

Simply add enum types or values to the class. The reflection-based registration in `StartupExtensions` picks them up automatically — no further registration code is needed.

```csharp
/// <summary>
/// Roles for Reports.
/// </summary>
public enum Reports
{
    View,
    Export,
}
```

### 3. Enforce with `[RequireRole]` on controller actions

```csharp
[HttpPost("my-entity")]
[RequireRole(Roles.MyEntity.Edit)]
public async Task<MyEntityLookup> UpsertMyEntity([FromBody] UpsertMyEntityCommand command)
{
    return await this.myEntityService.UpsertAsync(command);
}

[HttpGet("my-entity")]
// No [RequireRole] → must be added to ActionConventionOptions.IgnoreActions, or use IsHumanUser default
public async Task<List<MyEntityLookup>> GetMyEntities()
{
    return await this.myEntityService.GetAllAsync();
}
```

- Any GET endpoint not requiring a role must be listed in `ActionConventionOptions.IgnoreActions`, otherwise startup will throw.

### 4. Enforce programmatically in services

```csharp
await this.authorisationService.AssertUserHasRoleAsync(Roles.MyEntity.Edit);
```

Use this for business-logic level checks where you need to authorize mid-method rather than at the API boundary.

### 5. Mirror on the frontend

After defining backend roles, create the matching TypeScript roles file. See [frontend-create-roles](../frontend-create-roles/SKILL.md).

## Quality checklist

- [ ] `*Roles.cs` implements `IRoles`
- [ ] `ResourceName` is consistent with other roles files in the project
- [ ] Each role enum has an XML doc comment
- [ ] `[RequireRole]` applied to all write endpoints
- [ ] GET endpoints either have `[RequireRole]` or are registered in `IgnoreActions`
- [ ] Frontend roles mirrored after backend changes
- [ ] No unused role values left in the enum
