<#
.SYNOPSIS
  Retrieves information for a specific commit in a GitHub repository.
.PARAMETER Owner
  The owner of the GitHub repository.
.PARAMETER RepositoryName
  The name of the GitHub repository.
.PARAMETER CommitHash
  The hash of the commit to retrieve information for.
.OUTPUTS
  A response object, containing the fields: 'success', 'responseCode', and 'body'.
  Note that the response body does not need deserialisation.
#>
function Get-GitHubCommit {
  Param(
    [Parameter(Mandatory = $true, Position = 0)][string]$Owner,
    [Parameter(Mandatory = $true, Position = 1)][string]$RepositoryName,
    [Parameter(Mandatory = $true, Position = 2)][string]$CommitHash,
    [string]$AccessToken = $Env:GitHubToken
  )

  if ([string]::IsNullOrEmpty($AccessToken)) {
    throw "Get-GitHubCommit: GitHub Access Token is required."
  }

  $headers = @{
    "Accept"               = "application/vnd.github+json"
    "Authorization"        = "Bearer $AccessToken"
    "X-GitHub-Api-Version" = "2022-11-28"
  }

  $url = "https://api.github.com/repos/$Owner/$RepositoryName/commits/$CommitHash"
  return (Invoke-GetRequest $url $headers)
}

<#
.SYNOPSIS
  Retrieves pull requests associated with a specific commit in a GitHub repository.
.PARAMETER Owner
  The owner of the GitHub repository.
.PARAMETER RepositoryName
  The name of the GitHub repository.
.PARAMETER CommitHash
  The hash of the commit to retrieve information for.
.PARAMETER HeadPullRequestOnly
  If specified, and more than one pull request is associated with the commit, only the
  pull request where the commit is the head will be returned.
.PARAMETER DefaultBranch
  The default branch of the repository. Defaults to 'main'. Used when HeadPullRequestOnly is
  specified to help identify the correct pull request.
.OUTPUTS
  A response object, containing the fields: 'success', 'responseCode', and 'body'.
  Note that the response body does not need deserialisation.
#>
function Get-GitHubCommitPullRequests {
  Param(
    [Parameter(Mandatory = $true, Position = 0)][string]$Owner,
    [Parameter(Mandatory = $true, Position = 1)][string]$RepositoryName,
    [Parameter(Mandatory = $true, Position = 2)][string]$CommitHash,
    [string]$AccessToken = $Env:GitHubToken,
    [switch]$HeadPullRequestOnly,
    [string]$DefaultBranch = "main"
  )

  if ([string]::IsNullOrEmpty($AccessToken)) {
    throw "Get-GitHubCommit: GitHub Access Token is required."
  }

  $headers = @{
    "Accept"               = "application/vnd.github+json"
    "Authorization"        = "Bearer $AccessToken"
    "X-GitHub-Api-Version" = "2022-11-28"
  }

  # https://api.github.com/repos/SingularSystems/neo-iac-sampleproject/commits/153ea4231c92d2e341d5ac1b74f7e7e0b1cd3ca8/pulls
  $url = "https://api.github.com/repos/$Owner/$RepositoryName/commits/$CommitHash/pulls"
  $result = (Invoke-GetRequest $url $headers)

  # If we only want the head request, and more than one was returned, apply some filtering logic
  if ($result.success -and $HeadPullRequestOnly.IsPresent -and $result.body.Count -gt 1) {
    $headPullRequest = $null
    $multipleHeadPRsFound = $false
    Write-Host "Pull requests returned: $($result.body.Count)"
    foreach ($pullRequest in $result.body) {
      if ($null -ne $pullRequest.base -and $pullRequest.base.ref -eq $DefaultBranch) {
        if ($null -ne $headPullRequest) {
          $multipleHeadPRsFound = $true
        }

        $headPullRequest = $pullRequest
      }
    }

    if ($multipleHeadPRsFound) {
      Write-Log "WARNING (Get-GitHubCommitPullRequests): More than one pull request was found with a base referring to the default branch '$DefaultBranch'. The last matching pull request will be returned." $Global:LogStyles.WarningColour
    }

    if ($null -eq $headPullRequest) {
      # If no PRs were found, return the first one in the list
      Write-Log "WARNING (Get-GitHubCommitPullRequests): Multiple pull requests were found, but none with a base referring to the default branch '$DefaultBranch'. The first pull request in the list will be returned." $Global:LogStyles.WarningColour
      $result.body = @($result.body[0])
    } else {
      # Otherwise replace the full list with just the head PR
      $result.body = @($headPullRequest)
    }
  }

  return $result
}

