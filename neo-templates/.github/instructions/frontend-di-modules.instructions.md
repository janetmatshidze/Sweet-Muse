---
description: "Use when registering new services, API clients, or types in the DI container. Covers AppServices.Module, AppServices.ServiceIdentifier, module registration, scope rules, exported types, and service resolution."
applyTo: ["src/**/*Types.ts", "src/**/*Module.ts", "src/**/*ExportedTypes.ts"]
---
# Neo Dependency Injection & Modules

## Related Skills

- [frontend-register-di-service](../skills/frontend-register-di-service/SKILL.md) — Step-by-step: register a new API client or service in DI

## Overview

## Type Symbols (ServiceIdentifier)

Type symbols define the DI identifiers. Each service/client gets a `ServiceIdentifier` typed to its interface.

```ts
import { AppServices } from '@singularsystems/neo-core';
import { IMyEntityQueryApiClient } from './ApiClients/MyArea/MyEntityQueryApiClient';

const MyModuleTypes = {
    ApiClients: {
        MyEntityQueryApiClient: new AppServices.ServiceIdentifier<IMyEntityQueryApiClient>("MyModule.ApiClients.MyEntityQueryApiClient"),
    },
    Services: {
        DataCache: new AppServices.ServiceIdentifier<MyDataCache>("MyModule.Services.DataCache"),
    }
}
```

### Rules

- String identifier format: `"ModuleName.Category.ClassName"`
- Generic type is always the **interface** (`IXxxApiClient`), not the concrete class
- Group by `ApiClients`, `Services`, etc. inside the module's type object

## Module Registration (Binding)

Modules bind concrete classes to their type symbols.

```ts
import { AppServices } from '@singularsystems/neo-core';
import { MyModuleTypes } from './MyModuleTypes';
import MyEntityQueryApiClient from './ApiClients/MyArea/MyEntityQueryApiClient';
import { MyDataCache } from './Services/MyDataCache';

export const MyModule = new AppServices.Module("MyModule", container => {

    // Api Clients
    container.bind(MyModuleTypes.ApiClients.MyEntityQueryApiClient).to(MyEntityQueryApiClient).inSingletonScope();

    // Services
    container.bind(MyModuleTypes.Services.DataCache).to(MyDataCache).inSingletonScope();
});
```

### Scope Rules

- **ApiClient and Service** bindings use `.inSingletonScope()`
- **ViewModel and Component** bindings use `.inTransientScope()` — only required when exporting outside of the module for use in other modules
- Group bindings by category with comments: `// Api Clients`, `// Services`

## Exported Types (Cross-Module Access)

When module A needs types from module B, use an exported types file:

```ts
// MyModuleExportedTypes.ts
import { AppServices } from '@singularsystems/neo-core';

const MyModuleExportedTypes = {
    Services: {
        SomeService: new AppServices.ServiceIdentifier<ISomeService>("MyModule.Services.SomeService"),
    }
}

export { MyModuleExportedTypes }
```

These are imported into `AppTypes.ts` and merged into the global Types object to make them accessible from other modules.

## Resolving Services

In ViewModels and services, resolve via constructor defaults:

```ts
constructor(
    taskRunner = AppService.get(Types.Neo.TaskRunner),
    private apiClient = AppService.get(Types.Domain.ApiClients.SomeApiClient)) {
    super(taskRunner);
}
```

In Views and Components, resolve via linked ViewModel. If no ViewModel, resolve directly:

```ts
const service = AppService.get(Types.Domain.Services.SomeService);
```
