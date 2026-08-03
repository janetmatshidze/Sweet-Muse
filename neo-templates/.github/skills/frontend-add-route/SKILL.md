---
name: frontend-add-route
description: Add a new page route or sidebar menu item to the application
argument-hint: "<ViewName> [path] [role]"
---

Use this skill when a new View needs to be accessible via the sidebar menu or as a non-menu route. Routes are defined in per-module `*Routes.ts` files and merged by `RouteService`.

## Steps

### 1. Create the View and ViewModel

If not already created, see [frontend-create-view](../frontend-create-view/SKILL.md).

### 2. Add the route to the module's routes file

Open the relevant `*Routes.ts` file (e.g. `src/Domain/DomainRoutes.ts`):

```ts
import MyEntityView from './Views/MyArea/MyEntityView';
import * as Roles from '../Domain/Models/Security/Roles';

const MenuRoutes: IAppMenuItem[] = [
    {
        name: "My Area",
        children: [
            {
                name: "My Entity",
                path: "/my-entity",
                icon: "list",
                component: MyEntityView,
                role: Roles.MyCategory.View,   // omit if publicly accessible to all users
            }
        ]
    },
];
```

**Route property reference:**

| Property | Purpose |
|---|---|
| `name` | Display name in sidebar |
| `path` | URL path |
| `component` | View class to render |
| `icon` | Material Symbols icon name |
| `exact` | Exact path match (use for `/`) |
| `allowAnonymous` | Skip authentication |
| `role` | Required role string |
| `children` | Nested items (creates collapsible group) |

### 3. Register as a non-menu (pure) route (optional)

For views that are navigated to programmatically (not shown in sidebar):

```ts
const PureRoutes: IAppRoute[] = [
    { path: "/my-entity/:entityId", component: MyEntityDetailView }
];
```

### 4. Import the routes file in `RouteService` (new modules only)

For existing modules (`DomainRoutes`, `ReportingRoutes`, etc.), the `RouteService` already spreads their routes — no change needed.

If you are creating a brand new module:

```ts
// src/App/Services/RouteService.ts
import * as NewModuleRoutes from '../NewModule/NewModuleRoutes';

private getMenuRoutes(): IAppMenuItem[] {
    return [
        ...DomainRoutes.MenuRoutes,
        ...NewModuleRoutes.MenuRoutes,   // ← add here
        this.getAdministrationRoute(),
    ];
}
```

### 5. Apply role-based access

```ts
import * as DomainRoles from '../Domain/Models/Security/Roles';

{ name: "Reports", path: "/reports", component: ReportsView, role: DomainRoles.Reporting.View }
```

Frontend roles should mirror backend roles. See [frontend-create-roles](../frontend-create-roles/SKILL.md) if roles don't exist yet.

### 6. Views with URL parameters

No special route config needed. Neo's `viewParams` system handles URL segments automatically based on the `static params` defined on the View class.

## Quality checklist

- [ ] Route added to the correct module's `*Routes.ts` file
- [ ] Icon uses a valid Material Symbols name
- [ ] Role applied if the route is not publicly accessible to all authenticated users
- [ ] `RouteService` updated if this is a new module
- [ ] Order in the `MenuRoutes` array reflects intended sidebar order
