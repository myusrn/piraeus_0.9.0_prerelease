function Invoke-MSBuild ([string]$MSBuildPath, [string]$MSBuildParameters) {
    Invoke-Expression "$MSBuildPath $MSBuildParameters"
}

Get-ChildItem -Path ./Output -Recurse | Remove-Item -force -recurse

Invoke-MSBuild -MSBuildPath "MSBuild.exe" -MSBuildParameters "..\..\SiloHost\Piraeus.Silo\Piraeus.Silo\Piraeus.Silo.csproj /p:BuildProjectReferences=true /p:OutputPath=..\..\Docker\Orleans-Silo\Output /p:Configuration=Release"

