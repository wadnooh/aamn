#Requires -Version 5.1
param(
    [string]$FtpHost = "2.57.91.91",
    [string]$FtpUser = "u798103903",
    [string]$FtpPassword = "",
    [string]$RemoteDir = "public_html",
    [string]$LocalDir = "e:\FLY\publish\hostinger-site"
)

$ErrorActionPreference = "Continue"

if ([string]::IsNullOrWhiteSpace($FtpPassword)) {
    Write-Host "Please provide your Hostinger FTP password for user: $FtpUser" -ForegroundColor Yellow
    $sec = Read-Host "Enter FTP Password" -AsSecureString
    $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec)
    $FtpPassword = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR)
}

Write-Host "==> Starting Automated FTP Upload to Hostinger ($FtpHost)..." -ForegroundColor Cyan

function Upload-Directory {
    param($source, $target)
    
    $files = Get-ChildItem -Path $source
    foreach ($file in $files) {
        $remotePath = "$target/$($file.Name)"
        $uri = "ftp://$FtpHost/$remotePath"
        
        if ($file.PSIsContainer) {
            try {
                $req = [System.Net.FtpWebRequest]::Create($uri)
                $req.Credentials = New-Object System.Net.NetworkCredential($FtpUser, $FtpPassword)
                $req.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory
                $null = $req.GetResponse()
            } catch {}
            Upload-Directory $file.FullName $remotePath
        } else {
            try {
                Write-Host "Uploading $($file.Name) -> $remotePath"
                $client = New-Object System.Net.WebClient
                $client.Credentials = New-Object System.Net.NetworkCredential($FtpUser, $FtpPassword)
                $client.UploadFile($uri, $file.FullName)
            } catch {
                Write-Host "Failed uploading $($file.Name): $($_.Exception.Message)" -ForegroundColor Red
            }
        }
    }
}

Upload-Directory $LocalDir $RemoteDir
Write-Host ""
Write-Host "==> Automated Deployment to https://wadnooh.com Complete!" -ForegroundColor Green
