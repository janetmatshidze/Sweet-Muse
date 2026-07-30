---
name: frontend-add-catalogue-entry
description: Add a new catalogue entry end-to-end on the frontend — model, API client, data cache, catalogue entry class, route registration, and (if needed) a new menu group.
argument-hint: "<EntityName> [plural form]"
---

Use this skill when adding a new catalogue type to the frontend. Catalogue entries are inline-edit grids (with an optional modal form) backed by `DomainDataCache` and `ICatalogueApiClient`. See `frontend-catalogue.instructions.md` for full patterns.

## When this skill applies

Use this skill after the backend endpoint for the catalogue entity is already in place (see `backend-add-catalogue-entity`). You need:
- A backend GET + POST endpoint under `/api/catalogue/{entity-route}`
- A backend role defined (e.g. `Roles.Catalogue.EditMyEntities`)

## Steps

### 1. Create the model

Create the TypeScript model in `src/Domain/Models/<Group>/<ModelName>.ts`. See `frontend-models.instructions.md` and `frontend-catalogue.instructions.md` §1 for the full pattern.

```ts
import { Rules, Validation } from '@singularsystems/neo-core';
import {ProjectPrefix}SimpleSoftDeleteAuditModelBase from '../Base/{ProjectPrefix}SimpleSoftDeleteAuditModelBase';

export default class MyEntity extends {ProjectPrefix}SimpleSoftDeleteAuditModelBase {
    static typeName = "MyEntity";

    constructor() {
        super();
        this.makeObservable();
    }

    public myEntityId: number = 0;

    @Rules.StringLength(100)
    public name: string = "";

    protected static addBusinessRules(rules: Validation.Rules<MyEntity>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        return this.isNew || !this.name ? "New my entity" : this.name;
    }
}
```

### 2. Add the edit role to `AdminRoles.ts` (if needed)

In `src/Domain/Models/Security/AdminRoles.ts`, add a value to the `Catalogue` enum:

```ts
export enum Catalogue {
    // ... existing
    EditMyEntities = "Administration.Catalogue.Edit My Entities",
}
```

Skip this step if the catalogue should be editable by all authenticated users.

### 3. Update `CatalogueApiClient.ts`

Add `getMyEntities` and `updateMyEntities` to both the `ICatalogueApiClient` interface and the `CatalogueApiClient` implementation. See `frontend-api-clients.instructions.md` and `frontend-catalogue.instructions.md` §2.

```ts
// Interface
getMyEntities(): AxiosPromise<Array<MyEntity>>;
updateMyEntities(myEntities: Array<MyEntity>): AxiosPromise<Array<MyEntity>>;

// Implementation
public getMyEntities = ApiHelper.createGet<Array<MyEntity>>("catalogue/my-entities", MyEntity);
public updateMyEntities = ApiHelper.createPost<Array<MyEntity>>("catalogue/my-entities", MyEntity);
```

### 4. Register in `DomainDataCache.ts`

Add a `registerList` call in `src/Domain/Services/DomainDataCache.ts`:

```ts
public myEntities = this.registerList(MyEntity, this.catalogueApiClient.getMyEntities, LifeTime.Short);
```

Use `LifeTime.Short` for data that changes occasionally. Use `LifeTime.Long` only for data that changes very rarely.

### 5. Create the catalogue entry class

Create `src/Domain/Views/Catalogue/Entries/MyEntityCatalogueEntry.tsx`.

**Simple model (1–2 fields) — inline grid editing:**

```tsx
import { Catalogue } from '@singularsystems/neo-react-services';
import { NeoGrid, Model } from '@singularsystems/neo-react';
import { Catalogue as CatalogueRoles } from '../../../Models/Security/AdminRoles';
import MyEntity from '../../../Models/Group/MyEntity';
import CatalogueEntry from '../CatalogueEntry';
import DomainDataCache from '../../../Services/DomainDataCache';
import { ICatalogueApiClient } from '../../../ApiClients/CatalogueApiClient';

export default class MyEntityCatalogueEntry extends CatalogueEntry<MyEntity> {

    constructor() {
        super("My Entities", MyEntity);
    }

    public get editRole() {
        return CatalogueRoles.EditMyEntities; // or null if no role required
    }

    public getCacheEntry(cache: DomainDataCache) {
        return cache.myEntities;
    }

    public getSaveEndpoint(apiClient: ICatalogueApiClient) {
        return apiClient.updateMyEntities;
    }

    public renderRow(item: MyEntity, meta: Model.TransformMetaType<MyEntity>) {
        return (
            <NeoGrid.Row>
                <NeoGrid.Column bind={meta.name} label="Name" />
                <Catalogue.ButtonColumn />
            </NeoGrid.Row>
        );
    }
}
```

**Complex model (3+ fields) — add `renderFormControls` and switch `renderRow` to `display`:**

```tsx
public renderRow(item: MyEntity, meta: Model.TransformMetaType<MyEntity>) {
    return (
        <NeoGrid.Row>
            <NeoGrid.Column display={meta.name} label="Name" />
            <Catalogue.ButtonColumn />
        </NeoGrid.Row>
    );
}

public renderFormControls(item: MyEntity, meta: Model.TransformMetaType<MyEntity>) {
    return (
        <Neo.GridLayout md={2}>
            <Neo.FormGroup label="Name" bind={meta.name} />
            <Neo.FormGroup label="Other Field" bind={meta.otherField} />
        </Neo.GridLayout>
    );
}
```

Set `this.modalSize = "lg"` in the constructor when using a modal form.

See `frontend-catalogue.instructions.md` §4–6 for FK column and form control patterns.

### 6. Register in `CatalogueRoutes.ts`

Import the entry class and append it to the relevant group array in `src/Domain/Views/Catalogue/CatalogueRoutes.ts`:

```ts
import MyEntityCatalogueEntry from './Entries/MyEntityCatalogueEntry';

export const catalogueRoutes = {
    existingGroup: [
        // ... existing entries
        { name: "My Entities", component: MyEntityCatalogueEntry },
    ] as IAppMenuItem[],
};
```

**Adding a new group?** Add a new key to the object, then proceed to step 7.

### 7. Update `CatalogueView.tsx` (new group only)

Only required if a new group key was added in step 6. Add a `tryRenderSection` call inside `renderMenu()`:

```tsx
{this.tryRenderSection(catalogueRoutes.myNewGroup, children =>
    <Neo.Card title="My New Group">{children}</Neo.Card>)}
```

Place the card in whichever column makes sense for layout balance.

## Quality checklist

- [ ] Model extends the correct project audit base class and calls `makeObservable()`
- [ ] `toString()` returns a meaningful label for new and existing items
- [ ] `getXxx` / `updateXxx` added to both the interface and implementation in `CatalogueApiClient`
- [ ] `registerList` added to `DomainDataCache` with appropriate `LifeTime`
- [ ] `editRole` returns the correct role string (or `null` if no restriction)
- [ ] `renderRow` uses `bind` for inline editing or `display` when `renderFormControls` is present
- [ ] `renderRow` ends with `<Catalogue.ButtonColumn />`
- [ ] Modal size set in constructor if `renderFormControls` is implemented
- [ ] Entry appended to the correct group in `CatalogueRoutes.ts`
- [ ] `CatalogueView.tsx` updated only if a new group was added
