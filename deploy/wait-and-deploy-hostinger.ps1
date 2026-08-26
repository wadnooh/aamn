#Requires -Version 5.1
<#
Waits until Hostinger web hosting appears for wadnooh.com, then deploys static site.
#>
param(
    [int]$TimeoutMinutes = 45,
    [int]$PollSeconds = 20
)

$ErrorActionPreference = "Stop"
$envFile = "e:\FLY\deploy\.hostinger-token.env"
$token = (Select-String -Path $envFile -Pattern 'HOSTINGER_API_TOKEN=(.+)').Matches.Groups[1].Value
$base = "https://developers.hostinger.com"
$domain = "wadnooh.com"
$archive = "e:\FLY\publish\hostinger-site.zip"
$headers = @{ Authorization = "Bearer $token"; Accept = "application/json" }

if (-not (Test-Path $archive)) { throw "Missing $archive - run go-live / hostinger package build first" }

Write-Host "Waiting for Hostinger website on $domain ..." -ForegroundColor Cyan
Write-Host "Buy Web Hosting in hPanel if not purchased yet, then attach domain wadnooh.com"
Start-Process "https://hpanel.hostinger.com/"

$deadline = (Get-Date).AddMinutes($TimeoutMinutes)
$website = $null
while ((Get-Date) -lt $deadline) {
    try {
        $list = Invoke-RestMethod "$base/api/hosting/v1/websites?domain=$domain" -Headers $headers -TimeoutSec 30
        $items = @()
        if ($list.data) { $items = @($list.data) } elseif ($list -is [array]) { $items = $list }
        if ($items.Count -gt 0) {
            $website = $items[0]
            Write-Host "Found website: $($website | ConvertTo-Json -Compress)" -ForegroundColor Green
            break
        }
        # also check unfiltered
        $all = Invoke-RestMethod "$base/api/hosting/v1/websites" -Headers $headers -TimeoutSec 30
        if ($all.meta.total -gt 0) {
            Write-Host "Hosting websites detected: $($all.meta.total)" -ForegroundColor Green
            $website = $all.data[0]
            break
        }
    } catch {
        Write-Host "poll error: $($_.Exception.Message)"
    }
    Write-Host ("[{0}] still 0 websites... buy hosting then wait" -f (Get-Date).ToString("HH:mm:ss"))
    Start-Sleep -Seconds $PollSeconds
}

if (-not $website) { throw "Timed out waiting for hosting website. Purchase Web Hosting in hPanel and rerun." }

Write-Host "Deploying static archive to $domain ..." -ForegroundColor Cyan

# Hostinger MCP-style deploy is custom; try known REST patterns + file manager upload APIs
$candidates = @(
    "$base/api/hosting/v1/websites/$domain/deploy-static",
    "$base/api/hosting/v1/accounts/$($website.username)/websites/$domain/deployments",
    "$base/api/hosting/v1/websites/deploy-static"
)

$deployed = $false
foreach ($url in $candidates) {
    Write-Host "Trying $url"
    $out = & curl.exe -sS -w "`nHTTP:%{http_code}" -X POST $url `
        -H "Authorization: Bearer $token" `
        -H "Accept: application/json" `
        -F "domain=$domain" `
        -F "archive=@$archive" `
        -F "remove_archive=true"
    Write-Host $out
    if ($out -match "HTTP:20\d") { $deployed = $true; break }
}

if (-not $deployed) {
    Write-Host "REST deploy not available. Opening File Manager instructions." -ForegroundColor Yellow
    Write-Host "Upload this zip to public_html and extract:" -ForegroundColor Cyan
    Write-Host "  $archive"
    Start-Process "https://hpanel.hostinger.com/"
    explorer.exe /select,$archive
    throw "Auto deploy endpoint unavailable; zip selected for manual upload"
}

Write-Host "Clearing cache..." -ForegroundColor Cyan
try {
    if ($website.username) {
        Invoke-RestMethod -Method Delete -Uri "$base/api/hosting/v1/accounts/$($website.username)/websites/$domain/cache/clear" -Headers $headers -TimeoutSec 30 | Out-Null
    }
} catch {}

Write-Host ""
Write-Host "DONE: https://$domain" -ForegroundColor Green
Start-Process "https://$domain"
