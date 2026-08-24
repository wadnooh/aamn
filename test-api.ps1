# اختبار سريع لتطبيق السفر
# Quick Test for Wad Nooh Software & Computer

Write-Host "🧪 اختبار تطبيق السفر السوداني" -ForegroundColor Cyan
Write-Host "=================================" -ForegroundColor Cyan
Write-Host ""

$baseUrl = "http://localhost:5162/api"
$timeout = 5

Write-Host "ملاحظة: تأكد أن التطبيق يعمل أولاً!" -ForegroundColor Yellow
Write-Host "شغل التطبيق من Visual Studio (F5) ثم شغل هذا الملف" -ForegroundColor Yellow
Write-Host ""
Write-Host "اضغط Enter للمتابعة..." -ForegroundColor Green
Read-Host

Write-Host ""
Write-Host "🔍 اختبار Endpoints..." -ForegroundColor Cyan
Write-Host ""

# Test Flights
try {
    Write-Host "[1/5] اختبار الخدمات الجوية..." -NoNewline
    $response = Invoke-WebRequest -Uri "$baseUrl/flights" -TimeoutSec $timeout -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host " ✅ نجح" -ForegroundColor Green
        $flights = $response.Content | ConvertFrom-Json
        Write-Host "      عدد الخدمات: $($flights.Count)" -ForegroundColor Gray
    }
} catch {
    Write-Host " ❌ فشل" -ForegroundColor Red
    Write-Host "      هل التطبيق يعمل؟" -ForegroundColor Yellow
}

# Test Hotels
try {
    Write-Host "[2/5] اختبار الفنادق..." -NoNewline
    $response = Invoke-WebRequest -Uri "$baseUrl/hotels" -TimeoutSec $timeout -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host " ✅ نجح" -ForegroundColor Green
        $hotels = $response.Content | ConvertFrom-Json
        Write-Host "      عدد الفنادق: $($hotels.Count)" -ForegroundColor Gray
    }
} catch {
    Write-Host " ❌ فشل" -ForegroundColor Red
}

# Test Tourist Attractions
try {
    Write-Host "[3/5] اختبار المعالم التقنية..." -NoNewline
    $response = Invoke-WebRequest -Uri "$baseUrl/touristattractions" -TimeoutSec $timeout -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host " ✅ نجح" -ForegroundColor Green
        $attractions = $response.Content | ConvertFrom-Json
        Write-Host "      عدد المعالم: $($attractions.Count)" -ForegroundColor Gray
    }
} catch {
    Write-Host " ❌ فشل" -ForegroundColor Red
}

# Test Flight Bookings
try {
    Write-Host "[4/5] اختبار حجوزات الطيران..." -NoNewline
    $response = Invoke-WebRequest -Uri "$baseUrl/flightbookings" -TimeoutSec $timeout -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host " ✅ نجح" -ForegroundColor Green
        $bookings = $response.Content | ConvertFrom-Json
        Write-Host "      عدد الحجوزات: $($bookings.Count)" -ForegroundColor Gray
    }
} catch {
    Write-Host " ❌ فشل" -ForegroundColor Red
}

# Test Hotel Bookings
try {
    Write-Host "[5/5] اختبار حجوزات الفنادق..." -NoNewline
    $response = Invoke-WebRequest -Uri "$baseUrl/hotelbookings" -TimeoutSec $timeout -UseBasicParsing
    if ($response.StatusCode -eq 200) {
        Write-Host " ✅ نجح" -ForegroundColor Green
        $bookings = $response.Content | ConvertFrom-Json
        Write-Host "      عدد الحجوزات: $($bookings.Count)" -ForegroundColor Gray
    }
} catch {
    Write-Host " ❌ فشل" -ForegroundColor Red
}

Write-Host ""
Write-Host "=================================" -ForegroundColor Cyan
Write-Host "✅ انتهى الاختبار!" -ForegroundColor Green
Write-Host ""
Write-Host "لفتح API في المتصفح:" -ForegroundColor Cyan
Write-Host "  http://localhost:5000/api/flights" -ForegroundColor White
Write-Host ""

pause
