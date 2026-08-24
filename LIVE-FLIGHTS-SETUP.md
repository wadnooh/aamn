# تفعيل الإنتاج — ود نوح v3

## ما تم بناؤه

1. **بحث حي** من شبكة شركات الطيران عبر طبقة Duffel (NDC)
2. **حجز مباشر** بدون موافقة أدمن
3. **تحديث تلقائي** للمسارات الشائعة كل 15 دقيقة
4. **مساعد ذكاء اصطناعي** للبحث الطبيعي والتوصيات

بدون مفاتيح API يعمل النظام بوضع **demo حي** بنفس التدفق.

## تفعيل Duffel (حجز حقيقي)

1. أنشئ حساباً على [https://duffel.com](https://duffel.com)
2. انسخ مفتاح API (test ثم live)
3. في `appsettings.json` أو متغيرات البيئة:

```json
"FlightProvider": {
  "Provider": "duffel",
  "DuffelApiKey": "duffel_test_..."
}
```

أو:

```powershell
$env:FlightProvider__DuffelApiKey = "duffel_test_..."
```

## تفعيل OpenAI (اختياري)

```json
"AI": {
  "Provider": "openai",
  "OpenAiApiKey": "sk-..."
}
```

بدون المفتاح يعمل المساعد المحلي لفهم العربية.

## التشغيل

```powershell
dotnet run --project SudanTravelApp.API\SudanTravelApp.API.csproj --launch-profile http
```

افتح: http://localhost:5162
