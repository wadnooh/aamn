@echo off
:: تشغيل كمسؤول - يحل مشكلة NuGet Lock
:: Run as Administrator - Fixes NuGet Lock Issue

echo ========================================
echo   تطبيق السفر السوداني
echo   Wad Nooh Software & Computer (Admin Mode)
echo ========================================
echo.

:: التحقق من صلاحيات المسؤول
net session >nul 2>&1
if %errorLevel% == 0 (
    echo [✓] يعمل بصلاحيات المسؤول
) else (
    echo [!] تحذير: لا يعمل بصلاحيات مسؤول
    echo.
    echo للحل:
    echo 1. اضغط بزر الماوس الأيمن على هذا الملف
    echo 2. اختر "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo.
echo [FIX] حذف NuGet lock file...
del "C:\Windows\Temp\NuGetScratch\lock" /F /Q >nul 2>&1
if %ERRORLEVEL% EQU 0 (
    echo [✓] تم حذف lock file
) else (
    echo [i] Lock file غير موجود أو تم حذفه مسبقاً
)

echo.
cd SudanTravelApp.API

echo [1/4] تنظيف المشروع...
dotnet clean --nologo >nul 2>&1
echo [✓] تم التنظيف

echo [2/4] استعادة حزم NuGet...
dotnet restore --force-evaluate
if %ERRORLEVEL% NEQ 0 (
    echo [✗] فشل restore!
    echo.
    echo جرب الطريقة الأخرى:
    echo - افتح Visual Studio
    echo - اضغط F5
    pause
    exit /b 1
)
echo [✓] تم استعادة الحزم

echo [3/4] بناء المشروع...
dotnet build --nologo
if %ERRORLEVEL% NEQ 0 (
    echo [✗] فشل البناء!
    pause
    exit /b 1
)
echo [✓] تم البناء بنجاح

echo.
echo ========================================
echo [✓] السيرفر يعمل الآن!
echo ========================================
echo.
echo افتح المتصفح:
echo   http://localhost:5000/api/flights
echo   http://localhost:5000/api/hotels
echo   http://localhost:5000/api/touristattractions
echo.
echo اضغط Ctrl+C للإيقاف
echo ========================================
echo.

dotnet run --no-build

pause
