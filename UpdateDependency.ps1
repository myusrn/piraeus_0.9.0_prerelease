$filename = $args[0]
$version = $args[1]
$project = $args[2]

[xml]$xmlDocument = Get-Content -Path $filename
$exp = "//dependency[@id='" + $project + "']"
$exp = "/*/*/*/*/*[local-name()='dependency' and @id='$project' and namespace-uri()='http://schemas.microsoft.com/packaging/2012/06/nuspec.xsd']"

$node = $xmlDocument.DocumentElement.SelectSingleNode($exp)
if (!$node) { Write-Host "variable is null" }

$elem = [System.Xml.XmlElement]$node
$elem.SetAttribute('version', $version)
$xmlDocument.Save($filename)