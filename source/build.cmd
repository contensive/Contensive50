

rem @echo off
rem Setup deployment folder
set deploymentNumber=%1
set year=%date:~12,4%
set month=%date:~4,2%
set day=%date:~7,2%

IF [%deploymentNumber%] == [] (
echo No deployment folder provided on the command line, use current date
	set deploymentNumber=%year%%month%%day%
)

IF EXIST "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%" (
	del "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%\*.*" /Q
	rd "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%"
)
md "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%"

rem build solution 
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" contensive.sln /p:DeployOnBuild=true /p:PublishProfile=defaultSite
xcopy "..\WebDeploymentPackage\*.zip" "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%"

rem build the CLI Setup installation
"C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" contensive.sln /build Debug /project CliSetup\setup3.vdproj 
xcopy ".\CliSetup\Debug\*.msi" "C:\Users\jay\Desktop\deployments\v51\Install\%deploymentNumber%"

rem build CPBaseClass Nuget
cd cpbase51
IF EXIST "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "Contensive.CPBaseClass.nuspec" -version 5.1.%deploymentNumber%
xcopy "Contensive.CPBaseClass.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
cd ..

rem build CPBaseClass Nuget
cd processor
IF EXIST "Contensive.Processor.5.1.%deploymentNumber%.nupkg" (
	del "Contensive.Processor.5.1.%deploymentNumber%.nupkg" /Q
)
"nuget.exe" pack "processor.csproj" -version 5.1.%deploymentNumber%
xcopy "Contensive.Processor.5.1.%deploymentNumber%.nupkg" "C:\Users\jay\Documents\nugetLocalPackages" /Y
cd ..

rem upgrade nuget for ContensiveMvc

pause


