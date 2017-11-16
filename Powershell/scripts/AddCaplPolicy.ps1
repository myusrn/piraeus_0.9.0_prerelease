import-module C:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll



$serviceUrl = "http://localhost:4163"
$key = "12345678"
$securityToken = Get-SecurityToken -ServiceUrl $serviceUrl -Key $key
#$match = New-CaplMatch -Type Literal -ClaimType "http://www.skunklab.io/role" -Required $true
#$operation = New-CaplOperation -Type Equal -Value 'manage'
#$rule = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation
#$policy1 = New-CaplPolicy -PolicyID "http://www.skunklab.io/policy/resource1/pub" -EvaluationExpression $rule
#$policy2 = New-CaplPolicy -PolicyID "http://www.skunklab.io/policy/resource1/sub" -EvaluationExpression $rule
#Add-CaplPolicy -ServiceUrl $serviceUrl -SecurityToken $securitytoken -Policy $policy1
#Add-CaplPolicy -ServiceUrl $serviceUrl -SecurityToken $securitytoken -Policy $policy2
#Set-ResourceMetadata -Enabled $true -RequireEncryptedChannel $false -ResourceUriString "http://www.skunklab.io/resource1" -PublishPolicyUriString "http://www.skunklab.io/policy/resource1/pub" -SubscribePolicyUriString "http://www.skunklab.io/policy/resource1/sub" -ServiceUrl $serviceUrl -SecurityToken $securityToken



