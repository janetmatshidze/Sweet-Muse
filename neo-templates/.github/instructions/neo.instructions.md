---
applyTo: "**/*"
---

## Neo

The Neo framework is a set of NuGet packages providing the following functionality.

> **Looking up Neo APIs?** Use the `/neo-context` skill — it maps every package to its local source tree so you can search the code directly rather than relying on package documentation.

### Backend

#### Packages

- [Neo.Analyzers](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Analyzers): Code analysis rules
- [Neo.AuthorisationServer.Api](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.AuthorisationServer.Api) and `Neo.AuthorisationServer.*`: Authorisation
- [Neo.Azure.Blob](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Azure.Blob): Blob management
- [Neo.Azure.KeyVault](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Azure.KeyVault): Secrets management
- [Neo.Azure.ServiceBus](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Azure.ServiceBus): Service bus management
- [Neo.Core](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Core): Core utilities
- [Neo.DbScriptRunner](https://github.com/SingularSystems/neo-tools/pkgs/nuget/Neo.DbScriptRunner): Auditable SQL data adjustments in production using [DbUp](https://www.nuget.org/packages/dbup).
- [Neo.Identity.Api.OpenIddict](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Identity.Api.OpenIddict) and `Neo.Identity.*`: Identity/authentication
- [Neo.Jobs](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Jobs): Background job scheduling
- [Neo.Model](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Model): Entity base classes, DbContext helpers
- [Neo.Model.SqlServer](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Model.SqlServer): SQL Server integration
- [Neo.Model.Swagger](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Model.Swagger): API documentation
- [Neo.NotificationServer.Api](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.NotificationServer.Api) and `Neo.NotificationServer.*`: Notifications
- [Neo.Reporting.Api](https://github.com/SingularSystems/neo-packages/pkgs/nuget/Neo.Reporting.Api) and `Neo.Reporting.*`: Report/PDF generation

### Frontend

#### Packages

- [@singularsystems/neo-core](https://www.npmjs.com/package/@singularsystems/neo-core): Core functionality for the Neo client library
- [@singularsystems/neo-authorisation](https://www.npmjs.com/package/@singularsystems/neo-authorisation): the front end client for the neo authorisation server service.
- [@singularsystems/neo-canvas-grid](https://www.npmjs.com/package/@singularsystems/neo-canvas-grid): the Neo canvas grid.
- [@singularsystems/neo-integrity-checking](https://www.npmjs.com/package/@singularsystems/neo-integrity-checking): integrity checking.
- [@singularsystems/neo-notifications](https://www.npmjs.com/package/@singularsystems/neo-notifications): the Neo notification server.
- [@singularsystems/neo-react](https://www.npmjs.com/package/@singularsystems/neo-react): React application logic and components for the Neo client library
- [@singularsystems/neo-react-services](https://www.npmjs.com/package/@singularsystems/neo-react-services): Neo React services
- [@singularsystems/neo-reporting](https://www.npmjs.com/package/@singularsystems/neo-reporting): the Neo reporting server

## Related Skills

- [neo-context](../skills/neo-context/SKILL.md) — Look up Neo APIs and usage patterns from local Neo source
- [backend-db-script-runner](../skills/backend-db-script-runner/SKILL.md) — Create and submit an auditable SQL data fix
