# Clones or updates all Neo framework repositories into the given target directory.
#
# Usage: .\pull-neo-repos.ps1 [-TargetDir <path>]
#
#   -TargetDir   Directory to clone repos into. Defaults to ../neo relative to the git repo root.
#
# For repos that already exist, a `git pull` is performed.
# For repos that do not exist, a `git clone` is performed.

param(
    [string]$TargetDir = ""
)

$repos = @(
    "neo-core",
    "neo-analyzers",
    "neo-tools",
    "neo-ui"
)

$org = "SingularSystems"

if (-not $TargetDir) {
    $repoRoot = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0) {
        $repoRoot = $PSScriptRoot
    }
    $TargetDir = [System.IO.Path]::GetFullPath((Join-Path $repoRoot ".." "neo"))
}

if (-not (Test-Path $TargetDir)) {
    Write-Host "Creating directory: $TargetDir" -ForegroundColor Cyan
    New-Item -ItemType Directory -Path $TargetDir -Force | Out-Null
}

Write-Host "Neo repos target: $TargetDir" -ForegroundColor Cyan
Write-Host ""

$failed = @()

foreach ($repo in $repos) {
    $repoPath = Join-Path $TargetDir $repo
    if (Test-Path (Join-Path $repoPath ".git")) {
        Write-Host "Pulling $repo ..." -ForegroundColor Yellow
        Push-Location $repoPath
        git pull 2>&1 | Write-Host
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "git pull failed for $repo"
            $failed += $repo
        }
        Pop-Location
    } else {
        Write-Host "Cloning $repo ..." -ForegroundColor Yellow
        git clone "https://github.com/$org/$repo.git" $repoPath 2>&1 | Write-Host
        if ($LASTEXITCODE -ne 0) {
            Write-Warning "git clone failed for $repo"
            $failed += $repo
        }
    }
    Write-Host ""
}

if ($failed.Count -gt 0) {
    Write-Error "The following repos failed: $($failed -join ', ')"
    exit 1
}

Write-Host "All Neo repos are up to date in: $TargetDir" -ForegroundColor Green
Write-Output $TargetDir
exit 0
