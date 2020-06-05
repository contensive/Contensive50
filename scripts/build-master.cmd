rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [versionNumber]
rem versionNumber is 5.YYMM.DD.build-number, like 5.1908.24.5
rem

rem @echo off
rem Setup deployment folder
set versionMajor=5
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\MSBuild\Current\Bin\
rem set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentFolderRoot=C:\Deployments\Contensive5\Dev\
set NuGetLocalPackagesFolder=C:\NuGetLocalPackages\
set year=%date:~12,4%
set month=%date:~4,2%
set day=%date:~7,2%
if %day% GEQ 10 goto dateOk
set day=%date:~8,1%
:dateOk
set versionMinor=%year%%month%
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
rem build Contensive common solution (CPBase +Models + Processor)
rem
cd ..\source

dotnet build -p:Version=%versionNumber% contensivecommon.sln

if errorlevel 1 (
   echo failure building common solution
   pause
   exit /b %errorlevel%
)
cd ..\scripts


rem ==============================================================
rem
rem pack Contensive common solution (CPBase +Models + Processor)
rem
cd ..\source

dotnet pack -p:PackageVersion=%versionNumber% contensivecommon.sln

if errorlevel 1 (
   echo failure packing common solution
   pause
   exit /b %errorlevel%
)

rem move packages to deplyment, and to local package folder

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

pause

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

pause


rem ==============================================================
rem
rem build cli and task server 
rem
cd ..\source
copy "cli\properties\assemblyinfo-src.cs" "cli\properties\assemblyinfo.cs"
cscript ..\scripts\replace.vbs "cli\properties\assemblyinfo.cs" "0.0.0.0" "%versionNumber%"

"%msbuildLocation%msbuild.exe" contensiveCli.sln
if errorlevel 1 (
   echo failure building processor.dll
   pause
   exit /b %errorlevel%
)
cd ..\scripts

pause


rem ==============================================================
rem
rem update aspx site nuget packages 
rem
cd ..\source\iisdefaultsite
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.CPBaseClass
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.DbModels
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder% -Id Contensive.Processor
cd ..\..\scripts

pause


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
xcopy "..\WebDeploymentPackage\*.zip" "%deploymentFolderRoot%%versionNumber%" /Y
cd ..\scripts

pause


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

pause

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

pause