$filename = $args[0]
$version = $args[1]

[xml]$xmlDocument = Get-Content -Path $filename
$versionNode = $xmlDocument.ChildNodes[1].ChildNodes[0].ChildNodes[1]
$versionNode.InnerText = $version
$xmlDocument.Save($filename)