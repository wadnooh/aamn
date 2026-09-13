@echo off
title بناء تطبيق آمن للأندرويد - Aamn Travel APK Builder
powershell -ExecutionPolicy Bypass -File "%~dp0android-app\build-standalone-apk.ps1"
pause
