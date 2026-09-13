@echo off
title بناء تطبيق آمن للأندرويد - Aamn Travel APK Builder
cd /d "%~dp0"
powershell -NoProfile -ExecutionPolicy Bypass -File "%~dp0android-app\build-standalone-apk.ps1"
echo.
pause
