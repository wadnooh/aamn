@echo off
title الاتحاد للخدمات - Auto Deploy Pipeline
echo Starting Auto-Deploy Pipeline...
powershell -ExecutionPolicy Bypass -File "%~dp0deploy\sync-and-deploy.ps1"
pause
