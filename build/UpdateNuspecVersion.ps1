$filename = $args[0]
$version = $args[1]
$prerelease = $args[2]

$vstring = $null

If(([string]::$prerelease).Length -eq 0) { $vstring = $version + "-prerelease" } Else { $vstring = $version }

[xml]$xmlDocument = Get-Content -Path $filename
$versionNode = $xmlDocument.ChildNodes[1].ChildNodes[0].ChildNodes[1]
$versionNode.InnerText = $vstring
$xmlDocument.Save($filename)