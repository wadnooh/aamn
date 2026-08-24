# ربط التطبيق بالدومين wadnooh.com

## الحالة الحالية
- الدومين `wadnooh.com` و `www.wadnooh.com` يشيران إلى IP: **2.57.91.91**
- الاستضافة: **Hostinger** (الصفحة الحالية Parked Domain)
- التطبيق جاهز للإنتاج على هذا الدومين

## 1) إعدادات DNS في Hostinger

من لوحة Hostinger → Domains → wadnooh.com → DNS / Nameservers:

| Type | Name | Value | TTL |
|------|------|-------|-----|
| A | @ | IP السيرفر الذي يشغّل التطبيق | 300 |
| A | www | نفس IP السيرفر | 300 |
| CNAME | www | wadnooh.com | 300 |

> إذا بقي الدومين على Hostinger Shared Hosting بدون دعم ASP.NET، انقل الموقع إلى **VPS** أو **Cloud** يشغّل .NET، أو استخدم Docker.

## 2) نشر سريع (Windows / IIS)

```powershell
cd d:\FLY
.\deploy\publish-wadnooh.ps1
```

ثم:
1. ارفع محتويات `publish\wadnooh` أو الملف `publish\wadnooh-site.zip`
2. في IIS أنشئ موقع:
   - Site name: `wadnooh`
   - Binding: `wadnooh.com` و `www.wadnooh.com` على المنفذ 443
   - Physical path: مجلد النشر
3. ثبّت **ASP.NET Core Hosting Bundle**
4. فعّل شهادة SSL (Let's Encrypt / Hostinger SSL)

## 3) نشر بـ Docker (موصى به على VPS)

على السيرفر:

```bash
git clone <repo> /opt/wadnooh   # أو ارفع الملفات
cd /opt/wadnooh
docker compose up -d --build
```

ثم اربط Nginx:

```bash
sudo cp deploy/nginx-wadnooh.com.conf /etc/nginx/sites-available/wadnooh.com
sudo ln -s /etc/nginx/sites-available/wadnooh.com /etc/nginx/sites-enabled/
sudo certbot --nginx -d wadnooh.com -d www.wadnooh.com
sudo nginx -t && sudo systemctl reload nginx
```

التطبيق يستمع داخلياً على المنفذ `5162`، وNginx يمرّر من `https://wadnooh.com`.

## 4) مفاتيح الإنتاج (اختيارية)

على السيرفر عيّن:

```bash
export FlightProvider__DuffelApiKey="duffel_live_..."
export AI__Provider="openai"
export AI__OpenAiApiKey="sk-..."
export PublicBaseUrl="https://wadnooh.com"
export ASPNETCORE_ENVIRONMENT="Production"
```

أو عدّل `appsettings.Production.json`.

## 5) التحقق بعد الربط

- https://wadnooh.com/
- https://wadnooh.com/api/info
- https://wadnooh.com/api/flights/status
- https://www.wadnooh.com/  (يجب أن يحوّل إلى بدون www)

## ملاحظات
- `AllowedHosts` مضبوط على `wadnooh.com;www.wadnooh.com`
- التحويل من `www` إلى الدومين الرئيسي مفعّل
- HTTPS و Forwarded Headers جاهزة خلف Nginx/IIS
