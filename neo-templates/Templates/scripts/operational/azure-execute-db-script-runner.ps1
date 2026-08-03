<#
.SYNOPSIS
  Executes the DB Script Runner against specified SQL Servers in the target environments.
.PARAMETER ProjectPrefix
  The project prefix, used to generate resource names. (Required)
.PARAMETER LocationPrefix
  The location prefix, used to generate resource names. (Required)
.PARAMETER Environment
  The prefix of the environment to execute the script runner against. (Required)
.PARAMETER Servers
  A comma separated list of SQL Server name suffixes to execute the script runner against. (Optional)
  If not specified, scripts will be executed against all SQL Servers defined in the AppSpace state for the environment.
.PARAMETER AllowedDbNamePrefixes
  A comma separated list of database name prefixes (E.g. "NSMP.PP.,NSMP.PRD.") which scripts are allowed to reference (I.e. read from).
  If not specified, scripts may reference any database on the server(s). (Optional)
.PARAMETER AllowedAffectedDbNamePrefixes
  A comma separated list of database name prefixes (E.g. "NSMP.PP.,NSMP.PRD.") which scripts are allowed to affect (I.e. write to).
  If not specified, scripts may affect any database on the server(s). (Optional)
.PARAMETER DatabaseName
  The name suffix of the database to store the Scripts Audit table under. (Optional, defaults to "DbUpdates")
  To avoid naming conflicts with other projects or environments, the full database name will use the 'database_prefix'
  value from the app space's IaC state for the associated SQL Server.
  Typically, this will be in the format: "{ProjectPrefix}.{Environment}.$AuditDatabaseName", E.g. "NSMP.UAT.DbUpdates".
.PARAMETER HostPrefix
  The host environment prefix. (Optional)
  This is only needed for ShareTrust environments, as they don't contain a reference to their host environment in the app environment state.
.PARAMETER NugetConfig
  The content of the NuGet.Config file to enable GitHub packages authentication when building the Docker image.
  (Required. Defaults to the value in the 'NugetConfig' environment variable)
.PARAMETER DefaultTimeoutSeconds
  Default timeout in seconds for each script execution. (Optional, defaults to 120)
.PARAMETER ValidateOnly
  If specified, scripts will be validated but not executed.
.PARAMETER ExecutionMode
  Controls whether the journal check and expected row count validation are bypassed. (Optional, defaults to "RunOnce")
  - RunOnce: Journal check prevents re-running previously executed scripts. Folder, SubFolder, and Scripts parameters are
    ignored; the Environment name is always used as the folder and the SQL Server key is always used as the sub-folder.
  - RunAlways: Journal and row-count checks are bypassed, audit history is tracked. 
    Folder, SubFolder, and Scripts parameters are used. Requires Folder to be set.
.PARAMETER Folder
  The folder within the Scripts directory to target. (Optional)
  Required when ExecutionMode is RunAlways; ignored in RunOnce mode.
.PARAMETER SubFolder
  The sub-folder within the Scripts/<Folder> directory to target. (Optional)
  Only used when ExecutionMode is RunAlways; ignored in RunOnce mode.
  When not specified, all scripts in the folder will be executed.
.PARAMETER Scripts
  One or more script names or wildcard patterns to target within the selected folder/sub-folder. (Optional)
  Only used when ExecutionMode is RunAlways; ignored in RunOnce mode.
  Supports '*' wildcards. E.g. "UpdateConfig.sql", "*Integration.sql".
  Passed to DbScriptRunner as a semicolon-delimited SCRIPTS environment variable.
.PARAMETER PreserveContext
  If specified, the global context will be preserved after script execution for reuse in
  subsequent scripts. This helps to speed up local development testing. (Optional, defaults to false)
#>
[CmdletBinding()]
Param(
  [Parameter(Mandatory)][string]$ProjectPrefix,
  [Parameter(Mandatory)][string]$LocationPrefix,
  [Parameter(Mandatory)][string]$Environment,
  [string]$Servers = "",
  [string]$AllowedDbNamePrefixes = "",
  [string]$AllowedAffectedDbNamePrefixes = "",
  [string]$DatabaseName = "DbUpdates",
  [string]$HostPrefix = $Env:HostPrefix,
  [string]$NugetConfig = $Env:NugetConfig,
  [int]$DefaultTimeoutSeconds,
  [switch]$ValidateOnly,
  [string][ValidateSet("RunOnce", "RunAlways")]$ExecutionMode = "RunOnce",
  [string]$Folder = "",
  [string]$SubFolder = "",
  [string]$Scripts = "",
  [switch]$PreserveContext
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

# Validate parameters which use environment variable defaults
if ([string]::IsNullOrEmpty($NugetConfig)) { throw "NugetConfig is required" }
if ($ExecutionMode -eq "RunAlways" -and [string]::IsNullOrWhiteSpace($Folder)) {
  throw "'Folder' is required when 'ExecutionMode' is 'RunAlways'."
}

# Locate the IaC root path and load libraries
$iacPath = $PSScriptRoot; while ($iacPath -and (Split-Path $iacPath -Leaf) -ne "IaC") { $iacPath = Split-Path $iacPath }
if (!$iacPath) { throw "Could not locate the IaC root folder. Please ensure this script is nested beneath an 'IaC' parent folder." }
$rootPath = (Split-Path $iacPath -Parent)
$scriptsPath = (Join-Path $iacPath "scripts")
. (Join-Path $scriptsPath "libraries/common.ps1")
. (Join-Path $scriptsPath "libraries/github.ps1")

Write-Logs @(
  "`n",
  " █▀▄ █▀▄   █▀▀ █▀▀ █▀▄ ▀█▀ █▀█ ▀█▀   █▀▄ █ █ █▀█ █▀█ █▀▀ █▀▄",
  " █ █ █▀▄   ▀▀█ █   █▀▄  █  █▀▀  █    █▀▄ █ █ █ █ █ █ █▀▀ █▀▄",
  " ▀▀  ▀▀    ▀▀▀ ▀▀▀ ▀ ▀ ▀▀▀ ▀    ▀    ▀ ▀ ▀▀▀ ▀ ▀ ▀ ▀ ▀▀▀ ▀ ▀"
) $Global:LogStyles.Heading1Colour

$context = Get-GlobalContext -New:$(!$PreserveContext)

try {
  # Ensure that a DB Script Runner project exists in the repository
  Write-Debug "Locating DB Script Runner project..."
  $blueprintPath = (Join-Path $rootPath "blueprint.json")
  if (!(Test-Path $blueprintPath)) {
    throw "No 'blueprint.json' file found in the root folder. This is required to locate the DB Script Runner project."
  }

  $blueprint = (Get-Content $blueprintPath -Raw | ConvertFrom-Json)
  $projects = $blueprint.projects | Where-Object { $_.type -eq "DotNet" -and $_.dotNet.type -eq "DbScriptRunner" }
  if ($projects.Count -eq 0) {
    throw "No 'DbScriptRunner' project found in the blueprint. Please ensure a DB Script Runner project is defined."
  } elseif ($projects.Count -gt 1) {
    throw "Multiple 'DbScriptRunner' projects found in the blueprint. Please ensure only one DB Script Runner project is defined."
  }

  $scriptRunnerProject = $projects[0]

  # Gather Audit Details
  # ====================
  # Collect Pipeline, Commit and PR information to be stored in the script run audit table.
  # (NOTE: This step is tightly coupled to GoCD as the CI/CD system, and GitHub as the source control system.
  #        If the required environment variables are not present, this step will be skipped. If we need to support
  #        other systems in the future, this step will need to be refactored.)
  $pipelineInstance = $null
  if (
    ![string]::IsNullOrEmpty($Env:GO_PIPELINE_LABEL) -and
    ![string]::IsNullOrEmpty($Env:GO_REVISION_SOURCE) -and
    ![string]::IsNullOrEmpty($Env:GitHubRepository)) {

    Write-Log "`n[ Audit Details ]" $Global:LogStyles.Heading1Colour
    Write-Debug "Fetching Commit and Pull Request Information..."
    $serverUrl = $Env:GO_SERVER_URL
    $pipelineName = $Env:GO_PIPELINE_NAME
    $pipelineLabel = $Env:GO_PIPELINE_LABEL
    $commitHash = $Env:GO_REVISION_SOURCE

    $repoDetails = $Env:GitHubRepository.Split("/")
    if ($repoDetails.Count -ne 2) {
      throw "Invalid format on GitHubRepository environment variable. Expected 'Owner/RepositoryName'."
    }
    $repoOwner = $repoDetails[0]
    $repoName = $repoDetails[1]

    # Get the Commit URL and the associated Pull Request details
    $commitResponse = (Get-GitHubCommit -Owner $repoOwner -RepositoryName $repoName -CommitHash $commithash)
    $commitUrl = $commitResponse.success ? $commitResponse.body.html_url : ""

    $pullRequestsResponse = (Get-GitHubCommitPullRequests -Owner $repoOwner -RepositoryName $repoName -CommitHash $commitHash -HeadPullRequestOnly)
    if ($pullRequestsResponse.success -and $pullRequestsResponse.body.Count -gt 0) {
      $pullRequest = $pullRequestsResponse.body[0]
      $pullRequestUrl = $pullRequest.html_url
      $pullRequestNumber = $pullRequest.number
    }

    $pipelineInstance = @{
      PipelineName         = $pipelineName
      PipelineJobTag       = $Env:GO_PIPELINE_LABEL
      PipelineJobUrl       = "$($serverUrl)/tab/build/detail/$($pipelineName)/$($pipelineLabel)/stage/1/job"
      GitCommitHash        = $commitHash
      GitCommitUrl         = $commitUrl
      GitPullRequestNumber = $pullRequestNumber ?? ""
      GitPullRequestUrl    = $pullRequestUrl ?? ""
    }

    Write-Logs (Format-KeyValues ([ordered]@{
          "Pipeline Job Tag" = $pipelineInstance.PipelineJobTag
          "Pipeline Job URL" = $pipelineInstance.PipelineJobUrl
          "Git Commit Hash"  = $pipelineInstance.GitCommitHash
          "Git Commit URL"   = $pipelineInstance.GitCommitUrl
          "Git PR Number"    = $pipelineInstance.GitPullRequestNumber
          "Git PR URL"       = $pipelineInstance.GitPullRequestUrl
        })
    ) $Global:LogStyles.EmphasisColour
  } else {
    Write-Logs "`nNo commit information found, skipping retrieval of commit and pull request details." $Global:LogStyles.WarningColour
  }

  # Build Docker Image
  # ==================
  Write-Log "`n[ Build DB Script Runner ]" $Global:LogStyles.Heading1Colour

  # Search for the Dockerfile under the DBScriptRunner project folder
  $dockerfile = Get-ChildItem -Path (Join-Path $rootPath $scriptRunnerProject.folder) -Recurse -Filter "Dockerfile" | Select-Object -First 1
  if ($null -eq $dockerfile) {
    throw "No 'Dockerfile' found in the DB Script Runner project folder '$($scriptRunnerProject.folder)'. Please ensure a Dockerfile is present."
  }

  # Change the working directory to the location of the Dockerfile
  Push-Location

  $buildPath = $dockerfile.Directory.FullName
  Set-Location $buildPath

  # If a pipeline name is available, it is used in the registry name to avoid naming conflicts / pipelines on the same project
  # overwriting each other's images.
  $registry = "local"
  $repository = "$($ProjectPrefix.ToLowerInvariant())-db-scriptrunner"
  if ($null -ne $pipelineInstance -and ![string]::IsNullOrWhiteSpace($pipelineInstance.PipelineName)) {
    $repository = "db-scriptrunner-$($pipelineInstance.PipelineName.ToLowerInvariant())"
  }

  $tag = "v1"
  $imageTag = "$($registry)/$($repository):$($tag)"

  Write-Logs (Format-KeyValues ([ordered]@{
        "Project Path" = $scriptRunnerProject.folder
        "Dockerfile"   = $dockerfile.FullName
        "Image Name"   = $imageTag
      })
  ) $Global:LogStyles.EmphasisColour

  Write-Log "`nBuilding Dockerfile..." $Global:LogStyles.SuppressedColour
  $arguments = "build -f $dockerfile -t $imageTag --secret id=NugetConfig,env=NugetConfig ."
  & docker ($arguments.Split(" "))

  if (-not $?) {
    throw "Error building Docker image"
  }

  # Context Setup & State Retrieval
  # ===============================

  # If PreserveContext is enabled, and the context has been populated previously, check if the environment
  # has changed. If it has, force a re-create of the context
  if ($PreserveContext -and ($null -ne $context.Environment)) {
    if ($context.Environment.ProjectPrefix -ne $ProjectPrefix -or
      $context.Environment.LocationPrefix -ne $LocationPrefix -or
      $context.Environment.HostPrefix -ne $HostPrefix -or
      $context.Environment.Environment -ne $Environment -or
      $context.Environment.Servers -ne $Servers) {
      Write-Log "`nHost Environment has changed. Re-creating context." $Global:LogStyles.WarningColour
      $context = Get-GlobalContext -New
    } else {
      Write-Log "`nHost Environment has not changed. Reusing context from a previous run." $Global:LogStyles.SuppressedColour
    }
  }

  # Populate the context if PreserveContext is disabled, or if the context has not been populated yet
  if (!$PreserveContext -or ($null -eq $context.Environment)) {
    $context.Add("Environment", (New-Context @{
          ProjectPrefix  = $ProjectPrefix
          LocationPrefix = $LocationPrefix
          HostPrefix     = $HostPrefix
          Environment    = $Environment
          Servers        = $Servers
        }))

    # Configure context for the runtime environment if a script is present.
    $runtimeContextScript = "$scriptsPath/context/configure-runtime-context.ps1"
    if (Test-Path $runtimeContextScript) {
      & $runtimeContextScript
    }

    # Configure cloud provider context if a script is present.
    $cloudContextScript = "$scriptsPath/context/configure-cloud-context.ps1"
    if (Test-Path $cloudContextScript) {
      & $cloudContextScript
    }

    # Load state for the environment
    Write-Log "`n[ Loading Environment State ]" $Global:LogStyles.Heading1Colour
    Write-Log "`nLoading state for environment '$Environment'..." $Global:LogStyles.Heading2Colour
    & $scriptsPath/context/configure-state-context.ps1 -ProjectPrefix $ProjectPrefix -LocationPrefix $LocationPrefix -HostPrefix $HostPrefix -Environment $Environment
  }

  # Script Runner Execution
  # =======================
  Write-Host "`n[ Execute Script Runner ]" -ForegroundColor Cyan

  $state = $context.State

  # The properties object is an IEnumerable, so has no Length or Count property. Instead we use a Linq function to get the count.
  if ($null -eq $state -or [System.Linq.Enumerable]::Count($state.PSObject.Properties) -eq 0) {
    throw "No state found for environment '$Environment'. Aborting run."
  }

  # If SQL Keys were specified on the environment configuration, use those. Otherwise, default to all SQL Servers defined in the AppSpace state.
  $isShareTrust = ($ProjectPrefix -eq "st" -or $ProjectPrefix -eq "stt")
  $sqlKeys = @()
  if (![string]::IsNullOrEmpty($Servers)) {
    $sqlKeys = $Servers.Split(",")
  } else {
    $sqlKeys = $state.AppSpace.sql_servers.PSObject.Properties | ForEach-Object { $_.Name }
  }

  # Loop through the SQL Servers linked to the environment
  foreach ($sqlKey in $sqlKeys) {
    $sqlServer = $state.AppSpace.sql_servers.$sqlKey
    $sqlType = $sqlServer.type

    Write-Logs "`n$($Environment.ToUpper()) Environment: $sqlKey" $Global:LogStyles.Heading1Colour
    Write-Logs "─────────────────────────────────────────────────────────────────────" $Global:LogStyles.SuppressedColour

    # Get the details of the SQL Server to run against
    $sqlServersHostState = $sqlType -eq "shared" ? $state.SharedHosts.sql_servers : $state.HostSpace.sql_servers
    $sqlServerState = $sqlServersHostState.$sqlKey
    $auditDatabaseName = "$($sqlServer.database_prefix)$DatabaseName"
    if ($isShareTrust) {
      # The database prefix for ShareTrust only includes the environment. The "ST." part is hard coded elsewhere.
      $auditDatabaseName = "$($sqlServer.database_prefix)ST.$($DatabaseName)"
    }

    if ($null -eq $sqlServerState) {
      Write-Log "WARNING: SQL Server '$sqlKey' not found in the $($sqlType -eq "shared" ? "Shared Hosts" : "Host Space") state. Skipping..." $Global:LogStyles.WarningColour
      continue
    }

    $hostname = $sqlServerState.network.private_ip

    Write-Logs (Format-KeyValues ([ordered]@{
          "SQL Server"                        = "$($sqlServerState.virtual_machine.computer_name) ($($hostname))"
          "Type"                              = "$sqlType"
          "Allowed DB Name Prefixes"          = "$AllowedDbNamePrefixes"
          "Allowed Affected DB Name Prefixes" = "$AllowedAffectedDbNamePrefixes"
          "Audit Database"                    = "$auditDatabaseName"
          "Secure Enclaves"                   = "$([bool]($sqlServerState.sql_enable_flags.enable_secure_enclaves ?? $sqlServerState.enable_secure_enclaves ?? $sqlServer.enable_secure_enclaves))"
        })
    ) $Global:LogStyles.EmphasisColour
    Write-Logs ""

    Write-Debug "Fetching SQL User Credentials"

    $keyVaultName = ($state.AppSpace.key_vault.key_vault ?? $state.AppSpace.key_vault).name
    $appSpaceState = ($isShareTrust ? $state.AppSpace : $state.AppSpaceBase)
    $servicePrincipalTenantId = ($appSpaceState.azure.tenant_id ?? "")
    $dbScriptRunnerServicePrincipal = $appSpaceState.local_principals.db_script_runner_service_principal
    $servicePrincipalKeyVaultName = ($appSpaceState.key_vault.key_vault ?? $appSpaceState.key_vault).name
    $servicePrincipalClientId = $dbScriptRunnerServicePrincipal.application.client_id ?? ""
    $servicePrincipalClientSecretName = $dbScriptRunnerServicePrincipal.password_properties.key_vault_secret_name ?? ""
    
    $hasServicePrincipalCredentials =
    ![string]::IsNullOrWhiteSpace($servicePrincipalTenantId) -and
    ![string]::IsNullOrWhiteSpace($servicePrincipalClientId) -and
    ![string]::IsNullOrWhiteSpace($servicePrincipalClientSecretName) -and
    ![string]::IsNullOrWhiteSpace($servicePrincipalKeyVaultName)

    $servicePrincipalClientSecret = ""

    $sqlUsername = $sqlServerState.sql_users.application
    $sqlPasswordSecretName = "KeyVault--SqlAppPassword"
    $sqlPassword = (az keyvault secret show --name $sqlPasswordSecretName --vault-name $keyVaultName --query value -o tsv)

    $secureEnclavesEnabled = [bool]($sqlServerState.sql_enable_flags.enable_secure_enclaves ?? $sqlServerState.enable_secure_enclaves ?? $sqlServer.enable_secure_enclaves)
    $connectionString = "Server=$($hostname),1433;Database=$auditDatabaseName;User ID=$sqlUsername;Password=$sqlPassword;Encrypt=True;TrustServerCertificate=True;Connection Timeout=30;Column Encryption Setting=Enabled;$($secureEnclavesEnabled ? 'Attestation Protocol=NONE;' : '')MultipleActiveResultSets=True;Persist Security Info=True;"
    $ConnectionStringSecretFilePath = (Join-Path $rootPath "connection-string.txt")
    $ServicePrincipalSecretFilePath = (Join-Path $rootPath "azure-client-secret.txt")
    try {
      # All Script Runner parameters are passed in through environment variables
      $envVars = @()
      $envVars += @("--env", "EXECUTION_MODE=$ExecutionMode")

      # Folder/SubFolder/Scripts configuration
      $effectiveFolder = $ExecutionMode -eq "RunAlways" ? $Folder : $Environment
      $effectiveSubFolder = $ExecutionMode -eq "RunAlways" ? $SubFolder : $sqlKey
      $envVars += @("--env", "SCRIPTS_FOLDER=$effectiveFolder")
      $envVars += @("--env", "SCRIPTS_SUBFOLDER=$effectiveSubFolder")
      if (![string]::IsNullOrEmpty($Scripts)) {
        $envVars += @("--env", "SCRIPTS=$Scripts")
      }

      # Add optional parameters if specified
      if (![string]::IsNullOrEmpty($AllowedDbNamePrefixes)) {
        $envVars += @("--env", "ALLOWED_DB_NAME_PREFIXES=$AllowedDbNamePrefixes")
      }

      if (![string]::IsNullOrEmpty($AllowedAffectedDbNamePrefixes)) {
        $envVars += @("--env", "ALLOWED_AFFECTED_DB_NAME_PREFIXES=$AllowedAffectedDbNamePrefixes")
      }

      if ($null -ne $DefaultTimeoutSeconds -and $DefaultTimeoutSeconds -gt 0) {
        $envVars += @("--env", "DEFAULT_TIMEOUT_SECONDS=$DefaultTimeoutSeconds")
      }

      if ($ValidateOnly.IsPresent) {
        $envVars += @("--env", "VALIDATE_ONLY=true")
      }

      if ($null -ne $pipelineInstance) {
        # If available, send through the pipeline instance details
        $envVars += @(
          "--env", "PIPELINE_JOB_TAG=$($pipelineInstance.PipelineJobTag)",
          "--env", "PIPELINE_JOB_URL=$($pipelineInstance.PipelineJobUrl)",
          "--env", "GIT_COMMIT_HASH=$($pipelineInstance.GitCommitHash)",
          "--env", "GIT_COMMIT_URL=$($pipelineInstance.GitCommitUrl)",
          "--env", "GIT_PR_NUMBER=$($pipelineInstance.GitPullRequestNumber)",
          "--env", "GIT_PR_URL=$($pipelineInstance.GitPullRequestUrl)"
        )
      }

      # Temporarily store the connection string in a file to be mounted as a secret
      # (Unfortunately we can't securely pass in an environment variable because Docker only supports the --secret switch on 'docker build', not 'docker run')
      $connectionString | Out-File -FilePath $ConnectionStringSecretFilePath -Encoding ascii -Force

      # Run the DB Script Runner Docker image
      Write-Debug "Executing DB Script Runner"
      $arguments = @("run")
      $arguments += @("--mount", "type=bind,source=$($ConnectionStringSecretFilePath),target=/run/secrets/ConnectionString,readonly")

      if ($hasServicePrincipalCredentials) {
        $servicePrincipalClientSecret = az keyvault secret show --name $servicePrincipalClientSecretName --vault-name $servicePrincipalKeyVaultName --query value -o tsv
        if (![string]::IsNullOrWhiteSpace($servicePrincipalClientSecret)) {
          $envVars += @("--env", "AZURE_TENANT_ID=$servicePrincipalTenantId")
          $envVars += @("--env", "AZURE_CLIENT_ID=$servicePrincipalClientId")
          $servicePrincipalClientSecret | Out-File -FilePath $ServicePrincipalSecretFilePath -Encoding ascii -Force
          $arguments += @("--mount", "type=bind,source=$($ServicePrincipalSecretFilePath),target=/run/secrets/AzureClientSecret,readonly")
        }
      }

      $arguments += $envVars
      $arguments += $imageTag

      & docker $arguments

      if (-not $?) {
        throw "Error executing DB Script Runner"
      }

      Write-Logs "`n─────────────────────────────────────────────────────────────────────`n" $Global:LogStyles.SuppressedColour
    } finally {
      if (Test-Path $ConnectionStringSecretFilePath -PathType Leaf) {
        Remove-Item $ConnectionStringSecretFilePath -ErrorAction Continue
      }
      if (Test-Path $ServicePrincipalSecretFilePath -PathType Leaf) {
        Remove-Item $ServicePrincipalSecretFilePath -ErrorAction Continue
      }
    }
  }
} finally {
  Pop-Location

  # Ensure the global context cleanup is run before exiting
  Write-Log "`nCleaning up Context" $Global:LogStyles.SuppressedColour
  if (!$PreserveContext) {
    $context.Cleanup()
    $Global:Context = $null
  } else {
    Write-Log "Preserve Context enabled. Skipping cleanup." $Global:LogStyles.WarningColour
  }
}

Write-Log "`nEnvironment Script Runs Complete" $Global:LogStyles.SuccessColour
