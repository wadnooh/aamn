#Requires -Version 5.1
param(
    [string]$OutputDir = "e:\FLY\publish\wadnooh",
    [string]$Configuration = "Release"
)

$ErrorActionPreference = "Stop"
$project = "e:\FLY\SudanTravelApp.API\SudanTravelApp.API.csproj"

Write-Host "==> Building production package for wadnooh.com" -ForegroundColor Cyan

if (Test-Path $OutputDir) {
    Remove-Item $OutputDir -Recurse -Force
}
New-Item -ItemType Directory -Path $OutputDir | Out-Null
New-Item -ItemType Directory -Path (Join-Path $OutputDir "logs") | Out-Null

dotnet publish $project -c $Configuration -o $OutputDir --nologo
if ($LASTEXITCODE -ne 0) { throw "Publish failed" }

# Ensure production config and web.config are present
Copy-Item "e:\FLY\SudanTravelApp.API\appsettings.Production.json" (Join-Path $OutputDir "appsettings.Production.json") -Force
if (Test-Path "e:\FLY\SudanTravelApp.API\web.config") {
    Copy-Item "e:\FLY\SudanTravelApp.API\web.config" (Join-Path $OutputDir "web.config") -Force
}

$zipPath = "e:\FLY\publish\wadnooh-site.zip"
if (Test-Path $zipPath) { Remove-Item $zipPath -Force }
Compress-Archive -Path (Join-Path $OutputDir "*") -DestinationPath $zipPath -Force

Write-Host ""
Write-Host "Publish ready:" -ForegroundColor Green
Write-Host "  Folder: $OutputDir"
Write-Host "  Zip:    $zipPath"
Write-Host ""
Write-Host "Next: upload to Hostinger VPS / Web App and point wadnooh.com DNS A record to the server IP."
