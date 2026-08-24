# نشر ود نوح على Hostinger

## مهم
استضافة Hostinger المشتركة **لا تشغّل ASP.NET**.
لذلك النشر على Hostinger يكون كالتالي:
- واجهة الموقع على `wadnooh.com` (Hostinger)
- الـ API يعمل عبر السيرفر الحي الحالي (Cloudflare tunnel / لاحقاً VPS)

## الحزم الجاهزة
| الملف | الاستخدام |
|------|-----------|
| `publish/hostinger-site.zip` | الموقع الكامل (واجهة + ربط API حي) |
| `publish/hostinger-redirect.zip` | تحويل فوري من الدومين للتطبيق الحي |

## الطريقة 1: File Manager (الأسرع)
1. افتح [hPanel](https://hpanel.hostinger.com/)
2. Websites → **wadnooh.com** → **File Manager**
3. ادخل `public_html`
4. احذف صفحة Parked الافتراضية إن وجدت
5. ارفع `d:\FLY\publish\hostinger-site.zip`
6. Extract داخل `public_html`
7. تأكد أن `index.html` و `.htaccess` في الجذر
8. افتح https://wadnooh.com

## الطريقة 2: API Token
1. من hPanel: Profile → API → Create token
2. نفّذ:
```powershell
$env:HOSTINGER_API_TOKEN="YOUR_TOKEN"
powershell -ExecutionPolicy Bypass -File d:\FLY\deploy\hostinger-deploy.ps1
```

## الطريقة 3: VPS (تشغيل ASP.NET كاملاً على Hostinger)
إذا عندك VPS:
```bash
# على السيرفر
docker compose up -d --build
# ثم اربط الدومين A record إلى IP الـ VPS
```

## التحقق
- https://wadnooh.com/
- يجب أن تظهر واجهة ود نوح وتبحث عن خدمات تقنية
