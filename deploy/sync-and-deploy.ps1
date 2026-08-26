#Requires -Version 5.1
<#
.SYNOPSIS
    Wad Nooh AAMN - Professional One-Click Auto-Deploy & Sync Pipeline
    Synchronizes static assets, builds clean production package, and pushes to GitHub/Hostinger.
#>

$ErrorActionPreference = "Stop"
$root = "e:\FLY"
$src = Join-Path $root "SudanTravelApp.API\wwwroot"
$dest = Join-Path $root "publish\wadnooh-clean-site"

Write-Host "==========================================================" -ForegroundColor Cyan
Write-Host "   WAD NOOH AAMN - AUTOMATED DEPLOYMENT & SYNC PIPELINE   " -ForegroundColor Cyan
Write-Host "==========================================================" -ForegroundColor Cyan

# 1. Sync files from wwwroot to repo root
Write-Host "`n[1/4] Synchronizing web assets to repository root..." -ForegroundColor Yellow
$htmlFiles = @("index.html", "about.html", "services.html", "projects.html", "contact.html", "admin.html", "specialty.html", "osh.html", "guide.html", "verify.html")
foreach ($f in $htmlFiles) {
    $srcPath = Join-Path $src $f
    if (Test-Path $srcPath) { Copy-Item $srcPath $root -Force }
}

$dirs = @("css", "js", "images", "data")
foreach ($d in $dirs) {
    $s = Join-Path $src $d
    $t = Join-Path $root $d
    if (Test-Path $s) {
        if (-not (Test-Path $t)) { New-Item -ItemType Directory -Path $t -Force | Out-Null }
        Copy-Item (Join-Path $s "*") $t -Recurse -Force
    }
}

if (Test-Path (Join-Path $src ".htaccess")) {
    Copy-Item (Join-Path $src ".htaccess") (Join-Path $root ".htaccess") -Force
}

# 2. Build Clean Production Package
Write-Host "[2/4] Building clean production package for Hostinger..." -ForegroundColor Yellow
if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
New-Item -ItemType Directory -Force -Path $dest | Out-Null
Copy-Item (Join-Path $src "*") -Destination $dest -Recurse -Force

$tunnel = (Get-Content (Join-Path $root "deploy\runtime\public-url.txt") -Raw -ErrorAction SilentlyContinue)
if ($tunnel) { $tunnel = $tunnel.Trim() }
else { $tunnel = "https://onion-respected-karaoke-channels.trycloudflare.com" }

$apiBase = "$tunnel/api"
foreach ($rel in @("index.html", "about.html", "services.html", "projects.html", "contact.html", "admin.html", "js\wadnooh-eng.js", "js\wep-gate.js", "js\auth-portal.js", "js\main.js")) {
    $p = Join-Path $dest $rel
    if (-not (Test-Path $p)) { continue }
    $text = [IO.File]::ReadAllText($p, [Text.UTF8Encoding]::new($false))
    $text = [regex]::Replace($text, "const API_BASE = '[^']*'", "const API_BASE = '$apiBase'")
    $text = $text.Replace("const API_BASE = '/api';", "const API_BASE = '$apiBase';")
    [IO.File]::WriteAllText($p, $text, [Text.UTF8Encoding]::new($false))
}

$cleanZip = Join-Path $root "publish\wadnooh-clean-site.zip"
Remove-Item $cleanZip -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $dest "*") -DestinationPath $cleanZip -Force

# 3. Git Stage, Commit and Push
Write-Host "[3/4] Pushing updates to GitHub (wadnooh/aamn)..." -ForegroundColor Yellow
Set-Location $root
git add -A
$status = git status --porcelain
if ($status) {
    git commit -m "Auto-Deploy: Sync production assets and site updates"
    git push origin main
} else {
    Write-Host "Working tree clean, pushing current branch..." -ForegroundColor Gray
    git push origin main
}

# 4. Success Summary
Write-Host "`n[4/4] Pipeline completed successfully!" -ForegroundColor Green
Write-Host "----------------------------------------------------------" -ForegroundColor Green
Write-Host "  GitHub Repo: https://github.com/wadnooh/aamn" -ForegroundColor White
Write-Host "  Live Domain: https://wadnooh.com" -ForegroundColor White
Write-Host "  Clean Zip:   $cleanZip" -ForegroundColor White
Write-Host "==========================================================`n" -ForegroundColor Cyan
