@echo off
echo ========================================
echo   تطبيق السفر السوداني
echo   Wad Nooh Software & Computer
echo ========================================
echo.
echo ملاحظة مهمة:
echo ------------
echo لحل مشكلة NuGet Lock، استخدم احدى الطرق التالية:
echo.
echo [الطريقة 1 - الأسهل] استخدام Visual Studio:
echo   1. افتح Visual Studio
echo   2. اضغط F5 مباشرة
echo   3. سيعمل التطبيق!
echo.
echo [الطريقة 2] تشغيل كمسؤول:
echo   1. اضغط بزر الماوس الأيمن على هذا الملف
echo   2. اختر "Run as administrator"
echo   3. ثم اضغط Enter للمتابعة
echo.
echo [الطريقة 3] حذف Lock File يدوياً:
echo   افتح PowerShell كمسؤول ونفذ:
echo   Remove-Item "C:\Windows\Temp\NuGetScratch\lock" -Force
echo   ثم شغل هذا الملف مرة اخرى
echo.
echo ========================================
pause

cd SudanTravelApp.API

echo.
echo [1/4] تنظيف المشروع...
dotnet clean --nologo >nul 2>&1

echo [2/4] استعادة الحزم...
dotnet restore --force-evaluate >nul 2>&1

if %ERRORLEVEL% NEQ 0 (
    echo [خطأ] فشل restore!
    echo.
    echo الحل: استخدم Visual Studio واضغط F5
    echo او شغل PowerShell كمسؤول واحذف:
    echo   C:\Windows\Temp\NuGetScratch\lock
    pause
    exit /b 1
)

echo [3/4] بناء المشروع...
dotnet build --nologo

if %ERRORLEVEL% NEQ 0 (
    echo [خطأ] فشل البناء!
    pause
    exit /b 1
)

echo [4/4] تشغيل التطبيق...
echo.
echo ========================================
echo ✓ السيرفر يعمل الآن!
echo ========================================
echo.
echo افتح المتصفح على:
echo   - http://localhost:5000
echo   - https://localhost:5001
echo.
echo اختبر API:
echo   GET http://localhost:5000/api/flights
echo   GET http://localhost:5000/api/hotels  
echo   GET http://localhost:5000/api/touristattractions
echo.
echo اضغط Ctrl+C للإيقاف
echo ========================================
echo.

dotnet run --no-build

pause
