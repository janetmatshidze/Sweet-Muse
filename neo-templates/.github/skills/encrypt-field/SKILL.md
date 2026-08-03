---
name: encrypt-field
description: >
  Guides implementation of SQL Server Always Encrypted for entity fields.
  Use when you need to encrypt sensitive data at rest using column-level encryption —
  such as PII, financial data, or credentials —
  with Azure Key Vault as the key store provider.
---

# SQL Server Always Encrypted — Column Encryption Pattern

Use this pattern to encrypt sensitive entity fields at rest using SQL Server Always Encrypted
with Azure Key Vault as the column master key store. The skill covers both first-time project
setup and per-field encryption.

---

## How It Works

SQL Server Always Encrypted protects sensitive data using a two-tier key hierarchy:

1. **Column Master Key (CMK)** — stored in Azure Key Vault. Never leaves the vault.
2. **Column Encryption Key (CEK)** — encrypted by the CMK and stored in the database.
   The SQL client driver decrypts the CEK in memory using the CMK, then uses it to
   encrypt/decrypt column values on the client side.

Data is encrypted **before** it reaches SQL Server and decrypted **after** it leaves.
SQL Server never sees plaintext values. With **secure enclaves** enabled, SQL Server can
perform server-side filtering, sorting, and pattern matching on encrypted data inside a
trusted enclave — without exposing plaintext to the database engine.

### Encryption Types

| Type | Behaviour | Use When |
|------|-----------|----------|
| **Randomized** (default) | Same plaintext produces different ciphertext each time. More secure. | The column is only stored/displayed — never filtered, sorted, joined, or indexed. |
| **Deterministic** | Same plaintext always produces the same ciphertext. Enables equality comparison. | The column is used in `WHERE` clauses, unique indexes, joins, or `GROUP BY`. |

> **Enclave note:** When `EnclaveEnabled` is `true`, the Neo framework automatically
> upgrades deterministic encryption to randomized because the enclave provides server-side
> query operations (LIKE, range, sorting) regardless of encryption type. This gives you the
> security benefits of randomized encryption with the query flexibility of deterministic.

---

## Detection — Is Always Encrypted Already Set Up?

Before performing first-time setup, check whether the project already has encryption configured:

1. **Search for the registration call:**
   ```
   grep -r "AddNeoSqlAlwaysEncryptedWithKeyVaultAsKeyStoreProvider" --include="*.cs"
   ```
2. **Search for the config section:**
   ```
   grep -r "AlwaysEncrypted" --include="*.json"
   ```
3. **Search for existing encrypted columns:**
   ```
   grep -r "EncryptedColumn" --include="*.cs"
   ```

If all three return results, skip to [Encrypting a Field](#step-4--decorate-the-entity-property).
If none return results, follow the full setup below.

---

## First-Time Setup

### Step 1 — Add NuGet Packages

Add the following packages to the appropriate projects. Versions are managed centrally in
`Directory.Packages.props`.

| Package | Target Project | Purpose |
|---------|---------------|---------|
| `Neo.Model.SqlServer` | The `*.Models` project where entities are defined | Provides the `[EncryptedColumn]` attribute, `IColumnEncryptionService`, encryption builders |
| `Neo.Azure.KeyVault` | The `*.Api` / host project where services are registered | Provides `AddNeoSqlAlwaysEncryptedWithKeyVaultAsKeyStoreProvider` for Key Vault integration |

Add to `Directory.Packages.props` if not already present:

```xml
<PackageVersion Include="Neo.Model.SqlServer" Version="1.1.1" /> <!-- Use the version already pinned by your solution (CPM). -->
<PackageVersion Include="Neo.Azure.KeyVault" Version="1.1.1" /> <!-- Use the version already pinned by your solution (CPM). -->
```

Then add `<PackageReference>` entries (version-less, since CPM is used) to each project file.

---

### Step 1a — Enable VBS Enclaves on Developer Machine

If you enable secure enclaves (`AlwaysEncrypted:EnclaveEnabled = true`), secure enclaves must be enabled on the local SQL Server instance. Run the following SQL to check the current enclave status:

```sql
SELECT [value],
  CASE [value] WHEN 0 THEN 'No enclave' WHEN 1 THEN 'VBS' ELSE 'Other' END AS [value_description],
  [value_in_use],
  CASE [value_in_use] WHEN 0 THEN 'No enclave' WHEN 1 THEN 'VBS' ELSE 'Other' END AS [value_in_use_description]
FROM sys.configurations
WHERE [name] = 'column encryption enclave type';
```

- `value` — what the server is capable of
- `value_in_use` — what is currently configured

If `value` is `0` (no enclave), run the following PowerShell script **as Administrator**
to enable VBS and configure SQL Server. **Reboot after running.**

```powershell
[CmdletBinding()]
param(
  [string]$ServerName = "(local)"
)

# Ensure the SqlServer module is installed
if (-not (Get-Module -ListAvailable -Name SqlServer)) {
  Write-Host "Installing SqlServer module"
  Install-Module -Name SqlServer -Force
}

Write-Host "Enabling VBS Secure Enclaves"

$regkey = "HKLM:\SYSTEM\CurrentControlSet\Control\DeviceGuard"
$keyName = "EnableVirtualizationBasedSecurity"
$setupVBS = $true
if ((Get-ItemProperty $regkey).PSObject.Properties.Name -contains $keyName -eq $true) {
  Get-ItemProperty -Path $regkey -Name $keyName | ForEach-Object {
    if ($_.EnableVirtualizationBasedSecurity -eq 1) {
      Write-Host "Virtualisation Based Security (VBS) is already enabled."
      $setupVBS = $false
    }
  }
}

if ($setupVBS -eq $true) {
  Write-Host "Enabling Virtualisation Based Security (VBS) in registry"
  Set-ItemProperty -Path HKLM:\SYSTEM\CurrentControlSet\Control\DeviceGuard `
    -Name EnableVirtualizationBasedSecurity -Value 1
}

Write-Host "Enabling Column Encryption Enclave Type on SQL Server"
$sql = @"
  EXEC sys.sp_configure 'column encryption enclave type', 1;
  GO
  RECONFIGURE;
  GO
"@

Invoke-Sqlcmd -ServerInstance $ServerName -Query $sql -TrustServerCertificate

Write-Host "Restarting SQL Server"
Get-Service MSSQLSERVER | Restart-Service -Force
if (!$?) {
  Write-Host "`nFailed to restart SQL Server service. Please restart it manually." -ForegroundColor Yellow
}

Write-Host "Done"
```

After rebooting, re-run the SQL diagnostic query to confirm `value_in_use` is `1` (VBS).

---

### Step 2 — Update Connection Strings

Add the following parameters to every connection string that targets an encrypted database.

| Parameter | Required | Purpose |
|-----------|----------|---------|
| `Column Encryption Setting=Enabled` | Yes | Enables the Always Encrypted integration in the SQL client |
| `MultipleActiveResultSets=True` | Yes | Required for queries run during in-place encryption of columns |
| `Persist Security Info=True` | Yes | Prevents the password from being removed after the initial connection (required because the connection string is accessed through the DbContext) |
| `Attestation Protocol=NONE` | Dev only | Skips attestation for local VBS enclaves |

#### Development (`appsettings.Development.json`)

```json
"ConnectionStrings": {
  "Main": "Server=(local); Database=MyApp.Domain; Trusted_Connection=True; Encrypt=True; TrustServerCertificate=True; Connection Timeout=30; Column Encryption Setting=Enabled; MultipleActiveResultSets=True; Persist Security Info=True; Attestation Protocol=NONE;"
}
```

> **Local dev requirement:** Developers must run SQL Server locally with
> **VBS (Virtualization-Based Security) enclaves** enabled. See
> [Step 1a — Enable VBS Enclaves on Developer Machine](#step-1a--enable-vbs-enclaves-on-developer-machine)
> for setup instructions.

#### Production / Deployed Environments

```
Column Encryption Setting=Enabled; MultipleActiveResultSets=True; Persist Security Info=True;
```

The attestation protocol for deployed environments is configured per-environment in the
deployment configuration (e.g. Helm values, appsettings template). Typically:

- `Attestation Protocol=HGS` — for environments using Host Guardian Service
- `Attestation Protocol=AAS` — for environments using Azure Attestation Service
- `Attestation Protocol=NONE` — for environments where enclave attestation is not required

---

### Step 3 — Add the `AlwaysEncrypted` Configuration Section

#### `appsettings.json` (base/production defaults)

```json
"AlwaysEncrypted": {
  "Enabled": true,
  "KeyStoreProviderName": "AZURE_KEY_VAULT",
  "DatabaseKeyPrefix": "",
  "EnclaveEnabled": true,
  "DropDefaultConstraints": true
}
```

| Setting | Default | Purpose |
|---------|---------|---------|
| `Enabled` | `true` | Master switch for encryption operations. Set to `true` in environments where encryption/migrations should run. |
| `KeyStoreProviderName` | — | **Required.** Key store provider identifier. Always `AZURE_KEY_VAULT`. |
| `DatabaseKeyPrefix` | — | **Required.** Prefix for CMK/CEK names in the database. Format: `{project-abbrev}-{environment}`. Set per-environment in deployment config. Leave empty in base `appsettings.json`. |
| `EnclaveEnabled` | `false` | Enable secure enclave support. Set to `true` to allow server-side `LIKE`, range queries, and sorting on encrypted columns. |
| `DropDefaultConstraints` | `false` | Auto-drop default constraints on columns before encryption. Set to `true` — Always Encrypted does not support default constraints. Application-level defaults (EF `HasDefaultValue`) still work. |
| `MasterEncryptionKeyName` | `CMK1` | Name for the CMK record in the database. Generally does not need to be changed. |
| `ColumnEncryptionKeyName` | `CEK1` | Name for the CEK record in the database. Generally does not need to be changed. |
| `CopyCommandsTimeout` | `120` | Seconds to wait for each batch copy during in-place encryption. Increase if you see timeout failures on large tables. |
| `AlterCommandsTimeout` | `600` | Seconds to wait for DDL operations when adding/altering encrypted columns. |
| `DataCopyBatchSize` | `100` | Records per batch during in-place encryption. Max 1000. Increase for large tables to improve throughput. |

> **⚠️ `DatabaseKeyPrefix` is permanent.** Once a key has been created with a given prefix,
> it cannot be renamed. Once data has been encrypted with that key, you may not be able to
> simply drop and recreate the database. Get this right the first time.

#### `appsettings.Development.json` (local dev override)

```json
"AlwaysEncrypted": {
  "DatabaseKeyPrefix": "myapp-ldev"
}
```

> The prefix follows the pattern `{project-abbreviation}-{environment}`.
> For example: `nect-ldev` (nectar local dev), `myapp-qa` (QA), `myapp-prod` (production).

---

### Step 3a — Register Encryption Services

In the API/host project's `StartupExtensions.cs` (or `Program.cs`), register the Always
Encrypted services:

```csharp
/// <summary>
/// Adds Always Encrypted services.
/// </summary>
/// <param name="services">The service collection.</param>
/// <param name="environment">The web host environment.</param>
/// <param name="configuration">The configuration.</param>
/// <returns>The service collection.</returns>
public static IServiceCollection AddAlwaysEncrypted(
  this IServiceCollection services,
  IWebHostEnvironment environment,
  IConfiguration configuration)
{
  return services.AddNeoSqlAlwaysEncryptedWithKeyVaultAsKeyStoreProvider(configuration, "Main");
}
```

The `"Main"` parameter is the Key Vault configuration key — it tells the provider which
Key Vault connection to use for retrieving credentials. This corresponds to the `Key`
property in the project's Key Vault configuration:

```json
"KeyVaults": [
  {
    "Key": "Main",
    "Name": "myapp-dev-kv-main",
    "AuthenticationMethod": "DefaultCredential",
    "Enabled": true,
    "AddAsConfigProvider": true,
    "InjectSecrets": true
  }
]
```

Call this method during service registration:

```csharp
services.AddAlwaysEncrypted(environment, configuration);
```

> **Required using:**
> ```csharp
> using Neo.Azure.KeyVault.Extensions;
> ```

---

### Step 3b — Add Database Key Initialisation to the Async Initializer

The CMK and CEK must be created in the database before any columns can be encrypted.
This is done in an `IAsyncInitializer` that runs at application startup.

**If an `IAsyncInitializer` already exists** for the target DbContext (e.g. a seed data
initializer or migration initializer), add the encryption configuration there.

**If no initializer exists**, create one:

```csharp
namespace MyApp.Models.Migrations.Initializers
{
  using System.Threading;
  using System.Threading.Tasks;
  using Extensions.Hosting.AsyncInitialization;
  using Microsoft.EntityFrameworkCore;
  using Microsoft.Extensions.Configuration;
  using Microsoft.Extensions.Logging;
  using Neo.Cryptography;
  using Neo.Extensions;

  /// <summary>
  /// Migrates the database and configures Always Encrypted keys.
  /// </summary>
  public class AppDbAsyncInitializer(
    AppDbContext dbContext,
    IConfiguration configuration,
    ILogger<AppDbAsyncInitializer> logger,
    IColumnEncryptionConfigurationService encryptionConfigurationService,
    IColumnEncryptionService columnEncryptionService) : IAsyncInitializer
  {
    /// <inheritdoc/>
    public async Task InitializeAsync(CancellationToken cancellationToken)
    {
      await dbContext.Database.MigrateAsync(cancellationToken);

      if (configuration.GetBoolean("AlwaysEncrypted:Enabled", false) ?? false)
      {
        await this.ConfigureDatabaseEncryptionAsync();
      }
    }

    private async Task ConfigureDatabaseEncryptionAsync()
    {
      // Initialise the database keys (CMK and CEK).
      // In these templates, the database key name is commonly "Main" (keep it consistent per database).
      await encryptionConfigurationService.ConfigureDatabaseAsync(dbContext, "Main");

      // Column encryption calls go here — see Step 5.
    }
  }
}
```

> **Key name guidance:** Use a meaningful name for the `ConfigureDatabaseAsync` second
> parameter that identifies the database or module. For example:
> - `"Domain"` for the main application database
> - `"Identity"` for the identity database
> - `"Reporting"` for a reporting database
>
> If multiple DbContexts target the **same** database, they share the same CEK —
> call `ConfigureDatabaseAsync` once with the same key name.
> If they target **different** databases, each gets its own call with a distinct key name.

---

### Step 3c — Infrastructure: Key Vault Access Policies (Terraform)

The following identities need **key permissions** (`get`, `unwrapKey`, `wrapKey`) on the
Azure Key Vault that stores the Column Master Key:

1. **API service identity** — the managed identity of the application service
2. **DB Script Runner service principal** — for running database migrations/scripts
3. **Developer identities** — via an Azure AD group for local development

Example Terraform configuration in `key-vault.tf`:

```hcl
# Key users who need access to database encryption keys.
# This includes the application service identity, the DB Script Runner
# service principal, and any developer groups.
key_users = merge(
  # Application data access groups
  var.access.data_readers.enabled ? module.aad_groups.groups.data_readers.principal_map : {},
  var.access.data_writers.enabled ? module.aad_groups.groups.data_writers.principal_map : {},

  # DB Script Runner needs key access for Always Encrypted migrations
  local.db_script_runner_service_principal != null ? {
    db_script_runner = {
      name      = local.db_script_runner_service_principal.service_principal.name
      object_id = local.db_script_runner_service_principal.service_principal.object_id
    }
  } : {}
)
```

The `key_users` map is passed to the Key Vault module which grants each identity the
required key permissions. The exact structure depends on your Terraform Key Vault module,
but the permissions needed are:

```hcl
key_permissions = ["Get", "List", "Create", "UnwrapKey", "WrapKey", "Verify", "Sign", "Decrypt", "Encrypt"]
```

> **Additional environments:** Staging, UAT, and other deployed environments follow the
> same pattern — each environment's Key Vault has its own access policies granting key
> permissions to that environment's service identities. The Terraform configuration is
> typically parameterised by environment, so the same module applies to all environments
> with different variable values.

---

## Encrypting a Field

### Step 4 — Decorate the Entity Property

Add the `[EncryptedColumn]` attribute to the property you want to encrypt:

```csharp
using System.Data;
using Neo.Model.SqlServer.AlwaysEncrypted;

public class Employee : ModelBase<Employee>
{
  public int EmployeeId { get; set; }

  [Required]
  [StringLength(100)]
  public string Name { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the employee's ID number. Encrypted at rest.
  /// </summary>
  [Required]
  [StringLength(20)]
  [EncryptedColumn(SqlDbType.NVarChar)]
  public string IdNumber { get; set; } = string.Empty;

  /// <summary>
  /// Gets or sets the employee's salary. Encrypted at rest.
  /// </summary>
  [EncryptedColumn(SqlDbType.Decimal, Precision = 18, Scale = 2)]
  public decimal Salary { get; set; }
}
```

#### `[EncryptedColumn]` Attribute Properties

| Property | Default | Notes |
|----------|---------|-------|
| `DataType` | — | **Required.** The `SqlDbType` of the column. See reference table below. |
| `EncryptionType` | `Randomized` | `Randomized` or `Deterministic`. See guidance below. |
| `Precision` | — | For numeric types (e.g. `decimal`). Must be specified explicitly. |
| `Scale` | — | For numeric types (e.g. `decimal`). Must be specified explicitly. |
| `Collation` | — | For encrypted string columns, use `Latin1_General_BIN2`. |
| `ColumnEncryptionKey` | `CEK1` | Name of the CEK to use. Generally does not need to be changed. |
| `EncryptionAlgorithm` | `AEAD_AES_256_CBC_HMAC_SHA_256` | Generally does not need to be changed. |

> **`[Column]` attribute:** In cases where the EF Core default column type does not match
> the `SqlDbType` specified in `[EncryptedColumn]`, add a `[Column(TypeName = "...")]`
> attribute to ensure the migration generates the correct column type. For example, a
> `DateTime` property mapped to `SqlDbType.Date` needs `[Column(TypeName = "date")]`.

> **String lengths:** Always specify a length attribute (e.g. `[MaxLength]` or
> `[StringLength]`) on encrypted string properties. As a rule of thumb, if the data type
> has size, precision, or scale options, specify them explicitly.

#### Choosing the Encryption Type

By default, `[EncryptedColumn]` uses **Randomized** encryption. To use **Deterministic**:

```csharp
[EncryptedColumn(SqlDbType.NVarChar, EncryptionType = EncryptionType.Deterministic, Collation = "Latin1_General_BIN2")]
public string EmailAddress { get; set; } = string.Empty;
```

| Use Randomized (default) when | Use Deterministic when |
|-------------------------------|----------------------|
| Column is only stored and displayed | Column is used in `WHERE` equality checks and there is no Secure Enclaves |
| No indexing on the column | Column participates in a unique index |
| Maximum security is required | Column is used in `JOIN` or `GROUP BY` |

> **With enclaves enabled** (recommended), the framework automatically upgrades
> deterministic to randomized because the enclave handles server-side query operations
> regardless. You get full query flexibility with the stronger randomized encryption.

#### SqlDbType Reference

| C# Type | SqlDbType | Notes |
|---------|-----------|-------|
| `string` | `SqlDbType.NVarChar` | Most common. Use for names, emails, addresses. |
| `int` | `SqlDbType.Int` | For encrypted integer identifiers |
| `long` | `SqlDbType.BigInt` | For encrypted long identifiers |
| `decimal` | `SqlDbType.Decimal` | Set `Precision` and `Scale`. E.g. `Precision = 18, Scale = 2` for currency. |
| `DateTime` | `SqlDbType.DateTime2` | **Use `DateTime2`, not `DateTime`** — `DateTime` is not supported by Always Encrypted. |
| `DateTimeOffset` | `SqlDbType.DateTimeOffset` | For timezone-aware timestamps |
| `bool` | `SqlDbType.Bit` | For encrypted boolean flags |
| `byte[]` | `SqlDbType.VarBinary` | For encrypted binary data |
| `Guid` | `SqlDbType.UniqueIdentifier` | For encrypted GUIDs |

---

### Step 5 — Add the `EncryptColumnsAsync` Call

In the initializer's `ConfigureDatabaseEncryptionAsync` method (created in Step 3b),
add an `EncryptColumnsAsync` call for each entity that has encrypted columns:

```csharp
private async Task ConfigureDatabaseEncryptionAsync()
{
  await encryptionConfigurationService.ConfigureDatabaseAsync(dbContext, "Domain");

  logger.LogInformation("Encrypting database columns");

  await columnEncryptionService.EncryptColumnsAsync<Employee>(
    dbContext,
    employee => employee.EmployeeId,
    columnEncryptionBuilder =>
    {
      columnEncryptionBuilder.EncryptAllDecoratedColumns();
    });
}
```

**Parameters:**
- **`TEntity`** — the entity type with encrypted columns
- **`dbContext`** — the DbContext that manages this entity
- **`primaryKeyPropertyExpression`** — lambda selecting the entity's primary key property
- **`buildAction`** — configures which columns to encrypt

`EncryptAllDecoratedColumns()` scans the entity for all properties decorated with
`[EncryptedColumn]` and encrypts them. This is the recommended approach — it keeps the
attribute as the single source of truth.

> **Value object properties:** `EncryptAllDecoratedColumns()` automatically traverses
> properties that inherit from `ValueObject<>`. It detects value object navigation
> properties, resolves the prefixed column names (e.g. `OidcConfig_ClientId`), and
> encrypts any `[EncryptedColumn]`-decorated properties found within them. You do not
> need to call `EncryptValueObjectColumn` separately if the value object properties
> already have `[EncryptedColumn]` attributes — `EncryptAllDecoratedColumns()` handles
> them automatically.

> **Adding a new encrypted field to an entity that already has encryption?**
> If there is already an `EncryptColumnsAsync<TEntity>` call for this entity, you only
> need to add the `[EncryptedColumn]` attribute to the new property. The existing
> `EncryptAllDecoratedColumns()` call will pick it up automatically on next startup.

#### Alternative Builder Methods

The builder also supports encrypting columns individually, which is useful when you need
to specify default values or when `[EncryptedColumn]` attributes are not used:

```csharp
(columnEncryptionBuilder b) =>
{
  // Encrypt a specific column
  b.EncryptColumn(e => e.ExampleName);

  // Encrypt a column and set a default value for existing NULL rows
  b.EncryptColumnWithDefault(e => e.ExampleDate, DateTime.Now.AddDays(-7).Date);

  // Encrypt a value object column (see Advanced Patterns)
  b.EncryptValueObjectColumn<ValueObjectModel>(
    e => e.Name, $"{nameof(SampleModel.ValueObject)}_");

  // Encrypt a value object column with a default value
  b.EncryptValueObjectColumnWithDefault<ValueObjectModel>(
    e => e.TestNumber, $"{nameof(SampleModel.ValueObject)}_", 100);
}
```

---

## Advanced Patterns

### Value Object Columns — Manual Approach

In most cases, `EncryptAllDecoratedColumns()` handles value object properties automatically
(see note above). However, if the value object type does not inherit from `ValueObject<>`
or you need to encrypt a property that is **not** decorated with `[EncryptedColumn]`, use
`EncryptValueObjectColumn` explicitly:

```csharp
await columnEncryptionService.EncryptColumnsAsync<MyIdentityProvider>(
  dbContext,
  provider => provider.IdentityProviderId,
  columnEncryptionBuilder =>
  {
    columnEncryptionBuilder.EncryptValueObjectColumn<OidcProviderConfig>(
      config => config.ClientId,
      $"{nameof(IdentityProvider.OidcConfig)}_",
      new EncryptedColumnAttribute(SqlDbType.NVarChar));

    columnEncryptionBuilder.EncryptValueObjectColumn<OidcProviderConfig>(
      config => config.ClientSecret,
      $"{nameof(IdentityProvider.OidcConfig)}_",
      new EncryptedColumnAttribute(SqlDbType.NVarChar));
  });
```

**Parameters:**
- **`TValueObject`** — the value object type containing the property
- **`columnPropertyExpression`** — lambda selecting the property within the value object
- **`columnNamePrefix`** — the database column name prefix (typically `{NavigationPropertyName}_`)
- **`columnAttributes`** — an `EncryptedColumnAttribute` instance specifying data type and options

---

### Metadata Classes for Third-Party Entities

When you cannot modify the entity class directly (e.g. it inherits from a third-party base
class like ASP.NET Identity's `IdentityUser`), use a **metadata class** to apply the
`[EncryptedColumn]` attributes:

```csharp
using System.ComponentModel.DataAnnotations;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using Neo.Model.SqlServer.AlwaysEncrypted;

/// <summary>
/// The application user entity.
/// </summary>
[MetadataType(typeof(ApplicationUserMetadata))]
public class ApplicationUser : IdentityUser<int>
{
  // Properties inherited from IdentityUser — cannot add attributes directly.
  // Additional custom properties can have attributes applied directly.

  [EncryptedColumn(SqlDbType.NVarChar)]
  public string FirstName { get; set; } = string.Empty;

  [EncryptedColumn(SqlDbType.NVarChar)]
  public string LastName { get; set; } = string.Empty;
}

/// <summary>
/// Metadata class for applying attributes to inherited properties.
/// </summary>
[SuppressMessage("Design", "CS8618", Justification = "Metadata class — properties are not instantiated directly.")]
public class ApplicationUserMetadata
{
  [EncryptedColumn(SqlDbType.NVarChar)]
  public string Email { get; set; }

  [EncryptedColumn(SqlDbType.NVarChar)]
  public string NormalizedEmail { get; set; }

  [EncryptedColumn(SqlDbType.NVarChar)]
  public string NormalizedUserName { get; set; }

  [EncryptedColumn(SqlDbType.NVarChar)]
  public string PhoneNumber { get; set; }

  [EncryptedColumn(SqlDbType.NVarChar)]
  public string UserName { get; set; }
}
```

The metadata class must:
- Have properties with the **exact same names** as the target entity
- Be referenced via `[MetadataType(typeof(...))]` on the entity class
- Only contain properties that need attribute overrides — you do not need to redeclare all properties

---

## Migrating Existing Data

When encrypting columns on an entity that **already has data** in the database, the
`EncryptColumnsAsync` call handles the migration automatically on application startup.
However, there are important considerations:

### Default Constraints

Always Encrypted does not support SQL Server default constraints on encrypted columns.
The `DropDefaultConstraints: true` setting causes the framework to silently drop default
constraints before encrypting. Without this setting, startup will throw an exception if
default constraints are found.

> Application-level defaults set via EF's `HasDefaultValue()` or property initialisers
> (e.g. `= string.Empty`) are unaffected — they are applied by EF before the INSERT
> reaches SQL Server.

### Index Considerations

- **Unique indexes** on encrypted columns still work with deterministic encryption.
- With **enclaves enabled**, nonclustered indexes work with randomized encryption too and help
  records are looked up based on that column.
- If you have an existing index on a column being encrypted and are **not** using enclaves,
  you may need to drop and recreate the index after encryption.
- The framework handles column type changes (e.g. ensuring the column type matches the
  `SqlDbType` declared in the attribute).

### Performance

You should warn the user about the following:

- `EncryptColumnsAsync` reads all existing rows, encrypts them client-side, and writes them
  back in batches (default batch size: 100 rows, configurable via `dataCopyBatchSize` parameter).
- For **large tables** (millions of rows), the initial encryption can take significant time
  at startup. Consider:
  - Running the encryption during a maintenance window
  - Increasing the `CopyCommandsTimeout` in `AlwaysEncryptedOptions` (default: 120 seconds)
  - Adjusting `AlterCommandsTimeout` (default: 600 seconds) for DDL operations

### Large Tables (100k+ Rows)

For tables with large amounts of existing data (100k+ rows), the automatic batch encryption
at startup may not be practical — it can cause timeout issues, and services such as Azure
App Service may terminate the application if startup takes too long.

In this scenario, encrypt the columns manually through **SQL Server Management Studio (SSMS)**
instead. The Neo library compares the encryption requirements with the columns' current
encryption configuration in the database on each startup, and only processes columns that
still need encryption. It does not matter whether the in-place encryption was performed by
the application or through SSMS.

### Idempotent by Design

The `EncryptColumnsAsync` call is **idempotent** — if a column is already encrypted, it is
skipped. This means the initializer can safely run on every startup without re-encrypting
data that is already encrypted.

---

## Connecting to an Encrypted Database with SSMS

Before connecting, ensure you have the required Key Vault key permissions (`Get`, `List`,
`UnwrapKey`, `Verify`, `Decrypt`).

### Steps

1. Open SQL Server Management Studio
2. **Connect** → **Database Engine...**
3. Enter the server name and authentication details, then click **Options**
4. Go to the **Always Encrypted** tab:
   - Check **Enable Always Encrypted (column encryption)**
   - Check **Enable secure enclaves**
   - Set **Attestation Protocol** to `None` (for VBS enclaves)
5. Click **Connect**
6. Run a `SELECT` query on a table with encrypted columns — an Azure login prompt will
   appear. Sign in to retrieve the master key from Key Vault.
7. You should now see decrypted data in the encrypted columns.

---

## References

- [Always Encrypted](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-database-engine?view=sql-server-ver16)
- [Overview of Key Management](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/overview-of-key-management-for-always-encrypted?view=sql-server-ver16)
- [Always Encrypted Cryptography](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-cryptography?view=sql-server-ver16)
- [Secure Enclaves](https://learn.microsoft.com/en-us/sql/relational-databases/security/encryption/always-encrypted-enclaves?view=sql-server-ver16)
- [Virtualization Based Security](https://learn.microsoft.com/en-us/windows-hardware/design/device-experiences/oem-vbs)

---

## Checklist

### First-Time Setup
- [ ] `Neo.Model.SqlServer` added to the `*.Models` project
- [ ] `Neo.Azure.KeyVault` added to the `*.Api` / host project
- [ ] VBS enclaves enabled on local SQL Server (run diagnostic query to confirm)
- [ ] Connection strings include `Column Encryption Setting=Enabled`, `MultipleActiveResultSets=True`, `Persist Security Info=True`
- [ ] Development connection strings include `Attestation Protocol=NONE`
- [ ] `AlwaysEncrypted` configuration section added to `appsettings.json`
- [ ] `DatabaseKeyPrefix` set in `appsettings.Development.json` (format: `{project}-ldev`)
- [ ] `AddNeoSqlAlwaysEncryptedWithKeyVaultAsKeyStoreProvider` called in service registration
- [ ] `ConfigureDatabaseAsync` called in an `IAsyncInitializer` (guarded by `AlwaysEncrypted:Enabled`)
- [ ] Key Vault access policies grant key permissions to API service, DB script runner, and developers

### Per-Field Encryption
- [ ] Property decorated with `[EncryptedColumn(SqlDbType.X)]`
- [ ] String properties have `[MaxLength]` or `[StringLength]` specified
- [ ] Numeric types have `Precision` and `Scale` set on the attribute
- [ ] `[Column(TypeName = "...")]` added if EF Core default type doesn't match `SqlDbType`
- [ ] Encryption type chosen appropriately (randomized default, deterministic only if needed without enclaves)
- [ ] `EncryptColumnsAsync<TEntity>` call exists in the initializer for this entity
- [ ] If using `EncryptAllDecoratedColumns()`, no additional code needed for new fields on an already-encrypted entity (including value object properties that inherit from `ValueObject<>`)
- [ ] If encrypting a value object property that does **not** inherit from `ValueObject<>`, `EncryptValueObjectColumn` used with correct prefix
- [ ] If encrypting an inherited property, metadata class pattern used
