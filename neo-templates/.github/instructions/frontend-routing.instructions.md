---
description: "Use when adding new routes or menu items. Covers IRoute, IMenuRoute, route properties, menu nesting, role-based access, view params in routes, per-module route files, RouteService integration, and adding new routes."
applyTo: ["src/**/*Routes.ts", "src/App/Services/RouteService.ts"]
---
# Neo Routing

## Route Interfaces

```ts
import { Routing as NeoRouting } from '@singularsystems/neo-core';

export interface IAppRoute extends NeoRouting.IRoute {
    // custom props if needed
}

export interface IAppMenuItem extends NeoRouting.IMenuRoute {
    header?: boolean;
}
```

## Route Properties

| Property        | Type                  | Purpose |
| ---             | ---                   | ---     |
| `name`          | `string`              | Display name in sidebar menu |
| `path`          | `string`              | URL path |
| `component`     | `React.ComponentType` | View component to render |
| `icon`          | `string`              | Material Symbols icon name |
| `exact`         | `boolean`             | Exact path matching |
| `allowAnonymous`| `boolean`             | Skip authentication |
| `role`          | `string`              | Required role for access |
| `children`      | `IAppMenuItem[]`      | Nested menu items (creates group) |
| `routeChildren` | route config          | Sub-routes within a view |
| `header`        | `boolean`             | Display as section header |

## Per-Module Route Files

Each domain module defines its routes in a `*Routes.ts` file:

```ts
// src/Domain/DomainRoutes.ts
import { IAppMenuItem, IAppRoute } from '../App/Services/RouteService';
import MyEntityView from './Views/MyArea/MyEntityView';

const MenuRoutes: IAppMenuItem[] = [
    {
        name: "My Area",
        children: [
            { name: "My Entity", path: "/my-entity", icon: "list", component: MyEntityView }
        ]
    },
];

const PureRoutes: IAppRoute[] = [];

export { MenuRoutes, PureRoutes }
```

## Menu Grouping

Top-level items with `children` create collapsible menu sections:

```ts
{
    name: "Administration",
    children: [
        { name: "Theme", path: "/theme", icon: "palette", component: ThemeView, role: Roles.Themes.View },
        { name: "Users", path: "/users", icon: "people", component: UserManagementView },
    ]
}
```

## RouteService (src/App/Services/RouteService.ts)

The central route service merges routes from all modules:

```ts
private getMenuRoutes(): IAppMenuItem[] {
    return [
        { name: "Home", path: '/', component: Home, icon: "home", exact: true, allowAnonymous: true },
        ...ReportingRoutes.MenuRoutes,
        ...DomainRoutes.MenuRoutes,
        this.getAdministrationRoute(),
    ];
}
```

Routes are spread into the menu array. Order determines sidebar display order.

## Adding a New Route

See [frontend-add-route](../skills/frontend-add-route/SKILL.md) for the step-by-step procedure.

## Role-Based Access

```ts
import * as DomainRoles from '../Domain/Models/Security/AdminRoles';

{ name: "Theme", path: "/theme", component: ThemeView, role: DomainRoles.Themes.View }
```

## Views with URL Params

When a view uses params, the route path is the base path — Neo handles params automatically via the `viewParams` system.

```ts
class MyParams {
    entityId = {};
    tab = {};
}

export default class MyView extends Views.ViewBase<MyVM, MyParams> {
    public static params = new MyParams();
    // ...
}
```

## Related Skills

- [frontend-add-route](../skills/frontend-add-route/SKILL.md) — Step-by-step: add a route and menu item
- [frontend-create-view](../skills/frontend-create-view/SKILL.md) — Create the View + VM that a route points to
- [frontend-create-roles](../skills/frontend-create-roles/SKILL.md) — Define role strings for route guards

## Dynamic Menu Routes

Use `Routing.MenuRoute` to define menu items that navigate to the same view with different initial parameters:

```ts
{
    ...entityBaseRoute,
    icon: "list",
    children: categories.map(cat =>
        new Routing.MenuRoute(
            entityBaseRoute,
            { name: cat.name },
            { categoryId: cat.id }
        )
    ),
}
```
