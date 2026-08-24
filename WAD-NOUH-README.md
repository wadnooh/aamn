# 🇸🇩 ود نوح للبرمجيات والكمبيوتر
# Wad Nouh Software & Computer

<div dir="rtl">

## 📖 نبذة عن ود نوح

**ود نوح للبرمجيات والكمبيوتر** هو نظام متكامل لبرمجة المواقع والتطبيقات وصيانة الكمبيوتر وخدمات الكهرباء والإلكترونيات في السودان. 
تم بناؤه باستخدام أحدث التقنيات ASP.NET Core 10.0 مع واجهة ويب عربية تفاعلية وسهلة الاستخدام.

---

## ✨ المميزات

### 🛫 حجز الخدمات الجوية
- **13 خدمة** بين المدن السودانية
- البحث المتقدم عن خدمات
- عرض تفاصيل الخدمات (الوقت، السعر، المقاعد المتاحة)
- طلب خدمات مباشر
- توليد رقم حجز تلقائي
- إلغاء الحجوزات

### 🏨 حجز الفنادق
- **12 فندق** من 3 إلى 5 نجوم
- تصفح الفنادق في مختلف المدن
- البحث حسب المدينة والتقييم والسعر
- حجز الغرف مع حساب تلقائي للتكلفة
- إدارة كاملة للحجوزات

### 🏛️ المعالم التقنية
- **18 معلم تقني** متنوع:
  - 6 مواقع أثرية تاريخية
  - 7 معالم طبيعية
  - 2 متحف
  - 2 أسواق ومعالم دينية
- استكشاف المعالم التاريخية والطبيعية
- البحث حسب المدينة أو الفئة
- معلومات تفصيلية عن كل معلم
- أسعار الدخول ومواعيد العمل

---

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
│   └── TravelDbContext.cs (43 سجل بيانات واقعية)
├── wwwroot/                  # Static Files
│   ├── index.html (الصفحة الرئيسية)
│   └── guide.html (الدليل التقني)
├── Program.cs                # App Configuration
└── SudanTravelApp.API.csproj
```

---

## 🚀 التشغيل السريع

### الطريقة 1: استخدام Visual Studio (موصى به)

1. **افتح المشروع**: `SudanTravelApp.API\SudanTravelApp.API.csproj`
2. **اضغط**: `F5` أو زر "Start" الأخضر
3. **سيفتح تلقائياً على**: `https://localhost:7086`

### الطريقة 2: سطر الأوامر

```powershell
cd SudanTravelApp.API
dotnet restore
dotnet run
```

### الطريقة 3: Scripts السريعة

**PowerShell:**
```powershell
.\RUN.ps1
```

**Command Prompt:**
```cmd
RUN.bat
```

---

## 📊 البيانات الأولية

التطبيق يأتي مع **43 سجل** من البيانات الواقعية:

### ✈️ الخدمات الجوية (13 خدمة)

#### شركات الطيران:
- الخطوط الجوية السودانية (9 خدمات)
- بدر للطيران (2 خدمة)
- تاركو للطيران (2 خدمة)

#### المسارات:
- الخرطوم ↔ بورتسودان
- الخرطوم → نيالا
- الخرطوم → الأبيض
- الخرطوم → الفاشر
- الخرطوم → كسلا
- الخرطوم → دنقلا
- الخرطوم → جوبا
- الخرطوم → أسوان
- الخرطوم → القضارف
- الخرطوم → وادي حلفا

### 🏨 الفنادق (12 فندق)

| الفندق | المدينة | التقييم | السعر/الليلة |
|--------|---------|---------|---------------|
| فندق كورنثيا الخرطوم | الخرطوم | ⭐⭐⭐⭐⭐ | 50,000 جنيه |
| فندق السلام روتانا | الخرطوم | ⭐⭐⭐⭐⭐ | 45,000 جنيه |
| فندق غراند هوليداي فيلا | الخرطوم | ⭐⭐⭐⭐ | 35,000 جنيه |
| فندق هيلتون الخرطوم | الخرطوم | ⭐⭐⭐⭐ | 38,000 جنيه |
| فندق أكروبول | أم درمان | ⭐⭐⭐ | 25,000 جنيه |
| فندق بورتسودان | بورتسودان | ⭐⭐⭐⭐ | 32,000 جنيه |
| ريد سي ريزورت | بورتسودان | ⭐⭐⭐ | 28,000 جنيه |
| فندق مروي الهرمي | مروي | ⭐⭐⭐ | 24,000 جنيه |
| فندق سلام نيالا | نيالا | ⭐⭐⭐ | 20,000 جنيه |
| فندق توتيل كسلا | كسلا | ⭐⭐⭐ | 22,000 جنيه |
| فندق النوبة | دنقلا | ⭐⭐⭐ | 21,000 جنيه |
| فندق الأبيض | الأبيض | ⭐⭐⭐ | 19,000 جنيه |

### 🏛️ المعالم التقنية (18 معلم)

#### آثار تاريخية (6):
1. أهرامات مروي - موقع تراث عالمي لليونسكو
2. جبل البركل - معابد فرعونية
3. معبد صلب - بناء أمنحتب الثالث
4. الكرو - مقابر ملكية نوبية
5. النقعة - معبد الأسد
6. المصورات الصفراء - حضارة مروية

#### متاحف (2):
7. المتحف القومي السوداني
8. بيت الخليفة

#### طبيعة (7):
9. البحر الأحمر - غوص عالمي
10. سنقنيب - محمية بحرية
11. ملتقى النيلين
12. جبل مرة - بركان خامد
13. محمية الدندر
14. جبل التاكا
15. الشلال الرابع
16. توتي آيلاند

#### أسواق ومعالم (3):
17. سوق أم درمان
18. قبة المهدي

---

## 🌐 الواجهات والصفحات

### بعد التشغيل ستتوفر الصفحات التالية:

- **الصفحة الرئيسية**: `https://localhost:7086`
  - حجز الخدمات
  - حجز الفنادق
  - المعالم التقنية
  - إدارة الحجوزات

- **الدليل التقني**: `https://localhost:7086/guide.html`
  - نظرة عامة عن السودان
  - المعالم التقنية المفصلة
  - المدن الرئيسية
  - الثقافة والتراث
  - معلومات عملية

- **API**: `https://localhost:7086/api`

---

## 🔌 نقاط نهاية API

### الخدمات
```
GET    /api/flights                        - جميع الخدمات
GET    /api/flights/{id}                   - خدمة محددة
GET    /api/flights/search?from=...&to=... - البحث
POST   /api/flights                        - إضافة خدمة
PUT    /api/flights/{id}                   - تحديث
DELETE /api/flights/{id}                   - حذف
```

### الفنادق
```
GET    /api/hotels                         - جميع الفنادق
GET    /api/hotels/{id}                    - فندق محدد
GET    /api/hotels/search?city=...         - البحث
POST   /api/hotels                         - إضافة
PUT    /api/hotels/{id}                    - تحديث
DELETE /api/hotels/{id}                    - حذف
```

### حجوزات الطيران
```
GET    /api/flightbookings                 - جميع الحجوزات
GET    /api/flightbookings/{id}            - حجز محدد
GET    /api/flightbookings/reference/{ref} - بالرقم المرجعي
POST   /api/flightbookings                 - حجز جديد
PUT    /api/flightbookings/{id}/cancel     - إلغاء
```

### حجوزات الفنادق
```
GET    /api/hotelbookings                  - جميع الحجوزات
GET    /api/hotelbookings/{id}             - حجز محدد
GET    /api/hotelbookings/reference/{ref}  - بالرقم المرجعي
POST   /api/hotelbookings                  - حجز جديد
PUT    /api/hotelbookings/{id}/cancel      - إلغاء
```

### المعالم التقنية
```
GET    /api/touristattractions             - جميع المعالم
GET    /api/touristattractions/{id}        - معلم محدد
GET    /api/touristattractions/city/{city} - حسب المدينة
GET    /api/touristattractions/category/{cat} - حسب الفئة
```

---

## 🛠️ التقنيات المستخدمة

- **Backend**: ASP.NET Core 10.0 Web API
- **Database**: Entity Framework Core (In-Memory)
- **Frontend**: HTML5, CSS3, JavaScript (Vanilla)
- **API Style**: RESTful
- **Language**: C# 12.0

---

## ⚙️ المتطلبات

- .NET 10.0 SDK أو أحدث
- Visual Studio 2026 أو VS Code (اختياري)
- Windows 10/11 أو Linux/macOS

---

## 📚 التوثيق

- **FINAL-SUMMARY.md** - ملخص شامل للمشروع
- **SUDAN-TOURISM-GUIDE.md** - دليل تقني مفصّل
- **READY-TO-RUN.md** - دليل التشغيل
- **API-EXAMPLES.md** - أمثلة API الكاملة

---

## 🎓 مثالي للتعلم

المشروع يغطي:
- ✅ ASP.NET Core 10.0 Web API
- ✅ Entity Framework Core
- ✅ RESTful API Design
- ✅ In-Memory Database
- ✅ CORS Configuration
- ✅ بناء واجهات عربية
- ✅ أنظمة الحجز الإلكتروني

---

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

---

## 📞 الدعم

للأسئلة والدعم:
- راجع ملفات التوثيق
- اطلع على `READY-TO-RUN.md`
- اقرأ `FINAL-SUMMARY.md`

---

<div align="center">

## 🇸🇩 ود نوح للبرمجيات والكمبيوتر

**نظام متكامل لخدمة التقنية في السودان**

![Made with Love](https://img.shields.io/badge/Made%20with-Love-red)
![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-blue)
![Status](https://img.shields.io/badge/Status-Ready-success)

**🚀 جاهز للتشغيل الآن!**

</div>

</div>
