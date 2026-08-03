---
description: "Use when creating or editing API client classes. Covers Data.ApiClientBase, interface definitions, typed AxiosPromise returns, CQRS command/query split, and Model type helpers."
applyTo: "src/**/ApiClients/**/*.ts"
---
# Neo API Clients

## Overview

API clients follow CQRS — separate **Query** and **Command** clients per domain area.

## Query API Client

```ts
import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../../DomainTypes';
import MyEntityLookup from '../../Contracts/MyArea/Lookups/MyEntityLookup';

export interface IMyEntityQueryApiClient {
    getItems(id?: number): AxiosPromise<Array<Model.PlainObject<MyEntityLookup>>>;
}

@injectable()
export default class MyEntityQueryApiClient extends Data.ApiClientBase implements IMyEntityQueryApiClient {

    constructor(config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/my-area/entity`);
    }

    public getItems(id?: number): AxiosPromise<Array<Model.PlainObject<MyEntityLookup>>> {
        return this.axios.get(`${this.apiPath}${!!id ? `?id=${id}` : ''}`);
    }
}
```

## Command API Client

```ts
import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../../DomainTypes';
import UpsertMyEntityCommand from '../../Contracts/MyArea/Commands/UpsertMyEntityCommand';
import MyEntityLookup from '../../Contracts/MyArea/Lookups/MyEntityLookup';

export interface IMyEntityCommandApiClient {
    upsertEntity(command: Model.PartialPlainObject<UpsertMyEntityCommand>): AxiosPromise<Model.PlainObject<MyEntityLookup>>;
    deleteEntity(entityId: number): AxiosPromise;
}

@injectable()
export default class MyEntityCommandApiClient extends Data.ApiClientBase implements IMyEntityCommandApiClient {

    constructor(config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/my-area/entity`);
    }

    public upsertEntity(command: Model.PartialPlainObject<UpsertMyEntityCommand>): AxiosPromise<Model.PlainObject<MyEntityLookup>> {
        return this.axios.post(`${this.apiPath}`, command);
    }

    public deleteEntity(entityId: number): AxiosPromise {
        return this.axios.delete(`${this.apiPath}/${entityId}`);
    }
}
```

## Related Skills

- [frontend-create-api-client](../skills/frontend-create-api-client/SKILL.md) — Step-by-step: create a query or command API client
- [frontend-register-di-service](../skills/frontend-register-di-service/SKILL.md) — Register the client in DI

## Key Conventions

### Always Define a Companion Interface

Export a named interface (`IXxxApiClient`) alongside the default class. The interface is used for the DI type symbol.

### Model Type Helpers

| Type                          | Usage |
| ---                           | ---   |
| `Model.PlainObject<T>`        | Server -> client response for **untracked** models (lookups) |
| `Model.PlainTrackedObject<T>` | Server -> client response for **tracked** models (entities with audit fields) |
| `Model.PartialPlainObject<T>` | Client -> server payload (commands, criteria) |
| `Data.PageRequest<TCriteria>` | Paged search request wrapper |
| `Data.PageResult<T>`          | Paged search response wrapper |

### HTTP Method Conventions

| Operation              | HTTP Method              | Return Type |
| ---                    | ---                      | ---         |
| Get single/list        | `this.axios.get(...)`    | `AxiosPromise<Model.PlainObject<T>>` or array |
| Create/Update (upsert) | `this.axios.post(...)`   | `AxiosPromise<Model.PlainObject<T>>` |
| Delete                 | `this.axios.delete(...)` | `AxiosPromise` (no body) |
| Paged search (find)    | `this.axios.post(...)`   | `AxiosPromise<Data.PageResult<Model.PlainObject<T>>>` |

### Paged Search Example

```ts
// Interface
findItems(request: Model.PartialPlainObject<Data.PageRequest<SomeCriteria>>): AxiosPromise<Data.PageResult<Model.PlainObject<SomeLookup>>>;

// Implementation
public findItems(request: Model.PartialPlainObject<Data.PageRequest<SomeCriteria>>) {
    return this.axios.post(`${this.apiPath}/find`, request);
}
```

### Constructor

Always inject config via default param: `config = AppService.get(Types.App.Config)`
Base URL set via: `super(\`${config.apiPath}/endpoint-path\`)`

### HTTP Methods Available

Use `this.axios` (Axios instance from base class):
- `this.axios.get(url)`
- `this.axios.post(url, data)`
- `this.axios.put(url, data)`
- `this.axios.delete(url)`
