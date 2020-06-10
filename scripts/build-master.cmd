rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [versionNumber]
rem versionNumber is 5.YYMM.DD.build-number, like 5.1908.24.5
rem

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

del /s /q  "..\source\cpbase51\bin"
rd /s /q  "..\source\cpbase51\bin"

del /s /q  "..\source\cpbase51\obj"
rd /s /q  "..\source\cpbase51\obj"

del /s /q  "..\source\iisDefaultSite\bin"
rd /s /q  "..\source\iisDefaultSite\bin"

del /s /q  "..\source\iisDefaultSite\obj"
rd /s /q  "..\source\iisDefaultSite\obj"

del /s /q  "..\source\models\bin"
rd /s /q  "..\source\models\bin"

del /s /q  "..\source\models\obj"
rd /s /q  "..\source\models\obj"

del /s /q  "..\source\processor\bin"
rd /s /q  "..\source\processor\bin"

del /s /q  "..\source\processor\obj"
rd /s /q  "..\source\processor\obj"

del /s /q  "..\source\processortests\bin"
rd /s /q  "..\source\processortests\bin"

del /s /q  "..\source\processortests\obj"
rd /s /q  "..\source\processortests\obj"

del /s /q  "..\source\taskservice\bin"
rd /s /q  "..\source\taskservice\bin"

del /s /q  "..\source\taskservice\obj"
rd /s /q  "..\source\taskservice\obj"

del /q "..\WebDeploymentPackage\*.*"

rem ==============================================================
rem
rem build and pack Contensive common solution (CPBase +Models + Processor)
rem

cd ..\source

rem clean, build, pack contensivecommon.sln /property:Version=%versionNumber% --no-incremental
dotnet clean contensivecommon.sln
rem dotnet pack contensivecommon.sln /property:PackageVersion=%versionNumber% /property:Version=%versionNumber%
dotnet build CPBase51/CPBase51.csproj --no-dependencies /property:Version=4.1.2.0 /property:AssemblyVersion=4.1.2.0 /property:FileVersion=4.1.2.0

pause

dotnet build Models/Models.csproj --no-dependencies /property:Version=%versionNumber%

pause

dotnet build Processor/Processor.csproj --no-dependencies /property:Version=%versionNumber%

pause

dotnet pack contensivecommon.sln --no-build --no-restore /property:PackageVersion=%versionNumber%

pause

if errorlevel 1 (
   echo failure building common solution
   pause
   exit /b %errorlevel%
)

cd ..\scripts

rem ==============================================================
rem
rem move packages to deplyment, and to local package folder

cd ..\source

move /y "CPBase51\bin\debug\Contensive.CPBaseClass.%versionNumber%.nupkg" "%deploymentFolderRoot%%versionNumber%\"
rem copy this package to the local package source so the next project builds all upgrade the assembly
xcopy "%deploymentFolderRoot%%versionNumber%\Contensive.CPBaseClass.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y

move /y "Models\Bin\Debug\Contensive.DBModels.%versionNumber%.nupkg" "%deploymentFolderRoot%%versionNumber%\"
rem copy this package to the local package source so the next project builds all upgrade the assembly
xcopy "%deploymentFolderRoot%%versionNumber%\Contensive.DBModels.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y

move /y "Processor\bin\debug\Contensive.Processor.%versionNumber%.nupkg" "%deploymentFolderRoot%%versionNumber%\"
rem copy this package to the local package source so the next project builds all upgrade the assembly
xcopy "%deploymentFolderRoot%%versionNumber%\Contensive.Processor.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y

cd ..\scripts

rem ==============================================================
rem
rem update cli, taskservice nuget packages 
rem

cd ..\source\cli
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\taskservice
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\..\scripts

rem ==============================================================
rem
rem build cli and task server 
rem
cd ..\source

dotnet build contensivecli.sln --no-incremental  /property:Version=%versionNumber%
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
rem hack - copy aoBase51.xml file from processor project to cli setup
rem because I wasted 4 hrs trying to get nuget to install it as a content file
rem

xcopy ..\source\Processor\aoBase51.xml ..\Cli\*.* /Y

rem ==============================================================
rem
rem build cli setup
rem
cd ..\source
del CliSetup.out.txt /Q
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\devenv.exe" /Rebuild Debug contensiveCli.sln /project "CliSetup\setup3.vdproj" /projectconfig Debug
if errorlevel 1 (
   echo failure building CLIsetup
   pause
   exit /b %errorlevel%
)
xcopy ".\CliSetup\Debug\*.msi" "%deploymentFolderRoot%%versionNumber%" /Y
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

