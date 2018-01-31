$project = $args[0]
$version = $args[1]

[xml]$xmlDocument = Get-Content -Path "packages.config"
$node = $xmlDocument.DocumentElement.SelectSingleNode("//package[@id='$project']") 
$elem = [System.Xml.XmlElement]$node

$elem.SetAttribute("version", $version)
$xmlDocument.Save("packages.config")