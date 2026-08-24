@echo off
chcp 65001 >nul
echo.
echo ═══════════════════════════════════════════
echo 🇸🇩 ود نوح للبرمجيات والكمبيوتر
echo Wad Nouh Software ^& Tourism
echo ═══════════════════════════════════════════
echo.

echo [1/4] التحقق من .NET SDK...
dotnet --version >nul 2>&1
if %errorlevel% neq 0 (
    echo ✗ ERROR: .NET SDK غير مثبت!
    echo يرجى تثبيت .NET 10 SDK
    pause
    exit /b 1
)
echo ✓ .NET SDK موجود

echo.
echo [2/4] تنظيف الملفات المؤقتة...
if exist "SudanTravelApp.API\bin" rd /s /q "SudanTravelApp.API\bin" 2>nul
if exist "SudanTravelApp.API\obj" rd /s /q "SudanTravelApp.API\obj" 2>nul
echo ✓ تم

echo.
echo [3/4] الانتقال لمجلد المشروع...
cd SudanTravelApp.API
echo ✓ تم

echo.
echo [4/4] تشغيل السيرفر المحلي...
echo.
echo ═══════════════════════════════════════════
echo 🚀 جاري التشغيل...
echo ═══════════════════════════════════════════
echo.
echo سيفتح على: https://localhost:7086
echo للإيقاف: Ctrl+C
echo.

dotnet run

cd ..
pause

