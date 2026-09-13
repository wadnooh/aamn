@echo off
chcp 65001 >nul
title رفع مباشر إلى Hostinger عبر FTP - الاتحاد للخدمات
color 0B
echo ================================================================
echo    الاتحاد للخدمات - الرفع المباشر عبر FTP إلى Hostinger
echo    Domain: https://2-aa.com
echo    FTP Host: 2.57.91.91
echo    FTP User: u798103903
echo ================================================================
echo.
powershell -ExecutionPolicy Bypass -Command "& { param($FtpHost='2.57.91.91', $FtpUser='u798103903', $LocalDir='e:\FLY\publish\aamn-bus-booking-platform', $RemoteDir='public_html'); Write-Host 'الرجاء إدخال كلمة مرور FTP الخاصة بحساب Hostinger: ' -ForegroundColor Yellow; $sec = Read-Host -AsSecureString; $BSTR = [System.Runtime.InteropServices.Marshal]::SecureStringToBSTR($sec); $pwd = [System.Runtime.InteropServices.Marshal]::PtrToStringAuto($BSTR); if ([string]::IsNullOrWhiteSpace($pwd)) { Write-Host 'تم الإلغاء.' -ForegroundColor Red; exit }; Write-Host 'جاري رفع الملفات إلى public_html على الدومين 2-aa.com...' -ForegroundColor Cyan; function Upload-Dir($src, $tgt) { $files = Get-ChildItem -Path $src; foreach ($f in $files) { $rPath = \"$tgt/$($f.Name)\"; $uri = \"ftp://$FtpHost/$rPath\"; if ($f.PSIsContainer) { try { $req = [System.Net.FtpWebRequest]::Create($uri); $req.Credentials = New-Object System.Net.NetworkCredential($FtpUser, $pwd); $req.Method = [System.Net.WebRequestMethods+Ftp]::MakeDirectory; $null = $req.GetResponse() } catch {} ; Upload-Dir $f.FullName $rPath } else { try { Write-Host \"Uploading $($f.Name) -> $rPath\"; $cli = New-Object System.Net.WebClient; $cli.Credentials = New-Object System.Net.NetworkCredential($FtpUser, $pwd); $cli.UploadFile($uri, $f.FullName) } catch { Write-Host \"فشل رفع $($f.Name): $($_.Exception.Message)\" -ForegroundColor Red } } } ; Upload-Dir $LocalDir $RemoteDir; Write-Host 'تم النشر بنجاح على https://2-aa.com' -ForegroundColor Green; Start-Process 'https://2-aa.com' }"
echo.
pause
