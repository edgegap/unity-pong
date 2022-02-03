$buildFolder = (Resolve-Path "$PSScriptRoot/../Build/Server/StandaloneLinux64")

if (-Not (Test-Path -Path $buildFolder)) {
    "Cannot find build direcotry ($buildFolder)";
    Exit
}

Echo "Creating Dockerfiler in $buildFolder"
$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
$fileContent = @"
FROM ubuntu:bionic
MAINTAINER Edgegap <youremail@edgegap.com>

ARG DEBIAN_FRONTEND=noninteractive
ARG DOCKER_VERSION=17.06.0-ce

RUN apt-get update && \
apt-get install -y libglu1 xvfb libxcursor1

COPY ./ /root/build/

WORKDIR /root/
ENTRYPOINT ["/bin/bash", "/root/build/entrypoint.sh"]
"@
$dockerfilePath = "$buildFolder\Dockerfile"
[System.IO.File]::WriteAllLines("$buildFolder/Dockerfile", $fileContent, $Utf8NoBomEncoding)

Echo "Creating entrypoint.sh in $buildFolder"
$Utf8NoBomEncoding = New-Object System.Text.UTF8Encoding $False
$fileContent = @"
chmod +x /root/build/edgepong_sever.exe
xvfb-run --auto-servernum --server-args='-screen 0 640x480x24:32' /root/build/edgepong_sever.exe -batchmode -nographics
"@
[System.IO.File]::WriteAllLines("$buildFolder/entrypoint.sh", $fileContent, $Utf8NoBomEncoding)

$imageName = "harbor.edgegap.net/edgegap-experimental/unity-pong-server:0.1"

docker build  -f $dockerfilePath -t $imageName $buildFolder

if ($LASTEXITCODE -eq 1) {
    Exit $LASTEXITCODE
}

Echo "Done"