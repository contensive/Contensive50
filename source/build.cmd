rem 
rem run >build [deploymentNumber like 190824.5]
rem

rem @echo off
rem Setup deployment folder
set msbuildLocation=C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\MSBuild\15.0\Bin\
set deploymentNumber=%1
set year=%date:~12,4%
set month=%date:~4,2%
set day=%date:~7,2%

rem
rem if deployment number not entered, set it to date.1
rem
IF [%deploymentNumber%] == [] (
	echo No deployment folder provided on the command line, use current date
	set deploymentNumber=%year%%month%%day%.1
)
rem
rem if deployment folder exists, delete it and make directory
rem
IF EXIST "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%" (
	del "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%\*.*" /Q
	rd "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%"
)
md "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%"

rem ==============================================================
rem
rem build cpbaseclass 
rem
"%msbuildLocation%msbuild.exe" contensiveCPBase.sln
if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   exit /b %errorlevel%
)

rem ==============================================================
rem
rem build CPBaseClass Nuget
rem
cd cpbase51
IF EXIST "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "Contensive.CPBaseClass.nuspec" -version 5.1.%deploymentNumber%
if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   exit /b %errorlevel%
)
xcopy "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
xcopy "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%" /Y
cd ..

rem ==============================================================
rem
rem update processor nuget packages  
rem
cd Processor
nuget update Processor.csproj -noninteractive
cd ..\ProcessorTests
nuget update ProcessorTests.csproj -noninteractive
cd ..

rem ==============================================================
rem
rem build the Processor 
rem
"%msbuildLocation%msbuild.exe" contensive.sln

rem ==============================================================
rem
rem build Processor Nuget 
rem
cd processor
IF EXIST "Contensive.Processor.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.Processor.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "processor.nuspec" -version 5.1.%deploymentNumber%
if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   exit /b %errorlevel%
)
xcopy "Contensive.Processor.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
xcopy "Contensive.Processor.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%" /Y
cd ..

rem ==============================================================
rem
rem update cli, taskservice nuget packages 
rem
cd cli
nuget update cli.csproj -noninteractive
cd ..\taskservice
nuget update taskservice.csproj -noninteractive
cd ..

rem ==============================================================
rem
rem build cli and task server 
rem
"%msbuildLocation%msbuild.exe" contensiveCli.sln
if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   exit /b %errorlevel%
)

rem ==============================================================
rem
rem build cli setup
rem
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" contensiveCli.sln /build Debug /project CliSetup\setup3.vdproj 
rem if errorlevel 1 (
rem    echo Failure Reason Given is %errorlevel%
rem    exit /b %errorlevel%
rem )
xcopy ".\CliSetup\Debug\*.msi" "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%" /Y
rem if errorlevel 1 (
rem    echo Failure Reason Given is %errorlevel%
rem    exit /b %errorlevel%
rem )

rem ==============================================================
rem
rem update aspx site nuget packages 
rem
cd iisdefaultsite
nuget update iisdefaultsite.vbproj -noninteractive
cd ..

rem ==============================================================
rem
rem build aspx and publish 
rem
"%msbuildLocation%msbuild.exe" contensiveAspx.sln /p:DeployOnBuild=true /p:PublishProfile=defaultSite
if errorlevel 1 (
   echo Failure Reason Given is %errorlevel%
   exit /b %errorlevel%
)
xcopy "..\WebDeploymentPackage\*.zip" "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%" /Y

rem ==============================================================
rem
rem upgrade nuget for ContensiveMvc
rem


