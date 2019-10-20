rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >UpgradeLocalNugetPackages [deploymentNumber]
rem deploymentNumber is YYMMDD.build-number, like 190824.5
rem

rem @echo off
rem Setup deployment folder
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentFolderRoot=C:\Deployments\Contensive51\Dev\
set NuGetLocalPackagesPath=C:\NuGetLocalPackages\
set deploymentNumber=%1
set year=%date:~12,4%
set month=%date:~4,2%
set day=%date:~7,2%

rem
rem if deployment number not entered, set it to date.1
rem
IF [%deploymentNumber%] == [] (
	echo No deployment folder provided on the command line, use current date
	set deploymentTimeStamp=%year%%month%%day%
)
rem
rem determine last deployment folder
rem

set suffix=1
set deploymentNumber=%deploymentTimeStamp%.%suffix%
if not exist "%deploymentFolderRoot%%deploymentNumber%" (
   echo No current NuGet Package Folder, put deployment folder name on user, like 191020.2
   pause
   exit /b %errorlevel%
)
:tryagain
set lastValid=%deploymentNumber%
set deploymentNumber=%deploymentTimeStamp%.%suffix%
if not exist "%deploymentFolderRoot%%deploymentNumber%" goto :setfolder
set /a suffix=%suffix%+1
goto tryagain
:setfolder
set deploymentNumber=%lastValid%

rem ==============================================================
rem
rem CPBaseClass Nuget
rem
cd ..\source\cpbase51
xcopy "%deploymentFolderRoot%%deploymentNumber%\Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" "%NuGetLocalPackagesPath%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem ContensiveDbModels Nuget
rem

cd ..\source\Models
xcopy "%deploymentFolderRoot%%deploymentNumber%\Contensive.DbModels.5.1.%deploymentNumber%.nupkg" "%NuGetLocalPackagesPath%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem Processor Nuget 
rem
cd ..\source\processor
xcopy "%deploymentFolderRoot%%deploymentNumber%\Contensive.Processor.5.1.%deploymentNumber%.nupkg" "%NuGetLocalPackagesPath%" /Y
cd ..\..\scripts
pause