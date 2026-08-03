<#
.SYNOPSIS
Deploys a Neo WebAPI to Azure App Service

.DESCRIPTION
This script deploys a docker image to an existing App Service instance (provisioning is handled by terraform). The following steps are performed:
- Set App Settings
- Set Connection Strings
- Set the Container to use from Azure Container Registry
#>
[CmdletBinding()]
param(
  [Parameter()][string]$ProjectPrefix = $Env:ProjectPrefix,
  [Parameter()]$LocationPrefix = $Env:LocationPrefix,
  [Parameter()]$HostPrefix = $Env:HostPrefix,
  [Parameter()]$Environment = $Env:EnvironmentPrefix,
  [Parameter(Mandatory = $true)][string]$ServiceName,
  [Parameter(Mandatory = $true)][string]$Registry,
  [Parameter(Mandatory = $true)][string]$ImageTag,
  [Parameter()][string]$TenantId = $Env:AZURE_TENANT_ID,
  [Parameter()][string]$SubscriptionId = $Env:AZURE_SUBSCRIPTION_ID,
  [Parameter()][string]$ResourceGroup = $Env:AZURE_RESOURCE_GROUP,
  [Parameter()][string]$ClientId = $Env:AZURE_CLIENT_ID,
  [Parameter()][string]$ClientSecret = $Env:AZURE_CLIENT_SECRET,
  [Parameter()][string]$DockerRegistryUser = $Env:DOCKER_REGISTRY_USER,
  [Parameter()][string]$DockerRegistryPassword = $Env:DOCKER_REGISTRY_PASSWORD
)

$ErrorActionPreference = "Stop"

if ([string]::IsNullOrEmpty($Environment)) { throw "Environment is required" }

# Locate the IaC root path and load libraries
$iacPath = $PSScriptRoot; while ($iacPath -and (Split-Path $iacPath -Leaf) -ne "IaC") { $iacPath = Split-Path $iacPath }
if (!$iacPath) { throw "Could not locate the IaC root folder. Please ensure this script is nested beneath an 'IaC' parent folder." }
$scriptsPath = (Join-Path $iacPath "scripts")
if (!(Test-Path $scriptsPath)) { throw "Could not locate the scripts folder. Please ensure there is a 'scripts' folder underneath the 'IaC' folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")
. (Join-Path $scriptsPath "libraries/templating.ps1")

Write-Logs @(
  "`n",
  " █▀▄ █▀▀ █▀█ █   █▀█ █ █   █▀█ █▀█ █▀█   █▀▀ █▀▀ █▀▄ █ █ ▀█▀ █▀▀ █▀▀",
  " █ █ █▀▀ █▀▀ █   █ █  █    █▀█ █▀▀ █▀▀   ▀▀█ █▀▀ █▀▄ ▀▄▀  █  █   █▀▀",
  " ▀▀  ▀▀▀ ▀   ▀▀▀ ▀▀▀  ▀    ▀ ▀ ▀   ▀     ▀▀▀ ▀▀▀ ▀ ▀  ▀  ▀▀▀ ▀▀▀ ▀▀▀"
  ""
) $Global:LogStyles.Heading1Colour

$context = Get-GlobalContext -New

try {
  $stopwatch = [system.diagnostics.stopwatch]::StartNew()

  # Configure context for the runtime environment if a script is present.
  $runtimeContextScript = "$scriptsPath/context/configure-runtime-context.ps1"
  if (Test-Path $runtimeContextScript) {
    & $runtimeContextScript
  }
  $runtimeContextTime = $stopwatch.Elapsed.TotalMilliseconds

  # Configure cloud provider context if a script is present.
  $cloudContextScript = "$scriptsPath/context/configure-cloud-context.ps1"
  if (Test-Path $cloudContextScript) {
    & $cloudContextScript
  }
  $cloudContextTime = $stopwatch.Elapsed.TotalMilliseconds

  # Configure state context if a script is present.
  $stateContextScript = "$scriptsPath/context/configure-state-context.ps1"
  if (Test-Path $stateContextScript) {
    & $stateContextScript -ProjectPrefix $ProjectPrefix -LocationPrefix $LocationPrefix -HostPrefix $HostPrefix -Environment $Environment
  }
  $stateContextTime = $stopwatch.Elapsed.TotalMilliseconds

  # Create a list of the possible settings files
  $settingsFileNames = @(
    "$ServiceName.$Environment.psd1.sbn",
    "$ServiceName.$Environment.psd1",
    "$ServiceName.psd1.sbn"
    "$ServiceName.psd1"
  )

  Write-Log "`n[ App Settings ]" $Global:LogStyles.Heading1Colour

  # Use the first file that exists in the apps settings folder
  $appConfigPath = (Join-Path $iacPath "config/apps")
  $settingsFilePath = $null
  foreach ($fileName in $settingsFileNames) {
    $path = (Join-Path $appConfigPath "$ServiceName/$fileName")
    if (Test-Path $path) {
      $settingsFilePath = $path
      break
    }
  }

  if (-not $settingsFilePath) {
    throw "App settings file not found for service '$ServiceName' in environment '$Environment'. Searched for: $($settingsFileNames -join ', ')"
  }

  # If the file found is a scriban template, render it
  if ($settingsFilePath.EndsWith(".sbn")) {
    Write-Log "Rendering app settings template: $(Split-Path $settingsFilePath -Leaf)"
    $basePath = (Split-Path $settingsFilePath -Parent)
    $templateFileName = (Split-Path $settingsFilePath -Leaf)
    $renderedSettingsFilePath = (Join-Path $basePath "~$($templateFileName.replace(".sbn", [string]::Empty))")

    $templateContent = Get-Content $settingsFilePath -Raw
    Build-Template $templateContent $context.State | Out-File -FilePath $renderedSettingsFilePath
    $settingsFilePath = $renderedSettingsFilePath
  }

  Write-Log "Using App Settings file: $(Split-Path $settingsFilePath -Leaf)"

  $settings = (Import-PowerShellDataFile -Path $settingsFilePath)
  $appServiceName = $null

  if ($context.State -and $context.State.AppSpace) {
    $appSpace = $context.State.AppSpace
    $ResourceGroup = $appSpace.resource_group.name
    $appServiceName = $appSpace.app_services.services.$ServiceName.name
  }


  Write-Log "`nDeployment Configuration" $Global:LogStyles.Heading2Colour
  Write-Logs (Format-KeyValues ([ordered]@{
        "TenantId"         = $TenantId
        "SubscriptionId"   = $SubscriptionId
        "Resource Group"   = $ResourceGroup
        "App Service Name" = $appServiceName
        "Environment"      = $Environment
      })) $Global:LogStyles.EmphasisColour

  # Ensure that the parameters which use environment variable defaults actually have values.
  if ([string]::IsNullOrEmpty($TenantId)) { throw "TenantId is required" }
  if ([string]::IsNullOrEmpty($SubscriptionId)) { throw "SubscriptionId is required" }
  if ([string]::IsNullOrEmpty($DockerRegistryUser) ) { throw "DockerRegistryUser must be provided" }
  if ([string]::IsNullOrEmpty($DockerRegistryPassword) ) { throw "DockerRegistryPassword must be provided" }
  if ([string]::IsNullOrEmpty($ResourceGroup)) { throw "ResourceGroup is required" }
  if ([string]::IsNullOrEmpty($appServiceName)) { throw "AppServiceName is required" }

  Write-Log "`n[ Deploy App Service ]" $Global:LogStyles.Heading1Colour

  # Stop the App
  Write-Log "Stopping App Service '$appServiceName'"
  $app = az webapp stop --resource-group $ResourceGroup --name $appServiceName

  # Set the App Settings
  Write-Log "Updating App Settings"
  # Need to make the call like this because the settings argument doesn't get passed properly otherwise due to the spaces
  # between the settings. (We would end up with just one setting containing its value, as well as all other the settings)
  $appSettings = ($settings.AppSettings.GetEnumerator() | ForEach-Object { "$($_.Key)=""$($_.Value)""" }) -join " "
  $result = Invoke-Expression -Command "az webapp config appsettings set -g $ResourceGroup -n $appServiceName --settings $AppSettings"
  if (!$result) {
    throw "Error updating App Settings"
  }

  # Set the Connection Strings
  if ($settings.ConnectionStrings.Count -gt 0) {
    # One or more connections: set them on the app service
    Write-Log "Updating Connection Strings"
    $connectionStrings = ($settings.ConnectionStrings.GetEnumerator() | ForEach-Object { "$($_.Key)=""$($_.Value)""" }) -join ' '
    $result = Invoke-Expression -Command "az webapp config connection-string set -g $ResourceGroup -n $appServiceName -t SQLServer --settings $connectionStrings"
    if (!$result) {
      throw "Error updating Connection Strings"
    }
  }

  # Build up the container image name
  $imageName = "$($Registry)/$($ProjectPrefix).$($ServiceName):$($ImageTag)"

  # Set the container
  Write-Log "Setting container config for image '$imageName'"

  $app = az webapp config container set `
    --name $appServiceName `
    --resource-group $ResourceGroup `
    --container-image-name $imageName `
    --container-registry-url "https://$($Registry)" `
    --container-registry-user $DockerRegistryUser `
    --container-registry-password $DockerRegistryPassword
  if (!$app) {
    throw "Error setting container config for image"
  }

  # Start the App
  Write-Log "Starting App Service '$AppServiceName'"
  az webapp start --resource-group $ResourceGroup --name $appServiceName

  $appServiceDeployTime = $stopwatch.Elapsed.TotalMilliseconds

} finally {
  if ($ShowTimings) {
    Write-Log "`nExecution Timings:" $Global:LogStyles.Heading2Colour
    Write-Log "Configure Runtime Context    : $([int]$runtimeContextTime)ms"
    Write-Log "Configure Cloud Context      : $([int]($cloudContextTime - $runtimeContextTime))ms"
    Write-Log "Configure State Context      : $([int]($stateContextTime - $cloudContextTime))ms"
    Write-Log "Deploy App Service           : $([int]($appServiceDeployTime - $stateContextTime))ms"

    Write-Log "`nTotal Time Elapsed           : $([int]($stopwatch.Elapsed.TotalMilliseconds))ms"
  }

  $stopwatch.Stop()

  # Ensure the global context cleanup is run before exiting
  Write-Log "`nCleaning up Context" $Global:LogStyles.SuppressedColour
  $context.Cleanup()
}

Write-Log "`nDeployment Successful`n" $Global:LogStyles.SuccessColour
