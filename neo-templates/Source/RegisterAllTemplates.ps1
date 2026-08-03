## Powershell script to register all NEO "dotnet" templates in the current folder.
# Written by Stewart Moss
# 2012-08-03
# Run from neo templates folder using this ADMINISTRATOR command shell.
# powershell "./RegisterAllTemplates.ps1"

function Neo-RegisterTemplate {
    Param (
        [Parameter(Position=0, Mandatory)][string]$foldername
      )    
    dotnet new -i $foldername
}

# list the current folder into $dirList, change the "." to a fixed path if you having trouble.
$dirList = dir . | ?{$_.PSISContainer}
foreach ($dir in $dirList) 
{ 
    Neo-RegisterTemplate($dir.FullName)
}

Echo Done!