# Project Security Model

## User Types

The `user_type` JWT claim (defined in `{Project}ClaimTypes.UserType`) identifies the category of user. Two human user types exist:

| Value | Enum | Description |
|-------|------|-------------|
| `User` (1) | `UserType.User` | Every non-platform user — farmers, agronomists, back-office admins, drone pilots, etc. Module access is governed by RBAC roles. |
| `PlatformAdministrator` (3) | `UserType.PlatformAdministrator` | Can administer all tenants, add new tenants, and automatically has access to every tenant. Their roles within each tenant can still be customized. |

Service accounts (machine-to-machine OAuth clients) currently carry **no** `user_type` claim at all.

---

## Authorization Policies

Policies are registered in `Startup.cs` via `serviceCollection.AddAuthorization(...)`.

### `IsHumanUser` (defined in `Policies.IsHumanUser`)

Requires an authenticated user **and** a `user_type` claim with value `User` or `PlatformAdministrator`.  
Registered by `options.Add{Project}AuthorisationPolicies()` (in `AuthorizationOptionsExtensions`).

This is the **default policy** applied to all endpoints in both the Domain API and the IdentityServer API, unless there is another Auth Filter applied (e.g. `AllowAnonymous` or `Authorize(Policies.IsService)`).

### `IsService`

A machine-to-machine policy for service-to-service calls authenticated with client-credential OAuth tokens (no `user_type` claim).  
Registered by `options.AddIsServicePolicy(serviceCollection)` (Neo framework extension). Used sparingly for internal service endpoints that must not be callable by human users (e.g. `UserManagementController.FindInvitedUser` when called from the Domain API).

### `LocalApi.PolicyName` (OpenIddict)

An OpenIddict built-in policy that validates tokens issued to local clients by this IdentityServer instance.  
Used at the controller level on the IdentityServer's own API controllers (`UserManagementController`, `MultiTenancyController`, `IdentityProviderController`) so that only callers with a valid local access token can invoke them.

> **Important**: OpenIddict's own endpoints (`/connect/token`, `/connect/authorize`, etc.) manage their own security and must **not** be covered by an ASP.NET Core `FallbackPolicy`. The `IdentityApiActionConvention` scopes itself to `api/`-prefixed routes only to avoid interfering with these endpoints.

---

## Action Conventions

For the general action convention pattern (applying default auth filters and enforcing `[RequireRole]`), see `backend-authentication-authorization.instructions.md`.

Project-specific conventions:

1. The base logic lives in `ActionConventionBase` (`Server/Core/{Project}.Core/Security/ActionConventionBase.cs`).
2. **Enforce `[RequireRole]`** — any action that is not explicitly exempted, anonymous, or using an alternative `[Authorize]` attribute **must** carry a `[RequireRole(...)]` attribute. Violations throw an `InvalidOperationException` at startup, preventing accidental deployment of unprotected endpoints.
3. Where a role is not required (e.g., **GET** Catalogue endpoints), register the action on `ActionConventionOptions` using `IgnoreActions`.

---

## RBAC Roles

Roles are defined as `IRoles` implementations (see `backend-authentication-authorization.instructions.md` for full conventions).

### Domain API — `AdminRoles` (resource: `"Administration"`)

**Location**: `{Project}.Core.Models/Security/AdminRoles.cs`

### Reporting — `{Project}.Reporting.Security.Roles` (resource: `"Reporting"`)

**Location**: `{Project}.Reporting/Security/Roles.cs`

### IdentityServer — `{Project}.IdentityServer.Models.Security.Roles` (resource: `"Identity"`)

**Location**: `{Project}.IdentityServer.Models/Models/Security/Roles.cs`

---

## Enforcement at a Glance

```
Request arrives
│
├─ OpenIddict endpoint (/connect/*)
│   └─ Handled entirely by OpenIddict — no ASP.NET Core policy applied
│
├─ Razor Page (Identity UI)
│   └─ Cookie authentication — no action convention applied
│
└─ API controller (route starts with api/)
    │
    ├─ [AllowAnonymous] on action or controller → no auth required
    │
    ├─ [Authorize(Policy = "...")] on controller (e.g. LocalApi) → that policy enforced
    │   └─ Action must still have [RequireRole] unless in IgnoreActions list
    │
    └─ No explicit auth → convention injects AuthorizeFilter(IsHumanUser)
        └─ Action must have [RequireRole(...)] unless in IgnoreActions list
            └─ Missing [RequireRole] → InvalidOperationException at startup
```

