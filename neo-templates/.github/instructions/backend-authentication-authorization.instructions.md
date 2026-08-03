---
applyTo: "**/*ActionConvention.cs,**/*Controller.cs,**/*Roles.cs"
---

# Authentication and Authorization Backend Implementation

Use these rules when implementing or modifying authentication and authorization backend functionality.

## Related Skills

- [backend-create-roles](../skills/backend-create-roles/SKILL.md) — Step-by-step: define backend RBAC roles and wire to endpoints
- [frontend-create-roles](../skills/frontend-create-roles/SKILL.md) — Mirror backend roles on the frontend

## Authentication

- Use an authentication action convention that implements `IActionModelConvention` and applies the appropriate authentication scheme(s) to API endpoints.
- This class should apply a default and strong policy to all endpoints that are not explicitly marked with their own policy.
- Therefore, policies should not need to be applied to individual endpoints unless they require a different scheme or set of schemes than the default.

## Authorization

- Neo provides an RBAC system that can be enforced in the system.
- Roles are defined in categories which are grouped into Resources.
- Roles must be defined in `*Roles.cs` classes which implement the `IRoles` interface (from `Neo.AuthorisationServer.Client`). These classes usually reside in the Models\Security folder.
- Each role category is defined as an enum in that class, and you can group as many roles as needed within that category, and as many categories as needed into the class.
- Example:

```csharp
public class Roles : IRoles
{
  /// <inheritdoc/>
  public string ResourceName => "Application";

  /// <inheritdoc/>
  public string DisplayName => "Application";

  /// <summary>
  /// Roles for Companies.
  /// </summary>
  public enum Companies
  {
    View,
    Edit,
  }

  /// <summary>
  /// Roles for Banks.
  /// </summary>
  public enum Banks
  {
    View,
    Edit,
  }

  /// <summary>
  /// Roles for Approver Level.
  /// </summary>
  public enum ApproverLevel
  {
    FirstApprover,
    SecondApprover,
    FinalApprover,
  }
}
```

- These role classes are then registered in the `StartupExtensions` class. There is some reflection code that automatically picks up all `IRoles` implementations and registers them.
- Roles can then be enforced in 2 ways:
  - Using the `RequireRole` attribute on controllers or endpoints passing in the role enum value (e.g. `[RequireRole(Roles.Companies.Edit)]`).
  - Using the `IAuthorisationService` from `Neo.AuthorisationServer.Client` in code to assert the current user has the required role. (e.g. `await authorisationService.AssertUserHasRoleAsync(Roles.Companies.Edit)`).
- The `IAuthorisationService` can also be used to check roles without throwing a 403.

## Client Side

Server side security should always be done first, then improve the user experience by preventing the user from performing actions they are not authorized to do in the client.

For client side instructions, see `frontend-authorization.instructions.md`.
