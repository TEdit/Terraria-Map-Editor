$source = ".\TEditXna\bin\Release"
$version = Get-Content .\teditversion.txt
$destination = ".\build\TEdit3_BETA_$version.zip"

if (!(Test-Path -path .\build))
{
    New-Item .\build -type directory
}

Write-Host "Zipping $source ..." -ForegroundColor Yellow

 If(Test-path $destination) {Remove-item $destination}

Add-Type -assembly "system.io.compression.filesystem"

[io.compression.zipfile]::CreateFromDirectory($Source, $destination) 

Write-Host "Created $destination." -ForegroundColor Green