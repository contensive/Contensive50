rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [deploymentNumber]
rem deploymentNumber is YYMMDD.build-number, like 190824.5
rem

rem @echo off
rem Setup deployment folder
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentFolderRoot=C:\Users\jay\Desktop\deployments\v51\Dev\
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
rem if deployment folder exists, delete it and make directory
rem

set suffix=1
:tryagain
set deploymentNumber=%deploymentTimeStamp%.%suffix%
if not exist "%deploymentFolderRoot%%deploymentNumber%" goto :makefolder
set /a suffix=%suffix%+1
goto tryagain
:makefolder
md "%deploymentFolderRoot%%deploymentNumber%"

rem ==============================================================
rem
rem clean build folders
rem

rd /S /Q "..\source\cpbase51\bin"
rd /S /Q "..\source\cpbase51\obj"

rd /S /Q "..\source\models\bin"
rd /S /Q "..\source\models\obj"

rd /S /Q "..\source\processor\bin"
rd /S /Q "..\source\processor\obj"

rd /S /Q "..\source\processortests\bin"
rd /S /Q "..\source\processortests\obj"

rd /S /Q "..\source\cli\bin"
rd /S /Q "..\source\cli\obj"

rd /S /Q "..\source\clisetup\debug"
rd /S /Q "..\source\clisetup\Debug-DevApp"
rd /S /Q "..\source\clisetup\Debug-StagingDefaultApp"
rd /S /Q "..\source\clisetup\DevDefaultApp"
rd /S /Q "..\source\clisetup\Release"
rd /S /Q "..\source\clisetup\StagingDefaultApp"

rd /S /Q "..\source\iisDefault\bin"
rd /S /Q "..\source\iisDefault\obj"

rd /S /Q "..\source\taskservice\bin"
rd /S /Q "..\source\taskservice\obj"

del /Q "..\WebDeploymentPackage\*.*"

rem ==============================================================
rem
rem build cpbaseclass 
rem
cd ..\source
"%msbuildLocation%msbuild.exe" contensiveCPBase.sln
if errorlevel 1 (
   echo failure building CPBase.dll
   pause
   exit /b %errorlevel%
)
cd ..\scripts

rem ==============================================================
rem
rem build CPBaseClass Nuget
rem
cd ..\source\cpbase51
IF EXIST "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "Contensive.CPBaseClass.nuspec" -version 5.1.%deploymentNumber%
if errorlevel 1 (
   echo failure in nuget CPBase
   pause
   exit /b %errorlevel%
)
xcopy "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
xcopy "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" "%deploymentFolderRoot%%deploymentNumber%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem update models nuget packages  
rem

cd ..\source\Models
nuget update Models.csproj -noninteractive
cd ..\ModelTests
nuget update ModelTests.csproj -noninteractive
cd ..\..\scripts

rem ==============================================================
rem
rem build Models
rem

cd ..\source
"%msbuildLocation%msbuild.exe" contensiveDbModels.sln
 
if errorlevel 1 (
   echo failure building contensiveDbModels.dll
   pause
   exit /b %errorlevel%
)
cd ..\scripts

rem ==============================================================
rem
rem build ContensiveDbModels Nuget
rem

cd ..\source\Models
IF EXIST "Contensive.Models.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.Models.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "Contensive.DbModels.nuspec" -version 5.1.%deploymentNumber%
if errorlevel 1 (
   echo failure in nuget Contensive.DbModels
   pause
   exit /b %errorlevel%
)
xcopy "Contensive.DbModels.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
xcopy "Contensive.DbModels.5.1.%deploymentNumber%.nupkg" "%deploymentFolderRoot%%deploymentNumber%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem update processor nuget packages  
rem
cd ..\source\Processor
nuget update Processor.csproj -noninteractive
cd ..\ProcessorTests
nuget update ProcessorTests.csproj -noninteractive
cd ..\..\scripts

rem ==============================================================
rem
rem build Processor 
rem
cd ..\source
"%msbuildLocation%msbuild.exe" contensive.sln
if errorlevel 1 (
   echo failure building processor.dll
   pause
   exit /b %errorlevel%
)
cd ..\scripts

rem ==============================================================
rem
rem build Processor Nuget 
rem
cd ..\source\processor
IF EXIST "Contensive.Processor.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.Processor.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "processor.nuspec" -version 5.1.%deploymentNumber%
if errorlevel 1 (
   echo failure building processor.dll
   pause
   exit /b %errorlevel%
)
xcopy "Contensive.Processor.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
xcopy "Contensive.Processor.5.1.%deploymentNumber%.nupkg" "%deploymentFolderRoot%%deploymentNumber%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem update cli, taskservice nuget packages 
rem
cd ..\source\cli
nuget update cli.csproj -noninteractive
cd ..\taskservice
nuget update taskservice.csproj -noninteractive
cd ..\..\scripts

rem ==============================================================
rem
rem build cli and task server 
rem
cd ..\source
"%msbuildLocation%msbuild.exe" contensiveCli.sln
if errorlevel 1 (
   echo failure building processor.dll
   pause
   exit /b %errorlevel%
)
cd ..\scripts

rem ==============================================================
rem
rem update aspx site nuget packages 
rem
cd ..\source\iisdefaultsite
nuget update iisdefaultsite.vbproj -noninteractive
cd ..\..\scripts

rem ==============================================================
rem
rem build aspx and publish 
rem
cd ..\source
"%msbuildLocation%msbuild.exe" contensiveAspx.sln /p:DeployOnBuild=true /p:PublishProfile=defaultSite
if errorlevel 1 (
   echo failure building contensiveAspx
   pause
   exit /b %errorlevel%
)
xcopy "..\WebDeploymentPackage\*.zip" "%deploymentFolderRoot%%deploymentNumber%" /Y
cd ..\scripts

rem ==============================================================
rem
rem build cli setup
rem
cd ..\source
del CliSetup.out.txt /Q
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" /Rebuild Debug contensiveCli.sln /project "CliSetup\setup3.vdproj" /projectconfig Debug /out CliSetup.out.txt
if errorlevel 1 (
   echo failure building CLIsetup
   pause
   exit /b %errorlevel%
)
xcopy ".\CliSetup\Debug\*.msi" "%deploymentFolderRoot%%deploymentNumber%" /Y
cd ..\scripts

rem ==============================================================
rem
rem upgrade nuget for ContensiveMvc
rem


pause