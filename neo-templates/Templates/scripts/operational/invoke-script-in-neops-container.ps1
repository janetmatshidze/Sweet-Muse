<#
.SYNOPSIS
  Executes a PowerShell script inside a Neo.PS docker container.
  (This is a standalone 'bootstrap' script to get a custom script running in a Neo.PS container with all dependancies installed what)
  Dependencies: PowerShell 7+, Docker
.PARAMETER ScriptPath
  The path of the script to execute. Must be relative to the VolumePath
.PARAMETER VolumePath
  The path to map into the container as a volume
.PARAMETER ContainerRegistryAuthType
  The kind of authentication required by the registry. Possible values: None, Azure  (Defaults to 'Azure')
.PARAMETER ContainerRegistry
  The container registry holding the container to run (Defaults to singular.azurecr.io)
.PARAMETER ContainerRepository
  The container repository of the container to run (Defaults to neo-ps)
.PARAMETER ContainerImageTag
  The container tag / version of the container (Defaults to 1.0.0)
.PARAMETER EnvironmentVariables
  Comma separated list of environment variables to pass into the container.
  Supports using wildcards (E.g. 'NeoAz*'). Note that environment variable values
  may not have spaces in them.
  (By default this is blank, any all environment variables with the Prefix 'Neo' are passed in)
.PARAMETER Shell
  The shell to use when executing the script in the container. (Defaults to 'pwsh')
.PARAMETER Interactive
  Should the session be interactive?
.PARAMETER UnboundArgs
  Additional parameters required by the script can be specified, and they will be automatically passed in when the script is executed in the container.
  Note that the script cannot accept any parameters with the same name as one of the parameters in this script.
  Most primitive data types can be used, such as [string], [int] or [DateTime], but [bool] should be avoided in favour of using [switch].
  Passing of complex object types is not supported.
#>
[CmdletBinding()]
param(
  [Parameter(Mandatory, Position = 0)][string]$ScriptPath,
  [string]$ScriptArguments = "",
  [string]$VolumePath = ".",
  [string]$AssetsVolumePath = ".",
  [ValidateSet("", "None", "Azure")]$ContainerRegistryAuthType = $Env:NeoPsContainerRegistryAuthType,
  [string]$ContainerRegistry = $Env:NeoPsContainerRegistry,
  [string]$ContainerRepository = $Env:NeoPsContainerRepository,
  [string]$ContainerImageTag = ![string]::IsNullOrEmpty($Env:NeoPsVersion) ? "v$($Env:NeoPsVersion)" : "v1.2.154",
  [string]$EnvironmentVariables = "Neo*,Operations*,CodeCoverage*,AZURE_*,AAD_*,CLOUDFLARE_*,GO_PIPELINE_*",
  [string]$Shell = "pwsh",
  [bool]$Interactive = $false,
  [Parameter(ValueFromRemainingArguments)] $UnboundArgs
)

$ErrorActionPreference = "Stop"
$InformationPreference = "Continue"

# Set defaults if no values were provided for certain parameters
if ([string]::IsNullOrEmpty($ContainerRegistryAuthType)) { $ContainerRegistryAuthType = "Azure" }
if ([string]::IsNullOrEmpty($ContainerRegistry)) { $ContainerRegistry = "singular.azurecr.io" }
if ([string]::IsNullOrEmpty($ContainerRepository)) { $ContainerRepository = "neo-ps" }
if ([string]::IsNullOrEmpty($ContainerImageTag)) { $ContainerImageTag = "1.2.154" }

Write-Host "======================================" -ForegroundColor Cyan
Write-Host "    Neo.PS Container Script Runner" -ForegroundColor Cyan
Write-Host "======================================`n" -ForegroundColor Cyan

# Check if the container image exists locally
$imageName = "$ContainerRegistry/$($ContainerRepository):v$($ContainerImageTag)"
$imageId = docker images -q $imageName

if ([string]::IsNullOrEmpty($imageId) -and $ContainerRegistryAuthType -ne "None") {
  # If not, we need to connect to the container registry so docker can pull it down
  Write-Information "Image '$imageName' does not exist locally. Connecting to ACR..."

  if ($ContainerRegistryAuthType -eq "Azure") {
    # Use existing login, if there is one
    $azToken = (az account get-access-token | ConvertFrom-Json)
    if ($null -ne $azToken -and [DateTime]$azToken.expiresOn -gt [DateTime]::Now) {
      Write-Information "Using existing login"
    } else {
      $tenantId = $Env:AZURE_TENANT_ID
      $servicePrincipalClientId = $Env:AZURE_CLIENT_ID
      $servicePrincipalSecret = $Env:AZURE_CLIENT_SECRET
      if ([string]::IsNullOrEmpty($tenantId)) { throw "Unable to connect to Azure, TenantId not provided" }
      if ([string]::IsNullOrEmpty($servicePrincipalClientId)) { throw "Unable to connect to Azure, ServicePrincipalClientId not provided" }
      if ([string]::IsNullOrEmpty($servicePrincipalSecret)) { throw "Unable to connect to Azure, ServicePrincipalSecret not provided" }

      $azLogin = az login -t $tenantId -u $servicePrincipalClientId -p $servicePrincipalSecret --service-principal
      if (!$azLogin) { throw "Error logging into Azure" }
    }

    $acrLogin = az acr login --name $ContainerRegistry
    if (!$acrLogin) { throw "Error logging into Azure Container ContainerRegistry '$ContainerRegistry'" }
  }
}

# Get Environment Variables to pass into the docker container
$neoVariables = @{}
$EnvironmentVariables.Split(",") | ForEach-Object {
  $value = (Get-ChildItem Env:$_)
  if ($value -is [Object[]]) {
    $value | ForEach-Object { $neoVariables[$_.Name] = $_.Value }
  } elseif ($value -is [hashtable]) {
    foreach ($key in $value.Keys) {
      $neoVariables[$key] = $value.$key
    }
  } elseif ($value -is [System.Collections.DictionaryEntry]) {
    $neoVariables[$value.Name] = $value.Value
  } elseif (![string]::IsNullOrWhiteSpace($value)) {
    # Assume it's a single key/value pair
    $neoVariables[$_] = $value
  } else {
    # No environment variables were found. Leave neoVariables empty.
  }
}

Write-Host "Environment Variables found:"
$neoVariables.Keys | ForEach-Object { Write-Host " - $_" }

# Run the script inside the docker container
$hostVolumePath = (Resolve-Path $VolumePath -ErrorAction Stop)
$hostScriptPath = (Resolve-Path $ScriptPath)
$scriptRelativePath = ((Split-Path -Path $hostScriptPath).Replace($hostVolumePath, ".").Replace("\", "/"))
$scriptName = (Split-Path -Path $hostScriptPath -Leaf)

$containerVolumePath = "/tmp/scripts"
$containerWorkingPath = "$containerVolumePath/$scriptRelativePath"
$containerScriptPath = "$containerVolumePath/$scriptRelativePath/$scriptName"

# Check if we need to mount external volumes with needed assets.
$hostAssetsPath = (Resolve-Path $AssetsVolumePath -ErrorAction Stop)
$containerAssetsPath = "/tmp"

# We need to dynamically generate the docker command to include the environment variables.
# Since Invoke-Expression is dangerous and can lead to code injections, we use the
# call operator (&) instead.
$envVars = ($neoVariables.ForEach({ $_.GetEnumerator().ForEach({
          # Escape any double quotes in environment strings, or the docker command will fail.
          $escapedValue = $_.Value.Replace("`"", "\`"")
          return "--env $($_.Name)=$($escapedValue)"
        }) | Join-String -Separator " " }))

$interactivityFlags = $Interactive ? "-it" : "" # This loses the Write-NeoInformation colours, will be fixed in the future.
$terminal = "xterm-256color"

$verbose = $VerbosePreference -eq "Continue" ? $true : $false
$preferenceParams = "-InformationAction:$InformationPreference -Verbose:$verbose -ErrorAction:$ErrorActionPreference -WarningAction:$WarningPreference"

try {

  $dockerArguments = "run -v $($hostVolumePath):$($containerVolumePath) -v $($hostAssetsPath):$($containerAssetsPath) -w $containerWorkingPath"
  $dockerArguments += ![string]::IsNullOrEmpty($terminal) ? " -e TERM=$terminal" : ""
  $dockerArguments += ![string]::IsNullOrEmpty($envVars) ? " $envVars" : ""
  $dockerArguments += ![string]::IsNullOrEmpty($interactivityFlags) ? " $interactivityFlags" : ""
  $dockerArguments += " $imageName $Shell $containerScriptPath $UnboundArgs $preferenceParams"

  Write-Debug "`n======================================================================================================="
  Write-Debug $($dockerArguments)
  Write-Debug "=======================================================================================================`n"

  # The call operator requires an array of strings with no spaces for the arguments
  Write-Host "`nExecuting Script '$scriptName' in container '$imageName'..." -ForegroundColor Cyan
  Write-Host "=======================================================================================================`n" -ForegroundColor Cyan
  & docker ($dockerArguments.Split(" "))

  # Ensure that we throw an error if one occurred in the containerised script
  if (-not $? ) {
    throw "Error occurred during containerised script execution."
  }
} catch { 
  throw "Error occurred during containerised script execution: $($_.Exception.Message)"
}
# If we logged into any services, log out
if ($azLogin) {
  Write-Information "`nLogging out of Azure..."
  az logout
}

Write-Host "`n=======================================================================================================" -ForegroundColor Cyan
Write-Host "Containerised Script Execution Completed" -ForegroundColor Cyan