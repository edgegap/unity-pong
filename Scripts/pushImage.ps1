param (
    [string]$version
)

. "$PSScriptRoot/variables.ps1" -tag $version

docker push "${imageName}:${imageTag}"