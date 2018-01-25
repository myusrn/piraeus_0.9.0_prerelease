




import-module C:\_git\core\src\Piraeus\Powershell\Piraeus.Module\Piraeus.Module\bin\Release\Piraeus.Module.dll





#$url = "http://localhost:3111"

#$url = "http://localhost:1733"


$url = "http://172.27.13.36"


$token = Get-PiraeusManagementToken -ServiceUrl $url -Key "12345678"

$token



$matchClaimType = "http://www.skunklab.io/role"



#CREATE ACCESS CONTROL POLICIES FOR A PIRAEUS RESOURCE



#create CAPL match expression; matches an identity's 'literal' claim type

#$match = New-CaplMatch -Type Literal -Required $true -ClaimType $matchClaimType

$match = New-CaplMatch -Type Literal -ClaimType "http://www.skunklab.io/role" -Required $true                                                



#create CAPL operation for pub-policy; will check the identity's matched claim type equals 'pub' to authz publish

$pubOperation = New-CaplOperation -Type Equal -Value "A"



#create CAPL operation for sub-policy; will check identity's matched claim type equals 'sub' to authz subscribe

$subOperation = New-CaplOperation -Type Equal -Value "B"



#create CAPL rule for pub-policy; combines the match expression and operation into a binary expression

$pubRule = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $pubOperation



#create CAPL rule for sub-policy; combines the match expression and operation into a binary expression

$subRule = New-CaplRule -Evaluates $true -MatchExpression $match -Operation $subOperation



#create policy ID (URIs) to reference access control policies

$pubPolicyId = "http://www.skunklab.io/resource1/pub"

$subPolicyId = "http://www.skunklab.io/resource1/sub"



#create the publish CAPL policy; executes the rule to match an access control decision

$pubPolicy = New-CaplPolicy -PolicyID $pubPolicyId -EvaluationExpression $pubRule



#create the subscribe CAPL policy; executes the rule to match an access control decision

$subPolicy = New-CaplPolicy -PolicyID $subPolicyId -EvaluationExpression $subRule









#add the CAPL policies to Piraeus

Set-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $pubPolicy 

Set-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $subPolicy













#CREATE A PIRAEUS RESOURCE TO SEND/RECEIVE MESSAGES





#create a Resource that references the access control policies

$resource = "http://www.skunklab.io/resourcea"

Set-PiraeusResourceMetadata -ResourceUriString $resource -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $pubPolicyId -SubscribePolicyUriString $subPolicyId -ServiceUrl $url -SecurityToken $token



$resource2 = "http://www.skunklab.io/resourceb"

Set-PiraeusResourceMetadata -ResourceUriString $resource2 -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $subPolicyId -SubscribePolicyUriString $pubPolicyId -ServiceUrl $url -SecurityToken $token







$data1 = Get-PiraeusResourceMetadata -ResourceUriString $resource -ServiceUrl $url -SecurityToken $token

$data1



$data2 = Get-PiraeusResourceMetadata -ResourceUriString $resource2 -ServiceUrl $url -SecurityToken $token

$data2





#Summary

#Created 2 access control policies and 1 resource in Piraeus

#The resource metadata references each policy by its policy id.

#The Pub policy is used to authorize senders of messages to the resource

#The Sub policy is used to authorize receivers of messages from the resource

#It is possible now to send/receive messages from the resource iff the access control conditions are satisfied.

Set-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $pubPolicy 

Set-CaplPolicy -ServiceUrl $url -SecurityToken $token -Policy $subPolicy













#CREATE A PIRAEUS RESOURCE TO SEND/RECEIVE MESSAGES





#create a Resource that references the access control policies

$resource = "http://www.skunklab.io/resourcea"

Set-PiraeusResourceMetadata -ResourceUriString $resource -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $pubPolicyId -SubscribePolicyUriString $subPolicyId -ServiceUrl $url -SecurityToken $token



$resource2 = "http://www.skunklab.io/resourceb"

Set-PiraeusResourceMetadata -ResourceUriString $resource2 -Enabled $true -RequireEncryptedChannel $false -PublishPolicyUriString $subPolicyId -SubscribePolicyUriString $pubPolicyId -ServiceUrl $url -SecurityToken $token







$data1 = Get-PiraeusResourceMetadata -ResourceUriString $resource -ServiceUrl $url -SecurityToken $token

$data1



$data2 = Get-PiraeusResourceMetadata -ResourceUriString $resource2 -ServiceUrl $url -SecurityToken $token

$data2





#Summary

#Created 2 access control policies and 1 resource in Piraeus

#The resource metadata references each policy by its policy id.

#The Pub policy is used to authorize senders of messages to the resource

#The Sub policy is used to authorize receivers of messages from the resource

#It is possible now to send/receive messages from the resource iff the access control conditions are satisfied.