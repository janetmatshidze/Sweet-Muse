# Copilot Instructions

Version 0.3

## Skills Index

For step-by-step workflows (creating views, adding routes, defining roles, etc.), see the **[Skills Index](skills/README.md)**.

## Tech Stack

Unless specified in a project instructions file, assume the following tech stack.

#### Backend (.NET)
- **.NET 10**
- **C# 14.0** (LangVersion 14.0)
- **Entity Framework Core 10** for data access with SQL Server
- **SQL Server Always Encrypted** for sensitive data
- **Neo Framework** (Singular's proprietary framework for model apps, identity, authorization, notifications, reporting)
- **ASP.NET Core** Web APIs
- **OpenIddict** for OAuth2/OIDC authentication
- **Serilog** for structured logging
- **Azure Key Vault** for secret management
- **IronPdf** for PDF generation
- **EPPlus** for Excel operations
- **SignalR** for real-time notifications
- **Quartz** for job scheduling
- **xUnit.v3** for unit testing

#### Infrastructure
- **Docker** with BuildKit
- **Kubernetes/Helm** for deployment
- **Azure** cloud platform (AKS, Key Vault, Storage, etc.)
- **GoCD** for CI/CD pipelines
- **Terraform** for infrastructure provisioning
- **GitHub** for source control

## Backend Architecture

- **CQRS-lite**: separate `{Domain}QueryController` / `{Domain}CommandController` and matching services
- Controllers stay thin — delegate to services; never return or receive Models directly (use Lookups/Commands)
- Migrations in `*.Models.Migrations` projects; NuGet versions in `Directory.Packages.props`
- Config: layered `appsettings.json` + Options Pattern; validate at startup with `ConfigurationErrorsException`
- Multi-tenancy: all data scoped via `TenantId`; DbContexts implement `IMultiTenancyDbContext`
- Audit trail: use `AuditTrailProcessor`; entities implement `IAuditTrailEntity` / `ITemporalEntity` (SQL Server) or `AuditInclude` attribute
- Background jobs: Neo.Jobs + Quartz
- Service communication: HTTP clients; contracts in `*.Contracts` projects
- **Do not** use MediatR. **Do not** use the Repository Pattern.

## Unit Testing

- Framework: [xUnit.v3](https://www.nuget.org/packages/xunit.v3)
- Mock via interface implementations — do **not** use [Moq](https://www.nuget.org/packages/Moq)
- Use [Microsoft.EntityFrameworkCore.InMemory](https://www.nuget.org/packages/Microsoft.EntityFrameworkCore.InMemory) for DB contexts
- Test public, protected, and internal methods

## Code review

See [code-review.instructions.md](instructions/code-review.instructions.md).

## Instruction Files

| File | Area |
|---|---|
| [backend.instructions.md](instructions/backend.instructions.md) | Backend C# standards |
| [backend-authentication-authorization.instructions.md](instructions/backend-authentication-authorization.instructions.md) | Backend Authentication & Authorization |
| [backend-catalogue.instructions.md](instructions/backend-catalogue.instructions.md) | Backend Catalogue |
| [backend-exception-handling.instructions.md](instructions/backend-exception-handling.instructions.md) | Backend Exception handling |
| [backend-logging.instructions.md](instructions/backend-logging.instructions.md) | Backend Logging |
| [backend-sql.instructions.md](instructions/backend-sql.instructions.md) | SQL Server standards |
| [code-review.instructions.md](instructions/code-review.instructions.md) | Code Review standards |
| [frontend.instructions.md](instructions/frontend.instructions.md) | Frontend overview & architecture |
| [frontend-api-clients.instructions.md](instructions/frontend-api-clients.instructions.md) | Neo Frontend API clients |
| [frontend-authorization.instructions.md](instructions/frontend-authorization.instructions.md) | Frontend Authorization |
| [frontend-catalogue.instructions.md](instructions/frontend-catalogue.instructions.md) | Frontend Catalogue |
| [frontend-coding-standards.instructions.md](instructions/frontend-coding-standards.instructions.md) | Frontend coding standards |
| [frontend-components.instructions.md](instructions/frontend-components.instructions.md) | Neo Frontend components |
| [frontend-di-modules.instructions.md](instructions/frontend-di-modules.instructions.md) | Neo Frontend DI modules |
| [frontend-models.instructions.md](instructions/frontend-models.instructions.md) | Neo Frontend models & data types |
| [frontend-routing.instructions.md](instructions/frontend-routing.instructions.md) | Frontend Routing |
| [frontend-styling.instructions.md](instructions/frontend-styling.instructions.md) | Frontend styling standards |
| [frontend-views.instructions.md](instructions/frontend-views.instructions.md) | Neo Frontend Views & ViewModels |
| [identity.instructions.md](instructions/identity.instructions.md) | Identity |
| [neo.instructions.md](instructions/neo.instructions.md) | Neo framework packages |
| [neo-quality.instructions.md](instructions/neo-quality.instructions.md) | Quality gates |
| [project/project.instructions.md](instructions/project/project.instructions.md) | Project overview & settings |
| [project/project-backend.instructions.md](instructions/project/project-backend.instructions.md) | Project specific backend conventions |
| [project/project-frontend-api.instructions.md](instructions/project/project-frontend-api.instructions.md) | Project specific API client conventions |
| [project/project-frontend-di-modules.instructions.md](instructions/project/project-frontend-di-modules.instructions.md) | Project specific DI conventions |
| [project/project-frontend-models.instructions.md](instructions/project/project-frontend-models.instructions.md) | Project specific model conventions |
| [project/project-frontend-views.instructions.md](instructions/project/project-frontend-views.instructions.md) | Project specific view conventions |
| [project/project-security.instructions.md](instructions/project/project-security.instructions.md) | Project specific Security model |