# موقع ود نوح العالمي | Global Wad Nouh

## ما تم تفعيله
- واجهة **عربي / English** (زر اللغة أعلى الصفحة)
- وجهات عالمية: دبي، القاهرة، جدة، إسطنبول، الدوحة، لندن، باريس، فرانكفورت، نيويورك، أديس، نيروبي، جوهانسبرغ...
- بحث حي على شبكة طيران عالمية (Duffel عند تفعيل المفتاح)
- SEO: meta + hreflang لـ ar/en
- الدومين المستهدف: **https://wadnooh.com**

## الرابط الحي الحالي
انظر `deploy/runtime/public-url.txt` أو افتح آخر نفق Cloudflare.

English: أضف `?lang=en` أو اضغط **English**

## للانتشار العالمي على الدومين
1. اشترِ Web Hosting أو VPS من Hostinger
2. ارفع `publish/hostinger-site.zip` إلى `public_html`
3. فعّل CDN في Hostinger (إن وُجد) لتسريع الزوار حول العالم
4. أضف مفتاح Duffel للإنتاج الحقيقي:
   `FlightProvider__DuffelApiKey`

## هيكل عالمي موصى به
```text
الزائر (أي دولة)
   → CDN / Hostinger
   → wadnooh.com (واجهة AR/EN)
   → API (خدمات عالمية + حجز مباشر)
```
