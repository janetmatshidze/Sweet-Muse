<#
.SYNOPSIS
  Schedules one or more GoCD pipelines
.PARAMETER GoCdServerUrl
  The URL of the GoCD server, e.g: https://servername:8153/go. (Required, Defaults to the environment variable 'GO_SERVER_URL', with a fallback to 'http://singdevops1:8153/go')
.PARAMETER GoCdAccessToken
  The GoCD access token to use for authentication (Required, Defaults to the environment variable 'GO_ACCESS_TOKEN')
.PARAMETER PipelineNames
  A comma separated list of the pipeline names (Required)
.PARAMETER PipelinePrefix
  The prefix to use for the pipeline names. For when you want to use 'pre-generation' pipeline names. (Optional)
  When GoCD pipelines are generated from a blueprint, their names get converted to use underscores as the separator.
  Since 'internal' pipeline names within blueprints are typically dash separated, when generating pipeline names
  using a prefix, any dashes will be replaced with underscores to ensure the names match those in GoCD.
#>
[CmdletBinding()]
Param(
  [string]$GoCdServerUrl = $Env:GO_SERVER_URL ?? "http://singdevops1:8153/go",
  [string]$GoCdAccessToken = $Env:GO_ACCESS_TOKEN,
  [Parameter(Mandatory)][string]$PipelineNames,
  [string]$PipelinePrefix
)

# Locate the scripts root path and load libraries
$scriptsPath = $PSScriptRoot; while ($scriptsPath -and (Split-Path $scriptsPath -Leaf) -ne "scripts") { $scriptsPath = Split-Path $scriptsPath }
if (!$scriptsPath) { throw "Could not locate the scripts root folder. Please ensure this script is nested beneath a 'scripts' parent folder." }
. (Join-Path $scriptsPath "libraries/common.ps1")

# Configure context for the runtime environment if a script is present.
$contextScript = "$scriptsPath/context/configure-runtime-context.ps1"
if (Test-Path $contextScript) {
  & $contextScript
}

# Ensure all defaulted values are set
Assert-HasValue $GoCdServerUrl "GoCdServerUrl is required."
Assert-HasValue $GoCdAccessToken "GoCdAccessToken is required."

Write-Log "`n[ Schedule GoCD Pipelines ]" $Global:LogStyles.Heading1Colour

$pipelines = $PipelineNames -split ","
foreach ($pipeline in $pipelines) {
  if (![string]::IsNullOrEmpty($PipelinePrefix)) {
    $pipeline = "$($PipelinePrefix)_$($pipeline.replace("-", "_"))"
  }

  Write-Log "`nScheduling pipeline: $pipeline" $Global:LogStyles.EmphasisColour

  $requestUrl = "$GoCdServerUrl/api/pipelines/$pipeline/schedule"
  $body = "{ `"update_materials_before_scheduling`": true }"

  # Post to the GoCD API, putting the Access Token into the Authorisation Header as a Bearer Token
  $headers = @{
    "Authorization" = "Bearer $GoCdAccessToken"
    "Accept"        = "application/vnd.go.cd.v1+json"
  }

  $responseCode = $null
  $response = (Invoke-RestMethod -Method Post -Uri $requestUrl -Headers $headers -ContentType "application/json" -Body $body -SkipHttpErrorCheck -StatusCodeVariable "responseCode")

  if ($responseCode -eq 202) {
    Write-Log "$($responseCode): $($response.message)" $Global:LogStyles.SuccessColour
  } elseif ($responseCode -eq 409) {
    Write-Log "$($responseCode): $($response.message)" $Global:LogStyles.WarningColour
  } else {
    Write-Log "Unexpected Response Code" $colours.BrightRed
    Write-Log "$($responseCode): $($response.message)" $Global:LogStyles.ErrorColour
  }
}

Write-Log "`nPipeline scheduling completed." $Global:LogStyles.SuccessColour
