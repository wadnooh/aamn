# 🚀 دليل التشغيل - تطبيق السفر السوداني

## ⚠️ مشكلة NuGet Lock File

إذا واجهت خطأ:
```
Unable to obtain lock file access on 'C:\Windows\Temp\NuGetScratch\lock'
```

## ✅ الحلول المتاحة (مرتبة حسب السهولة)

---

### 🥇 الحل الأول: استخدام Visual Studio (الأسهل)

**هذا هو الحل الأسرع والأضمن!**

1. افتح Visual Studio
2. افتح ملف `SudanTravelApp.slnx`
3. اضغط **F5** مباشرة

✨ **سيعمل مباشرة!** Visual Studio سيتعامل مع NuGet تلقائياً.

---

### 🥈 الحل الثاني: استعادة الحزم في Visual Studio

إذا لم يعمل F5 مباشرة:

1. في **Solution Explorer**
2. انقر بزر الماوس الأيمن على **Solution 'SudanTravelApp'**
3. اختر **"Restore NuGet Packages"**
4. انتظر حتى ينتهي
5. اضغط **F5**

---

### 🥉 الحل الثالث: PowerShell كمسؤول

1. انقر بزر الماوس الأيمن على ملف `run-app.ps1`
2. اختر **"Run with PowerShell"** أو **"Open with PowerShell"**
3. إذا طلب صلاحيات، اضغط "Yes"

---

### 🔧 الحل الرابع: حذف Lock File يدوياً

افتح **PowerShell كمسؤول** (Run as Administrator) وانسخ والصق:

```powershell
# حذف ملف القفل
Remove-Item "C:\Windows\Temp\NuGetScratch\lock" -Force -ErrorAction SilentlyContinue

# الانتقال للمشروع
cd "C:\Users\wadno\Desktop\FLY\SudanTravelApp.API"

# تنظيف
dotnet clean

# استعادة الحزم
dotnet restore --force-evaluate

# بناء
dotnet build

# تشغيل
dotnet run
```

---

### 🛠️ الحل الخامس: إعادة تشغيل Visual Studio كمسؤول

1. أغلق Visual Studio تماماً
2. انقر بزر الماوس الأيمن على أيقونة Visual Studio
3. اختر **"Run as Administrator"**
4. افتح المشروع
5. اضغط **F5**

---

## 📡 بعد التشغيل الناجح

سترى في Console:
```
Now listening on: http://localhost:5000
Now listening on: https://localhost:5001
```

### اختبر API:

افتح المتصفح واذهب إلى:

#### 1. الخدمات الجوية
```
http://localhost:5000/api/flights
```

#### 2. الفنادق
```
http://localhost:5000/api/hotels
```

#### 3. المعالم التقنية
```
http://localhost:5000/api/touristattractions
```

#### 4. حجوزات الخدمات
```
http://localhost:5000/api/flightbookings
```

#### 5. حجوزات الفنادق
```
http://localhost:5000/api/hotelbookings
```

---

## 🧪 اختبار API بأمثلة عملية

### البحث عن خدمات من الخرطوم
```
http://localhost:5000/api/flights/search?from=الخرطوم
```

### البحث عن فنادق في الخرطوم
```
http://localhost:5000/api/hotels/search?city=الخرطوم
```

### المعالم التقنية في مروي
```
http://localhost:5000/api/touristattractions/city/مروي
```

---

## 💡 ملاحظات

- **العملة**: جنيه سوداني (SDG)
- **قاعدة البيانات**: SQLite (ملف `SudanTravel.db`)
- **البيانات الأولية**: يتم إنشاؤها تلقائياً عند أول تشغيل

---

## 🆘 إذا استمرت المشكلة

أرسل screenshot للخطأ أو اتصل بالدعم الفني.

---

**تم بناؤه بـ ❤️ للسودان 🇸🇩**
