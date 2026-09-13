@echo off
echo ========================================================
echo   Aamn App - Google Play Keystore Generation Tool
echo ========================================================
echo.
echo Generating release upload keystore (aamn-release-key.jks)...
echo.

keytool -genkey -v -keystore aamn-release-key.jks -alias aamn-key-alias -keyalg RSA -keysize 2048 -validity 10000 -storepass AamnTravel2026! -keypass AamnTravel2026! -dname "CN=Aamn Bus Booking, OU=Mobile, O=Aamn Co, L=Khartoum, S=Khartoum, C=SD"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo ========================================================
    echo  [SUCCESS] Keystore created: aamn-release-key.jks
    echo  Alias: aamn-key-alias
    echo  Keystore Password: AamnTravel2026!
    echo  Key Password: AamnTravel2026!
    echo ========================================================
) else (
    echo.
    echo [ERROR] Failed to generate keystore. Make sure JDK / keytool is installed and in your PATH.
)
pause
