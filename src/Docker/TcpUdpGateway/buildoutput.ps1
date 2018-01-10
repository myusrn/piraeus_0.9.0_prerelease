function Invoke-MSBuild ([string]$MSBuildPath, [string]$MSBuildParameters) {
    Invoke-Expression "$MSBuildPath $MSBuildParameters"
}

Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\..\Gateways\TcpUdpGateway\TcpUdpGateway\TcpUdpGateway.csproj /p:OutputPath=..\..\..\Docker\TcpUdpGateway\Output /p:Configuration=Release"