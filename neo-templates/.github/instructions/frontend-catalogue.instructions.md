---
applyTo: "src/Domain/Views/Catalogue/**,src/Domain/Models/**,src/Domain/ApiClients/CatalogueApiClient.ts,src/Domain/Services/DomainDataCache.ts"
---

# Catalogue UI Instructions

The catalogue system is a framework for managing lists of reference/lookup data (e.g. crop types, substance types, climate areas). Each catalogue type gets an inline-edit grid and an optional modal form, powered by the `Catalogue` namespace from `@singularsystems/neo-react-services`. The application wraps this with its own `CatalogueEntry<T>` base class that provides typed access to `DomainDataCache` and `ICatalogueApiClient`.

## Related Skills

- [frontend-add-catalogue-entry](../skills/frontend-add-catalogue-entry/SKILL.md) — Step-by-step: add a new catalogue entry end-to-end on the frontend

---

## Architecture

Adding a new catalogue type requires touching **all** of the following:

| Concern                 | File(s) |
| ---                     | ---     |
| Domain model            | `src/Domain/Models/<Group>/<ModelName>.ts` |
| API interface + client  | `src/Domain/ApiClients/CatalogueApiClient.ts` |
| Data cache registration | `src/Domain/Services/DomainDataCache.ts` |
| Catalogue entry class   | `src/Domain/Views/Catalogue/Entries/<ModelName>CatalogueEntry.tsx` |
| Route registration      | `src/Domain/Views/Catalogue/CatalogueRoutes.ts` |
| Edit security role      | `src/Domain/Models/Security/AdminRoles.ts` (if editing requires a role) |
| Menu card               | `src/Domain/Views/Catalogue/CatalogueView.tsx` (only if adding a new section group) |

---

## 1. Model Class

The catalogue model will be needed, see `frontend-models.instructions.md`.

```ts
import { Rules, Validation } from '@singularsystems/neo-core';
import {ProjectPrefix}SimpleSoftDeleteAuditModelBase from '../Base/{ProjectPrefix}SimpleSoftDeleteAuditModelBase';

export default class ClimateArea extends {ProjectPrefix}SimpleSoftDeleteAuditModelBase {
    static typeName = "ClimateArea";

    constructor() {
        super();
        this.makeObservable();
    }

    public climateAreaId: number = 0;

    @Rules.StringLength(30)
    public climateAreaName: string = "";

    // Client only properties / methods

    protected static addBusinessRules(rules: Validation.Rules<ClimateArea>) {
        super.addBusinessRules(rules);
    }

    public toString(): string {
        if (this.isNew || !this.climateAreaName) {
            return "New climate area";
        } else {
            return this.climateAreaName;
        }
    }
}
```

---

## 2. API Client

`ICatalogueApiClient` declares the contract; `CatalogueApiClient` implements it. Every catalogue type requires one `getXxx` and one `updateXxx` method pair.

See `frontend-api-clients.instructions.md`

---

## 3. DomainDataCache Registration

`DomainDataCache` (`src/Domain/Services/DomainDataCache.ts`) is a `Data.CachedDataService`. Register each catalogue list with `registerList`:

```ts
public climateAreas = this.registerList(ClimateArea, this.catalogueApiClient.getClimateAreas, LifeTime.Short);
```

Use `LifeTime.Short` (30 minutes) for most catalogue data. Use `LifeTime.Long` (240 minutes) only for data that changes very rarely. The cache entry name (e.g. `climateAreas`) is what `getCacheEntry` returns in the catalogue entry class.

---

## 4. Catalogue Entry Class

Create a new file in `src/Domain/Views/Catalogue/Entries/`. The class extends the app's `CatalogueEntry<T>` (which itself extends `Catalogue.CatalogueEntryBase<T, DomainDataCache, ICatalogueApiClient>`).

### Required members

#### `constructor`
```ts
constructor() {
    super("Display Name", ModelClass);
    // Optional: set modal size if renderFormControls is used
    this.modalSize = "lg"; // "sm" | "md" | "lg"
}
```

#### `editRole`
Returns a role string that gates edit access, or `null` if no role check is needed:
```ts
public get editRole() {
    return CatalogueRoles.EditClimateAreas; // from AdminRoles.Catalogue enum
}
```

#### `getCacheEntry`
Points to the data cache list that holds the items for this catalogue:
```ts
public getCacheEntry(cache: DomainDataCache) {
    return cache.climateAreas;
}
```

#### `getSaveEndpoint`
Points to the API client method that saves the list:
```ts
public getSaveEndpoint(apiClient: ICatalogueApiClient) {
    return apiClient.updateClimateAreas;
}
```

#### `renderRow`
Renders one row in the inline-edit grid. Always end with `<Catalogue.ButtonColumn />`:
```tsx
public renderRow(item: ClimateArea, meta: Model.TransformMetaType<ClimateArea>) {
    return (
        <NeoGrid.Row>
            <NeoGrid.Column bind={meta.climateAreaName} label="Climate Area" />
            <Catalogue.ButtonColumn />
        </NeoGrid.Row>
    );
}
```

#### `renderFormControls` (optional)
Renders the body of the edit modal. If the model has **3 or more fields**, implement this method and switch `renderRow` to use `display` instead of `bind` so the grid becomes read-only:

```tsx
public renderFormControls(item: Substance, meta: Model.TransformMetaType<Substance>) {
    return (
        <Neo.GridLayout md={2}>
            <Neo.FormGroup label="Name" bind={meta.substanceName} />
            <Catalogue.FormGroup label="Type" bind={meta.substanceTypeId} catalogueType={SubstanceTypeCatalogueEntry} />
            <Neo.FormGroup label="Default Application Rate" bind={meta.defaultApplicationRate} />
            <Neo.FormGroup label="Description" bind={meta.description} />
        </Neo.GridLayout>
    );
}
```

If omitted, editing happens directly in the grid row.

---

## 5. NeoGrid Column Patterns in `renderRow`

### Plain editable column
```tsx
<NeoGrid.Column bind={meta.climateAreaName} label="Climate Area" />
```

### Read-only display column
Switch to `display` when `renderFormControls` is implemented:
```tsx
<NeoGrid.Column display={meta.substanceName} />
```

### FK resolved to read-only text via data cache
```tsx
<NeoGrid.Column label="Type" display={meta.substanceTypeId} select={{ itemSource: this.dataCache.substanceTypes, renderAsText: true }} />
```

### FK resolved to an editable dropdown linked to a sibling catalogue
```tsx
<Catalogue.Column label="Category" display={meta.cropHealthEventCategoryId} catalogueType={CropHealthEventCategoryCatalogueEntry} />
```
This renders the value as text in the grid but provides a dropdown with a link to manage the referenced catalogue inline.

---

## 6. Form Control Patterns in `renderFormControls`

### Simple field
```tsx
<Neo.FormGroup label="Name" bind={meta.substanceName} />
```

### FK dropdown with inline link to sibling catalogue
```tsx
<Catalogue.FormGroup label="Type" bind={meta.substanceTypeId} catalogueType={SubstanceTypeCatalogueEntry} />
```
This renders a dropdown and a small button that opens the sibling catalogue in a modal so the user can add missing values without leaving the current form.

### Grid layout
```tsx
<Neo.GridLayout md={2}>
    {/* fields */}
</Neo.GridLayout>
```
Use `md={2}` for two-column forms, `md={1}` for single-column. The `md` prop sets the number of columns at the medium breakpoint.

---

## 7. `CatalogueRoutes.ts`

`CatalogueRoutes.ts` exports a single `catalogueRoutes` const. Each key is a **section group** used both for the menu and for React Router child route generation. Each value is an `IAppMenuItem[]` where every element is `{ name: "Display Name", component: SomeCatalogueEntry }`.

```ts
export const catalogueRoutes = {
    farming: [
        { name: "Crop Types", component: CropTypeCatalogueEntry },
        { name: "Crop Cycle Stage Types", component: CropCycleStageTypeCatalogueEntry },
        { name: "Climate Areas", component: ClimateAreaCatalogueEntry },
    ] as IAppMenuItem[],
    metrics: [
        { name: "Metric Types", component: MetricTypeCatalogueEntry },
    ] as IAppMenuItem[],
};
```

**Adding an entry to an existing group:** import the new entry class and append `{ name: "...", component: NewEntry }` to the relevant array.

**Adding a new group:** add a new key to the object and import all entry classes for it. Then also update `CatalogueView.tsx` (see next section).

---

## 8. `CatalogueView.tsx`

This is the top-level route component. It only needs changes when a **new section group** is added to `catalogueRoutes`.

- **`constructor`** — passes `catalogueRoutes` to `super`. No changes needed.
- **`getRouteChildren()`** (static) — auto-derives React Router children from `catalogueRoutes`. No changes needed.
- **`auditApiClient()`** — returns `null`; replace with an actual audit client if audit history is required.
- **`filterRoutes()`** — applies sorting and security filtering via `routeSecurityService.menuItemAllowed` and text filtering via `this.viewModel.filter`. Can be overridden if you need to change sorting or filtering behaviour, but do not modify for standard use.
- **`renderMenu()`** — the only method to edit when a new group is added. Each group needs one `tryRenderSection` call wrapped in a `Neo.Card`. Groups are arranged in two `<div>` columns; place the new card in whichever column makes sense for balance. `tryRenderSection` automatically hides the card if the user has no access to any entry in the group.

```tsx
protected renderMenu() {
    return (
        <Neo.GridLayout lg={2} arrangeVertically>
            <div>
                {this.tryRenderSection(catalogueRoutes.substances, children =>
                    <Neo.Card title="Substances">{children}</Neo.Card>)}
                {this.tryRenderSection(catalogueRoutes.myNewGroup, children =>
                    <Neo.Card title="My New Group">{children}</Neo.Card>)}
            </div>
            <div>
                {this.tryRenderSection(catalogueRoutes.farming, children =>
                    <Neo.Card title="Farming">{children}</Neo.Card>)}
            </div>
        </Neo.GridLayout>
    );
}
```

---

## 9. Security Roles

Each catalogue type that requires edit security declares a role string in the `Catalogue` enum in `src/Domain/Models/Security/AdminRoles.ts`:

```ts
export enum Catalogue {
    EditClimateAreas = "Administration.Catalogue.Edit Climate Areas",
    EditCropTypes = "Administration.Catalogue.Edit Crop Types",
    // ...
}
```

The string format is `"Administration.Catalogue.Edit <Friendly Name>"`. Import this enum aliased as `CatalogueRoles` in the entry class:

```ts
import { Catalogue as CatalogueRoles } from '../../../Models/Security/AdminRoles';
```

Return `null` from `editRole` only if the catalogue should be editable by all authenticated users with no role restriction.

---

## 10. End-to-End Checklist

When adding a new catalogue type, touch these files **in order**:

1. **`src/Domain/Models/<Group>/<ModelName>.ts`** — create the model class
2. **`src/Domain/Models/Security/AdminRoles.ts`** — add the edit role to the `Catalogue` enum (if needed)
3. **`src/Domain/ApiClients/CatalogueApiClient.ts`** — add `getXxx` / `updateXxx` to both the interface and the implementation class
4. **`src/Domain/Services/DomainDataCache.ts`** — register with `this.registerList(...)`
5. **`src/Domain/Views/Catalogue/Entries/<ModelName>CatalogueEntry.tsx`** — create the catalogue entry class
6. **`src/Domain/Views/Catalogue/CatalogueRoutes.ts`** — append to an existing group array (or add a new group key)
7. **`src/Domain/Views/Catalogue/CatalogueView.tsx`** — add `tryRenderSection` in `renderMenu()` **only if a new group was added in step 6**
