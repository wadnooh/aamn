#Requires -Version 5.1
param(
    [int]$IntervalSec = 90,
    [int]$Port = 5162
)

$ErrorActionPreference = "Continue"
$root = "d:\FLY"
$urlFile = Join-Path $root "deploy\runtime\public-url.txt"
$logFile = Join-Path $root "deploy\runtime\watchdog.log"
$goLive = Join-Path $root "deploy\go-live-wadnooh.ps1"
$refresh = Join-Path $root "deploy\refresh-hostinger-api.ps1"

function Write-Log([string]$msg) {
    $line = "{0}  {1}" -f (Get-Date -Format "yyyy-MM-dd HH:mm:ss"), $msg
    Add-Content -Path $logFile -Value $line -Encoding UTF8
    Write-Host $line
}

function Test-PublicStack {
    try {
        $null = Invoke-RestMethod "http://127.0.0.1:$Port/api/info" -TimeoutSec 4
    } catch {
        return @{ Ok = $false; Reason = "local-api-down" }
    }

    if (-not (Test-Path $urlFile)) {
        return @{ Ok = $false; Reason = "no-tunnel-url" }
    }

    $url = (Get-Content $urlFile -Raw).Trim()
    if ([string]::IsNullOrWhiteSpace($url)) {
        return @{ Ok = $false; Reason = "empty-tunnel-url" }
    }

    try {
        $info = Invoke-RestMethod "$url/api/info" -TimeoutSec 12
        if (-not $info.version) {
            return @{ Ok = $false; Reason = "tunnel-bad-payload"; Url = $url }
        }
        return @{ Ok = $true; Url = $url; Version = $info.version }
    } catch {
        return @{ Ok = $false; Reason = "tunnel-unreachable"; Url = $url }
    }
}

Write-Log "Watchdog started interval=$IntervalSec"
while ($true) {
    $status = Test-PublicStack
    if ($status.Ok) {
        Write-Log ("OK v={0} {1}" -f $status.Version, $status.Url)
    } else {
        Write-Log ("DOWN reason={0} - restarting go-live" -f $status.Reason)
        try {
            & powershell -NoProfile -ExecutionPolicy Bypass -File $goLive -Port $Port
            Start-Sleep -Seconds 10
            if (Test-Path $refresh) {
                & powershell -NoProfile -ExecutionPolicy Bypass -File $refresh
            }
        } catch {
            Write-Log ("Restart failed: {0}" -f $_.Exception.Message)
        }
    }
    Start-Sleep -Seconds $IntervalSec
}
