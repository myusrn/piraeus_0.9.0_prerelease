#import the Piraeus Powershell Module
import-module C:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Debug\Piraeus.Module.dll

#Set the URL for the Piraeus Management API and Primary to gain access
$url = "http://localhost:4163"
$key = "12345678"

#Get the management security token for the session
$token = Get-PiraeusManagementToken -ServiceUrl $url -Key $key


#Setting up your first resource

#Create 2 access control policies using CAPL.
#First policy control access for publishers to the resource
#Second policy controls access for subscribers to the resource

$match = New-CaplMatch -Type Literal -ClaimType "http://www.skunklab.io/piraeus/role" -Required $true
$operation1 = New-CaplOperation -Type Equal -Value 'send'
$rule1 = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation1
$policy1 = New-CaplPolicy -PolicyID "http://www.skunklab.io/policy/resource1/pub" -EvaluationExpression $rule1


$operation2 = New-CaplOperation -Type Equal -Value 'listen'
$rule2 = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $operation2
$policy2 = New-CaplPolicy -PolicyID "http://www.skunklab.io/policy/resource1/sub" -EvaluationExpression $rule2


Set-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy1
Set-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $policy2

$p1 = Get-CaplPolicy -ServiceUrl $url -SecurityToken $token -PolicyId "http://www.skunklab.io/policy/resource1/pub"
$p2 = Get-CaplPolicy -ServiceUrl $url -SecurityToken $token -PolicyId "http://www.skunklab.io/policy/resource1/sub"

Set-PiraeusResourceMetadata -Enabled $true -RequireEncryptedChannel $false -ResourceUriString "http://www.skunklab.io/resource1" -PublishPolicyUriString "http://www.skunklab.io/policy/resource1/pub" -SubscribePolicyUriString "http://www.skunklab.io/policy/resource1/sub" -ServiceUrl $url -SecurityToken $token


$p1
$p2