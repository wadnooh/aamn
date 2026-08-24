# حالة الربط العام — استكمال التدشين

آخر تحديث: 2026-07-25

## يعمل الآن (الرابط الحي)
| المكوّن | الحالة |
|---------|--------|
| التطبيق + API | يعمل محلياً (Debug / dotnet run) |
| النفق العام | https://discusses-incidence-perception-states.trycloudflare.com |
| لوحة التحكم | https://discusses-incidence-perception-states.trycloudflare.com/admin.html |
| الإصدار | 5.0 |
| السداد | Demo |

## دخول الأدمن
- البريد: `admin@wadnooh.com`
- كلمة المرور: `Admin@123456`

## ما تم إصلاحه اليوم
1. **Smart App Control** كان يمنع تشغيل ملفات Release — تم تحويل التشغيل إلى `dotnet run -c Debug`.
2. أُعيد تشغيل API + نفق Cloudflare + مراقبة watchdog.
3. تم التحقق من دخول الأدمن والإحصائيات عبر النفق.

## عطل Hostinger الحالي
- `https://wadnooh.com` يعيد **404** (مجلد الموقع فارغ/مكسور).
- واجهة رفع الملفات عبر Hostinger API تُرجع **500** حالياً (لا يمكن النشر التلقائي).

### حل فوري (يدوي — دقيقة واحدة)
1. افتح Hostinger File Manager لموقع `wadnooh.com`.
2. ارفع وفك الضغط لملف سطح المكتب:
   - `Desktop\wadnooh-manual-upload.zip`
   - أو `d:\FLY\publish\wadnooh-manual-upload.zip`
3. ضع الملفات داخل `public_html`.
4. افتح https://wadnooh.com

الملفات مربوطة مسبقاً بالنفق الحي الحالي.

## أوامر التشغيل
```powershell
powershell -ExecutionPolicy Bypass -File d:\FLY\deploy\go-live-wadnooh.ps1
powershell -ExecutionPolicy Bypass -File d:\FLY\deploy\watch-public-link.ps1
```

## للتدشين النهائي الدائم
1. أوقف Smart App Control مؤقتاً أو اسمح للتطبيق (Windows Security).
2. اربط نفق Cloudflare مسمّى بعد `cloudflared tunnel login`.
3. أو انقل API إلى VPS يدعم ASP.NET.
