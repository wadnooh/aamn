# أمثلة API - تطبيق البرمجيات والكمبيوتر السوداني

## عنوان API الأساسي
```
https://localhost:7086/api
```

---

## 1. الخدمات الجوية ✈️

### الحصول على جميع الخدمات
```http
GET /api/flights
```

**استجابة:**
```json
[
  {
    "id": 1,
    "flightNumber": "SD101",
    "airline": "Sudan Airways",
    "departureCity": "الخرطوم",
    "arrivalCity": "بورتسودان",
    "departureTime": "2026-04-04T10:00:00",
    "arrivalTime": "2026-04-04T12:00:00",
    "price": 15000,
    "availableSeats": 120,
    "aircraftType": "Boeing 737"
  }
]
```

### البحث عن خدمات
```http
GET /api/flights/search?from=الخرطوم&to=بورتسودان
GET /api/flights/search?date=2026-04-05
```

### الحصول على خدمة محددة
```http
GET /api/flights/1
```

---

## 2. الفنادق 🏨

### الحصول على جميع الفنادق
```http
GET /api/hotels
```

**استجابة:**
```json
[
  {
    "id": 1,
    "name": "فندق كورنثيا",
    "city": "الخرطوم",
    "address": "شارع النيل، الخرطوم",
    "description": "فندق فاخر 5 نجوم على ضفاف النيل",
    "starRating": 5,
    "pricePerNight": 50000,
    "availableRooms": 50,
    "imageUrl": "/images/corinthia.jpg",
    "phoneNumber": "+249-183-779000"
  }
]
```

### البحث عن فنادق
```http
GET /api/hotels/search?city=الخرطوم
GET /api/hotels/search?minRating=5
GET /api/hotels/search?city=الخرطوم&minRating=4&maxPrice=60000
```

---

## 3. حجوزات الطيران 📝

### إنشاء حجز طيران جديد
```http
POST /api/flightbookings
Content-Type: application/json

{
  "flightId": 1,
  "passengerName": "أحمد محمد علي",
  "passengerEmail": "ahmed@example.com",
  "passengerPhone": "0912345678",
  "passportNumber": "SD123456789",
  "numberOfSeats": 2
}
```

**استجابة:**
```json
{
  "id": 1,
  "flightId": 1,
  "passengerName": "أحمد محمد علي",
  "passengerEmail": "ahmed@example.com",
  "passengerPhone": "0912345678",
  "passportNumber": "SD123456789",
  "numberOfSeats": 2,
  "totalPrice": 30000,
  "bookingDate": "2026-04-02T22:30:00",
  "status": "Confirmed",
  "bookingReference": "FLT202604022230001234"
}
```

### البحث عن حجز برقم الحجز
```http
GET /api/flightbookings/reference/FLT202604022230001234
```

### الحصول على جميع الحجوزات
```http
GET /api/flightbookings
```

### إلغاء حجز طيران
```http
PUT /api/flightbookings/1/cancel
```

---

## 4. حجوزات الفنادق 🏨

### إنشاء حجز فندق جديد
```http
POST /api/hotelbookings
Content-Type: application/json

{
  "hotelId": 1,
  "guestName": "فاطمة أحمد",
  "guestEmail": "fatima@example.com",
  "guestPhone": "0923456789",
  "checkInDate": "2026-05-01",
  "checkOutDate": "2026-05-05",
  "numberOfRooms": 2,
  "numberOfGuests": 4
}
```

**استجابة:**
```json
{
  "id": 1,
  "hotelId": 1,
  "guestName": "فاطمة أحمد",
  "guestEmail": "fatima@example.com",
  "guestPhone": "0923456789",
  "checkInDate": "2026-05-01T00:00:00",
  "checkOutDate": "2026-05-05T00:00:00",
  "numberOfRooms": 2,
  "numberOfGuests": 4,
  "totalPrice": 400000,
  "bookingDate": "2026-04-02T22:35:00",
  "status": "Confirmed",
  "bookingReference": "HTL202604022235005678"
}
```

### البحث عن حجز فندق برقم الحجز
```http
GET /api/hotelbookings/reference/HTL202604022235005678
```

### إلغاء حجز فندق
```http
PUT /api/hotelbookings/1/cancel
```

---

## 5. المعالم التقنية 🏛️

### الحصول على جميع المعالم
```http
GET /api/touristattractions
```

**استجابة:**
```json
[
  {
    "id": 1,
    "name": "أهرامات مروي",
    "city": "مروي",
    "description": "أهرامات نوبية قديمة تعود للمملكة الكوشية",
    "category": "آثار تاريخية",
    "imageUrl": "/images/meroe-pyramids.jpg",
    "entryFee": 5000,
    "openingHours": "8:00 ص - 5:00 م"
  }
]
```

### معالم مدينة محددة
```http
GET /api/touristattractions/city/الخرطوم
```

### معالم حسب الفئة
```http
GET /api/touristattractions/category/آثار تاريخية
```

---

## أمثلة باستخدام cURL

### حجز خدمة طيران
```bash
curl -X POST https://localhost:7086/api/flightbookings \
  -H "Content-Type: application/json" \
  -d '{
    "flightId": 1,
    "passengerName": "محمد أحمد",
    "passengerEmail": "mohamed@example.com",
    "passengerPhone": "0912345678",
    "passportNumber": "SD987654",
    "numberOfSeats": 1
  }'
```

### البحث عن فنادق
```bash
curl https://localhost:7086/api/hotels/search?city=الخرطوم&minRating=5
```

---

## أمثلة باستخدام PowerShell

### حجز فندق
```powershell
$body = @{
    hotelId = 1
    guestName = "سارة محمد"
    guestEmail = "sara@example.com"
    guestPhone = "0934567890"
    checkInDate = "2026-06-01"
    checkOutDate = "2026-06-03"
    numberOfRooms = 1
    numberOfGuests = 2
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:7086/api/hotelbookings" `
    -Method Post `
    -Body $body `
    -ContentType "application/json"
```

### الحصول على جميع الخدمات
```powershell
Invoke-RestMethod -Uri "https://localhost:7086/api/flights" -Method Get
```

---

## ملاحظات مهمة 📝

1. **رقم المنفذ**: قد يختلف رقم المنفذ (7086) حسب إعدادات Visual Studio
2. **HTTPS**: التطبيق يستخدم HTTPS بشكل افتراضي
3. **أرقام الحجز**: يتم توليدها تلقائياً بالتنسيق:
   - حجوزات الطيران: `FLT{timestamp}{random}`
   - حجوزات الفنادق: `HTL{timestamp}{random}`
4. **السعر الإجمالي**: يتم حسابه تلقائياً:
   - الطيران: `سعر الخدمة × عدد المقاعد`
   - الفندق: `سعر الليلة × عدد الغرف × عدد الليالي`
