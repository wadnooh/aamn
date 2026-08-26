# Patch Hostinger static site to current tunnel URL and redeploy (WNC Phase 1).
$ErrorActionPreference = "Stop"
$root = "e:\FLY"
$tunnel = (Get-Content (Join-Path $root "deploy\runtime\public-url.txt") -Raw).Trim()
if (-not $tunnel) { throw "public-url.txt empty" }

$src = Join-Path $root "SudanTravelApp.API\wwwroot"
$dest = Join-Path $root "publish\hostinger-site"
if (Test-Path $dest) { Remove-Item $dest -Recurse -Force }
Copy-Item (Join-Path $src "*") -Destination $dest -Recurse -Force

# Static hosting: point API_BASE at Cloudflare tunnel
$apiBase = "$tunnel/api"
foreach ($rel in @("index.html", "admin.html", "js\wadnooh-eng.js", "js\wep-gate.js")) {
    $p = Join-Path $dest $rel
    if (-not (Test-Path $p)) { continue }
    $text = [IO.File]::ReadAllText($p, [Text.UTF8Encoding]::new($false))
    $text = [regex]::Replace($text, "const API_BASE = '[^']*'", "const API_BASE = '$apiBase'")
    $text = $text.Replace("const API_BASE = '/api';", "const API_BASE = '$apiBase';")
    [IO.File]::WriteAllText($p, $text, [Text.UTF8Encoding]::new($false))
}

# Simple .htaccess for SPA-ish static
$ht = "DirectoryIndex index.html`nOptions -Indexes`n"
[IO.File]::WriteAllText((Join-Path $dest ".htaccess"), $ht, [Text.UTF8Encoding]::new($false))

$zip = Join-Path $root "publish\hostinger-site.zip"
Remove-Item $zip -Force -ErrorAction SilentlyContinue
Compress-Archive -Path (Join-Path $dest "*") -DestinationPath $zip -Force
Write-Host "Packed $zip ($([math]::Round((Get-Item $zip).Length/1MB,2)) MB) -> API_BASE=$apiBase"

Push-Location (Join-Path $root "deploy\runtime")
try { node .\deploy-static.mjs } finally { Pop-Location }
Write-Host "Hostinger refreshed -> $tunnel"
