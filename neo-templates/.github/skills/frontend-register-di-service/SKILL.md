---
name: frontend-register-di-service
description: Register a new API client or service in the Inversify DI container
argument-hint: "<ServiceName> <module name e.g. Domain|App|Identity>"
---

Use this skill after creating a new API client or service class that needs to be injected via `AppService.get(Types...)`. Registration requires changes to two files: the Types file (adds the identifier) and the Module file (binds the class).

## Steps

### 1. Add a `ServiceIdentifier` to the Types file

Open `src/{Module}/{Module}Types.ts` (e.g. `DomainTypes.ts`) and add an identifier typed to the **interface**:

```ts
import { AppServices } from '@singularsystems/neo-core';
import { IMyEntityQueryApiClient } from './ApiClients/MyArea/MyEntityQueryApiClient';
import { IMyEntityCommandApiClient } from './ApiClients/MyArea/MyEntityCommandApiClient';

const DomainTypes = {
    ApiClients: {
        // ... existing entries
        MyEntityQueryApiClient: new AppServices.ServiceIdentifier<IMyEntityQueryApiClient>(
            "Domain.ApiClients.MyEntityQueryApiClient"
        ),
        MyEntityCommandApiClient: new AppServices.ServiceIdentifier<IMyEntityCommandApiClient>(
            "Domain.ApiClients.MyEntityCommandApiClient"
        ),
    },
    Services: {
        // add services here
    }
}
```

**Identifier string format:** `"ModuleName.Category.ClassName"` (e.g. `"Domain.ApiClients.CropCycleConfigQueryApiClient"`)

### 2. Bind the class in the Module file

Open `src/{Module}/{Module}Module.ts` (e.g. `DomainModule.ts`) and bind the concrete class:

```ts
import MyEntityQueryApiClient from './ApiClients/MyArea/MyEntityQueryApiClient';
import MyEntityCommandApiClient from './ApiClients/MyArea/MyEntityCommandApiClient';

export const DomainAppModule = new AppServices.Module("Domain", container => {

    // Api Clients
    container.bind(DomainTypes.ApiClients.MyEntityQueryApiClient)
        .to(MyEntityQueryApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.MyEntityCommandApiClient)
        .to(MyEntityCommandApiClient).inSingletonScope();

    // Services
    // container.bind(...).to(...).inSingletonScope();
});
```

**Scope rules:**
- API clients and services → `.inSingletonScope()`
- ViewModels and components (only when exported cross-module) → `.inTransientScope()`

### 3. Expose to other modules (cross-module access only)

If another module needs to inject this service, export it via `{Module}ExportedTypes.ts`:

```ts
// DomainExportedTypes.ts
const DomainExportedTypes = {
    Services: {
        MySharedService: new AppServices.ServiceIdentifier<IMySharedService>(
            "Domain.Services.MySharedService"
        ),
    }
}
export { DomainExportedTypes }
```

Then merge into `AppTypes.ts`:

```ts
import { DomainExportedTypes } from '../Domain/DomainExportedTypes';

const Types = {
    // ...
    Domain: DomainExportedTypes,
};
```

### 4. Resolve in a ViewModel

```ts
constructor(
    taskRunner = AppService.get(Types.Neo.TaskRunner),
    private queryClient = AppService.get(Types.Domain.ApiClients.MyEntityQueryApiClient),
    private commandClient = AppService.get(Types.Domain.ApiClients.MyEntityCommandApiClient)) {
    super(taskRunner);
    this.makeObservable();
}
```

## Quality checklist

- [ ] `ServiceIdentifier` generic type is the **interface**, not the concrete class
- [ ] Identifier string follows `"Module.Category.ClassName"` format
- [ ] Concrete class bound in Module file with correct scope (`.inSingletonScope()` for clients/services)
- [ ] Cross-module types exported via `ExportedTypes.ts` and merged into `AppTypes.ts`
- [ ] Module boot order in `AppSetup.ts` unchanged (Domain registers last by convention)
