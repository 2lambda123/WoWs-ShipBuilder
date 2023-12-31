﻿param(
    [string]$buildConfig="Release",
    [string][Parameter(Mandatory=$false)]$signingCert,
    [string][Parameter(Mandatory=$false)]$signingPassword
)

$frameworkVersion="net7.0"

Write-Output "Building application"
dotnet build -c $buildConfig

Write-Output "Publishing build"
dotnet publish WoWsShipBuilder.UI -c $buildConfig -p:PublishProfile=PublishWindows
$publishDir = "WoWsShipBuilder.UI\bin\$buildConfig\publish"
Copy-Item -Path WoWsShipBuilder.UI\bin\$buildConfig\$frameworkVersion\runtimes\win7-x64\native\av_libGLESv2.dll -Destination $publishDir
Copy-Item -Path WoWsShipBuilder.UI\bin\$buildConfig\$frameworkVersion\Third-Party-Licenses.txt -Destination $publishDir

#Write-Output "Packing published build into nuget package"
#dotnet pack WoWsShipBuilder.UI -c $buildConfig --no-build

Write-Output "Creating Squirrel.Windows release"
$version = Get-Content -Path WoWsShipBuilder.UI\buildInfo.txt
$signingParams = ""
if ($signingCert) {
    Write-Output "Signing release"
    $signingParams = "--signParams=`"/a /f $signingCert /p $signingPassword /fd sha256 /tr http://timestamp.digicert.com /td sha256`""
}
Tools\Squirrel.exe pack --releaseDir=".\releases" --icon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --appIcon="WoWsShipBuilder.UI\Assets\ShipBuilderIcon_bg.ico" --no-delta --splashImage="LoadingIcon.gif" --packId="WoWsShipBuilder" --packVersion="$version" --packDir="$publishDir" --packTitle="WoWsShipBuilder" --includePdb $signingParams


Write-Output "Local release test completed"