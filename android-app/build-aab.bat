@echo off
echo ========================================================
echo   Building Aamn Google Play Android App Bundle (.AAB)
echo ========================================================
echo.

call gradlew.bat bundleRelease

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo  [SUCCESS] Google Play .AAB Bundle created!
    echo  Upload file location:
    echo  app\build\outputs\bundle\release\app-release.aab
    echo ========================================================
) else (
    echo.
    echo [ERROR] Bundle build failed. Please check the error messages above.
)
pause
