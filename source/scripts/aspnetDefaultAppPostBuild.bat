rem 
rem aspnetDefaultApp project - onSuccess post build
rem starts in scripts folder
rem 
cd "C:\Users\Administrator\Documents\GitHub\Contensive50"
xcopy /R "source\aspnetDefaultApp\bin\*.dll"  "installSource\clibResources\appPatterns\aspnet\appRoot\bin\" /Y

cd "installSource"
del clibresources.zip
"c:\Program Files\7-zip\7z.exe" a -r clibresources.zip  clibresources
