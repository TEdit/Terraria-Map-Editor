rmdir /S /Q .\BIN
echo Copying Files...
mkdir .\BIN
if exist ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" copy ".\TEdit3Installer\bin\Release\TEdit3Installer.msi" .\BIN\