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

GOTO SKIP
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Core.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
msbuild Piraeus.Core.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\Piraeus.Core.log /flp2:logfile=..\..\..\BuildOutput\Piraeus.Core_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\Piraeus.Core_warnings.log;warningsonly  
nuget pack .\Piraeus.Core.nuspec -OutputDirectory "..\..\Nuget"

:SKIP

cd ..\..\Piraeus\Piraeus.GrainInterfaces
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.GrainInterfaces.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.GrainInterfaces.nuspec" %@version% "Piraeus.Core"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version% 
nuget restore -PackagesDirectory "..\..\Orleans\packages"
nuget install Piraeus.Core -Source "c:\_git\core\src\nuget" -OutputDirectory "..\..\Orleans\packages"

msbuild Piraeus.GrainInterfaces.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\Piraeus.GrainInterfaces.log /flp2:logfile=..\..\..\BuildOutput\Piraeus.GrainInterfaces_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\Piraeus.GrainInterfaces_warnings.log;warningsonly  
nuget pack Piraeus.GrainInterfaces.nuspec -OutputDirectory "..\..\Nuget"

GOTO ENDLINE

cd ..\..\Piraeus\Piraeus.Grains
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Grains.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version% 
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.Grains.nuspec" %@version% "Piraeus.GrainInterfaces"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.GrainInterfaces" %@version%
msbuild Piraeus.Grains.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\Piraeus.Grains.log /flp2:logfile=..\..\..\BuildOutput\Piraeus.Grains_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\Piraeus.Grains_warnings.log;warningsonly  
nuget pack Piraeus.Grains.nuspec -OutputDirectory "..\..\Nuget"



cd ..\..\SkunkLab\SkunkLab.Storage
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Storage\SkunkLab.Storage.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\SkunkLab.Storage\Properties\AssemblyInfo.cs" "%@version%"
nuget restore
msbuild SkunkLab.Storage.sln /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\SkunkLab.Storage.log /flp2:logfile=..\..\..\BuildOutput\SkunkLab.Storage_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\SkunkLab.Storage_warnings.log;warningsonly
nuget pack .\SkunkLab.Storage\SkunkLab.Storage.nuspec -OutputDirectory "..\..\Nuget"


cd ..\..\SkunkLab\SkunkLab.Diagnostics\SkunkLab.Diagnostics
powershell.exe ..\..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Diagnostics.nuspec" %@version%
powershell.exe ..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\UpdateDependency.ps1 ".\SkunkLab.Diagnostics.nuspec" %@version% "SkunkLab.Storage"
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Storage" %@version%
nuget restore
msbuild SkunkLab.Diagnostics.csproj /t:clean /p:configuration=Debug
msbuild SkunkLab.Diagnostics.csproj /t:clean /p:configuration=Release
msbuild SkunkLab.Diagnostics.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\SkunkLab.Diagnostics.log /flp2:logfile=..\..\..\..\BuildOutput\SkunkLab.Diagnostics_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\SkunkLab.Diagnostics_warnings.log;warningsonly
nuget pack .\SkunkLab.Diagnostics.nuspec -OutputDirectory "..\..\..\Nuget"

cd ..\..\..\SkunkLab\SkunkLab.Security
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Security\SkunkLab.Security.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\SkunkLab.Security\Properties\AssemblyInfo.cs" "%@version%"
nuget restore
msbuild .\SkunkLab.Security.csproj /t:clean /p:configuration=Debug
msbuild .\SkunkLab.Security.csproj /t:clean /p:configuration=Release
msbuild .\SkunkLab.Security\SkunkLab.Security.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\SkunkLab.Security.log /flp2:logfile=..\..\..\BuildOutput\SkunkLab.Security_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\SkunkLab.Security_warnings.log;warningsonly
nuget pack .\SkunkLab.Security\SkunkLab.Security.nuspec -OutputDirectory "..\..\Nuget"




cd ..\..\SkunkLab\SkunkLab.Channels\SkunkLab.Channels
powershell.exe ..\..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Channels.nuspec" %@version%
powershell.exe ..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\UpdateDependency.ps1 ".\SkunkLab.Channels.nuspec" %@version% "SkunkLab.Diagnostics"
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Diagnostics" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Storage" %@version%
nuget restore
msbuild .\SkunkLab.Channels.csproj /t:clean /p:configuration=Debug
msbuild .\SkunkLab.Channels.csproj /t:clean /p:configuration=Release
msbuild .\SkunkLab.Channels.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\SkunkLab.Channels.log /flp2:logfile=..\..\..\..\BuildOutput\SkunkLab.Channels_errors.log;errorsonly /flp3:logfile=..\..\..\..\BuildOutput\SkunkLab.Channels_warnings.log;warningsonly
nuget pack .\SkunkLab.Channels.nuspec -OutputDirectory "..\..\..\Nuget"



cd ..\..\..\SkunkLab\SkunkLab.Protocols\SkunkLab.Protocols
powershell.exe ..\..\..\..\UpdateNuspecVersion.ps1 ".\SkunkLab.Protocols.nuspec" %@version%
powershell.exe ..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\UpdateDependency.ps1 ".\SkunkLab.Protocols.nuspec" %@version% "SkunkLab.Security"
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Security" %@version%
nuget restore
msbuild .\SkunkLab.Protocols.csproj /t:clean /p:configuration=Debug
msbuild .\SkunkLab.Protocols.csproj /t:clean /p:configuration=Release
msbuild .\SkunkLab.Protocols.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\SkunkLab.Protocols.log /flp2:logfile=..\..\..\..\BuildOutput\SkunkLab.Protocols_errors.log;errorsonly /flp3:logfile=..\..\..\..\BuildOutput\SkunkLab.Protocols_warnings.log;warningsonly
nuget pack .\SkunkLab.Protocols.nuspec -OutputDirectory "..\..\..\Nuget"



cd ..\..\..\Piraeus\Piraeus.Configuration
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Configuration.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
nuget restore
msbuild Piraeus.Configuration.csproj /t:clean /p:configuration=Debug
msbuild Piraeus.Configuration.csproj /t:clean /p:configuration=Release
msbuild Piraeus.Configuration.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\Piraeus.Configuration.log /flp2:logfile=..\..\..\BuildOutput\Piraeus.Configuration_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\Piraeus.Configuration_warnings.log;warningsonly
nuget pack .\Piraeus.Configuration.nuspec -OutputDirectory "..\..\Nuget"




cd ..\..\Piraeus\Piraeus.Adapters
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Adapters.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version%
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.GrainInterfaces" %@version%
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.Adapters.nuspec" %@version% "Piraeus.Grains"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Grains" %@version%
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.Adapters.nuspec" %@version% "Piraeus.Configuration"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Configuration" %@version%
powershell.exe ..\..\..\UpdatePackage.ps1 "SkunkLab.Diagnostics" %@version%
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.Adapters.nuspec" %@version% "SkunkLab.Channels"
powershell.exe ..\..\..\UpdatePackage.ps1 "SkunkLab.Channels" %@version%
powershell.exe ..\..\..\UpdatePackage.ps1 "SkunkLab.Security" %@version%
powershell.exe ..\..\..\UpdatePackage.ps1 "SkunkLab.Storage" %@version%
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.Adapters.nuspec" %@version% "SkunkLab.Protocols"
powershell.exe ..\..\..\UpdatePackage.ps1 "SkunkLab.Protocols" %@version%
nuget restore
msbuild Piraeus.Adapters.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\Piraeus.Adapters.log /flp2:logfile=..\..\..\BuildOutput\Piraeus.Adapters_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\Piraeus.Adapters_warnings.log;warningsonly
nuget pack .\Piraeus.Adapters.nuspec -OutputDirectory "..\..\Nuget"


cd ..\..\Piraeus\Piraeus.SiloHost
powershell.exe ..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.SiloHost.nuspec" %@version%
powershell.exe ..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version%
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.GrainInterfaces" %@version%
powershell.exe ..\..\..\UpdateDependency.ps1 ".\Piraeus.SiloHost.nuspec" %@version% "Piraeus.Grains"
powershell.exe ..\..\..\UpdatePackage.ps1 "Piraeus.Grains" %@version%
nuget restore
msbuild Piraeus.SiloHost.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\BuildOutput\Piraeus.SiloHost.log /flp2:logfile=..\..\..\BuildOutput\Piraeus.SiloHost_errors.log;errorsonly /flp3:logfile=..\..\..\BuildOutput\Piraeus.SiloHost_warnings.log;warningsonly
nuget pack .\Piraeus.SiloHost.nuspec -OutputDirectory "..\..\Nuget"

cd ..\..\Piraeus\Powershell\Piraeus.Module\Piraeus.Module
powershell.exe ..\..\..\..\..\UpdateNuspecVersion.ps1 ".\Piraeus.Module.nuspec" %@version%
powershell.exe ..\..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\..\UpdateDependency.ps1 ".\Piraeus.Module.nuspec" %@version% "Piraeus.Core"
powershell.exe ..\..\..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version%
nuget restore
msbuild Piraeus.Module.csproj /t:Clean,Rebuild,restore /p:Configuration=Release /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\Piraeus.Module.log /flp2:logfile=..\..\..\..\BuildOutput\Piraeus.Module_errors.log;errorsonly /flp3:logfile=..\..\..\..\BuildOutput\Piraeus.Module_warnings.log;warningsonly
nuget pack .\Piraeus.Module.nuspec -OutputDirectory "..\..\..\..\Nuget"



cd ..\..\..\..\Gateways\WebGateway\WebGateway
powershell.exe ..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.GrainInterfaces" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Grains" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Configuration" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Diagnostics" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Security" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Channels" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Protocols" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Adapters" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Storage" %@version%
nuget restore
msbuild WebGateway.csproj /t:clean /p:configuration=Debug
msbuild WebGateway.csproj /t:clean /p:configuration=Release
msbuild WebGateway.csproj /t:Clean,Rebuild,restore /p:Configuration=Debug /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\WebGateway.log /flp2:logfile=..\..\..\..\BuildOutput\WebGateway_errors.log;errorsonly /flp3:logfile=..\..\..\..\BuildOutput\WebGateway_warnings.log;warningsonly


cd ..\..\..\SkunkLab\SkunkLab.Servers\lib
powershell.exe ..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.GrainInterfaces" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Grains" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Configuration" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Diagnostics" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Security" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Channels" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Protocols" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Adapters" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "SkunkLab.Storage" %@version%
nuget restore
msbuild SkunkLab.Servers.csproj /t:clean /p:configuration=Debug
msbuild SkunkLab.Servers.csproj /t:clean /p:configuration=Release
msbuild SkunkLab.Servers.csproj /t:Clean,Rebuild,restore /p:Configuration=Debug /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\SkunkLab.Servers.log /flp2:logfile=..\..\..\..\BuildOutput\SkunkLab.Servers_errors.log;errorsonly /flp3:logfile=..\..\..\..\BuildOutput\SkunkLab.Servers_warnings.log;warningsonly


cd ..\..\..\Samples\Silo\LocalSilo
powershell.exe ..\..\..\..\VersionUpdate.ps1 ".\Properties\AssemblyInfo.cs" "%@version%"
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Core" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.GrainInterfaces" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.Grains" %@version%
powershell.exe ..\..\..\..\UpdatePackage.ps1 "Piraeus.SiloHost" %@version%
nuget restore
msbuild LocalSilo.csproj /t:clean /p:configuration=Debug
msbuild LocalSilo.Servers.csproj /t:clean /p:configuration=Release
msbuild LocalSilo.csproj /t:Clean,Rebuild,restore /p:Configuration=Debug /fl1 /fl2 /fl3 /flp1:logfile=..\..\..\..\BuildOutput\LocalSilo.log /flp2:logfile=..\..\..\..\BuildOutput\LocalSilo_errors.log;errorsonly /flp3:logfile=..\..\..\..\BuildOutput\LocalSilo_warnings.log;warningsonly




:ENDLINE