# Finds all z_EFCommands.txt files in the repository and resolves the solution
# directory that each command file's migrations should be run from.
#
# The z_EFCommands.txt file lives in either:
#   <MigrationsProject>/z_EFCommands.txt          (domain projects)
#   <MigrationsProject>/Migrations/z_EFCommands.txt  (most projects)
#
# The solution directory is the nearest ancestor folder that contains a .sln or .slnx file.
# If neither is found, the folder containing the migrations .csproj is used as a fallback.
#
# Output: one PSCustomObject per file with properties:
#   Label       - display name (path relative to repo root, unique across the repo)
#   FilePath    - absolute path to z_EFCommands.txt
#   SolutionDir - directory to cd into before running dotnet ef commands

param(
    [string]$RepoRoot = ""
)

if (-not $RepoRoot) {
    $RepoRoot = git rev-parse --show-toplevel 2>$null
    if ($LASTEXITCODE -ne 0) {
        $RepoRoot = (Get-Location).Path
    }
}

$RepoRoot = [System.IO.Path]::GetFullPath($RepoRoot)

function Find-SolutionDir([string]$startPath) {
    $dir = $startPath
    while ($dir) {
        $hasSln  = Get-ChildItem -Path $dir -Filter "*.sln"  -File -ErrorAction SilentlyContinue | Select-Object -First 1
        $hasSlnx = Get-ChildItem -Path $dir -Filter "*.slnx" -File -ErrorAction SilentlyContinue | Select-Object -First 1
        if ($hasSln -or $hasSlnx) {
            return $dir
        }
        # Do not walk above the repository root
        if ($dir -eq $RepoRoot) { break }
        $parent = Split-Path $dir -Parent
        if ($parent -eq $dir) { break }
        $dir = $parent
    }
    # Fallback: return the migrations project directory (parent of Migrations/ folder or parent of file)
    return $startPath
}

$commandFiles = Get-ChildItem -Path $RepoRoot -Recurse -Filter "z_EFCommands.txt" -ErrorAction SilentlyContinue

$results = @()

foreach ($file in $commandFiles) {
    $fileDir = $file.DirectoryName

    # The migrations project folder is the .csproj-containing ancestor.
    # Walk up from the file to find the first folder with a .csproj.
    $projectDir = $fileDir
    $dir = $fileDir
    while ($dir) {
        $csprojFiles = Get-ChildItem -Path $dir -Filter "*.csproj" -ErrorAction SilentlyContinue
        if ($csprojFiles) {
            $projectDir = $dir
            break
        }
        # Do not walk above the repository root
        if ($dir -eq $RepoRoot) { break }
        $parent = Split-Path $dir -Parent
        if ($parent -eq $dir) { break }
        $dir = $parent
    }

    $solutionDir = Find-SolutionDir -startPath $projectDir

    $relativePath = $projectDir.Substring($RepoRoot.TrimEnd('\', '/').Length).TrimStart('\', '/').Replace('\', '/')
    $results += [PSCustomObject]@{
        Label       = $relativePath
        FilePath    = $file.FullName
        SolutionDir = $solutionDir
    }
}

$results
