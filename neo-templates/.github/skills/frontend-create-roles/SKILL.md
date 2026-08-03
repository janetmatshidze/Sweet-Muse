---
name: frontend-create-roles
description: Mirror backend RBAC roles in a TypeScript enum file for route guards and UI visibility
argument-hint: "<ResourceName> [path to backend Roles.cs]"
---

Use this skill after backend roles have been defined (see [backend-create-roles](../backend-create-roles/SKILL.md)). Frontend roles are string enums that mirror the backend `IRoles` structure and are used for route access control and UI visibility checks.

## Steps

### 1. Locate or create the frontend roles file

The file mirrors the location of the backend roles class:

| Backend | Frontend |
|---|---|
| `Server/.../Models/Security/Roles.cs` | `src/Domain/Models/Security/Roles.ts` |
| `Server/.../Identity/Models/Security/Roles.cs` | `src/Identity/Models/Security/Roles.ts` |

### 2. Define the roles as string enums

Each backend enum category becomes an exported TypeScript `enum`. Each value is the fully-qualified role string:

```
"{ResourceName}.{Category Name}.{Role Name}"
```

Humanize the C# PascalCase names: `ApproverLevel` → `"Approver Level"`, `FirstApprover` → `"First Approver"`.

**Example:**

```ts
// src/Domain/Models/Security/Roles.ts

// Backend: public class Roles : IRoles { ResourceName => "Application"; }

export enum Companies {
    View = "Application.Companies.View",
    Edit = "Application.Companies.Edit",
}

export enum Reports {
    View = "Application.Reports.View",
    Export = "Application.Reports.Export",
}

export enum ApproverLevel {
    FirstApprover = "Application.Approver Level.First Approver",
    SecondApprover = "Application.Approver Level.Second Approver",
    FinalApprover = "Application.Approver Level.Final Approver",
}
```

> **Tip:** You can generate this file automatically using the Neo VS Code extension's TSScaffold command.

### 3. Apply to routes

```ts
// DomainRoutes.ts
import * as Roles from '../Domain/Models/Security/Roles';

{ name: "Companies", path: "/companies", component: CompaniesView, role: Roles.Companies.View }
```

### 4. Apply in Views for UI visibility

```ts
// In a View or ViewModel
import * as Roles from '../../Domain/Models/Security/Roles';

private authorisationService = AppService.get(Types.Neo.Security.AuthorisationService);

public get canEdit() {
    return this.authorisationService.hasRole(Roles.Companies.Edit);
}
```

```tsx
{this.viewModel.canEdit && (
    <Neo.Button icon="edit" text="Edit" onClick={() => this.viewModel.startEdit()} />
)}
```

## Quality checklist

- [ ] One exported enum per backend role category
- [ ] Enum values follow `"Resource.Category Name.Role Name"` format (humanized PascalCase)
- [ ] File location mirrors backend `Models/Security/` structure
- [ ] Route `role` props updated for any new view routes
- [ ] UI elements conditionally rendered using `hasRole()` where appropriate
- [ ] No hardcoded role strings in components — always import from `Roles.ts`
