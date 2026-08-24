@echo off
:: =============================================
:: حل نهائي لمشكلة NuGet في Visual Studio
:: =============================================

echo.
echo ╔══════════════════════════════════════════════╗
echo ║   حل مشكلة Build Failed في Visual Studio   ║
echo ╚══════════════════════════════════════════════╝
echo.

:: التحقق من الصلاحيات
net session >nul 2>&1
if %errorLevel% neq 0 (
    echo [!] يجب تشغيل هذا الملف كمسؤول!
    echo.
    echo الطريقة:
    echo   1. اضغط بزر الماوس الأيمن على هذا الملف
    echo   2. اختر "Run as administrator"
    echo.
    pause
    exit /b 1
)

echo [✓] يعمل بصلاحيات المسؤول
echo.

:: حذف lock file
echo [1/5] حذف NuGet lock file...
if exist "C:\Windows\Temp\NuGetScratch\lock" (
    del "C:\Windows\Temp\NuGetScratch\lock" /F /Q
    echo [✓] تم حذف lock file
) else (
    echo [i] Lock file غير موجود
)

:: حذف مجلدات البناء القديمة
echo.
echo [2/5] حذف مجلدات البناء القديمة...
cd SudanTravelApp.API
if exist "obj" rmdir /S /Q "obj"
if exist "bin" rmdir /S /Q "bin"
echo [✓] تم التنظيف

:: Restore
echo.
echo [3/5] استعادة حزم NuGet...
dotnet restore --force-evaluate --no-cache
if %ERRORLEVEL% neq 0 (
    echo [✗] فشل restore!
    echo.
    echo جرب:
    echo   1. أغلق Visual Studio تماماً
    echo   2. شغل هذا الملف كمسؤول مرة أخرى
    echo   3. افتح Visual Studio وجرب F5
    pause
    exit /b 1
)
echo [✓] نجح restore

:: Build
echo.
echo [4/5] بناء المشروع...
dotnet build
if %ERRORLEVEL% neq 0 (
    echo [✗] فشل البناء!
    pause
    exit /b 1
)
echo [✓] نجح البناء

:: إنشاء ملف exe
echo.
echo [5/5] التحقق من الملف التنفيذي...
if exist "bin\Debug\net10.0\SudanTravelApp.API.exe" (
    echo [✓] الملف التنفيذي موجود!
) else (
    if exist "bin\Debug\net10.0\SudanTravelApp.API.dll" (
        echo [✓] DLL موجود (سيعمل مع dotnet run)
    ) else (
        echo [✗] ملفات البناء مفقودة!
        pause
        exit /b 1
    )
)

echo.
echo ╔══════════════════════════════════════════════╗
echo ║          ✓ تم الإصلاح بنجاح!                ║
echo ╚══════════════════════════════════════════════╝
echo.
echo الآن:
echo   1. افتح Visual Studio
echo   2. اضغط F5
echo   3. يجب أن يعمل!
echo.

pause
