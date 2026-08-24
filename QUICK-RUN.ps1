# Quick Run - تشغيل سريع
Write-Host "🚀 ود نوح للبرمجيات والكمبيوتر - تشغيل سريع" -ForegroundColor Green
Write-Host "   Wad Nouh Software & Computer" -ForegroundColor Cyan
Write-Host ""

# الانتقال للمشروع والتشغيل مباشرة
Set-Location SudanTravelApp.API

Write-Host "⏳ جاري تشغيل السيرفر المحلي..." -ForegroundColor Yellow
Write-Host "📍 الصفحة ستفتح على: https://localhost:7086" -ForegroundColor Cyan
Write-Host ""

# تشغيل مباشر
Start-Process "https://localhost:7086" -ErrorAction SilentlyContinue
dotnet run
