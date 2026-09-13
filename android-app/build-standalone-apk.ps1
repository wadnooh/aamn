# Standalone APK Builder for Aamn Bus Booking Platform
$ErrorActionPreference = "Stop"

# Use relative path of the script directory (100% portable)
$root = $PSScriptRoot

# Locate Android SDK automatically
$sdkCandidates = @(
    $env:ANDROID_HOME,
    $env:ANDROID_SDK_ROOT,
    "C:\Users\wadno\AppData\Local\Android\Sdk",
    "$env:LOCALAPPDATA\Android\Sdk"
)

$sdk = $null
foreach ($cand in $sdkCandidates) {
    if ($cand -and (Test-Path "$cand\platforms\android-34\android.jar")) {
        $sdk = $cand
        break
    }
}

if (-not $sdk) {
    throw "Android SDK platform android-34 not found. Please install Android SDK."
}

# Locate build tools (prefer 34.0.0 or latest)
$buildToolsDir = Get-ChildItem "$sdk\build-tools" | Sort-Object Name -Descending | Select-Object -First 1
if (-not $buildToolsDir) {
    throw "Android build-tools not found in $sdk\build-tools."
}
$buildTools = $buildToolsDir.FullName

$platform = "$sdk\platforms\android-34"
$aapt2 = "$buildTools\aapt2.exe"
$aapt = "$buildTools\aapt.exe"
$d8 = "$buildTools\d8.bat"
$zipalign = "$buildTools\zipalign.exe"
$apksigner = "$buildTools\apksigner.bat"
$androidJar = "$platform\android.jar"

Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "   BUILDING AAMN TRAVEL ANDROID APK (STANDALONE COMPILER)   " -ForegroundColor Cyan
Write-Host "============================================================" -ForegroundColor Cyan
Write-Host "SDK Path        : $sdk" -ForegroundColor Gray
Write-Host "Build Tools     : $buildTools" -ForegroundColor Gray

$buildDir = "$root\build-standalone"
if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
New-Item -ItemType Directory -Path "$buildDir\gen" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildDir\classes" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildDir\dex" -Force | Out-Null

# 1. Compile Resources with AAPT2
Write-Host "`n[1/6] Compiling Android resources..." -ForegroundColor Yellow
& $aapt2 compile --dir "$root\app\src\main\res" -o "$buildDir\compiled_res.zip"
if ($LASTEXITCODE -ne 0) { throw "AAPT2 compile failed with code $LASTEXITCODE" }

# 2. Link Resources & Generate R.java with explicit API 21-34 targets
Write-Host "[2/6] Linking resources and generating R.java (Target API 34 / Android 14)..." -ForegroundColor Yellow
& $aapt2 link -o "$buildDir\unaligned.apk" `
    -I "$androidJar" `
    --manifest "$root\app\src\main\AndroidManifest.xml" `
    --java "$buildDir\gen" `
    "$buildDir\compiled_res.zip" `
    --min-sdk-version 21 `
    --target-sdk-version 34 `
    --version-code 1 `
    --version-name "1.0.0" `
    --auto-add-overlay
if ($LASTEXITCODE -ne 0) { throw "AAPT2 link failed with code $LASTEXITCODE" }

# 3. Compile Java Source Code with javac
Write-Host "[3/6] Compiling Java source files..." -ForegroundColor Yellow
$javaFiles = @(
    "$buildDir\gen\com\aamn\travel\R.java",
    "$root\app\src\main\java\com\aamn\travel\MainActivity.java",
    "$root\app\src\main\java\com\aamn\travel\WebAppInterface.java"
)
& javac -source 17 -target 17 -cp "$androidJar" -d "$buildDir\classes" $javaFiles
if ($LASTEXITCODE -ne 0) { throw "javac compilation failed with code $LASTEXITCODE" }

# 4. Dex Class Files with D8 (Target API 21+)
Write-Host "[4/6] Converting bytecode to Dalvik Executable (classes.dex)..." -ForegroundColor Yellow
$classFiles = (Get-ChildItem -Path "$buildDir\classes\com\aamn\travel\*.class").FullName
cmd /c "$d8 --release --min-api 21 --output `"$buildDir\dex`" --lib `"$androidJar`" $($classFiles -join ' ')"
if ($LASTEXITCODE -ne 0) { throw "D8 dexing failed with code $LASTEXITCODE" }

# Add classes.dex to APK
Set-Location "$buildDir\dex"
& $aapt add "$buildDir\unaligned.apk" "classes.dex"
Set-Location $root

# 5. Zipalign APK (4-byte alignment)
Write-Host "[5/6] Zip-aligning APK..." -ForegroundColor Yellow
& $zipalign -f -p 4 "$buildDir\unaligned.apk" "$buildDir\aligned.apk"
if ($LASTEXITCODE -ne 0) { throw "Zipalign failed with code $LASTEXITCODE" }

# 6. Keystore & Signing with apksigner (v1, v2, v3 schemes)
Write-Host "[6/6] Signing APK with release key..." -ForegroundColor Yellow
$keystore = "$root\aamn-release.keystore"
if (-not (Test-Path $keystore)) {
    Write-Host "Generating release keystore..." -ForegroundColor Cyan
    & keytool -genkey -v -keystore $keystore -alias aamn -keyalg RSA -keysize 2048 -validity 10000 -storepass aamn2026 -keypass aamn2026 -dname "CN=Aamn Travel, OU=Production, O=Aamn Ltd, L=Khartoum, ST=Khartoum, C=SD"
}

$outputApkLocal = "$root\aamn-travel.apk"
cmd /c "$apksigner sign --ks `"$keystore`" --ks-pass pass:aamn2026 --key-pass pass:aamn2026 --min-sdk-version 21 --out `"$outputApkLocal`" `"$buildDir\aligned.apk`""
if ($LASTEXITCODE -ne 0) { throw "APK signing failed with code $LASTEXITCODE" }

# Copy to external locations if available
if (Test-Path "e:\FLY") {
    Copy-Item $outputApkLocal "e:\FLY\aamn-travel.apk" -Force
}
if (Test-Path "e:\FLY\publish") {
    Copy-Item $outputApkLocal "e:\FLY\publish\aamn-travel.apk" -Force
}

# Verify APK
Write-Host "`nVerifying final APK signature and compliance..." -ForegroundColor Cyan
cmd /c "$apksigner verify --verbose `"$outputApkLocal`""

$apkItem = Get-Item $outputApkLocal
Write-Host "`n============================================================" -ForegroundColor Green
Write-Host " [SUCCESS] AAMN TRAVEL APK READY FOR DIRECT INSTALLATION!   " -ForegroundColor Green
Write-Host " Location : $($apkItem.FullName)" -ForegroundColor Cyan
Write-Host " Size     : $([math]::Round($apkItem.Length / 1KB, 2)) KB" -ForegroundColor Cyan
Write-Host " Target   : Android 5.0 (API 21) up to Android 14 (API 34)  " -ForegroundColor Green
Write-Host " Domain   : https://2-aa.com/                              " -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Green
