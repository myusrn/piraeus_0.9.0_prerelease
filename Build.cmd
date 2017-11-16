echo on
set @version=%1

@REM NOTE: This script must be run from a Visual Studio command prompt window

@setlocal
@ECHO off

SET CMDHOME=%~dp0.
if "%FrameworkDir%" == "" set FrameworkDir=%WINDIR%\Microsoft.NET\Framework
if "%FrameworkVersion%" == "" set FrameworkVersion=v4.0.30319

SET MSBUILDEXEDIR=%FrameworkDir%\%FrameworkVersion%
SET MSBUILDEXE=%MSBUILDEXEDIR%\MSBuild.exe

cd %CMDHOME%

cd src

cd Piraeus\Piraeus.Core
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Core.nuspec" %@version%
msbuild Piraeus.Core.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\Piraeus.Core.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Piraeus.GrainInterfaces
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.GrainInterfaces.nuspec" %@version%
msbuild Piraeus.GrainInterfaces.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack Piraeus.GrainInterfaces.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Piraeus.Grains
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Grains.nuspec" %@version%
msbuild Piraeus.Grains.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack Piraeus.Grains.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\SkunkLab\SkunkLab.Storage
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Storage\SkunkLab.Storage.nuspec" %@version%
msbuild SkunkLab.Storage.sln /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\SkunkLab.Storage\SkunkLab.Storage.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\SkunkLab\SkunkLab.Diagnostics
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Diagnostics\SkunkLab.Diagnostics.nuspec" %@version%
msbuild SkunkLab.Diagnostics.sln /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\SkunkLab.Diagnostics\SkunkLab.Diagnostics.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\SkunkLab\SkunkLab.Security
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Security\SkunkLab.Security.nuspec" %@version%
msbuild .\SkunkLab.Security\SkunkLab.Security.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\SkunkLab.Security\SkunkLab.Security.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\SkunkLab\SkunkLab.Channels
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Channels\SkunkLab.Channels.nuspec" %@version%
msbuild .\SkunkLab.Channels\SkunkLab.Channels.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\SkunkLab.Channels\SkunkLab.Channels.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\SkunkLab\SkunkLab.Protocols
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Protocols\SkunkLab.Protocols.nuspec" %@version%
msbuild .\SkunkLab.Protocols\SkunkLab.Protocols.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\SkunkLab.Protocols\SkunkLab.Protocols.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Piraeus.Configuration
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Configuration.nuspec" %@version%
msbuild Piraeus.Configuration.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\Piraeus.Configuration.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Piraeus.Adapters
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Adapters.nuspec" %@version%
msbuild Piraeus.Adapters.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\Piraeus.Adapters.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Piraeus.SiloHost
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.SiloHost.nuspec" %@version%
msbuild Piraeus.SiloHost.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\Piraeus.SiloHost.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Powershell\Piraeus.Module\Piraeus.Module
powershell.exe ..\..\..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Module.nuspec" %@version%
msbuild Piraeus.Module.csproj /t:Clean,Rebuild /p:Configuration=Release
nuget pack .\Piraeus.Module.nuspec -OutputDirectory "..\..\..\..\Nuget"


goto ENDLINE




cd ..\..\Piraeus\Piraeus.Adapters
msbuild Piraeus.Adapters.csproj /t:Rebuild

:ENDLINE