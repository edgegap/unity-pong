 	
param ($tag)


$buildFolder = (Resolve-Path "$PSScriptRoot/../Build/Server/StandaloneLinux64")
$imageName = "harbor.edgegap.net/edgegap-experimental/unity-pong-server"
$defaultTag = "0.2"
$imageTag = if ($tag -eq "" -or $tag -eq $null) { $defaultTag } else { $tag }