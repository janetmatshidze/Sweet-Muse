---
name: frontend-create-api-client
description: Create a typed Neo API client class (query or command) for a backend endpoint
argument-hint: "<EntityName> <query|command|both>"
---

Use this skill when a new backend controller or action needs to be called from the frontend. API clients follow CQRS — query clients handle GET operations, command clients handle POST/PUT/DELETE. Each client defines a companion interface used for DI.

## Steps

### 1. Determine file location

```
src/Domain/ApiClients/{DomainArea}/
├── {Entity}QueryApiClient.ts    ← GET operations
└── {Entity}CommandApiClient.ts  ← POST / DELETE operations
```

### 2. Create a Query API client

```ts
import { Data, Model } from '@singularsystems/neo-core';
import { AxiosPromise } from 'axios';
import { injectable } from 'inversify';
import { AppService, Types } from '../../DomainTypes';
import MyEntityLookup from '../../Contracts/MyArea/Lookups/MyEntityLookup';

export interface IMyEntityQueryApiClient {
    getItems(): AxiosPromise<Array<Model.PlainObject<MyEntityLookup>>>;
    findItems(request: Model.PartialPlainObject<Data.PageRequest<MyEntityLookupCriteria>>):
        AxiosPromise<Data.PageResult<Model.PlainObject<MyEntityLookup>>>;
}

@injectable()
export default class MyEntityQueryApiClient extends Data.ApiClientBase implements IMyEntityQueryApiClient {

    constructor(config = AppService.get(Types.App.Config)) {
        super(`${config.apiPath}/my-area/entity`);
    }

    public getItems(): AxiosPromise<Array<Model.PlainObject<MyEntityLookup>>> {
        return this.axios.get(this.apiPath);
    }

    public findItems(request: Model.PartialPlainObject<Data.PageRequest<MyEntityLookupCriteria>>) {
        return this.axios.post(`${this.apiPath}/find`, request);
    }
}
```

### 3. Create a Command API client

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
        return this.axios.post(this.apiPath, command);
    }

    public deleteEntity(entityId: number): AxiosPromise {
        return this.axios.delete(`${this.apiPath}/${entityId}`);
    }
}
```

### 4. HTTP method and return type reference

| Operation | HTTP method | Return type |
|---|---|---|
| Get single / list | `this.axios.get(...)` | `AxiosPromise<Model.PlainObject<T>>` or array |
| Create / update (upsert) | `this.axios.post(...)` | `AxiosPromise<Model.PlainObject<T>>` |
| Paged search | `this.axios.post(.../find, request)` | `AxiosPromise<Data.PageResult<Model.PlainObject<T>>>` |
| Delete | `this.axios.delete(...)` | `AxiosPromise` (no body) |

### 5. Register in DI

After creating the client, register it in the DI container. See [frontend-register-di-service](../frontend-register-di-service/SKILL.md).

## Quality checklist

- [ ] Interface (`IXxxApiClient`) exported from the same file
- [ ] Class decorated with `@injectable()`
- [ ] Config injected via default param: `config = AppService.get(Types.App.Config)`
- [ ] Base URL set in `super(...)` call
- [ ] Return types use `Model.PlainObject<T>`, `Model.PartialPlainObject<T>`, or `Data.PageResult<T>` correctly
- [ ] Do not check for 200 response — non-200 throws automatically
- [ ] Client registered in `DomainTypes.ts` and `DomainModule.ts`
