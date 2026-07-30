---
description: "Use when registering services or types in the DI container. Covers the DomainTypes, DomainModule, AppTypes structure, and module boot order conventions."
applyTo: ["src/**/*Types.ts", "src/**/*Module.ts", "src/**/*ExportedTypes.ts", "src/**/AppSetup.ts"]
---
# Project DI Conventions

## Related Skills

- [frontend-register-di-service](../../skills/frontend-register-di-service/SKILL.md) — Register a new API client or service in DI
- [frontend-create-api-client](../../skills/frontend-create-api-client/SKILL.md) — Create the API client before registering it

## DomainTypes.ts

Type symbols for the Domain module follow the identifier format `"Domain.Category.ClassName"`.

```ts
import { AppServices } from '@singularsystems/neo-core';
import { AppService, Types as AppTypes } from '../App/Services/AppService';
import { I{Entity}CommandApiClient } from './ApiClients/{DomainArea}/{Entity}CommandApiClient';
import { I{Entity}QueryApiClient } from './ApiClients/{DomainArea}/{Entity}QueryApiClient';

const DomainTypes = {
    ApiClients: {
        {Entity}CommandApiClient: new AppServices.ServiceIdentifier<I{Entity}CommandApiClient>("Domain.ApiClients.{Entity}CommandApiClient"),
        {Entity}QueryApiClient: new AppServices.ServiceIdentifier<I{Entity}QueryApiClient>("Domain.ApiClients.{Entity}QueryApiClient"),
    },
    Services: {
        DataCache: new AppServices.ServiceIdentifier<DomainDataCache>("Domain.Services.DataCache"),
    }
}

// Merge with App types for convenience
const Types = {
    ...AppTypes,
    Domain: DomainTypes
}

export { AppService, Types, DomainTypes }
```

## DomainModule.ts

```ts
import { AppServices } from '@singularsystems/neo-core';
import { DomainTypes } from './DomainTypes';
import {Entity}CommandApiClient from './ApiClients/{DomainArea}/{Entity}CommandApiClient';
import {Entity}QueryApiClient from './ApiClients/{DomainArea}/{Entity}QueryApiClient';

export const DomainAppModule = new AppServices.Module("Domain", container => {

    // Api Clients
    container.bind(DomainTypes.ApiClients.{Entity}CommandApiClient).to({Entity}CommandApiClient).inSingletonScope();
    container.bind(DomainTypes.ApiClients.{Entity}QueryApiClient).to({Entity}QueryApiClient).inSingletonScope();

    // Services
    container.bind(DomainTypes.Services.DataCache).to(DomainDataCache).inSingletonScope();
});
```

## DomainExportedTypes.ts (Cross-Module Access)

When types need to be accessed from other modules:

```ts
import { AppServices } from '@singularsystems/neo-core';

const DomainExportedTypes = {
    Services: {
        {ServiceName}: new AppServices.ServiceIdentifier<I{ServiceName}>("Domain.Services.{ServiceName}"),
    }
}

export { DomainExportedTypes }
```

## AppTypes.ts

The root type object merging all module exported types:

```ts
import { NeoReactTypes } from '@singularsystems/neo-react';
import { DomainExportedTypes } from '../Domain/DomainExportedTypes';

const Types = {
    App: {
        ApiClients: { /* app-level client types */ },
        Services: { /* app-level service types */ },
        Config: AppServices.NeoTypes.Config.ConfigModel.asType<AppConfig>(),
    },
    Neo: NeoReactTypes,
    Domain: DomainExportedTypes,
    // Add further module exported types here as the project grows
};

export default Types;
```

## Module Boot Order (AppSetup.ts)

When registering a new module, follow the order already established in `AppSetup.ts` — do not change the ordering of existing modules without understanding their dependency graph.

The general principle is:
- Neo core and React modules register first
- Auth, Identity, and infrastructure modules register next
- App-level modules register before domain modules
- Domain module(s) register last

Check `AppSetup.ts` for the authoritative order for this project.
