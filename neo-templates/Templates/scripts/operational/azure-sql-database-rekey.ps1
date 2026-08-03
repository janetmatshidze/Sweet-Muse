# This is required to import any module classes or enums referenced by this script.
using module Neo.PS.Azure
using module Neo.PS.Core

<#
.SYNOPSIS
  Rekey an Always Encrypted database by rotating the Column Master Key (CMK) from a source Key Vault to a target Key Vault.
  This script is intended for standalone rekey operations, separate from the database restore process.
.DESCRIPTION
  The script uses a double-roll mechanism to rotate the CMK:
  1. Roll from the existing CMK1 to an intermediate CMK1_rolling key
  2. Roll from the intermediate CMK1_rolling key back to CMK1 with the new key version
  3. Delete the intermediate CMK1_rolling key
  
  This ensures the database always has a key named "CMK1" available, which CALM applications expect.
.PARAMETER Project
  The client/project prefix used in resource naming (e.g., 'nsmp').
.PARAMETER SourceLocation
  The Azure location prefix for the source environment (e.g., 'we' for West Europe).
.PARAMETER SourceEnvironment
  The source environment prefix (e.g., 'dev', 'test', 'prod').
.PARAMETER TargetLocation
  The Azure location prefix for the target environment (e.g., 'we' for West Europe).
.PARAMETER TargetEnvironment
  The target environment prefix (e.g., 'dev', 'test', 'prod').
.PARAMETER FromSql
  The source SQL server key identifier from state (e.g., 'sql01', 'sql02').
.PARAMETER ToSql
  The target SQL server key identifier from state (e.g., 'sql01', 'sql02').
.PARAMETER DatabaseName
  The short name of the database to rekey (e.g., 'IdentityServer'). The full database name is
  constructed as '{DatabasePrefix}.{DatabaseName}' where DatabasePrefix is resolved from state.
  The CMK app name is derived automatically from the full database name by stripping non-alphanumeric
  characters (except hyphens) and lowercasing (e.g., 'NSMP.Dev.IdentityServer' becomes 'nsmpdevidentityserver').
.PARAMETER AutoApprove
  Automatically approves the rekey operation without prompting for confirmation. Use with caution,
  as this overrides the interactive confirmation step.
.PARAMETER PurgeEnabled
  If set, the previous CMK key version in the Target Key Vault will be soft-deleted after the rekey completes.
.PARAMETER EnclaveComputationsEnabled
  Override that will enable secure enclave computations for key vault keys. (Optional, defaults to false)
.PARAMETER PreserveContext
  If specified, the global context will be preserved after script execution for reuse in
  subsequent scripts. This can speed up local development/testing by avoiding repeated
  authentication and discovery calls. (Optional, defaults to false)
.EXAMPLE
  # Same-environment rekey (rotate key in place)
  .\azure-sql-database-rekey.ps1 `
    -Project "nsmp" `
    -SourceLocation "we" `
    -SourceEnvironment "prod" `
    -TargetLocation "we" `
    -TargetEnvironment "prod" `
    -FromSql "sql01" `
    -ToSql "sql01" `
    -DatabaseName "IdentityServer" `
    -AutoApprove
.EXAMPLE
  # Cross-environment rekey (after restore from prod to dev)
  .\azure-sql-database-rekey.ps1 `
    -Project "nsmp" `
    -SourceLocation "we" `
    -SourceEnvironment "prod" `
    -TargetLocation "we" `
    -TargetEnvironment "dev" `
    -FromSql "sql01" `
    -ToSql "sql01" `
    -DatabaseName "IdentityServer"
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory)][string]$Project,
  [Parameter(Mandatory)][string]$SourceLocation,
  [Parameter(Mandatory)][string]$SourceEnvironment,
  [Parameter(Mandatory)][string]$TargetLocation,
  [Parameter(Mandatory)][string]$TargetEnvironment,
  [Parameter(Mandatory)][string]$FromSql,
  [Parameter(Mandatory)][string]$ToSql,
  [Parameter(Mandatory)][string]$DatabaseName,
  [Parameter()][switch]$AutoApprove,
  [Parameter()][switch]$PurgeEnabled,
  [Parameter()][switch]$EnclaveComputationsEnabled,
  [Parameter()][switch]$PreserveContext
)

try {
  $ErrorActionPreference = "Stop"
  $InformationPreference = "Continue"
  $VerbosePreference = ($PSBoundParameters["Verbose"] -or [System.Convert]::ToBoolean($Env:NeoVerboseLogging)) ? "Continue" : "SilentlyContinue"
  $DebugPreference = ($PSBoundParameters["Debug"] -or [System.Convert]::ToBoolean($Env:NeoDebugLogging)) ? "Continue" : "SilentlyContinue"
  $VerboseEnabled = $VerbosePreference -eq "Continue"
  
  Write-NeoLogs @(
    "",
    " █▀▄ █▀█ ▀█▀ █▀█ █▀▄ █▀█ █▀▀ █▀▀   █▀▄ █▀▀ █ █ █▀▀ █ █",
    " █ █ █▀█  █  █▀█ █▀▄ █▀█ ▀▀█ █▀▀   █▀▄ █▀▀ █▀▄ █▀▀  █ ",
    " ▀▀  ▀ ▀  ▀  ▀ ▀ ▀▀  ▀ ▀ ▀▀▀ ▀▀▀   ▀ ▀ ▀▀▀ ▀ ▀ ▀▀▀  ▀ ",
    ""
  ) $Global:NeoLogStyles.Heading1Colour

  # Locate the scripts root path and load libraries
  $scriptsPath = $PSScriptRoot
  while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { 
    $scriptsPath = Split-Path $scriptsPath 
  }
  if (-not $scriptsPath) { 
    throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." 
  }
  . (Join-Path $scriptsPath "libraries/common.ps1")

  [PSCustomObject]$context = Get-GlobalContext -New:$(!$PreserveContext)

  # Configure runtime and cloud contexts
  Write-NeoLog "Configuring Runtime and Cloud Contexts" $Global:NeoLogStyles.SuppressedColour
  $contextScript = "$scriptsPath/context/configure-runtime-context.ps1"
  if (Test-Path $contextScript) {
    & $contextScript
  }

  $cloudContextScript = "$scriptsPath/context/configure-cloud-context.ps1"
  if (Test-Path $cloudContextScript) {
    & $cloudContextScript -ConnectPowerShellAz
  }

  # Configure state context scripts
  $stateContextScript = "$scriptsPath/context/configure-state-context.ps1"
  if (-not (Test-Path $stateContextScript)) {
    throw "State context script not found at '$stateContextScript'. Ensure this script is run from a repo with the expected 'scripts/context' structure."
  }

  # Configure source environment state
  Write-NeoLog "`nConfiguring source environment state..." $Global:NeoLogStyles.EmphasisColour
  if ($null -ne $context.SourceState) {
    $context.PSObject.Properties.Remove("SourceState")
  }
  & $stateContextScript -StateKey "SourceState" -ProjectPrefix $Project -LocationPrefix $SourceLocation -Environment $SourceEnvironment

  # Configure target environment state
  Write-NeoLog "Configuring target environment state..." $Global:NeoLogStyles.EmphasisColour
  if ($null -ne $context.TargetState) {
    $context.PSObject.Properties.Remove("TargetState")
  }
  & $stateContextScript -StateKey "TargetState" -ProjectPrefix $Project -LocationPrefix $TargetLocation -Environment $TargetEnvironment

  # Get state objects
  [PSCustomObject]$sourceState = $context.SourceState
  [PSCustomObject]$targetState = $context.TargetState

  # Extract configuration from state objects
  Write-NeoLog "`nExtracting configuration from state..." $Global:NeoLogStyles.TextColour

  # Resolve state sub-objects per SQL key
  # AppSpace holds app-level config (key vault, database prefix, sql host type)
  # HostSpace/SharedHosts holds infra-level config (VM name, admin user, secrets)
  [PSCustomObject]$sourceAppSpace = $sourceState.AppSpace
  [PSCustomObject]$targetAppSpace = $targetState.AppSpace

  if (-not $sourceAppSpace.sql_servers.$FromSql) {
    throw "SQL server key '$FromSql' not found in source AppSpace state for environment '$SourceEnvironment'."
  }
  if (-not $targetAppSpace.sql_servers.$ToSql) {
    throw "SQL server key '$ToSql' not found in target AppSpace state for environment '$TargetEnvironment'."
  }

  # Determine SQL type (Dedicated uses HostSpace, Shared uses SharedHosts)
  $sourceSqlType = $sourceAppSpace.sql_servers.$FromSql.type
  $targetSqlType = $targetAppSpace.sql_servers.$ToSql.type

  $expectedSourceHostStateName = ($sourceSqlType -eq [SqlServerTypeEnum]::Dedicated) ? "HostSpace" : "SharedHosts"
  if (-not $sourceState.PSObject.Properties.Name.Contains($expectedSourceHostStateName) -or -not $sourceState.$expectedSourceHostStateName) {
    throw "Source state for environment '$SourceEnvironment' does not contain the expected '$expectedSourceHostStateName' configuration required for SQL server key '$FromSql'."
  }

  $expectedTargetHostStateName = ($targetSqlType -eq [SqlServerTypeEnum]::Dedicated) ? "HostSpace" : "SharedHosts"
  if (-not $targetState.PSObject.Properties.Name.Contains($expectedTargetHostStateName) -or -not $targetState.$expectedTargetHostStateName) {
    throw "Target state for environment '$TargetEnvironment' does not contain the expected '$expectedTargetHostStateName' configuration required for SQL server key '$ToSql'."
  }

  $sourceHostState = $sourceState.$expectedSourceHostStateName
  $targetHostState = $targetState.$expectedTargetHostStateName

  if (-not $sourceHostState.sql_servers.$FromSql) {
    throw "SQL server key '$FromSql' not found in source host state for environment '$SourceEnvironment'."
  }
  if (-not $targetHostState.sql_servers.$ToSql) {
    throw "SQL server key '$ToSql' not found in target host state for environment '$TargetEnvironment'."
  }

  # Extract Key Vault names from state
  $sourceKeyVaultName = $sourceAppSpace.key_vault.key_vault.name
  $targetKeyVaultName = $targetAppSpace.key_vault.key_vault.name

  # Extract SQL server details from target host state
  $sqlVmName = $targetHostState.sql_servers.$ToSql.virtual_machine.name
  $sqlAdminUser = $targetHostState.sql_servers.$ToSql.sql_users.admin
  $sqlAdminSecretName = $targetHostState.sql_servers.$ToSql.sql_users.admin_secret_name
  $sqlHostType = $targetAppSpace.sql_servers.$ToSql.sql_host_type
  $isDedicatedTarget = ($targetSqlType -eq [SqlServerTypeEnum]::Dedicated)
  $hostKeyVaultName = $isDedicatedTarget ? $targetHostState.key_vault.vault.name : $targetHostState.key_vault.name
  $targetResourceGroup = $targetHostState.recovery_services_vault.recovery_services_vault.resource_group_name

  function Get-PrimaryIpConfiguration([object]$SqlVm) {
    $networkInterfaceId = $SqlVm.NetworkProfile.NetworkInterfaces[0].Id
    $networkInterface = Get-AzNetworkInterface -ResourceId $networkInterfaceId
    $primaryIpConfiguration = $networkInterface.IpConfigurations | Where-Object { $_.Primary } | Select-Object -First 1
    if (-not $primaryIpConfiguration) {
      $primaryIpConfiguration = $networkInterface.IpConfigurations | Select-Object -First 1
    }
    return $primaryIpConfiguration
  }

  function Resolve-NeoAzSqlHostName([string]$HostType, [object]$SqlVm, [string]$ResourceGroupName, [string]$VmName) {
    $supportedHostTypes = @("private-hostname", "private-ip", "public-ip", "public-hostname", "auto")
    if ($supportedHostTypes -notcontains $HostType) {
      throw "Unsupported sql_host_type value: '$HostType'. Supported values are: 'private-hostname', 'private-ip', 'public-ip', 'public-hostname', 'auto'."
    }
    switch ($HostType) {
      "private-hostname" {
        return $SqlVm.OSProfile.ComputerName
      }
      "private-ip" {
        $primaryIpConfiguration = Get-PrimaryIpConfiguration($SqlVm)
        return $primaryIpConfiguration?.PrivateIpAddress
      }
      "public-ip" {
        $publicIpName = ($VmName -replace "-vm-", "-pip-")
        $publicIp = Get-AzPublicIpAddress -ResourceGroupName $ResourceGroupName -Name $publicIpName -ErrorAction SilentlyContinue
        return $publicIp?.IpAddress
      }
      "public-hostname" {
        $publicIpName = ($VmName -replace "-vm-", "-pip-")
        $publicIp = Get-AzPublicIpAddress -ResourceGroupName $ResourceGroupName -Name $publicIpName -ErrorAction SilentlyContinue
        return $publicIp?.DnsSettings?.Fqdn
      }
      "auto" {
        return (Resolve-NeoAzSqlHostName -HostType "public-hostname" -SqlVm $SqlVm -ResourceGroupName $ResourceGroupName -VmName $VmName) ??
        (Resolve-NeoAzSqlHostName -HostType "public-ip" -SqlVm $SqlVm -ResourceGroupName $ResourceGroupName -VmName $VmName) ??
        (Resolve-NeoAzSqlHostName -HostType "private-ip" -SqlVm $SqlVm -ResourceGroupName $ResourceGroupName -VmName $VmName)
      }
      default {
        throw "An unexpected error occurred while resolving the SQL Server hostname. Inputs supplied: [HostType:'$HostType']; [VM Name:'$VmName']; [Resource Group:'$ResourceGroupName']"
      }
    }
  }

  # Resolve SQL Server hostname based on host type
  Write-NeoLog "Resolving SQL Server hostname (type: $sqlHostType)..." $Global:NeoLogStyles.TextColour
  $sqlVm = Get-AzVM -ResourceGroupName $targetResourceGroup -Name $sqlVmName -ErrorAction Stop
  if ($null -eq $sqlVm) {
    throw "Could not find SQL VM '$sqlVmName' in resource group '$targetResourceGroup'."
  }

  $sqlServerName = Resolve-NeoAzSqlHostName -HostType $sqlHostType -SqlVm $sqlVm -ResourceGroupName $targetResourceGroup -VmName $sqlVmName
  if ([string]::IsNullOrWhiteSpace($sqlServerName)) {
    throw "Unable to resolve SQL Server hostname for VM '$sqlVmName' using host type '$sqlHostType'."
  }
  Write-Verbose "SQL Server hostname resolved as: $sqlServerName"

  # Retrieve SQL Admin password from Key Vault
  Write-NeoLog "Retrieving SQL Admin password from Key Vault '$hostKeyVaultName'..." $Global:NeoLogStyles.TextColour
  $secret = Get-AzKeyVaultSecret -VaultName $hostKeyVaultName -Name $sqlAdminSecretName -ErrorAction Stop
  if ($null -eq $secret) {
    throw "The '$sqlAdminSecretName' secret in vault '$hostKeyVaultName' is null."
  }
  $securePassword = $secret.SecretValue

  # Construct full database names from state prefix and the provided DatabaseName
  $sourceDatabasePrefix = $sourceAppSpace.sql_servers.$FromSql.database_prefix
  $targetDatabasePrefix = $targetAppSpace.sql_servers.$ToSql.database_prefix
  $sourceFullDatabaseName = "$sourceDatabasePrefix$DatabaseName"
  $fullDatabaseName = "$targetDatabasePrefix$DatabaseName"

  # Derive the CMK app names from the full database names
  # This matches the restore process pattern: strip non-alphanumeric chars (except hyphens) and lowercase
  $sourceAppName = ($sourceFullDatabaseName -replace '[^A-Za-z0-9-]', '').ToLowerInvariant()
  $targetAppName = ($fullDatabaseName -replace '[^A-Za-z0-9-]', '').ToLowerInvariant()
  Write-Verbose "Source CMK app name: $sourceAppName"
  Write-Verbose "Target CMK app name: $targetAppName"

  # Import SqlServer module for SMO types
  Write-Verbose "Importing SqlServer module..."
  Import-Module SqlServer -Verbose:$false | Out-Null

  # Display configuration summary
  Write-NeoLogHeading "Database Rekey Configuration"
  Write-NeoLog "[ General ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "Project"          = $Project
        "Host Key Vault"   = $hostKeyVaultName
        "Source SQL Key"   = $FromSql
        "Target SQL Key"   = $ToSql
        "Database Name"    = $DatabaseName
        "Source Database"  = $sourceFullDatabaseName
        "Target Database"  = $fullDatabaseName
        "Source DB Prefix" = $sourceDatabasePrefix
        "Target DB Prefix" = $targetDatabasePrefix
        "Source App Name"  = $sourceAppName
        "Target App Name"  = $targetAppName
        "Auto Approve"     = ($AutoApprove.IsPresent ? "Yes" : "No")
        "Purge Old Key"    = ($PurgeEnabled.IsPresent ? "Yes" : "No")
        "Enclave Enabled"  = ($EnclaveComputationsEnabled.IsPresent ? "Yes" : "No")
      })
  ) $Global:NeoLogStyles.EmphasisColour

  Write-NeoLog "`n[ Source Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "Location"    = $SourceLocation
        "Environment" = $SourceEnvironment
        "Key Vault"   = $sourceKeyVaultName
      })
  ) $Global:NeoLogStyles.EmphasisColour

  Write-NeoLog "`n[ Target Configuration ]" $Global:NeoLogStyles.Heading2Colour
  Write-NeoLogs (Format-NeoKeyValues ([ordered]@{
        "Location"          = $TargetLocation
        "Environment"       = $TargetEnvironment
        "Key Vault"         = $targetKeyVaultName
        "SQL VM"            = $sqlVmName
        "SQL Server"        = $sqlServerName
        "SQL Admin User"    = $sqlAdminUser
        "Admin Secret Name" = $sqlAdminSecretName
      })
  ) $Global:NeoLogStyles.EmphasisColour
  Write-NeoLogHeadingFooter

  # Initialize the Always Encrypted Manager with fluent configuration
  Write-NeoLog "Initializing Always Encrypted Manager..." $Global:NeoLogStyles.TextColour
  
  $manager = [AlwaysEncryptedManager]::new().
  WithAlwaysEncryptedManagerDetails($Project, $SourceEnvironment, $TargetEnvironment).
  WithKeyVaultStoreDetails($sourceKeyVaultName, $targetKeyVaultName).
  WithSqlStoreDetails($fullDatabaseName, $sqlAdminUser, $securePassword, $sqlServerName).
  WithSourceAppName($sourceAppName).
  WithTargetAppName($targetAppName).
  WithAutoApprove($AutoApprove.IsPresent).
  WithPurgeEnabled($PurgeEnabled.IsPresent).
  WithEnclaveComputations($EnclaveComputationsEnabled.IsPresent).
  WithVerbose($VerboseEnabled)

  
  # Execute the rekey process
  Write-NeoLog "Starting Database Rekey Process..." $Global:NeoLogStyles.Heading2Colour
  $manager.InitiateDatabaseRekey()

  Write-NeoLog "`nDatabase Rekey completed successfully." $Global:NeoLogStyles.SuccessColour

} catch {
  if ($_.FullyQualifiedErrorId -ne "SqlDatabaseLoadFailed") {
    Write-NeoLog "Database rekey failed: $($_.Exception.Message)" $Global:NeoLogStyles.ErrorColour
  }
  throw
} finally {
  # Ensure the global context cleanup is run before exiting
  if (-not $PreserveContext) {
    Write-NeoLog "Cleaning up Context" $Global:NeoLogStyles.SuppressedColour
    $Global:Context.Cleanup()
  } else {
    Write-NeoLog "Preserve Context enabled. Skipping cleanup." $Global:NeoLogStyles.WarningColour
  }

  # The below code removes all SQL Server Admin passwords from context
  Write-Verbose "Cleaning up secret environment variables matching 'NeoSqlAdminPassword_*'"
  # See: https://learn.microsoft.com/en-us/powershell/module/microsoft.powershell.core/about/about_environment_variables?view=powershell-7.5#use-the-environment-provider-and-item-cmdlets
  Remove-Item -Path Env:\$("NeoSqlAdminPassword_*") -Verbose:$VerboseEnabled -ErrorAction SilentlyContinue
}
