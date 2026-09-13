@echo off
title بناء تطبيق آمن للأندرويد - Aamn Travel APK Builder
echo ============================================================
echo   Building Aamn Android APK (Official Compiler)
echo ============================================================
echo.

powershell -ExecutionPolicy Bypass -File "%~dp0build-standalone-apk.ps1"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ============================================================
    echo  [SUCCESS] APK build and signature verified!
    echo  Output APK locations:
    echo    1. e:\FLY\aamn-travel.apk
    echo    2. e:\FLY\publish\aamn-travel.apk
    echo    3. %~dp0aamn-travel.apk
    echo ============================================================
) else (
    echo.
    echo [ERROR] Build failed. Please check the log messages above.
)
pause
