<#
.SYNOPSIS
  Restarts AKS deployments in a target client namespace.
.PARAMETER Project
  The project prefix used to resolve app-space state and default namespace values.
.PARAMETER Location
  The Azure location prefix for the target app space.
.PARAMETER HostPrefix
  The shared host / host-space prefix for the target AKS cluster.
.PARAMETER Environment
  The app-space environment whose deployments should be restarted.
.PARAMETER DeploymentNames
  Optional comma-separated list of deployment names to restart. If omitted, all deployments in the namespace are restarted.
.PARAMETER RolloutTimeoutSeconds
  Timeout in seconds to wait for each deployment rollout to complete. Defaults to 30.
.PARAMETER AutoApprove
  Automatically approves the restart operation.
.PARAMETER PreserveContext
  Preserves the global context for reuse after the script completes.
.PARAMETER DryRun
  Shows which deployments would be restarted without issuing restart commands.
#>
[CmdletBinding()]
param(
  [string]$Project = $Env:ProjectPrefix,
  [string]$Location = $Env:LocationPrefix,
  [string]$HostPrefix = $Env:HostPrefix,
  [string]$Environment = $Env:EnvironmentPrefix,
  [string]$DeploymentNames = "",
  [int]$RolloutTimeoutSeconds = 30,
  [switch]$AutoApprove,
  [switch]$PreserveContext,
  [switch]$DryRun
)

function Get-NormalizedDeploymentNames {
  [OutputType([string[]])]
  param([string]$Names = "")

  if ([string]::IsNullOrWhiteSpace($Names)) {
    return @()
  }

  return @($Names.Split(",") | ForEach-Object { $_.Trim().ToLowerInvariant() } | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
}

function Get-NormalizedDeploymentMap {
  [OutputType([hashtable])]
  param([string[]]$Names = @())

  $normalizedDeploymentMap = @{}
  foreach ($name in $Names) {
    if ([string]::IsNullOrWhiteSpace($name)) {
      continue
    }

    $normalizedName = $name.Trim().ToLowerInvariant()
    if (![string]::IsNullOrWhiteSpace($normalizedName)) {
      $normalizedDeploymentMap[$normalizedName] = $name
    }
  }

  return $normalizedDeploymentMap
}

function Invoke-Kubectl {
  [OutputType([string])]
  param([string[]]$Arguments, [string]$ErrorMessage)

  $output = & kubectl @Arguments 2>&1
  $exitCode = $LASTEXITCODE
  if ($exitCode -ne 0) {
    if ([string]::IsNullOrWhiteSpace($output)) {
      throw "$ErrorMessage [exit code: $exitCode]"
    }

    throw "$ErrorMessage [exit code: $exitCode] [kubectl output: $output]"
  }

  return $output
}

function Get-KubernetesDeployments {
  [OutputType([string[]])]
  param([string]$Namespace)

  $output = Invoke-Kubectl `
    -Arguments @("get", "deployment", "-n", $Namespace, "-o", "json") `
    -ErrorMessage "Failed to retrieve deployments from namespace '$Namespace'."

  if ([string]::IsNullOrWhiteSpace($output)) {
    throw "kubectl returned no deployment data for namespace '$Namespace'."
  }

  $deploymentResponse = $output | ConvertFrom-Json
  $deployments = @($deploymentResponse.items | ForEach-Object { $_.metadata.name } | Where-Object { ![string]::IsNullOrWhiteSpace($_) } | Sort-Object -Unique)
  if ($deployments.Count -eq 0) {
    throw "No deployments were found in namespace '$Namespace'."
  }

  return $deployments
}

try {
  $ErrorActionPreference = "Stop"
  $InformationPreference = "Continue"
  $VerbosePreference = ($PSBoundParameters["Verbose"] -or [System.Convert]::ToBoolean($Env:NeoVerboseLogging)) ? "Continue" : "SilentlyContinue"
  $DebugPreference = ($PSBoundParameters["Debug"] -or [System.Convert]::ToBoolean($Env:NeoDebugLogging)) ? "Continue" : "SilentlyContinue"
  $stopWatch = [System.Diagnostics.Stopwatch]::StartNew()

  if ($RolloutTimeoutSeconds -le 0) { throw "RolloutTimeoutSeconds must be greater than zero." }

  # Locate the IaC root path and load scripts libraries
  $iacPath = $PSScriptRoot; while ($iacPath -and (Split-Path $iacPath -Leaf) -ne "IaC") { $iacPath = Split-Path $iacPath }
  if (!$iacPath) { throw "Could not locate the IaC root folder. Please ensure this script is nested beneath an 'IaC' parent folder." }
  $scriptsPath = (Join-Path $iacPath "scripts")
  . (Join-Path $scriptsPath "libraries/common.ps1")

  [PSCustomObject]$context = Get-GlobalContext -New:$(!$PreserveContext)

  if ($PreserveContext -and ($null -ne $context.Environment)) {
    $environmentChanged =
    $context.Environment.Project -ne $Project -or
    $context.Environment.Location -ne $Location -or
    $context.Environment.Host -ne $HostPrefix -or
    $context.Environment.Environment -ne $Environment

    if ($environmentChanged) {
      Write-Log "Environment has changed. Re-creating context." $Global:LogStyles.WarningColour
      $context = Get-GlobalContext -New
    } else {
      Write-Log "Environment has not changed. Reusing context from a previous run." $Global:LogStyles.SuppressedColour
    }
  }

  $shouldConfigureContext =
  (!$PreserveContext) -or
  ($null -eq $context.Environment) -or
  ($null -eq $context.Azure) -or
  ($null -eq $context.State)

  if ($shouldConfigureContext) {
    $context.Add("Environment", (New-Context @{
          Project     = $Project
          Location    = $Location
          Host        = $HostPrefix
          Environment = $Environment
        }), $true)

    & "$scriptsPath/context/configure-runtime-context.ps1"
    & "$scriptsPath/context/configure-cloud-context.ps1"
    & "$scriptsPath/context/configure-state-context.ps1" -ProjectPrefix $Project -LocationPrefix $Location -HostPrefix $HostPrefix -Environment $Environment
    & "$scriptsPath/context/configure-kubernetes-context.ps1"
  }

  $resolvedNamespace = $Context.State.AppSpace.k8s_namespace.namespace.name ?? "$($Project.ToLower())-$($Environment.ToLower())"
  $clusterName = $Context.State.SharedHosts.aks_cluster.aks.name

  if ([string]::IsNullOrWhiteSpace($resolvedNamespace)) {
    throw "Failed to resolve the Kubernetes namespace for the target environment. Please ensure the app-space state is configured with a valid namespace, or ensure that the 'Project' and 'Environment' parameters are set to valid values that can be used to derive a default namespace."
  }

  if ([string]::IsNullOrWhiteSpace($clusterName)) {
    throw "Failed to resolve the AKS cluster name for the target environment. Please ensure the host state is configured with a valid AKS cluster name."
  }

  $availableDeployments = Get-KubernetesDeployments -Namespace $resolvedNamespace
  $requestedDeployments = Get-NormalizedDeploymentNames -Names $DeploymentNames
  $availableDeploymentMap = Get-NormalizedDeploymentMap -Names $availableDeployments

  if ($requestedDeployments.Count -gt 0) {
    $missingDeployments = @($requestedDeployments | Where-Object { -not $availableDeploymentMap.ContainsKey($_) })
    if ($missingDeployments.Count -gt 0) {
      throw "The following deployment(s) were not found in namespace '$resolvedNamespace': $($missingDeployments -join ", ")."
    }

    $targetDeployments = @($requestedDeployments | ForEach-Object { $availableDeploymentMap[$_] })
  } else {
    $targetDeployments = @($availableDeployments)
  }

  Write-Log "`n[ Restart AKS Deployments ]" $Global:LogStyles.Heading1Colour
  Write-Logs (Format-KeyValues ([ordered]@{
        "Project"                 = $Project
        "Location"                = $Location
        "Host"                    = $HostPrefix
        "Environment"             = $Environment
        "Cluster"                 = $clusterName
        "Namespace"               = $resolvedNamespace
        "Available Deployments"   = ($availableDeployments -join ", ")
        "Target Deployments"      = ($targetDeployments -join ", ")
        "Deployment Count"        = $targetDeployments.Count
        "Rollout Timeout Seconds" = $RolloutTimeoutSeconds
        "Auto Approve"            = ($AutoApprove.IsPresent ? "Yes" : "No")
        "Dry Run"                 = ($DryRun.IsPresent ? "Yes" : "No")
      })) $Global:LogStyles.EmphasisColour

  if (!$AutoApprove) {
    $restartSummary = "You are about to restart $($targetDeployments.Count) deployment(s) in namespace '$resolvedNamespace'."
    if (!(Show-ConfirmationMessage $restartSummary)) {
      Write-Log "Restart operation cancelled." $Global:LogStyles.WarningColour
      return
    }
  }

  foreach ($deploymentName in $targetDeployments) {
    if ($DryRun) {
      Write-Log "[DryRun] Would restart deployment '$deploymentName' in namespace '$resolvedNamespace'." $Global:LogStyles.WarningColour
      continue
    }

    Write-Log "`nRestarting deployment '$deploymentName'..." $Global:LogStyles.Heading2Colour
    $restartOutput = Invoke-Kubectl `
      -Arguments @("rollout", "restart", "deployment/$deploymentName", "--namespace", $resolvedNamespace) `
      -ErrorMessage "Failed to restart deployment '$deploymentName' in namespace '$resolvedNamespace'."
    
    if (![string]::IsNullOrWhiteSpace($restartOutput)) {
      Write-Host $restartOutput
    }

    Write-Log "Waiting for rollout to complete..." $Global:LogStyles.SuppressedColour
    $statusOutput = Invoke-Kubectl `
      -Arguments @("rollout", "status", "deployment/$deploymentName", "--namespace", $resolvedNamespace, "--timeout", "$($RolloutTimeoutSeconds)s") `
      -ErrorMessage "Rollout did not complete successfully for deployment '$deploymentName' in namespace '$resolvedNamespace'."
   
    if (![string]::IsNullOrWhiteSpace($statusOutput)) {
      Write-Host $statusOutput
    }
  }

  if ($DryRun) {
    Write-Log "`nDONE - Dry run completed. No deployments were restarted. (Time elapsed: $($stopWatch.Elapsed.ToString("hh\:mm\:ss")))" $Global:LogStyles.SuccessColour
  } else {
    Write-Log "`nDONE - Deployment restart operation completed successfully. (Time elapsed: $($stopWatch.Elapsed.ToString("hh\:mm\:ss")))" $Global:LogStyles.SuccessColour
  }
} catch {
  Write-Log "ERROR: AKS deployment restart operation failed." $Global:LogStyles.ErrorColour
  throw
} finally {
  if ($null -ne $stopWatch) {
    $stopWatch.Stop()
    Write-Verbose "Total script execution time: $($stopWatch.Elapsed.ToString("hh\:mm\:ss"))"
  }

  if ($PreserveContext) {
    Write-Log "Preserve Context enabled. Skipping cleanup." $Global:LogStyles.WarningColour
  } elseif ($null -ne $context -and ($context.PSObject.Methods.Name -contains "Cleanup")) {
    Write-Log "Cleaning up Context" $Global:LogStyles.SuppressedColour
    $context.Cleanup()
    $Global:Context = $null
  }
}
