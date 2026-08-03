# Resolves the Neo framework source root directory.
#
# Resolution order:
#   1. NEO_SOURCE environment variable (e.g. set NEO_SOURCE=C:\dev\neo)
#   2. .github/neo-local.env file in the repo root (key=value, one per line)
#   3. ../neo relative to the git repository root (default convention)
#
# A candidate path is only accepted when every expected Neo repo subfolder
# (e.g. neo-core) exists inside it, confirming the repos have actually been cloned.
#
# Usage: $neoRoot = .\find-neo-root.ps1
# Exits with code 1 and an error message if no valid path is found.

$repoRoot = git rev-parse --show-toplevel 2>$null
if ($LASTEXITCODE -ne 0) {
    $repoRoot = $PSScriptRoot
}

function Test-NeoRoot([string]$path) {
    # Verify the directory exists and contains every expected Neo repo subfolder,
    # so a partial checkout is not treated as valid and will trigger pull-neo-repos.ps1.
    $requiredRepos = @(
        "neo-core",
        "neo-analyzers",
        "neo-tools",
        "neo-ui"
    )
    if (-not (Test-Path $path)) { return $false }
    foreach ($repo in $requiredRepos) {
        if (-not (Test-Path (Join-Path $path $repo))) { return $false }
    }
    return $true
}

# 1. Environment variable
if ($env:NEO_SOURCE -and (Test-NeoRoot $env:NEO_SOURCE)) {
    Write-Output $env:NEO_SOURCE
    exit 0
}

# 2. Local config file
$localEnvFile = Join-Path $repoRoot ".github" "neo-local.env"
if (Test-Path $localEnvFile) {
    $line = Get-Content $localEnvFile | Where-Object { $_ -match '^NEO_SOURCE\s*=' } | Select-Object -First 1
    if ($line) {
        $value = ($line -split '=', 2)[1].Trim().Trim('"').Trim("'")
        if (Test-NeoRoot $value) {
            Write-Output $value
            exit 0
        }
    }
}

# 3. Default: ../neo relative to the repo root
$default = Join-Path $repoRoot ".." "neo"
$default = [System.IO.Path]::GetFullPath($default)
if (Test-NeoRoot $default) {
    Write-Output $default
    exit 0
}

Write-Error "Neo source root not found (or repos have not been cloned). Set the NEO_SOURCE environment variable or add 'NEO_SOURCE=<path>' to .github/neo-local.env"
exit 1
