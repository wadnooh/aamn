# تعليمات تشغيل تطبيق البرمجيات والكمبيوتر السوداني 🇸🇩

## الملفات المنشأة ✅

تم إنشاء المشروع بنجاح ويحتوي على:

### 1. Models (نماذج البيانات)
- `Flight.cs` - نموذج الخدمة الجوية
- `Hotel.cs` - نموذج الفندق  
- `FlightBooking.cs` - نموذج حجز الطيران
- `HotelBooking.cs` - نموذج حجز الفندق
- `TouristAttraction.cs` - نموذج المعلم التقني

### 2. Controllers (واجهات API)
- `FlightsController.cs` - إدارة الخدمات
- `HotelsController.cs` - إدارة الفنادق
- `FlightBookingsController.cs` - حجوزات الطيران
- `HotelBookingsController.cs` - حجوزات الفنادق
- `TouristAttractionsController.cs` - المعالم التقنية

### 3. Data (قاعدة البيانات)
- `TravelDbContext.cs` - سياق قاعدة البيانات مع بيانات أولية

### 4. UI (واجهة المستخدم)
- `wwwroot/index.html` - صفحة ويب تفاعلية كاملة

### 5. Configuration
- `Program.cs` - تكوين التطبيق
- `SudanTravelApp.API.csproj` - ملف المشروع

## خطوات التشغيل 🚀

### الطريقة 1: من خلال Visual Studio

1. افتح ملف `SudanTravelApp.API.csproj` في Visual Studio 2026
2. انتظر حتى يتم استعادة الحزم (NuGet Restore) تلقائياً
3. اضغط F5 أو اضغط على زر "Start" الأخضر
4. سيفتح المتصفح تلقائياً على `https://localhost:XXXX`

### الطريقة 2: من خلال سطر الأوامر

```powershell
# انتقل إلى مجلد المشروع
cd C:\Users\wadno\Desktop\FLY

# استعادة الحزم
dotnet restore SudanTravelApp.API\SudanTravelApp.API.csproj

# بناء المشروع
dotnet build SudanTravelApp.API\SudanTravelApp.API.csproj

# تشغيل التطبيق
dotnet run --project SudanTravelApp.API\SudanTravelApp.API.csproj
```

### حل مشكلة NuGet Lock (إذا واجهتك)

إذا ظهرت مشكلة مع NuGet Lock File:

```powershell
# أغلق Visual Studio أولاً
# ثم نفذ هذا الأمر بصلاحيات المسؤول
Remove-Item -Path "C:\Windows\Temp\NuGetScratch\lock" -Force -ErrorAction SilentlyContinue

# أعد فتح Visual Studio وجرب مرة أخرى
```

## البيانات الأولية 📊

التطبيق يأتي مع بيانات تجريبية جاهزة:

### الخدمات الجوية (5 خدمات)
- الخرطوم ↔ بورتسودان
- الخرطوم → نيالا
- الخرطوم → الأبيض
- الخرطوم → الفاشر

### الفنادق (5 فنادق)
- فندق كورنثيا (الخرطوم) - 5 نجوم
- فندق السلام روتانا (الخرطوم) - 5 نجوم
- فندق بورتسودان - 4 نجوم
- فندق مروي - 3 نجوم
- فندق نيالا - 3 نجوم

### المعالم التقنية (6 معالم)
- أهرامات مروي النوبية
- متحف السودان القومي
- البحر الأحمر
- ملتقى النيلين
- سوق أم درمان
- جبل البركل

## اختبار التطبيق 🧪

### 1. اختبار API مباشرة

استخدم متصفحك أو Postman:

```
GET https://localhost:7086/api/flights
GET https://localhost:7086/api/hotels
GET https://localhost:7086/api/touristattractions
```

### 2. استخدام الواجهة الرسومية

افتح: `https://localhost:7086/index.html`

الواجهة تحتوي على:
- علامات تبويب للخدمات، الفنادق، المعالم التقنية
- نماذج بحث متقدمة
- عرض جميل للبيانات
- دعم كامل للغة العربية

### 3. البحث عن خدمة

```
GET https://localhost:7086/api/flights/search?from=الخرطوم&to=بورتسودان
```

### 4. حجز خدمة

```
POST https://localhost:7086/api/flightbookings
Content-Type: application/json

{
  "flightId": 1,
  "passengerName": "أحمد محمد",
  "passengerEmail": "ahmed@example.com",
  "passengerPhone": "0912345678",
  "passportNumber": "SD123456",
  "numberOfSeats": 2
}
```

## المميزات الرئيسية ⭐

✅ API RESTful كاملة
✅ قاعدة بيانات In-Memory (لا تحتاج إعداد)
✅ واجهة ويب تفاعلية بالعربية
✅ بيانات تجريبية جاهزة
✅ دعم CORS للتطوير
✅ نظام حجوزات كامل
✅ توليد أرقام حجز تلقائياً
✅ إلغاء الحجوزات
✅ البحث والفلترة المتقدمة

## المنافذ الافتراضية 🔌

- HTTPS: `https://localhost:7086`
- HTTP: `http://localhost:5000` (إذا كان مفعّلاً)

## نصائح مهمة 💡

1. **التطوير**: استخدم In-Memory Database (مفعلة حالياً)
2. **الإنتاج**: غيّر إلى SQL Server أو PostgreSQL
3. **الأمان**: أضف Authentication و Authorization قبل النشر
4. **الأداء**: أضف Caching للبيانات المتكررة
5. **الاختبار**: أضف Unit Tests و Integration Tests

## المشاكل الشائعة وحلولها ❓

### المشكلة: لا يعمل التطبيق بعد التشغيل
**الحل**: تأكد من تثبيت .NET 10 SDK

### المشكلة: خطأ في استعادة الحزم
**الحل**: 
```powershell
dotnet nuget locals all --clear
dotnet restore --force
```

### المشكلة: الواجهة لا تعرض البيانات
**الحل**: تأكد من تغيير رقم المنفذ في `index.html` إلى المنفذ الصحيح

## التطوير المستقبلي 🔮

- [ ] إضافة نظام المصادقة (JWT)
- [ ] ربط بوابة دفع إلكتروني
- [ ] إرسال تأكيدات بالبريد الإلكتروني
- [ ] تقييمات وتعليقات المستخدمين
- [ ] خرائط تفاعلية
- [ ] تطبيق موبايل
- [ ] لوحة تحكم للمدراء

---

**تم التطوير بنجاح! ✨**

للدعم: افتح Issue في GitHub
