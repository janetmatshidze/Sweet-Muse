---
description: "Use when creating or editing React views or view models. Covers Views.ViewBase, Views.ViewModelBase, observer pattern, view params, navigation, child view models, task runners, page managers, property change observation, and the MVVM lifecycle."
applyTo: ["src/**/Views/**/*.tsx", "src/**/Views/**/*VM.ts"]
---
# Neo Views & ViewModels

## ViewModel Pattern

All ViewModels extend `Views.ViewModelBase` from `@singularsystems/neo-react`.

```ts
import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '../../Services/AppService';
import { List } from '@singularsystems/neo-core';

export default class MyEntityVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private queryApiClient = AppService.get(Types.Domain.ApiClients.MyEntityQueryApiClient),
        private commandApiClient = AppService.get(Types.Domain.ApiClients.MyEntityCommandApiClient)) {

        super(taskRunner);
        this.makeObservable();

        // Register child view models for nested components
        this.childComponentVM = this.registerViewModel(ChildComponentVM, { initialise: true });
    }

    // Child VMs
    public childComponentVM: ChildComponentVM;

    // Observable collections
    public items = new List(MyEntityLookup);

    // Observable state
    public selectedItem: MyEntityLookup | null = null;

    // Lifecycle — called by framework after view mounts
    public async initialise() {
        const data = await this.taskRunner.waitForData(this.queryApiClient.getItems());
        this.items.set(data);
    }

    // Clean up reactions and subscriptions
    public dispose() {
        // Dispose any reaction disposers, auto-runners, etc.
    }
}
```

### ViewModel Rules

- Constructor: first param is always `taskRunner = AppService.get(Types.Neo.TaskRunner)`
- Additional DI services follow as constructor params with defaults via `AppService.get()`
- Call `super(taskRunner)` then `this.makeObservable()`
- Register child VMs with `this.registerViewModel(ChildVM, { initialise: true })`
- Use `this.taskRunner.waitForData()` to load and unpack data from the response
- Use `this.taskRunner.run()` to run async commands that are likely to be POSTs
- Override `dispose()` to clean up reactions and subscriptions
- Default export, one VM per file, filename: `{ViewName}VM.ts`
- Properties closely related to a model instance (e.g. an `isExpanded` property) should be added to that model class, and not to the view model.
- Do not block initial page load by loading related data (e.g. for drop downs). 
    - Use an app data cache entry if possible, otherwise declare a property on the VM of type Data.ApiClientLookupDataSource, and use this as the data source for the drop down.
- Do not implement custom change tracking logic, use the isDirty flag on the relevant model instead.

## View Pattern

Views extend `Views.ViewBase<VM>` and use the `@observer` decorator from MobX.

```tsx
import React from 'react';
import { Neo, Views } from '@singularsystems/neo-react';
import MyEntityVM from './MyEntityVM';
import { observer } from 'mobx-react';

@observer
export default class MyEntityView extends Views.ViewBase<MyEntityVM> {

    constructor(props: unknown) {
        super("My Entities", MyEntityVM, props);
    }

    public render() {
        const viewModel = this.viewModel;

        return (
            <div>
                <Neo.Card title="My Entities" icon="list">
                    {/* View content using viewModel */}
                </Neo.Card>
            </div>
        );
    }
}
```

### View Rules

- Class-based components (not function components)
- `@observer` decorator from `mobx-react`
- `super("View Title", VMClass, props)` — first arg is the display name
- Access state via `this.viewModel`
- Don't use react state in any views or components that have a view model. Stateful data must be represented as properties on the view model.
- Don't override react lifecycle events in Views. Use `initialise()` on the ViewModel to load initial data.
- Default export, one View per file

## View Parameters (URL State)

Use view parameters to synchronise view state with the URL, enabling bookmarking, breadcrumbs, and browser back/forward navigation.
Not all state changes need to be reflected in the URL, but use view parameters for any state that users are likely to want to share or bookmark.

1. Define a params class with properties returning `{}` (or `{ required: true }`, `{ isQuery: true }`).
2. Pass as second generic argument to `ViewBase`.
3. Add a static `params` property.
4. Read/write params in `viewParamsUpdated()` and event handlers.

```tsx
class DetailParams {
    public entityId = { required: true };  // URL segment parameter
    public tab = { isQuery: true };        // Query string parameter (?tab=details)
}

@observer
export default class MyDetailView extends Views.ViewBase<MyDetailVM, DetailParams> {
    public static params = new DetailParams();

    constructor(props: unknown) {
        super("Detail", MyDetailVM, props);
    }

    // Called when URL params change
    protected viewParamsUpdated() {
        const entityId = this.viewParams.entityId.asNullableInt();
        this.viewModel.loadEntity(entityId);

        // Set breadcrumb description
        if (this.viewModel.selectedEntity) {
            this.viewParams.entityId.description = this.viewModel.selectedEntity.name;
        }
    }

    // Navigate by setting param values
    private selectEntity(entityId: number) {
        // Set the view parameter, which triggers viewParamsUpdated()
        this.viewParams.entityId.value = entityId;
    }
}
```

### ViewParams API

- `this.viewParams.paramName.asString()` — get as string
- `this.viewParams.paramName.asNullableInt()` — get as nullable number
- `this.viewParams.paramName.description` — set breadcrumb label
- `this.viewParams.setValues({ param: value })` — navigate/update params

### Key Rules

- Always read parameter values in `viewParamsUpdated()`, not elsewhere.
- When the user clicks a button to change state, set the view parameter value (not the VM property directly). This keeps the URL and view state in sync for browser back/forward.
- Use `this.viewParams.paramName.description` to set breadcrumb text.

## Page Leave Confirmation

Override `onLeave()` to warn users about unsaved changes:

```ts
public onLeave() {
    if (this.viewModel.hasUnsavedChanges) {
        return "Are you sure you want to leave without saving?";
    }
    return undefined;
}
```

## Navigation

Use the `navigation` helper on a view (or resolve from DI) to navigate between views:

```tsx
// Navigate to a view
this.navigation.navigateToView(MyDetailView);

// Navigate with parameters
this.navigation.navigateToView(MyDetailView, { entityId: 123 });

// Get path for use in a link
<Neo.Link to={this.navigation.getPathForView(MyDetailView)}>View Details</Neo.Link>
```

## Data Binding

Neo uses **property instances** (`model.meta.property`) for two-way data binding.

- `bind={model.meta.property}` — Two-way binding (editable)
- `display={model.meta.property}` — Read-only display

```tsx
// Two-way binding - user can edit
<Neo.FormGroup bind={viewModel.model.meta.firstName} />

// Read-only display
<Neo.FormGroup display={viewModel.model.meta.firstName} />

// The value is also directly accessible
<span>You typed: {viewModel.model.firstName}</span>
```

## Component ViewModels

Child components that need their own VM use the same binding pattern but are registered via `registerViewModel`:

```ts
// In parent VM
this.childVM = this.registerViewModel(ChildComponentVM, { initialise: true });
```

```tsx
// In parent View render
<ChildComponent viewModel={this.viewModel.childVM} />
```

## Page Manager (Paged/Searchable APIs)

```ts
public pageManager = new Data.PageManager(
    this.criteria,             // criteria model (ValueObject)
    SomeLookup,                // result model type
    this.apiClient.findItems,  // API method reference
    {
        pageSize: 15,
        sortColumn: "name",
        sortAscending: true,
        fetchInitial: true,
        initialTaskRunner: this.taskRunner,
    }
);
```

```tsx
// In the view
<Neo.Pager pageManager={viewModel.pageManager}>
    <NeoGrid.Grid items={viewModel.pageManager}>
        {(item, meta) => (
            <NeoGrid.Row>
                <NeoGrid.Column display={meta.name} sort />
                <NeoGrid.Column display={meta.status} sort />
            </NeoGrid.Row>
        )}
    </NeoGrid.Grid>
</Neo.Pager>
```

## Observing Model Property Changes

### Inside a Model (OnChanged decorator)

```ts
@Attributes.OnChanged<FilterCriteria>(c => c.onCategoryChanged)
public categoryId: number | null = null;

// Debounce text input by 500ms
@Attributes.OnChanged<FilterCriteria>(c => c.onSearchChanged, false, 500)
public search: string = "";

private onCategoryChanged(oldValue: number | null) {
    // React to category selection change immediately
}

private onSearchChanged(oldValue: string) {
    // React to search text change after 500ms delay
}
```

- Prefer component events (e.g. `onItemSelected`, `onBlur`) to react to changes when possible.

### Outside a Model (onAnyPropertyChanged)

Preferred for triggering searches from criteria models. Automatically delays for text inputs but fires immediately for dropdowns.

```ts
// In ViewModel
public criteria = new FilterCriteria();

public async initialise() {
    this.autoDispose(this.criteria.onAnyPropertyChanged(() => {
        this.search();
    }));
}
```

### Using MobX reaction directly

For fine-grained control over which properties to observe:

```ts
import { reaction, IReactionDisposer } from 'mobx';

private disposer?: IReactionDisposer;

public async initialise() {
    this.disposer = reaction(
        () => ({ search: this.criteria.search, categoryId: this.criteria.categoryId }),
        (result) => { this.performSearch(result); }
    );
}

public dispose() {
    this.disposer?.();
}
```

- In a VM, prefer `this.autoDispose()` to automatically clean up reactions when the VM is disposed.

## Related Skills

- [frontend-create-view](../skills/frontend-create-view/SKILL.md) — Step-by-step: create a new View + ViewModel
- [neo-paged-grid](../skills/neo-paged-grid/SKILL.md) — Full vertical slice: paged data grid with search (backend + frontend)

## TaskRunner

The TaskRunner manages loading state and error handling for async operations.

```ts
// Run async operations - view automatically shows loading indicator
public async loadData() {
    await this.taskRunner.run(async () => {
        const result = await this.apiClient.getEntities();
        this.items.set(result.data);
    });
}

// Simpler form - waitFor
await this.taskRunner.waitFor(this.apiClient.getEntities());

// waitForData - unpacks the response data
const data = await this.taskRunner.waitForData(this.apiClient.getEntities());
this.items.set(data);
```

- The `taskRunner` automatically handles error notifications and loading indicators in the view, so you don't need to manage those manually.
- Do not check for a 200 or success response.
- Use the `AxiosUtils.catchErrors` helper if you need to handle errors in a custom way.