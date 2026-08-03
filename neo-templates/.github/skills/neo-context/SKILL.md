---
name: neo-context
description: >
  Searches for and loads Neo framework context for this project.
  Use when working with Neo packages (Neo.Core, Neo.Model, Neo.Jobs, neo-react, etc.),
  when implementing features that use Neo APIs, or when asked about Neo conventions,
  patterns, or available functionality in this codebase.
---

# Neo Framework Context

The Neo framework is Singular's proprietary set of NuGet and npm packages that underpin this application. The full source code for every Neo package is available on local disk. **Always search those local source trees to look up APIs, interfaces, and patterns — do not attempt to decompile NuGet packages.**

## Resolving the Neo Source Root

Before searching any Neo source, run the discovery script bundled with this skill to find the correct root path on this machine:

```powershell
$neoRoot = & "$skillDir\find-neo-root.ps1"
```

The script resolves the path in this order:

1. **`NEO_SOURCE` environment variable** — set this once in your shell profile if your Neo repos are in a non-standard location: `$env:NEO_SOURCE = "C:\dev\neo"`
2. **`.github/neo-local.env`** in the repo root — add a line `NEO_SOURCE=C:\dev\neo` (this file is gitignored, so it is safe for per-developer overrides)
3. **`../neo` relative to the repository root** — the default convention (works if the project repo and the `neo` repos share a common parent folder)

### If the Neo repos are not found

If `find-neo-root.ps1` exits with a non-zero code the repos have not been cloned yet. In that case, use `ask_user` to ask the developer whether they want to pull them now:

> "The Neo source repos were not found on this machine. Would you like me to clone them now? They will be placed at `<resolved-target-dir>` (override by setting `NEO_SOURCE` or adding it to `.github/neo-local.env`)."

Offer two choices: **"Yes, clone the Neo repos"** and **"No, I'll set up the path myself"**.

If the developer chooses to clone, determine the target directory using the same priority order as `find-neo-root.ps1` (env var → local env file → `../neo` default), then run:

```powershell
$neoRoot = & "$skillDir\pull-neo-repos.ps1" -TargetDir "<resolved-target-dir>"
```

The script clones any missing repos and runs `git pull` on repos that are already present. Once it exits with code 0, `$neoRoot` holds the path and you can continue with the steps below. If the script fails, report the error to the developer and stop — do not attempt to search Neo source paths that do not exist.

If the developer declines, inform them of the three configuration options listed above and stop.

Once `$neoRoot` is resolved, substitute it for `{NEO_ROOT}` in all paths in the tables below.

When helping with Neo-related work, follow this process before generating any code.

## Step 1 — Load Core Neo Documentation

Check whether this project has Neo-specific instruction files and read any that exist:

- `.github/instructions/neo.instructions.md` — package catalogue and DB Script Runner standards
- `.github/instructions/backend.instructions.md` — C# coding conventions and patterns that interact with Neo
- `.github/instructions/backend-authentication-authorization.instructions.md` — if the task involves authorization
- `.github/instructions/identity.instructions.md` — if the task involves identity

## Step 2 — Search the Project Codebase for Existing Usage Patterns

After reading the Neo instructions, if more context is needed, search the current project to see how it already uses the same API so new code stays consistent. Run these from the repository root.

## Step 3 — Identify the Relevant Neo Package(s)

If more context is needed, use the tables below to map the package you need to its local source path. Then search that path directly.

### Backend (NuGet) — Local Source Paths

Each package lives in its own subfolder named exactly after the package (e.g. `Neo.Jobs` → `…\Neo.Jobs\`). The table below shows the source root for each repo and which packages it contains.

| Repo | Source Root | Packages |
|---|---|---|
| neo-core | `{NEO_ROOT}\neo-core\Source\` | `Neo.Core`, `Neo.Core.Web`, `Neo.Files`, `Neo.IntegrityChecking`, `Neo.Jobs`, `Neo.Model`, `Neo.Model.App`, `Neo.Model.Audit`, `Neo.Model.Excel`, `Neo.Model.Excel.EPPlus`, `Neo.Model.Import`, `Neo.Model.Import.Excel`, `Neo.Model.ISO`, `Neo.Model.Json`, `Neo.Model.OData`, `Neo.Model.Parsing`, `Neo.Model.Serilog`, `Neo.Model.SqlServer`, `Neo.Model.Swagger`, `Neo.Model.Testing`, `Neo.Core.Testing`, `Neo.OneTimeTokens`, `Neo.Polly`, `Neo.RabbitMQ`, `Neo.SqlServer.AuditExporter`, `Neo.AuthorisationServer.Api`, `Neo.AuthorisationServer.Client`, `Neo.AuthorisationServer.Core`, `Neo.AuthorisationServer.Models`, `Neo.AuthorisationServer.RabbitMQ`, `Neo.Identity.Api`, `Neo.Identity.Api.OpenIddict`, `Neo.Identity.Clients`, `Neo.Identity.Clients.OpenIddict`, `Neo.Identity.Core`, `Neo.Identity.Core.OpenIddict`, `Neo.IdentityServer.App.OpenIddict`, `Neo.IdentityServer.Core.OpenIddict`, `Neo.IdentityServer.Models`, `Neo.IdentityServer.Models.OpenIddict`, `Neo.Azure.Blob`, `Neo.Azure.Blob.FileStore`, `Neo.Azure.Core`, `Neo.Azure.KeyVault`, `Neo.Azure.ServiceBus`, `Neo.NotificationServer.Api`, `Neo.NotificationServer.Client`, `Neo.NotificationServer.Core`, `Neo.NotificationServer.Models`, `Neo.BulkNotifications.App`, `Neo.BulkNotifications.Contracts`, `Neo.BulkNotifications.Models`, `Neo.Reporting.Api`, `Neo.Reporting.App`, `Neo.Reporting.Contracts`, `Neo.Reporting.Html`, `Neo.Reporting.Models`, `Neo.Reporting.Pdf`, `Neo.CanvasGrid.Server`, `Neo.CanvasGrid.Server.EPPlus`, `Neo.Correspondence`, `Neo.Email.SendGrid`, `Neo.Sms.Core`, `Neo.Sms.Cellfind` |
| neo-analyzers | `{NEO_ROOT}\neo-analyzers\Source\` | `Neo.Analyzers` |
| neo-tools | `{NEO_ROOT}\neo-tools\Neo.DbScriptRunner\` | `Neo.DbScriptRunner` |

### Frontend (npm) — Local Source Paths

| Package | Local Source Path | Notes |
|---|---|---|
| `@singularsystems/neo-core` | `{NEO_ROOT}\neo-ui\core\src` | Core models, utilities, observables |
| `@singularsystems/neo-react` | `{NEO_ROOT}\neo-ui\react\src` | React components, ViewBase, NeoModel |
| `@singularsystems/neo-react-services` | `{NEO_ROOT}\neo-ui\react-services\src` | React services layer |
| `@singularsystems/neo-authorisation` | `{NEO_ROOT}\neo-core\Source\AuthorisationServer\Client\src` | Frontend authorization client |
| `@singularsystems/neo-notifications` | `{NEO_ROOT}\neo-core\Source\NotificationServer\Client\src` | Notification client |
| `@singularsystems/neo-reporting` | `{NEO_ROOT}\neo-core\Source\Reporting\Client\src` | Report/PDF viewer |
| `@singularsystems/neo-canvas-grid` | `{NEO_ROOT}\neo-core\Source\CanvasGrid\Client\src` | Canvas data grid |

## Step 4 — Search the Neo Source for the API / Interface You Need

Once you know the local path, search it directly. Examples:

```powershell
# Find an interface in Neo.Jobs  →  source root + package name = exact folder
Get-ChildItem "{NEO_ROOT}\neo-core\Source\Neo.Jobs" -Recurse -Filter "*.cs" |
    Select-String "interface IJob" | Select-Object Path, LineNumber, Line

# Find a method in Neo.Model.SqlServer
Get-ChildItem "{NEO_ROOT}\neo-core\Source\Neo.Model.SqlServer" -Recurse -Filter "*.cs" |
    Select-String "AddSoftDeleteQueryFilters" | Select-Object Path, LineNumber, Line

# Find a frontend type in neo-react
Get-ChildItem "{NEO_ROOT}\neo-ui\react\src" -Recurse -Filter "*.ts" |
    Select-String "class ViewBase" | Select-Object Path, LineNumber, Line
```

Or use the grep tool directly:
```
# Backend: search a specific package folder
grep -r "IJobScheduler" "{NEO_ROOT}\neo-core\Source\Neo.Jobs" --include="*.cs" -n

# Backend: search across the whole repo source root when unsure which package
grep -r "IJobScheduler" "{NEO_ROOT}\neo-core\Source" --include="*.cs" -n

# Frontend: find a type in neo-core
grep -r "NeoModel" "{NEO_ROOT}\neo-ui\core\src" --include="*.ts" -n
```

After finding the file, read it with the view tool to understand the full API before writing code.

### Backend searches

```
# Multi-tenancy
grep -r "IMultiTenancyDbContext\|ITenantEntity\|ITenantService\|RunWithOverrideTenantIdAsync" Server/ --include="*.cs" -l

# Background jobs
grep -r "IJob\b\|IJobScheduler\|Neo\.Jobs" Server/ --include="*.cs" -l

# Authorization / roles
grep -r "RequireRole\|AdminRoles" Server/ --include="*.cs" -l

# Notifications
grep -r "INotificationService\|NotificationServer" Server/ --include="*.cs" -l
```

### Frontend searches

```
# Views and ViewModels
grep -r "ViewBase\|@NeoModel\|@observer" src/ --include="*.tsx" --include="*.ts" -l

# DI / service resolution
grep -r "AppService\.get\|Types\." src/ --include="*.ts" --include="*.tsx" -l

# API calls
grep -r "TaskRunner\|ApiClient" src/ --include="*.ts" --include="*.tsx" -l
```

Adjust the `Server/` and `src/` paths to match the project layout if needed.