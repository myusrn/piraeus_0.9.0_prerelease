function Invoke-MSBuild ([string]$MSBuildPath, [string]$MSBuildParameters) {
    Invoke-Expression "$MSBuildPath $MSBuildParameters"
}

Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\..\SiloHost\Piraeus.Silo\Piraeus.Silo\Piraeus.Silo.csproj /p:OutputPath=..\..\..\Docker\OrleansSilo\Output /p:Configuration=Release"