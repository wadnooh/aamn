@echo off
echo ========================================================
echo   Building Aamn Android Debug/Release APK
echo ========================================================
echo.

call gradlew.bat assembleRelease

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo  [SUCCESS] APK build completed!
    echo  Output APK location:
    echo  app\build\outputs\apk\release\app-release-unsigned.apk
    echo ========================================================
) else (
    echo.
    echo [ERROR] Build failed. Please check the error messages above.
)
pause
