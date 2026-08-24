#Requires -Version 5.1
<#
Creates a named Cloudflare tunnel and prints DNS records for wadnooh.com.
Requires: cloudflared tunnel login (browser) completed first.
#>
$ErrorActionPreference = "Stop"
$cf = "C:\Program Files (x86)\cloudflared\cloudflared.exe"
$cert = Join-Path $env:USERPROFILE ".cloudflared\cert.pem"
if (-not (Test-Path $cert)) {
    Write-Host "Cloudflare login required. Opening browser..." -ForegroundColor Yellow
    & $cf tunnel login
    if (-not (Test-Path $cert)) { throw "Login not completed. Approve the browser page then rerun." }
}

$cfgDir = Join-Path $env:USERPROFILE ".cloudflared"
New-Item -ItemType Directory -Force -Path $cfgDir | Out-Null

Write-Host "==> Creating named tunnel wadnooh..." -ForegroundColor Cyan
$create = & $cf tunnel create wadnooh 2>&1 | Out-String
Write-Host $create

$cred = Get-ChildItem $cfgDir -Filter "*.json" | Sort-Object LastWriteTime -Descending | Select-Object -First 1
if (-not $cred) { throw "Tunnel credentials json not found in $cfgDir" }

$configPath = Join-Path $cfgDir "wadnooh-config.yml"
@"
tunnel: wadnooh
credentials-file: $($cred.FullName)

ingress:
  - hostname: wadnooh.com
    service: http://127.0.0.1:5162
  - hostname: www.wadnooh.com
    service: http://127.0.0.1:5162
  - service: http_status:404
"@ | Set-Content -Path $configPath -Encoding UTF8

Write-Host "==> Routing DNS..." -ForegroundColor Cyan
& $cf tunnel route dns wadnooh wadnooh.com
& $cf tunnel route dns wadnooh www.wadnooh.com

Write-Host "==> Starting named tunnel service..." -ForegroundColor Cyan
# Run tunnel in background
Start-Process -FilePath $cf -ArgumentList @("tunnel","--config",$configPath,"run","wadnooh") -WindowStyle Hidden

Write-Host ""
Write-Host "Done. Point Hostinger nameservers to Cloudflare (if not already)," -ForegroundColor Green
Write-Host "or ensure the DNS CNAME records created by 'tunnel route dns' are active."
Write-Host "Then open https://wadnooh.com"
