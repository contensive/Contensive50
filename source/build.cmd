
rem build solution 
rem "C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" contensive.sln 

rem build the CLI Setup installation
rem "C:\Program Files (x86)\Microsoft Visual Studio\2017\Community\Common7\IDE\devenv.exe" contensive.sln /build Debug /project CliSetup\setup3.vdproj 

rem build the aspxDefaultSite
"C:\Program Files (x86)\MSBuild\14.0\Bin\msbuild.exe" iisDefaultSite\iisDefaultSite.vbproj /p:DeployOnBuild=true /p:PublishProfile=defaultSite


pause