$scriptRoot = Split-Path -Parent $MyInvocation.MyCommand.Path

# Check if terraform-docs is installed
$terraformDocsInstalled = Get-Command terraform-docs -ErrorAction SilentlyContinue

if (-not $terraformDocsInstalled) {
  Write-Host "terraform-docs not found. Installing..."

  # Install terraform-docs using winget
  winget install Terraform-docs.Terraform-docs

  # Check if installation was successful
  $terraformDocsInstalled = Get-Command terraform-docs -ErrorAction SilentlyContinue

  if (-not $terraformDocsInstalled) {
    throw  "Failed to install terraform-docs. Please install it manually or check installation issues."
    return
  } else {
    Write-Host "terraform-docs installed successfully."
  }
} else {
  Write-Host "terraform-docs is already installed."
}

# Get all immediate directories in the script root
$directories = Get-ChildItem -Path $scriptRoot -Directory -Recurse

foreach ($dir in $directories) {
  # Change to the directory
  Set-Location -Path $dir.FullName

  # Generate the TF docs
  if (Test-Path (Join-Path -Path $dir.FullName -ChildPath "main.tf")) {
    # Generate the TF docs
    terraform-docs markdown table $dir.FullName --hide-empty=true --output-file README.Resources.md
  }
}

Set-Location -Path $scriptRoot