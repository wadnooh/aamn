# دليل وأرشيف الأكواد السابقة (غير التابعة للبصات)

تم نقل هذه الملفات وحفظها في هذا المجلد بناءً على طلبكم لفرز المشروع والاحتفاظ فقط بالملفات والأنظمة التابعة لمنصة وتطبيق حجز البصات والسفريات (آمن / الاتحاد للخدمات).

---

## فهرس الملفات المؤرشفة:

### 1. أكواد الطيران والفنادق والسياحة السابقة (Old Flights, Hotels & Tourism):
* **Controllers (متحكمات الـ API):**
  - `FlightBookingsController.cs` (حجوزات رحلات الطيران)
  - `FlightsController.cs` (عروض وبيانات الطيران)
  - `HotelBookingsController.cs` (حجوزات الفنادق)
  - `HotelsController.cs` (بيانات الفنادق)
  - `TouristAttractionsController.cs` (المعالم السياحية)
  - `MemberLecturesController.cs` (محاضرات وكورسات سابقة)
  - `CatalogController.cs` (فهارس سابقة)
  - `NewsletterController.cs` (نشرة بريدية سابقة)
* **Models (نماذج البيانات):**
  - `Flight.cs`, `FlightBooking.cs`
  - `Hotel.cs`, `HotelBooking.cs`
  - `TouristAttraction.cs`
  - `MemberLecture.cs`
* **Services (الخدمات والمزودات):**
  - `AirportCatalog.cs` (فهرس المطارات)
  - `DemoFlightProvider.cs`, `DuffelFlightProvider.cs`, `IFlightProvider.cs`
  - `FlightInventorySyncService.cs`, `FlightOfferCache.cs`
  - `AiStudyAssistant.cs`, `IAiStudyAssistant.cs`

### 2. وثائق وملاحظات سابقة:
* `API-EXAMPLES.md` (أمثلة برمجية لـ API الطيران والفنادق القديم)
* `DOMAIN-2-AA.md` (ملاحظات ربط الدومين القديمة)
* `HOSTINGER-DEPLOY.md` (ملاحظات سابقة)

### 3. شعارات وهوية سابقة (Wad Nooh):
* `wad-nooh-icon.png`
* `wad-nooh-logo.jpg`
* `wad-nooh-logo.png`

### 4. سكريبتات وملفات مؤقتة:
* `__pycache__` (كاش بايثون قديم)
* سكريبتات نشر وتجربة سابقة تم استبدالها بـ `deploy.bat` و `sync-and-deploy.ps1`.
