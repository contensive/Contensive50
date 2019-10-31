rem 
rem Must be run from the projects git\project\scripts folder - everything is relative
rem run >build [versionNumber]
rem versionNumber is 5.YYMM.DD.build-number, like 5.1908.24.5
rem

rem @echo off
rem Setup deployment folder
set versionMajor=5
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentFolderRoot=C:\Deployments\Contensive5\Dev\
set NuGetLocalPackagesFolder=C:\NuGetLocalPackages\
set year=%date:~12,4%
set month=%date:~4,2%
set day=%date:~7,2%
set versionMinor=%year%
set versionBuild=%month%%day%
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
rem build cpbaseclass 
rem
cd ..\source
rem set assembly version
copy "cpbase51\properties\assemblyinfo-src.cs" "cpbase51\properties\assemblyinfo.cs"
cscript ..\scripts\replace.vbs "cpbase51\properties\assemblyinfo.cs" "0.0.0.0" "%versionNumber%"

rem build
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
IF EXIST "Contensive.CPBaseClass.%versionNumber%.nupkg" (
	del "Contensive.CPBaseClass.%versionNumber%.nupkg" /Q
)
"nuget.exe" pack "Contensive.CPBaseClass.nuspec" -version %versionNumber%
if errorlevel 1 (
   echo failure in nuget CPBase
   pause
   exit /b %errorlevel%
)
rem no local nuget package folder - xcopy "Contensive.CPBaseClass.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y
move /y "Contensive.CPBaseClass.%versionNumber%.nupkg" "%deploymentFolderRoot%%versionNumber%\"
rem copy this package to the local package source so the next project builds all upgrade the assembly
xcopy "%deploymentFolderRoot%%versionNumber%\Contensive.CPBaseClass.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem update models nuget packages  
rem

cd ..\source\Models
nuget update Models.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\ModelTests
nuget update ModelTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\..\scripts

rem ==============================================================
rem
rem build Models
rem

cd ..\source
rem set assembly version
copy "models\properties\assemblyinfo-src.cs" "models\properties\assemblyinfo.cs"
cscript ..\scripts\replace.vbs "models\properties\assemblyinfo.cs" "0.0.0.0" "%versionNumber%"

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
IF EXIST "Contensive.Models.%versionNumber%.nupkg" (
	del "Contensive.Models.%versionNumber%.nupkg" /Q
)
"nuget.exe" pack "Contensive.DbModels.nuspec" -version %versionNumber%
if errorlevel 1 (
   echo failure in nuget Contensive.DbModels
   pause
   exit /b %errorlevel%
)
rem xcopy no local nuget package folder - "Contensive.DbModels.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y
move /y "Contensive.DbModels.%versionNumber%.nupkg" "%deploymentFolderRoot%%versionNumber%\"
rem copy this package to the local package source so the next project builds all upgrade the assembly
xcopy "%deploymentFolderRoot%%versionNumber%\Contensive.DbModels.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem update processor nuget packages  
rem

cd ..\source\Processor
nuget update Processor.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\ProcessorTests
nuget update ProcessorTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\..\scripts

rem ==============================================================
rem
rem build Processor 
rem
cd ..\source
copy "processor\properties\assemblyinfo-src.cs" "processor\properties\assemblyinfo.cs"
cscript ..\scripts\replace.vbs "processor\properties\assemblyinfo.cs" "0.0.0.0" "%versionNumber%"

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
IF EXIST "Contensive.Processor.%versionNumber%.nupkg" (
	del "Contensive.Processor.%versionNumber%.nupkg" /Q
)
"nuget.exe" pack "processor.nuspec" -version %versionNumber%
if errorlevel 1 (
   echo failure building processor.dll
   pause
   exit /b %errorlevel%
)
rem xcopy no local nuget package folder - "Contensive.Processor.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y
move /y "Contensive.Processor.%versionNumber%.nupkg" "%deploymentFolderRoot%%versionNumber%\"
rem copy this package to the local package source so the next project builds all upgrade the assembly
xcopy "%deploymentFolderRoot%%versionNumber%\Contensive.Processor.%versionNumber%.nupkg" "%NuGetLocalPackagesFolder%" /Y
cd ..\..\scripts

rem ==============================================================
rem
rem update cli, taskservice nuget packages 
rem

cd ..\source\cli
nuget update cli.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\taskservice
nuget update taskservice.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\..\scripts

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

rem ==============================================================
rem
rem update aspx site nuget packages 
rem
cd ..\source\iisdefaultsite
nuget update iisdefaultsite.vbproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
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
xcopy "..\WebDeploymentPackage\*.zip" "%deploymentFolderRoot%%versionNumber%" /Y
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
xcopy ".\CliSetup\Debug\*.msi" "%deploymentFolderRoot%%versionNumber%" /Y
cd ..\scripts
rem ==============================================================
rem
rem update nuget for all test projects
rem

cd ..\source\Models
cd ..\ModelTests
nuget update ModelTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\..\scripts

cd ..\source\Processor
cd ..\ProcessorTests
nuget update ProcessorTests.csproj -noninteractive -source nuget.org -source %NuGetLocalPackagesFolder%
cd ..\..\scripts

rem ==============================================================
rem
rem future - ContensiveMvc
rem

rem ==============================================================
rem
rem done
rem

pause