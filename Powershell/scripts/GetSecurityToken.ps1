
#import-module C:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll
$serviceUrl = "http://localhost:4163"
$key = "12345678"
$securityToken = Get-PiraeusManagementToken -ServiceUrl $serviceUrl -Key $key

$securityToken
