# تشغيل تطبيق السفر السوداني - إصدار محسّن
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "  تطبيق السفر السوداني" -ForegroundColor Green
Write-Host "  Wad Nooh Software & Computer - Enhanced Edition" -ForegroundColor Green
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

# التحقق من صلاحيات المسؤول
$isAdmin = ([Security.Principal.WindowsPrincipal] [Security.Principal.WindowsIdentity]::GetCurrent()).IsInRole([Security.Principal.WindowsBuiltInRole]::Administrator)

if (-not $isAdmin) {
    Write-Host "⚠️  تحذير: لا تعمل كمسؤول!" -ForegroundColor Yellow
    Write-Host "   لحل مشاكل NuGet، شغل PowerShell كمسؤول" -ForegroundColor Yellow
    Write-Host ""
}

# محاولة حذف lock file
try {
    if (Test-Path "C:\Windows\Temp\NuGetScratch\lock") {
        Write-Host "[FIX] محاولة حذف NuGet lock file..." -ForegroundColor Yellow
        Remove-Item "C:\Windows\Temp\NuGetScratch\lock" -Force -ErrorAction Stop
        Write-Host "✓ تم حذف lock file" -ForegroundColor Green
    }
} catch {
    Write-Host "⚠️  لم يمكن حذف lock file (قد تحتاج صلاحيات مسؤول)" -ForegroundColor Yellow
}

Write-Host ""

# الانتقال إلى مجلد المشروع
Set-Location -Path "SudanTravelApp.API"

Write-Host "[1/4] تنظيف المشروع..." -ForegroundColor Cyan
$cleanOutput = dotnet clean --nologo 2>&1
if ($LASTEXITCODE -eq 0) {
    Write-Host "✓ تم التنظيف" -ForegroundColor Green
}

Write-Host ""
Write-Host "[2/4] استعادة الحزم (NuGet Restore)..." -ForegroundColor Cyan

# محاولة restore
$restoreOutput = dotnet restore --force-evaluate --verbosity quiet 2>&1

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ فشل Restore!" -ForegroundColor Red
    Write-Host ""
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
    Write-Host "  الحل الأسهل:" -ForegroundColor Yellow
    Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Yellow
    Write-Host ""
    Write-Host "  1. افتح Visual Studio" -ForegroundColor White
    Write-Host "  2. في Solution Explorer، انقر بزر الماوس الأيمن على Solution" -ForegroundColor White
    Write-Host "  3. اختر 'Restore NuGet Packages'" -ForegroundColor White
    Write-Host "  4. انتظر حتى ينتهي" -ForegroundColor White
    Write-Host "  5. اضغط F5 للتشغيل" -ForegroundColor White
    Write-Host ""
    Write-Host "  أو شغل هذا Script كمسؤول (Run as Administrator)" -ForegroundColor White
    Write-Host ""
    pause
    exit 1
}

Write-Host "✓ تم استعادة الحزم" -ForegroundColor Green

Write-Host ""
Write-Host "[3/4] بناء المشروع..." -ForegroundColor Cyan
$buildOutput = dotnet build --nologo --verbosity quiet

if ($LASTEXITCODE -ne 0) {
    Write-Host "✗ فشل البناء!" -ForegroundColor Red
    Write-Host ""
    Write-Host $buildOutput
    pause
    exit 1
}

Write-Host "✓ تم البناء بنجاح" -ForegroundColor Green

Write-Host ""
Write-Host "[4/4] تشغيل التطبيق..." -ForegroundColor Cyan
Write-Host ""
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host "  ✓ السيرفر يعمل الآن!" -ForegroundColor Green  
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host ""
Write-Host "افتح المتصفح على:" -ForegroundColor Cyan
Write-Host "  🌐 http://localhost:5000" -ForegroundColor White
Write-Host "  🔒 https://localhost:5001" -ForegroundColor White
Write-Host ""
Write-Host "اختبر API Endpoints:" -ForegroundColor Cyan
Write-Host "  ✈️  GET http://localhost:5000/api/flights" -ForegroundColor White
Write-Host "  🏨 GET http://localhost:5000/api/hotels" -ForegroundColor White
Write-Host "  🗿 GET http://localhost:5000/api/touristattractions" -ForegroundColor White
Write-Host "  📋 GET http://localhost:5000/api/flightbookings" -ForegroundColor White
Write-Host "  🛏️  GET http://localhost:5000/api/hotelbookings" -ForegroundColor White
Write-Host ""
Write-Host "اضغط Ctrl+C للإيقاف" -ForegroundColor Yellow
Write-Host "━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━" -ForegroundColor Green
Write-Host ""

# تشغيل التطبيق
dotnet run --no-build
