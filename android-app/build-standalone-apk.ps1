# Standalone APK Builder for Aamn Bus Booking Platform
$ErrorActionPreference = "Stop"

$root = "e:\FLY\android-app"
$sdk = "C:\Users\wadno\AppData\Local\Android\Sdk"
$buildTools = "$sdk\build-tools\34.0.0"
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

$buildDir = "$root\build-standalone"
if (Test-Path $buildDir) { Remove-Item $buildDir -Recurse -Force }
New-Item -ItemType Directory -Path "$buildDir\gen" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildDir\classes" -Force | Out-Null
New-Item -ItemType Directory -Path "$buildDir\dex" -Force | Out-Null

# 1. Compile Resources with AAPT2
Write-Host "`n[1/6] Compiling Android resources..." -ForegroundColor Yellow
& $aapt2 compile --dir "$root\app\src\main\res" -o "$buildDir\compiled_res.zip"
if ($LASTEXITCODE -ne 0) { throw "AAPT2 compile failed with code $LASTEXITCODE" }

# 2. Link Resources & Generate R.java
Write-Host "[2/6] Linking resources and generating R.java..." -ForegroundColor Yellow
& $aapt2 link -o "$buildDir\unaligned.apk" -I "$androidJar" --manifest "$root\app\src\main\AndroidManifest.xml" --java "$buildDir\gen" "$buildDir\compiled_res.zip" --auto-add-overlay
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

# 4. Dex Class Files with D8
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

# 6. Keystore & Signing with apksigner
Write-Host "[6/6] Signing APK with release key..." -ForegroundColor Yellow
$keystore = "$root\aamn-release.keystore"
if (-not (Test-Path $keystore)) {
    Write-Host "Generating release keystore..." -ForegroundColor Cyan
    & keytool -genkey -v -keystore $keystore -alias aamn -keyalg RSA -keysize 2048 -validity 10000 -storepass aamn2026 -keypass aamn2026 -dname "CN=Aamn Travel, OU=Production, O=Aamn Ltd, L=Khartoum, ST=Khartoum, C=SD"
}

$finalApkRoot = "e:\FLY\aamn-travel.apk"
$finalApkPublish = "e:\FLY\publish\aamn-travel.apk"
$finalApkApp = "$root\aamn-travel.apk"

cmd /c "$apksigner sign --ks `"$keystore`" --ks-pass pass:aamn2026 --key-pass pass:aamn2026 --out `"$finalApkRoot`" `"$buildDir\aligned.apk`""
if ($LASTEXITCODE -ne 0) { throw "APK signing failed with code $LASTEXITCODE" }

Copy-Item $finalApkRoot $finalApkPublish -Force
Copy-Item $finalApkRoot $finalApkApp -Force

# Verify APK
Write-Host "`nVerifying final APK signature..." -ForegroundColor Cyan
cmd /c "$apksigner verify --verbose `"$finalApkRoot`""

$apkItem = Get-Item $finalApkRoot
Write-Host "`n============================================================" -ForegroundColor Green
Write-Host " [SUCCESS] AAMN TRAVEL APK BUILT & SIGNED SUCCESSFULLY! " -ForegroundColor Green
Write-Host " Location : $($apkItem.FullName)" -ForegroundColor Cyan
Write-Host " Size     : $([math]::Round($apkItem.Length / 1KB, 2)) KB" -ForegroundColor Cyan
Write-Host " Status   : Ready to install & run on any Android device" -ForegroundColor Green
Write-Host " Linked To: https://2-aa.com/" -ForegroundColor Cyan
Write-Host "============================================================`n" -ForegroundColor Green
