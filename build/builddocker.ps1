function Invoke-MSBuild ([string]$MSBuildPath, [string]$MSBuildParameters) {
    Invoke-Expression "$MSBuildPath $MSBuildParameters"
}

Get-ChildItem -Path ..\src\Docker\TcpUdpGateway\Output -Recurse | Remove-Item -force -recurse
Get-ChildItem -Path ..\src\Docker\Orleans-Silo\Output -Recurse | Remove-Item -force -recurse

Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\src\Gateways\TcpUdpGateway\TcpUdpGateway\TcpUdpGateway.csproj  /p:OutputPath=..\..\Docker\TcpUdpGateway\Output /p:Configuration=Release"

Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\src\SiloHost\Piraeus.Silo\Piraeus.Silo\Piraeus.Silo.csproj /p:OutputPath=..\..\Docker\Orleans-Silo\Output /p:Configuration=Release"

