#Requires -Version 5.1
param(
    [Parameter(Mandatory = $false)]
    [string]$ApiToken = $env:HOSTINGER_API_TOKEN,
    [string]$Domain = "wadnooh.com",
    [string]$ArchivePath = "e:\FLY\publish\hostinger-site.zip"
)

$ErrorActionPreference = "Stop"
$base = "https://api.hostinger.com"

if ([string]::IsNullOrWhiteSpace($ApiToken)) {
    Write-Host ""
    Write-Host "Hostinger API token required." -ForegroundColor Yellow
    Write-Host "1) Open: https://hpanel.hostinger.com/profile/api"
    Write-Host "2) Create token, then run:"
    Write-Host '   $env:HOSTINGER_API_TOKEN="YOUR_TOKEN"'
    Write-Host '   powershell -ExecutionPolicy Bypass -File e:\FLY\deploy\hostinger-deploy.ps1'
    Write-Host ""
    Write-Host "Or paste token now:"
    $ApiToken = Read-Host "HOSTINGER_API_TOKEN"
    if ([string]::IsNullOrWhiteSpace($ApiToken)) { throw "No token provided" }
}

$headers = @{
    Authorization = "Bearer $ApiToken"
    Accept        = "application/json"
}

Write-Host "==> Checking Hostinger API auth..." -ForegroundColor Cyan
try {
    $domains = Invoke-RestMethod -Method Get -Uri "$base/api/domains/v1/portfolio" -Headers $headers
    Write-Host "Auth OK" -ForegroundColor Green
} catch {
    # try alternate endpoints
    try {
        $null = Invoke-RestMethod -Method Get -Uri "$base/api/hosting/v1/websites" -Headers $headers
        Write-Host "Auth OK (hosting)" -ForegroundColor Green
    } catch {
        throw "Auth failed. Check token. Details: $($_.Exception.Message)"
    }
}

if (-not (Test-Path $ArchivePath)) { throw "Archive not found: $ArchivePath" }

Write-Host "==> Deploying static site to $Domain ..." -ForegroundColor Cyan
# Hostinger static deploy endpoint (multipart)
$form = @{
    domain = $Domain
}

# Prefer documented MCP-compatible flow:
# POST hosting deploy static with archive
$deployUrlCandidates = @(
    "$base/api/hosting/v1/websites/deploy-static",
    "$base/api/hosting/v1/website/deploy-static",
    "$base/api/hosting/v1/static-websites"
)

$uploaded = $false
foreach ($url in $deployUrlCandidates) {
    try {
        Write-Host "Trying $url"
        $response = curl.exe -sS -X POST $url `
            -H "Authorization: Bearer $ApiToken" `
            -H "Accept: application/json" `
            -F "domain=$Domain" `
            -F "archive=@$ArchivePath" `
            -F "remove_archive=true"
        Write-Host $response
        $uploaded = $true
        break
    } catch {
        Write-Host "Endpoint failed: $($_.Exception.Message)" -ForegroundColor Yellow
    }
}

if (-not $uploaded) {
    Write-Host ""
    Write-Host "API deploy endpoints may differ on your plan." -ForegroundColor Yellow
    Write-Host "Manual upload (guaranteed):" -ForegroundColor Cyan
    Write-Host "1) https://hpanel.hostinger.com/ -> Websites -> wadnooh.com -> File Manager"
    Write-Host "2) Open public_html"
    Write-Host "3) Upload: $ArchivePath"
    Write-Host "4) Extract zip, ensure index.html is in public_html root"
    Write-Host ""
    Start-Process "https://hpanel.hostinger.com/"
    throw "Could not auto-deploy via API. hPanel opened for manual upload."
}

Write-Host ""
Write-Host "Deploy requested. Wait 1-2 minutes then open https://$Domain" -ForegroundColor Green
Start-Process "https://$Domain"
