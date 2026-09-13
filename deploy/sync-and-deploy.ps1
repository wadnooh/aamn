#Requires -Version 5.1
<#
.SYNOPSIS
    AAMN Bus Booking Platform Deploy Pipeline
    Synchronizes static platform assets and builds a clean production package for Hostinger.
#>

$ErrorActionPreference = "Stop"
$root = "e:\FLY"
$src = Join-Path $root "SudanTravelApp.API\wwwroot"
$dest = Join-Path $root "publish\aamn-bus-booking-platform"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   AAMN PLATFORM - AUTOMATED DEPLOYMENT & SYNC PIPELINE   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Sync files from wwwroot to repo root
Write-Host "`n[1/3] Synchronizing web assets to repository root..." -ForegroundColor Yellow
$htmlFiles = @("index.html", "about.html", "services.html", "projects.html", "contact.html", "admin.html", "client.html", "operator.html", "privacy.html", "portal.html", "aamn-travel.apk", "send-mail.php", ".htaccess")
foreach ($f in $htmlFiles) {
    $srcPath = Join-Path $src $f
    if (Test-Path $srcPath) { Copy-Item $srcPath $root -Force }
}

$dirs = @("css", "js", "images", ".well-known")
foreach ($d in $dirs) {
    $s = Join-Path $src $d
    $t = Join-Path $root $d
    if (Test-Path $s) {
        if (-not (Test-Path $t)) { New-Item -ItemType Directory -Path $t -Force | Out-Null }
        Copy-Item (Join-Path $s "*") $t -Recurse -Force
    }
}

# 1.1 Apply Cache Busting to all HTML assets
$ts = (Get-Date).ToString("yyyyMMddHHmm")
$allHtml = Get-ChildItem -Path $src -Filter "*.html"
foreach ($h in $allHtml) {
    $content = [IO.File]::ReadAllText($h.FullName, [Text.UTF8Encoding]::new($false))
    $content = [regex]::Replace($content, 'href="css/style\.css(\?v=[^"]*)?"', "href=`"css/style.css?v=$ts`"")
    $content = [regex]::Replace($content, 'href="css/pages\.css(\?v=[^"]*)?"', "href=`"css/pages.css?v=$ts`"")
    $content = [regex]::Replace($content, 'src="js/main\.js(\?v=[^"]*)?"', "src=`"js/main.js?v=$ts`"")
    $content = [regex]::Replace($content, 'src="js/admin-site-config\.js(\?v=[^"]*)?"', "src=`"js/admin-site-config.js?v=$ts`"")
    $content = [regex]::Replace($content, 'src="js/platform-data\.js(\?v=[^"]*)?"', "src=`"js/platform-data.js?v=$ts`"")
    [IO.File]::WriteAllText($h.FullName, $content, [Text.UTF8Encoding]::new($false))
    $rootTarget = Join-Path $root $h.Name
    [IO.File]::WriteAllText($rootTarget, $content, [Text.UTF8Encoding]::new($false))
}
Write-Host "Cache busting version tag ($ts) applied to all pages." -ForegroundColor Green

# 2. Build Clean Production Package
Write-Host "[2/4] Building clean production package for Hostinger..." -ForegroundColor Yellow
if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item (Join-Path $src "*") -Destination $dest -Recurse -Force

# Ensure .htaccess is copied into dest
$srcHtaccess = Join-Path $src ".htaccess"
if (Test-Path $srcHtaccess) {
    Copy-Item $srcHtaccess (Join-Path $dest ".htaccess") -Force
}

$tunnel = (Get-Content (Join-Path $root "deploy\runtime\public-url.txt") -Raw -ErrorAction SilentlyContinue)
if ($tunnel) { $tunnel = $tunnel.Trim() }
else { $tunnel = "https://onion-respected-karaoke-channels.trycloudflare.com" }

$apiBase = "$tunnel/api"
foreach ($rel in @("index.html", "about.html", "services.html", "projects.html", "contact.html", "admin.html", "client.html", "operator.html", "portal.html", "js\main.js", "js\admin-site-config.js", "js\platform-data.js")) {
    $p = Join-Path $dest $rel
    if (-not (Test-Path $p)) { continue }
    $text = [IO.File]::ReadAllText($p, [Text.UTF8Encoding]::new($false))
    $text = [regex]::Replace($text, "const API_BASE = '[^']*'", "const API_BASE = '$apiBase'")
    $text = $text.Replace("const API_BASE = '/api';", "const API_BASE = '$apiBase';")
    [IO.File]::WriteAllText($p, $text, [Text.UTF8Encoding]::new($false))
}

$cleanZip = Join-Path $root "publish\aamn-bus-booking-platform.zip"
Remove-Item $cleanZip -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $dest "*"), (Join-Path $dest ".htaccess") -DestinationPath $cleanZip -Force

# 3. Git Stage, Commit and Push
Write-Host "[3/4] Pushing updates to GitHub (wadnooh/aamn)..." -ForegroundColor Yellow
Set-Location $root
git add -A
$status = git status --porcelain
if ($status) {
    git commit -m "Auto-Deploy: Sync production assets, operator payment flow, and hardened .htaccess"
    git push origin main
} else {
    Write-Host "Working tree clean, syncing branch..." -ForegroundColor Gray
}

# 4. Success Summary
Write-Host "`n[4/4] Direct deployment triggered successfully!" -ForegroundColor Green
Write-Host "----------------------------------------------------------" -ForegroundColor Green
Write-Host "  Live Domain : https://2-aa.com" -ForegroundColor Cyan
Write-Host "  Status      : Synced, Hardened, and Pushed to Origin" -ForegroundColor Green
Write-Host "==========================================================`n" -ForegroundColor Cyan
