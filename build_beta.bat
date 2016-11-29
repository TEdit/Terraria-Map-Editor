@echo off

echo Building Solution...
set msbuild="C:\Windows\Microsoft.NET\Framework\v4.0.30319\MSBuild.exe"
set nuget=".\.nuget\nuget.exe"
%nuget% restore .\TEditNoInstaller.sln
%msbuild% .\TEditNoInstaller.sln /property:Configuration=Release;Platform="Any CPU"

REM echo Copying Files...
REM if exist ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" copy ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" .\BIN\

Powershell.exe -executionpolicy bypass -File package.ps1

pause
