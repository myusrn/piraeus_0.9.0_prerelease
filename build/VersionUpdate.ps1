$filepath = $args[0]
$version = $args[1]

(get-content $filepath) -replace ("[0-9]+(\.([0-9]+|\*)){1,3}", $version) | out-file $filepath

