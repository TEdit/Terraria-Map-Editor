param(
    [string] $VersionPrefix = "4.2.8",
    [string] $VersionSuffix = $null
)

$buildArgs = @(
    "publish", 
    "-c"
    "Release"
    ".\src\TEdit.sln", 
    '/p:signcert="BC Code Signing"')

if ($null -ne $VersionPrefix) {
    $buildArgs += "/p:VersionPrefix=""$VersionPrefix"""
}

if ($null -ne $VersionSuffix) {
    $buildArgs += "/p:VersionSuffix=""$VersionSuffix"""
}

& dotnet $buildArgs

Remove-Item -Path ".\release" -Recurse -Force
Remove-Item -Path ".\TEdit*.zip"

mkdir -Path ".\release"

Copy-Item -Path ".\src\TEdit\bin\Release\net462\publish" -Destination ".\release\TEdit-$VersionPrefix\" -Recurse -Force
Copy-Item -Path ".\schematics" -Destination ".\release" -Recurse

# Create ZIP Release
Compress-Archive -Path ".\release\*" -DestinationPath ".\TEdit$VersionPrefix.zip"

# Create Installer

#Compress-Archive -Path ".\src\TEdit\bin\Release\net462\publish\*" -DestinationPath ".\TEdit$VersionPrefix-$VersionSuffix.zip"
