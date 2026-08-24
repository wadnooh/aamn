# ود نوح للبرمجيات والكمبيوتر - Script التشغيل
# Wad Nouh Software & Computer - PowerShell Run Script

Write-Host "===============================================" -ForegroundColor Cyan
Write-Host "🇸🇩 ود نوح للبرمجيات والكمبيوتر 🇸🇩" -ForegroundColor Green
Write-Host "Wad Nouh Software & Computer" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Cyan
Write-Host ""

# التحقق من .NET SDK
Write-Host "[1/5] التحقق من .NET SDK..." -ForegroundColor Yellow
try {
    $dotnetVersion = dotnet --version
    Write-Host "✓ .NET SDK موجود - الإصدار: $dotnetVersion" -ForegroundColor Green
} catch {
    Write-Host "✗ ERROR: .NET SDK غير مثبت!" -ForegroundColor Red
    Write-Host "يرجى تثبيت .NET 10 SDK من: https://dotnet.microsoft.com/download" -ForegroundColor Yellow
    Read-Host "اضغط Enter للخروج"
    exit 1
}

Write-Host ""
Write-Host "[2/5] إيقاف العمليات السابقة..." -ForegroundColor Yellow
Get-Process dotnet -ErrorAction SilentlyContinue | Stop-Process -Force -ErrorAction SilentlyContinue
Start-Sleep -Seconds 1
Write-Host "✓ تم" -ForegroundColor Green

Write-Host ""
Write-Host "[3/5] تنظيف الملفات المؤقتة..." -ForegroundColor Yellow
Remove-Item -Path "SudanTravelApp.API\bin" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "SudanTravelApp.API\obj" -Recurse -Force -ErrorAction SilentlyContinue
Write-Host "✓ تم" -ForegroundColor Green

Write-Host ""
Write-Host "[4/5] الانتقال إلى مجلد المشروع..." -ForegroundColor Yellow
Set-Location -Path "SudanTravelApp.API"
Write-Host "✓ تم" -ForegroundColor Green

Write-Host ""
Write-Host "[5/5] تشغيل التطبيق..." -ForegroundColor Yellow
Write-Host ""
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host "🚀 جاري بناء وتشغيل السيرفر المحلي..." -ForegroundColor Green
Write-Host "════════════════════════════════════════════════" -ForegroundColor Cyan
Write-Host ""
Write-Host "📌 بعد التشغيل ستفتح الصفحة تلقائياً على:" -ForegroundColor Yellow
Write-Host "   🌐 https://localhost:7086" -ForegroundColor White
Write-Host "   🌐 http://localhost:5000" -ForegroundColor White
Write-Host ""
Write-Host "💡 للإيقاف: اضغط Ctrl+C" -ForegroundColor Yellow
Write-Host ""

# تشغيل التطبيق مباشرة
dotnet run

Set-Location -Path ".."

