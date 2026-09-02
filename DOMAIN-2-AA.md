# ربط ونقل التطبيق بالكامل إلى الدومين الجديد 2-aa.com

## نظرة عامة
تم تحديث كافة إعدادات التطبيق (.NET Backend، واجهات الويب، المصادقة، وسائل الدفع، وإعدادات الخوادم) للعمل بشكل كامل على الدومين الجديد:
**https://2-aa.com**

---

## 1) إعدادات DNS لدومين 2-aa.com (في Hostinger / Cloudflare / مزود النطاق)

من لوحة تحكم النطاق 2-aa.com -> **DNS / Nameservers**:

| النوع (Type) | الاسم (Name) | القيمة (Value) | TTL |
|-------------|--------------|----------------|-----|
| **A** | @ | IP السيرفر الخاص بك (VPS / Dedicated) | 300 |
| **A** أو **CNAME** | www | @ أو 2-aa.com | 300 |

> إذا كنت تستخدم نفق Cloudflare Tunnel:
> شغّل السكربت: .\deploy\bind-cloudflare-domain.ps1 وسيقوم تلقائياً بربط الدومين الجديد 2-aa.com و www.2-aa.com.

---

## 2) توجيه الدومين القديم wadnooh.com (301 Permanent Redirect)

لضمان تحويل أي زوار قادمين من الدومين القديم wadnooh.com إلى 2-aa.com:
- تم إعداد توجيه IIS في web.config.
- تم إعداد ملف توجيه Nginx في deploy/nginx-wadnooh.com.conf.

---

## 3) النشر السريع (Windows / IIS)

`powershell
cd e:\FLY
.\deploy\publish-2-aa.ps1
`

ثم:
1. ارفع محتويات مجلد publish\2-aa أو الملف المضغوط publish\2-aa-site.zip.
2. في IIS أنشئ أو حدّث الموقع:
   - **Site name**: 2-aa
   - **Binding**: 2-aa.com و www.2-aa.com على المنفذ 80 و 443
   - **Physical path**: مجلد النشر publish\2-aa
3. ثبّت **ASP.NET Core Hosting Bundle**
4. فعّل شهادة SSL

---

## 4) النشر على Linux / VPS باستخدام Nginx و Docker

`ash
# 1. تشغيل التطبيق عبر Docker
cd /opt/2-aa
docker compose up -d --build

# 2. تفعيل إعداد Nginx
sudo cp deploy/nginx-2-aa.com.conf /etc/nginx/sites-available/2-aa.com
sudo ln -s /etc/nginx/sites-available/2-aa.com /etc/nginx/sites-enabled/
sudo certbot --nginx -d 2-aa.com -d www.2-aa.com
sudo nginx -t && sudo systemctl reload nginx
`

---

## 5) نشر الواجهة الأمامية على Hostinger Web Hosting

1. افتح [hPanel](https://hpanel.hostinger.com/)
2. Websites → **2-aa.com** → **File Manager**
3. ادخل إلى مجلد public_html
4. ارفع ملفات الموقع أو شغّل سكربت النشر التلقائي:
`powershell
.\deploy\sync-and-deploy.ps1
`

---

## 6) روابط التحقق بعد الربط

- الصفحة الرئيسية: **https://2-aa.com/**
- فحص معلومات النظام والـ API: **https://2-aa.com/api/info**
- لوحة الإدارة والتحكم: **https://2-aa.com/admin.html**
- فحص التوجيه: **https://www.2-aa.com/** (يحوّل تلقائياً إلى https://2-aa.com)
