# 🇸🇩 تطبيق البرمجيات والكمبيوتر السوداني
# Wad Nooh Software & Computer Sitelication

<div dir="rtl">

## 📖 نظرة عامة

تطبيق ويب متكامل لبرمجة المواقع والتطبيقات وصيانة الكمبيوتر وخدمات الكهرباء والإلكترونيات في السودان. 
تم بناؤه باستخدام ASP.NET Core 10.0 مع واجهة ويب عربية تفاعلية.

## ✨ المميزات

### 🛫 حجز الخدمات الجوية
- البحث عن خدمات بين المدن السودانية
- عرض تفاصيل الخدمات (الوقت، السعر، المقاعد المتاحة)
- طلب خدمات البرمجة والكمبيوتر
- توليد رقم حجز تلقائي
- إلغاء الحجوزات

### 🏨 حجز الفنادق
- تصفح الفنادق في مختلف المدن
- البحث حسب المدينة والتقييم والسعر
- حجز الغرف الفندقية
- حساب تلقائي للتكلفة الإجمالية
- إدارة الحجوزات

### 🏛️ المعالم التقنية
- استكشاف المعالم التاريخية والتقنية
- البحث حسب المدينة أو الفئة
- معلومات تفصيلية عن كل معلم
- أسعار الدخول ومواعيد العمل

## 🗂️ هيكل المشروع

```
SudanTravelApp.API/
├── Controllers/              # API Controllers
│   ├── FlightsController.cs
│   ├── HotelsController.cs
│   ├── FlightBookingsController.cs
│   ├── HotelBookingsController.cs
│   └── TouristAttractionsController.cs
├── Models/                   # Data Models
│   ├── Flight.cs
│   ├── Hotel.cs
│   ├── FlightBooking.cs
│   ├── HotelBooking.cs
│   └── TouristAttraction.cs
├── Data/                     # Database Context
│   └── TravelDbContext.cs
├── wwwroot/                  # Static Files
│   └── index.html
├── Program.cs                # App Configuration
└── SudanTravelApp.API.csproj
```

## 🚀 التشغيل السريع

### الطريقة 1: استخدام Script (موصى به)

**Windows (PowerShell):**
```powershell
.\RUN.ps1
```

**Windows (Command Prompt):**
```cmd
RUN.bat
```

### الطريقة 2: Visual Studio

1. افتح `SudanTravelApp.API.csproj`
2. اضغط `F5` أو زر "Start"
3. سيفتح المتصفح تلقائياً

### الطريقة 3: سطر الأوامر

```powershell
cd SudanTravelApp.API
dotnet restore
dotnet run
```

## 📊 البيانات الأولية

يأتي التطبيق مع بيانات تجريبية جاهزة:

### الخدمات الجوية (5 خدمات)
- الخرطوم ↔ بورتسودان (15,000 جنيه)
- الخرطوم → نيالا (18,000 جنيه)
- الخرطوم → الأبيض (12,000 جنيه)
- الخرطوم → الفاشر (16,000 جنيه)

### الفنادق (5 فنادق)
| الفندق | المدينة | التقييم | السعر/الليلة |
|--------|---------|---------|---------------|
| فندق كورنثيا | الخرطوم | ⭐⭐⭐⭐⭐ | 50,000 جنيه |
| فندق السلام روتانا | الخرطوم | ⭐⭐⭐⭐⭐ | 45,000 جنيه |
| فندق بورتسودان | بورتسودان | ⭐⭐⭐⭐ | 30,000 جنيه |
| فندق مروي | مروي | ⭐⭐⭐ | 25,000 جنيه |
| فندق نيالا | نيالا | ⭐⭐⭐ | 20,000 جنيه |

### المعالم التقنية (6 معالم)
- 🏛️ أهرامات مروي النوبية
- 🏺 متحف السودان القومي
- 🌊 البحر الأحمر
- 🌊 ملتقى النيلين
- 🛍️ سوق أم درمان
- ⛰️ جبل البركل

## 🔌 نقاط نهاية API

### الخدمات
```
GET    /api/flights
GET    /api/flights/{id}
GET    /api/flights/search?from={city}&to={city}&date={date}
POST   /api/flights
PUT    /api/flights/{id}
DELETE /api/flights/{id}
```

### الفنادق
```
GET    /api/hotels
GET    /api/hotels/{id}
GET    /api/hotels/search?city={city}&minRating={rating}&maxPrice={price}
POST   /api/hotels
PUT    /api/hotels/{id}
DELETE /api/hotels/{id}
```

### حجوزات الطيران
```
GET    /api/flightbookings
GET    /api/flightbookings/{id}
GET    /api/flightbookings/reference/{ref}
POST   /api/flightbookings
PUT    /api/flightbookings/{id}/cancel
```

### حجوزات الفنادق
```
GET    /api/hotelbookings
GET    /api/hotelbookings/{id}
GET    /api/hotelbookings/reference/{ref}
POST   /api/hotelbookings
PUT    /api/hotelbookings/{id}/cancel
```

### المعالم التقنية
```
GET    /api/touristattractions
GET    /api/touristattractions/{id}
GET    /api/touristattractions/city/{city}
GET    /api/touristattractions/category/{category}
```

## 📝 أمثلة الاستخدام

### حجز خدمة طيران

**طلب:**
```http
POST /api/flightbookings
Content-Type: application/json

{
  "flightId": 1,
  "passengerName": "أحمد محمد علي",
  "passengerEmail": "ahmed@example.com",
  "passengerPhone": "0912345678",
  "passportNumber": "SD123456",
  "numberOfSeats": 2
}
```

**استجابة:**
```json
{
  "id": 1,
  "bookingReference": "FLT202604030000001234",
  "totalPrice": 30000,
  "status": "Confirmed",
  "bookingDate": "2026-04-03T10:30:00"
}
```

### حجز فندق

**طلب:**
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

## 🌐 الواجهة الرسومية

افتح المتصفح على: `https://localhost:7086`

### المميزات:
- ✅ تصميم عربي من اليمين لليسار
- ✅ واجهة تفاعلية وسهلة الاستخدام
- ✅ نماذج بحث متقدمة
- ✅ عرض جميل للبيانات
- ✅ تصميم متجاوب (Responsive)

## 🛠️ التقنيات المستخدمة

- **Backend**: ASP.NET Core 10.0 Web API
- **Database**: Entity Framework Core (In-Memory)
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **API Style**: RESTful
- **Language**: C# 12.0

## ⚙️ المتطلبات

- .NET 10.0 SDK أو أحدث
- Visual Studio 2026 أو VS Code (اختياري)
- Windows 10/11

## 🔧 حل المشاكل الشائعة

### مشكلة: خطأ في NuGet Lock File

**الحل:**
```powershell
# أغلق Visual Studio أولاً
Remove-Item -Path "C:\Windows\Temp\NuGetScratch\lock" -Force
dotnet nuget locals all --clear
# أعد فتح Visual Studio
```

### مشكلة: فشل استعادة الحزم

**الحل:**
```powershell
dotnet nuget locals all --clear
dotnet restore --force --no-cache
```

### مشكلة: المنفذ مستخدم بالفعل

**الحل:**
غيّر المنفذ في `Properties/launchSettings.json`

## 📚 الملفات التوثيقية

- `START-HERE.md` - ابدأ من هنا!
- `INSTRUCTIONS.md` - تعليمات مفصلة
- `API-EXAMPLES.md` - أمثلة API كاملة
- `RUN.ps1` - Script تشغيل PowerShell
- `RUN.bat` - Script تشغيل CMD

## 🔮 التطوير المستقبلي

- [ ] نظام المصادقة والتفويض (JWT)
- [ ] بوابة الدفع الإلكتروني
- [ ] إشعارات البريد الإلكتروني
- [ ] تقييمات وتعليقات المستخدمين
- [ ] خرائط تفاعلية
- [ ] تطبيق موبايل (iOS & Android)
- [ ] لوحة تحكم للمدراء
- [ ] تقارير وإحصائيات
- [ ] دعم متعدد اللغات
- [ ] نظام الولاء والمكافآت

## 📄 الترخيص

هذا المشروع مفتوح المصدر ومتاح للاستخدام التعليمي والتجاري.

## 🤝 المساهمة

نرحب بمساهماتكم! للمساهمة:

1. Fork المشروع
2. أنشئ branch جديد (`git checkout -b feature/AmazingFeature`)
3. Commit التغييرات (`git commit -m 'إضافة ميزة رائعة'`)
4. Push إلى Branch (`git push origin feature/AmazingFeature`)
5. افتح Pull Request

## 📞 الدعم

للأسئلة والدعم:
- افتح Issue في GitHub
- راجع ملفات التوثيق
- اطلع على `API-EXAMPLES.md`

## 👨‍💻 المطور

تم التطوير بواسطة GitHub Copilot

---

<div align="center">

**صُنع بـ ❤️ للسودان 🇸🇩**

![Made with Love](https://img.shields.io/badge/Made%20with-Love-red)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-blue)
![Status](https://img.shields.io/badge/Status-Active-success)

</div>

</div>
