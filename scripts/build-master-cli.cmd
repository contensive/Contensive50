rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [versionNumber]
rem versionNumber is 5.YYMM.DD.build-number, like 5.1908.24.5
rem

c:
cd \Git\Contensive5\scripts

rem @echo off
rem Setup deployment folder
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\
rem set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentFolderRoot=C:\Deployments\Contensive5\Dev\
set NuGetLocalPackagesFolder=C:\NuGetLocalPackages\
set year=%date:~12,4%
set month=%date:~4,2%
if %month% GEQ 10 goto monthOk
set month=%date:~5,1%
:monthOk
set day=%date:~7,2%
if %day% GEQ 10 goto dayOk
set day=%date:~8,1%
:dayOk
set versionMajor=%year%
set versionMinor=%month%
set versionBuild=%day%
set versionRevision=1
rem
rem if deployment folder exists, delete it and make directory
rem
:tryagain
set versionNumber=%versionMajor%.%versionMinor%.%versionBuild%.%versionRevision%
if not exist "%deploymentFolderRoot%%versionNumber%" goto :makefolder
set /a versionRevision=%versionRevision%+1
goto tryagain
:makefolder
md "%deploymentFolderRoot%%versionNumber%"
rem ==============================================================
rem
rem clean build folders
rem

del /s /q  "..\source\cli\bin"
rd /s /q  "..\source\cli\bin"

del /s /q  "..\source\cli\obj"
rd /s /q  "..\source\cli\obj"

del /s /q  "..\source\clisetup\debug"
rd /s /q  "..\source\clisetup\debug"

del /s /q  "..\source\clisetup\Debug-DevApp"
rd /s /q  "..\source\clisetup\Debug-DevApp"

del /s /q  "..\source\clisetup\Debug-StagingDefaultApp"
rd /s /q  "..\source\clisetup\Debug-StagingDefaultApp"

del /s /q  "..\source\clisetup\DevDefaultApp"
rd /s /q  "..\source\clisetup\DevDefaultApp"

del /s /q  "..\source\clisetup\Release"
rd /s /q  "..\source\clisetup\Release"

del /s /q  "..\source\clisetup\StagingDefaultApp"
rd /s /q  "..\source\clisetup\StagingDefaultApp"


del /s /q  "..\source\taskservice\bin"
rd /s /q  "..\source\taskservice\bin"

del /s /q  "..\source\taskservice\obj"
rd /s /q  "..\source\taskservice\obj"

del /q "..\WebDeploymentPackage\*.*"


cd ..\source\cli
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\taskservice
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\..\scripts

pause

rem ==============================================================
rem
rem build cli and task server
rem
cd ..\source

dotnet clean ContensiveCli.sln

dotnet build Cli/Cli.csproj --no-dependencies /property:Version=%versionNumber%
if errorlevel 1 (
   echo failure building cli
   pause
   exit /b %errorlevel%
)

dotnet build TaskService/TaskService.csproj --no-dependencies /property:Version=%versionNumber%
if errorlevel 1 (
   echo failure building taskservice
   pause
   exit /b %errorlevel%
)

cd ..\scripts

pause

rem ==============================================================
rem
rem build cli installer
rem
cd ..\source
rem "C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\msbuild.exe" ContensiveCLIInstaller\ContensiveCLIInstaller.wixproj
"%msbuildLocation%msbuild.exe" ContensiveCLIInstaller\ContensiveCLIInstaller.wixproj
if errorlevel 1 (
echo failure building cli installer
   pause
   exit /b %errorlevel%
)

cd ..\scripts

rem ==============================================================
rem
rem update aspx site nuget packages 
rem
cd ..\source\iisdefaultsite
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\..\scripts

rem ==============================================================
rem
rem build aspx and publish 
rem
cd ..\source
dotnet build contensiveAspx.sln /property:DeployOnBuild=true /property:PublishProfile=defaultSite /property:Version=%versionNumber% --no-incremental
rem "%msbuildLocation%msbuild.exe" contensiveAspx.sln /p:DeployOnBuild=true /p:PublishProfile=defaultSite
if errorlevel 1 (
   echo failure building contensiveAspx
   pause
   exit /b %errorlevel%
)
xcopy "..\WebDeploymentPackage\*.zip" "%deploymentFolderRoot%%versionNumber%" /Y
cd ..\scripts

rem ==============================================================
rem
rem update nuget for all test projects
rem

cd ..\source\Models
cd ..\ModelTests
nuget update ModelTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update ModelTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update ModelTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\..\scripts

cd ..\source\Processor
cd ..\ProcessorTests
nuget update ProcessorTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update ProcessorTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update ProcessorTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\..\scripts

rem ==============================================================
rem
rem done
rem

