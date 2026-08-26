#Requires -Version 5.1
param([int]$Port = 5162)

$ErrorActionPreference = "Stop"
$root = "e:\FLY"
$cf = "C:\Program Files (x86)\cloudflared\cloudflared.exe"
$project = Join-Path $root "SudanTravelApp.API\SudanTravelApp.API.csproj"
$logDir = Join-Path $root "deploy\runtime"
New-Item -ItemType Directory -Force -Path $logDir | Out-Null

Write-Host "==> Building Debug (avoids Smart App Control block on Release publish)..." -ForegroundColor Cyan
dotnet build $project -c Debug --nologo
if ($LASTEXITCODE -ne 0) { throw "build failed" }

Write-Host "==> Stopping old processes on :$Port" -ForegroundColor Cyan
Get-NetTCPConnection -LocalPort $Port -ErrorAction SilentlyContinue |
    ForEach-Object { Stop-Process -Id $_.OwningProcess -Force -ErrorAction SilentlyContinue }
Get-Process cloudflared -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Get-CimInstance Win32_Process -Filter "Name = 'dotnet.exe'" |
    Where-Object { $_.CommandLine -match 'SudanTravelApp' } |
    ForEach-Object { Stop-Process -Id $_.ProcessId -Force -ErrorAction SilentlyContinue }
Start-Sleep -Seconds 2

Write-Host "==> Starting app via dotnet run (Debug)..." -ForegroundColor Cyan
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ASPNETCORE_URLS = "http://127.0.0.1:$Port"
$env:PublicBaseUrl = "https://wadnooh.com"
$env:DISABLE_HTTPS_REDIRECT = "1"
$appOut = Join-Path $logDir "app.out.log"
$appErr = Join-Path $logDir "app.err.log"
Remove-Item $appOut, $appErr -Force -ErrorAction SilentlyContinue
$app = Start-Process -FilePath "dotnet" -ArgumentList @(
    "run","--project",$project,"--no-build","--no-launch-profile","-c","Debug"
) -WorkingDirectory (Join-Path $root "SudanTravelApp.API") -PassThru -WindowStyle Hidden `
    -RedirectStandardOutput $appOut -RedirectStandardError $appErr

$ok = $false
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds 1
    try {
        $null = Invoke-RestMethod "http://127.0.0.1:$Port/api/info" -TimeoutSec 2
        $ok = $true
        break
    } catch {}
}
if (-not $ok) {
    if (Test-Path $appErr) { Write-Host (Get-Content $appErr -Raw) }
    if (Test-Path $appOut) { Write-Host (Get-Content $appOut -Raw) }
    throw "App failed to start"
}
Write-Host "App healthy (PID $($app.Id))" -ForegroundColor Green

Write-Host "==> Starting Cloudflare tunnel (http2)..." -ForegroundColor Cyan
$tunnelOut = Join-Path $logDir "tunnel.out.log"
$tunnelErr = Join-Path $logDir "tunnel.err.log"
Remove-Item $tunnelOut, $tunnelErr -Force -ErrorAction SilentlyContinue
$tunnel = Start-Process -FilePath $cf `
    -ArgumentList @("tunnel","--url","http://127.0.0.1:$Port","--no-autoupdate","--protocol","http2") `
    -PassThru -WindowStyle Hidden `
    -RedirectStandardOutput $tunnelOut -RedirectStandardError $tunnelErr

$url = $null
for ($i = 0; $i -lt 60; $i++) {
    Start-Sleep -Seconds 1
    $txt = ""
    if (Test-Path $tunnelOut) { $txt += Get-Content $tunnelOut -Raw -ErrorAction SilentlyContinue }
    if (Test-Path $tunnelErr) { $txt += Get-Content $tunnelErr -Raw -ErrorAction SilentlyContinue }
    if ($txt -match 'https://[a-z0-9-]+\.trycloudflare\.com') {
        $url = $Matches[0]
        break
    }
}
if (-not $url) {
    if (Test-Path $tunnelOut) { Write-Host (Get-Content $tunnelOut -Raw) }
    if (Test-Path $tunnelErr) { Write-Host (Get-Content $tunnelErr -Raw) }
    throw "Tunnel URL not found"
}

Set-Content (Join-Path $logDir "public-url.txt") $url
Set-Content (Join-Path $logDir "pids.txt") "app=$($app.Id)`ntunnel=$($tunnel.Id)"

# Wait for public DNS
$publicOk = $false
for ($i = 0; $i -lt 20; $i++) {
    Start-Sleep -Seconds 2
    try {
        $remote = Invoke-RestMethod "$url/api/info" -TimeoutSec 12
        Write-Host ("Public API OK version={0} domain={1}" -f $remote.version, $remote.domain) -ForegroundColor Green
        $publicOk = $true
        break
    } catch {}
}
if (-not $publicOk) { Write-Host "Public check still warming: $url" -ForegroundColor Yellow }

Write-Host ""
Write-Host "LIVE: $url" -ForegroundColor Green
Write-Host "TARGET DOMAIN: https://wadnooh.com (Hostinger upload API currently 500 - use LIVE URL)"
Write-Host $url
