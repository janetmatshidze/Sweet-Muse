---
name: neo-paged-grid
description: >
  Build a server-paged data grid using the Neo stack.
  Use when creating a list view with paging, search/filter criteria, and a NeoGrid.
  Covers the full vertical slice: C# Criteria POCO, Lookup POCO, QueryService with
  IQueryable projection, QueryController with PageRequest/PageResult, TypeScript
  Criteria ValueObject, Lookup model, API client, ViewModel with PageManager, and
  the View with Neo.Pager + NeoGrid.Grid. Triggers: paged grid, list view, PageManager,
  Neo.Pager, NeoGrid, searchable list, paged search.
argument-hint: "<Type> <Domain>"
---

Use this skill when building a list view that loads data from a paginated server-side endpoint. It covers the full vertical slice — backend first, then frontend.

## Placeholders

| Placeholder | Meaning | Example |
|---|---|---|
| `{Project}` | Root namespace of the project | `MyApp.Core` |
| `{Domain}` | Domain area / feature grouping | `Farms`, `CropCycles` |
| `{Type}` | PascalCase type name (singular) | `Farmer`, `CropCycle` |
| `{TypePlural}` | PascalCase type name (plural) | `Farmers`, `CropCycles` |
| `{type}` | camelCase type name (singular) | `farmer`, `cropCycle` |
| `{endpoint-name}` | URL segment for the API route | `farmers`, `crop-cycles` |
| `{project-types-import}` | Import path to the project's `AppService` and `Types` exports | `'../../DomainTypes'` |
| `{Module}` | DI module namespace that owns the API clients | `Domain`, `Identity` |

---

## Backend

### 1. Criteria (Contracts project)

Simple POCO — no base class, no attributes.

```csharp
namespace {Project}.Contracts.{Domain}.Criteria
{
  public class {Type}LookupCriteria
  {
    public string? Search { get; set; }

    // Add other filter fields as needed
  }
}
```

### 2. Lookup (Contracts project)

Simple POCO — no base class.

```csharp
namespace {Project}.Contracts.{Domain}.Lookups
{
  public class {Type}Lookup
  {
    public int {Type}Id { get; set; }

    public string Name { get; set; } = string.Empty;

    // Add other projected fields
  }
}
```

### 3. Query Service (App project)

- Declare a `static readonly Expression<Func<TEntity, TLookup>>` projection for EF translation and reuse.
- Expose a public query method returning `IQueryable<TLookup>` so other services can compose on it.
- Expose a public paged method that calls `.ToPageAsync(request)` for the controller to use.

```csharp
namespace {Project}.Services.{Domain}
{
  using System;
  using System.Linq;
  using System.Linq.Expressions;
  using System.Threading.Tasks;
  using Microsoft.EntityFrameworkCore;
  using Neo.Extensions;
  using Neo.Model.Services;
  using {Project}.Contracts.{Domain}.Criteria;
  using {Project}.Contracts.{Domain}.Lookups;

  public class {Type}QueryService({Project}DbContext dbContext)
  {
    private static readonly Expression<Func<{Type}, {Type}Lookup>> {Type}LookupProjection =
      entity => new {Type}Lookup
      {
        {Type}Id = entity.{Type}Id,
        Name = entity.Name,
        // Map other fields
      };

    /// <summary>Gets a reusable {type} lookup query.</summary>
    public IQueryable<{Type}Lookup> Get{TypePlural}Query({Type}LookupCriteria criteria)
    {
      return this.GetFiltered{TypePlural}Query(criteria)
        .OrderBy(e => e.Name)
        .Select({Type}LookupProjection);
    }

    /// <summary>Gets {typePlural} as a paged result.</summary>
    public Task<PageResult<{Type}Lookup>> Get{TypePlural}(
      PageRequest<{Type}LookupCriteria> request)
    {
      return this.Get{TypePlural}Query(request.Criteria).ToPageAsync(request);
    }

    private IQueryable<{Type}> GetFiltered{TypePlural}Query({Type}LookupCriteria criteria)
    {
      var query = dbContext.{TypePlural}.AsNoTracking();

      if (!string.IsNullOrWhiteSpace(criteria.Search))
      {
        var escaped = criteria.Search.Trim()
          .Replace(@"\", @"\\")
          .Replace("%", @"\%")
          .Replace("_", @"\_")
          .Replace("[", @"\[");
        var pattern = $"%{escaped}%";
        query = query.Where(e => EF.Functions.Like(e.Name, pattern, @"\"));
      }

      return query;
    }
  }
}
```

### 4. Query Controller (Api project)

- `[HttpGet]` with `[FromQuery] PageRequest<TCriteria>`.
- Returns `Task<PageResult<TLookup>>` directly — no `ActionResult` wrapper needed.
- `[RequireRole(...)]` references the project's security roles class.

```csharp
namespace {Project}.Api.Controllers.{Domain}
{
  using System.Threading.Tasks;
  using Microsoft.AspNetCore.Mvc;
  using Neo.Model.DataAnnotations;
  using Neo.Model.Services;
  using {Project}.Contracts.{Domain}.Criteria;
  using {Project}.Contracts.{Domain}.Lookups;
  using {Project}.Services.{Domain};

  [Route("api/{endpoint-name}")]
  [ApiController]
  public class {Type}QueryController({Type}QueryService queryService) : ControllerBase
  {
    [HttpGet]
    [RequireRole({Project}Roles.{Type}.View)]
    public Task<PageResult<{Type}Lookup>> Get{TypePlural}(
      [FromQuery] PageRequest<{Type}LookupCriteria> request)
    {
      return queryService.Get{TypePlural}(request);
    }
  }
}
```

---

## Frontend

### 5. Criteria model (`src/{Domain}/Criteria/`)

Extends `ValueObject`. Call `makeObservable()`. Must implement `toString()`.

```ts
import { NeoModel, ValueObject } from '@singularsystems/neo-core';

@NeoModel
export default class {Type}LookupCriteria extends ValueObject {

    constructor() {
        super();
        this.makeObservable();
    }

    public search: string = "";

    // Add other filter fields

    public toString(): string {
        return "{Type} Lookup Criteria";
    }
}
```

### 6. Lookup model (`src/{Domain}/Lookups/`)

Extends `LookupBase`. Call `makeBindable()`. Scalar props are `readonly`.

```ts
import { LookupBase } from '@singularsystems/neo-core';

export default class {Type}Lookup extends LookupBase {

    constructor() {
        super();
        this.makeBindable();
    }

    public readonly {type}Id: number = 0;
    public readonly name: string = "";

    // Add other readonly fields
}
```

### 7. API client (`src/{Domain}/ApiClients/{Type}QueryApiClient.ts`)

This skill uses **GET + `Utils.getQueryString`** (RESTful). If your project uses **POST to `/find`** instead, use `axios.post` with `Model.PartialPlainObject` and omit `Utils.getQueryString`.

- Paged method input type: `Model.PartialPlainNonTrackedObject<Data.PageRequest<TCriteria>>`.
- Use `Utils.getQueryString(request)` to serialise the paging request as a GET query string.

```ts
import { Data, Model, Utils } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '{project-types-import}';
import {Type}LookupCriteria from '../Criteria/{Type}LookupCriteria';
import {Type}Lookup from '../Lookups/{Type}Lookup';

export interface I{Type}QueryApiClient {
    get{TypePlural}(request: Model.PartialPlainNonTrackedObject<Data.PageRequest<{Type}LookupCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<{Type}Lookup>>>;
}

@injectable()
export default class {Type}QueryApiClient extends Data.ApiClientBase implements I{Type}QueryApiClient {

    constructor(config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/{endpoint-name}`);
    }

    public get{TypePlural}(request: Model.PartialPlainNonTrackedObject<Data.PageRequest<{Type}LookupCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<{Type}Lookup>>> {
        return this.axios.get(`${this.apiPath}?${Utils.getQueryString(request)}`);
    }
}
```

After creating, register the interface and class in the project's DI types and module files. See [frontend-register-di-service](../frontend-register-di-service/SKILL.md).

### 8. ViewModel (`src/{Domain}/Views/{Type}ListVM.ts`)

Wire criteria changes to `pageManager.refreshData()` via `onAnyPropertyChanged`.

For **text search fields**, debounce using `Utils.debounce` (use `KeystrokeDebounceTime` constant).  
For **all other criteria fields** (dropdowns, checkboxes), refresh immediately.

```ts
import { Data, Utils } from '@singularsystems/neo-core';
import { Views } from '@singularsystems/neo-react';
import { AppService, Types } from '{project-types-import}';
import {Type}LookupCriteria from '../Criteria/{Type}LookupCriteria';
import {Type}Lookup from '../Lookups/{Type}Lookup';

const KeystrokeDebounceTime = 300;

export default class {Type}ListVM extends Views.ViewModelBase {

    constructor(
        taskRunner = AppService.get(Types.Neo.TaskRunner),
        private queryApiClient = AppService.get(Types.{Module}.ApiClients.{Type}QueryApiClient)) {

        super(taskRunner);
        this.makeObservable();

        this.autoDispose(this.criteria.onAnyPropertyChanged(() => this.handleCriteriaChanged()));
    }

    public criteria = new {Type}LookupCriteria();

    public pageManager = new Data.PageManager(this.criteria, {Type}Lookup, this.queryApiClient.get{TypePlural}, {
        pageSize: 20,
        pageSizeOptions: [10, 20, 50],
        fetchInitial: true,
        initialTaskRunner: this.taskRunner,
    });

    private lastSearch: string = "";

    private handleCriteriaChanged() {
        if (this.criteria.search !== this.lastSearch) {
            this.lastSearch = this.criteria.search;
            void Utils.debounce(this, () => this.pageManager.refreshData(), KeystrokeDebounceTime);
            return;
        }

        void this.pageManager.refreshData();
    }
}
```

**PageManager options:**

| Option | Purpose |
|---|---|
| `pageSize` | Default page size |
| `pageSizeOptions` | Array of sizes shown in the page size dropdown |
| `fetchInitial: true` | Auto-fetch on mount (auto-search pattern) |
| `fetchInitial: false` | Wait for explicit trigger (Search button pattern) |
| `allowSort: false` | Disable column sorting |
| `initialTaskRunner` | Shows loading indicator during the initial fetch |
| `beforeFetch` | Callback to mutate the request before each fetch (e.g. inject a parent ID) |

**Explicit search pattern** (Search button instead of auto-search): set `fetchInitial: false`, remove `onAnyPropertyChanged`, and expose a `search()` method that calls `this.pageManager.refreshData()`.

### 9. View (`src/{Domain}/Views/{Type}ListView.tsx`)

Wrap `NeoGrid.Grid` inside `Neo.Pager`. When the grid is a direct child of `Neo.Pager`, the pager provides the data automatically — **do not pass an `items` prop** to the grid. Provide the lookup type as a generic type argument to `NeoGrid.Grid`.

Use `(item, meta)` in the row callback when binding columns with `display` or `sort`. Use `(item)` alone when rendering fully custom cell content.

```tsx
import React from 'react';
import { observer } from 'mobx-react';
import { Neo, NeoGrid, Views } from '@singularsystems/neo-react';
import {Type}Lookup from '../Lookups/{Type}Lookup';
import {Type}ListVM from './{Type}ListVM';

@observer
export default class {Type}ListView extends Views.ViewBase<{Type}ListVM> {

    constructor(props: unknown) {
        super("{Type} List", {Type}ListVM, props);
    }

    public render() {
        const viewModel = this.viewModel;

        return (
            <div>
                <Neo.Card title="{TypePlural}" icon="list">
                    <Neo.FormGroup
                        bind={viewModel.criteria.meta.search}
                        placeholder="Search..."
                    />

                    <Neo.Pager pageManager={viewModel.pageManager} pageControlProps={{ pageSizeLabel: "Show: " }}>
                        <NeoGrid.Grid<{Type}Lookup>>
                            {(item, meta) => (
                                <NeoGrid.Row>
                                    <NeoGrid.Column display={meta.name} sort />
                                    {/* Add other columns */}
                                    <NeoGrid.ButtonColumn>
                                        <Neo.Button size="sm" icon="edit" isOutline onClick={() => {/* navigate */}} />
                                    </NeoGrid.ButtonColumn>
                                </NeoGrid.Row>
                            )}
                        </NeoGrid.Grid>
                    </Neo.Pager>
                </Neo.Card>
            </div>
        );
    }
}
```

**Neo.Pager props:**

| Prop | Purpose |
|---|---|
| `pageManager` | Required — the `PageManager` instance |
| `pageControls="top"` | Move page controls above the grid (default is below) |
| `pageControlProps={{ pageSizeLabel: "Show: " }}` | Customise page control labels |

**NeoGrid.Column props:**

| Prop | Purpose |
|---|---|
| `display={meta.field}` | Read-only bound column |
| `bind={meta.field}` | Editable bound column |
| `sort` | Enable sortable column header |
| `label="Custom Label"` | Override the column header label |
| `hideBelow="md"` | Responsive hiding (`sm`, `md`, `lg`, `xl`, `xxl`) |
| `width={120}` | Fixed column width in px |
| `dateProps={{ formatString: "dd MMM yyyy" }}` | Date format override |

**NeoGrid.Row props:**

| Prop | Purpose |
|---|---|
| `onClick={() => ...}` | Make the entire row clickable |

---

## Quality checklist

- [ ] Backend Criteria POCO created in Contracts project (no base class)
- [ ] Backend Lookup POCO created in Contracts project (no base class)
- [ ] Query Service: static projection, public `IQueryable` query method, paged `ToPageAsync` method
- [ ] Query Controller: `[HttpGet]` + `[FromQuery] PageRequest<TCriteria>` + `[RequireRole]`
- [ ] Frontend Criteria: `ValueObject`, `@NeoModel`, `makeObservable()`, `toString()`
- [ ] Frontend Lookup: `LookupBase`, `makeBindable()`, `readonly` scalar props
- [ ] API client: `PartialPlainNonTrackedObject<PageRequest<TCriteria>>` + `Utils.getQueryString`
- [ ] API client registered in project DI types and module files
- [ ] ViewModel: `criteria`, `pageManager` as class properties (not in `initialise`), `onAnyPropertyChanged` via `autoDispose`, manual debounce for text fields
- [ ] View: `Neo.Pager` wrapping `NeoGrid.Grid` with no `items` prop on the grid
