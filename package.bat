set /p vers=<teditversion.txt

.\build\7zip\7za a -tzip ".\build\TEdit_BETAS_%vers%" ".\TEditXna\bin\Release\*.*" -mx9
.\build\7zip\7za a -tzip ".\build\TEdit_BETAS_%vers%" "teditversion.txt" -mx9