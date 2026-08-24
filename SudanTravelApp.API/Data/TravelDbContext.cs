using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SudanTravelApp.API.Models;

namespace SudanTravelApp.API.Data;

public class TravelDbContext : IdentityDbContext<ApplicationUser>
{
    public TravelDbContext(DbContextOptions<TravelDbContext> options) : base(options)
    {
    }

    public DbSet<Flight> Flights { get; set; }
    public DbSet<Hotel> Hotels { get; set; }
    public DbSet<FlightBooking> FlightBookings { get; set; }
    public DbSet<HotelBooking> HotelBookings { get; set; }
    public DbSet<TouristAttraction> TouristAttractions { get; set; }
    public DbSet<MembershipPlan> MembershipPlans { get; set; }
    public DbSet<UserMembership> UserMemberships { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<AdminNotification> AdminNotifications { get; set; }
    public DbSet<MemberLecture> MemberLectures { get; set; }
    public DbSet<EmailConfirmationToken> EmailConfirmationTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<MemberLecture>(e =>
        {
            e.HasIndex(x => x.UserId);
            e.HasIndex(x => new { x.UserId, x.UpdatedAtUtc });
            e.Property(x => x.TitleAr).HasMaxLength(300).IsRequired();
            e.Property(x => x.TitleEn).HasMaxLength(300);
            e.Property(x => x.Subject).HasMaxLength(200);
            e.Property(x => x.SpecialtyId).HasMaxLength(80);
            e.Property(x => x.CourseId).HasMaxLength(80);
            e.Property(x => x.LessonId).HasMaxLength(80);
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<EmailConfirmationToken>(e =>
        {
            e.HasIndex(x => x.Token).IsUnique();
            e.HasIndex(x => x.UserId);
            e.Property(x => x.Token).HasMaxLength(128).IsRequired();
            e.Property(x => x.Code).HasMaxLength(12).IsRequired();
            e.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
        });

        SeedData(modelBuilder);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        SeedHotels(modelBuilder);
        SeedTouristAttractions(modelBuilder);
    }

    private void SeedHotels(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Hotel>().HasData(
            new Hotel { Id = 1, Name = "فندق كورنثيا الخرطوم", City = "الخرطوم", Address = "شارع النيل الأزرق، الخرطوم", Description = "فندق فاخر 5 نجوم على ضفاف النيل الأزرق مع إطلالة بانورامية خلابة ومرافق عالمية", StarRating = 5, PricePerNight = 50000, AvailableRooms = 50, ImageUrl = "/images/corinthia.jpg", PhoneNumber = "+249-183-779000" },
            new Hotel { Id = 2, Name = "فندق السلام روتانا", City = "الخرطوم", Address = "شارع البلدية، الخرطوم", Description = "فندق فاخر في قلب العاصمة بمواصفات عالمية ومطاعم متنوعة", StarRating = 5, PricePerNight = 45000, AvailableRooms = 60, ImageUrl = "/images/rotana.jpg", PhoneNumber = "+249-183-770000" },
            new Hotel { Id = 3, Name = "فندق غراند هوليداي فيلا", City = "الخرطوم", Address = "الرياض، الخرطوم", Description = "فندق عصري 4 نجوم مع خدمات ممتازة وموقع استراتيجي", StarRating = 4, PricePerNight = 35000, AvailableRooms = 70, ImageUrl = "/images/holiday-villa.jpg", PhoneNumber = "+249-183-780000" },
            new Hotel { Id = 4, Name = "فندق هيلتون الخرطوم", City = "الخرطوم", Address = "شارع الجامعة، الخرطوم", Description = "فندق تاريخي 4 نجوم على ملتقى النيلين", StarRating = 4, PricePerNight = 38000, AvailableRooms = 55, ImageUrl = "/images/hilton.jpg", PhoneNumber = "+249-183-774100" },
            new Hotel { Id = 5, Name = "فندق أكروبول", City = "أم درمان", Address = "شارع الأربعين، أم درمان", Description = "فندق تقليدي 3 نجوم قرب سوق أم درمان التاريخي", StarRating = 3, PricePerNight = 25000, AvailableRooms = 40, ImageUrl = "/images/acropole.jpg", PhoneNumber = "+249-187-552000" },
            new Hotel { Id = 6, Name = "فندق بورتسودان", City = "بورتسودان", Address = "الكورنيش، بورتسودان", Description = "فندق 4 نجوم على البحر الأحمر مع مركز غوص", StarRating = 4, PricePerNight = 32000, AvailableRooms = 45, ImageUrl = "/images/portsudan.jpg", PhoneNumber = "+249-311-822000" },
            new Hotel { Id = 7, Name = "ريد سي ريزورت", City = "بورتسودان", Address = "شاطئ عروس، بورتسودان", Description = "منتجع شاطئي 3 نجوم مع أنشطة بحرية متنوعة", StarRating = 3, PricePerNight = 28000, AvailableRooms = 50, ImageUrl = "/images/redsea-resort.jpg", PhoneNumber = "+249-311-825000" },
            new Hotel { Id = 8, Name = "فندق مروي الهرمي", City = "مروي", Address = "المنطقة الأثرية، مروي", Description = "فندق 3 نجوم بالقرب من أهرامات مروي النوبية", StarRating = 3, PricePerNight = 24000, AvailableRooms = 30, ImageUrl = "/images/meroe.jpg", PhoneNumber = "+249-XXX-XXXXX" },
            new Hotel { Id = 9, Name = "فندق سلام نيالا", City = "نيالا", Address = "وسط المدينة، نيالا", Description = "فندق 3 نجوم مريح في عاصمة ولاية جنوب دارفور", StarRating = 3, PricePerNight = 20000, AvailableRooms = 35, ImageUrl = "/images/nyala.jpg", PhoneNumber = "+249-XXX-XXXXX" },
            new Hotel { Id = 10, Name = "فندق توتيل كسلا", City = "كسلا", Address = "شارع القاش، كسلا", Description = "فندق 3 نجوم بإطلالة على جبل التاكا الشهير", StarRating = 3, PricePerNight = 22000, AvailableRooms = 30, ImageUrl = "/images/kassala.jpg", PhoneNumber = "+249-XXX-XXXXX" },
            new Hotel { Id = 11, Name = "فندق النوبة", City = "دنقلا", Address = "كريمة، دنقلا", Description = "فندق 3 نجوم قرب المواقع الأثرية النوبية", StarRating = 3, PricePerNight = 21000, AvailableRooms = 25, ImageUrl = "/images/dongola.jpg", PhoneNumber = "+249-XXX-XXXXX" },
            new Hotel { Id = 12, Name = "فندق الأبيض", City = "الأبيض", Address = "وسط المدينة، الأبيض", Description = "فندق 3 نجوم في عاصمة شمال كردفان", StarRating = 3, PricePerNight = 19000, AvailableRooms = 28, ImageUrl = "/images/elobied.jpg", PhoneNumber = "+249-XXX-XXXXX" }
        );
    }

    private void SeedTouristAttractions(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TouristAttraction>().HasData(
            new TouristAttraction { Id = 1, Name = "أهرامات مروي", City = "مروي", Description = "أكثر من 200 هرم نوبي يعود تاريخها للمملكة الكوشية (القرن الثامن قبل الميلاد)، موقع تراث عالمي لليونسكو", Category = "آثار تاريخية", ImageUrl = "/images/meroe-pyramids.jpg", EntryFee = 5000, OpeningHours = "8:00 صباحاً - 5:00 مساءً" },
            new TouristAttraction { Id = 2, Name = "جبل البركل", City = "كريمة", Description = "جبل مقدس به معابد فرعونية ونقوش تاريخية، كان مركزاً دينياً للمملكة النوبية", Category = "آثار تاريخية", ImageUrl = "/images/jebel-barkal.jpg", EntryFee = 3000, OpeningHours = "8:00 صباحاً - 5:00 مساءً" },
            new TouristAttraction { Id = 3, Name = "معبد صلب", City = "سولب", Description = "معبد فرعوني ضخم بناه أمنحتب الثالث، يحتوي على أعمدة ضخمة ونقوش رائعة", Category = "آثار تاريخية", ImageUrl = "/images/soleb.jpg", EntryFee = 3000, OpeningHours = "8:00 صباحاً - 5:00 مساءً" },
            new TouristAttraction { Id = 4, Name = "الكرو", City = "دنقلا", Description = "مقبرة ملكية نوبية تحتوي على أهرامات وغرف دفن الملوك الكوشيين", Category = "آثار تاريخية", ImageUrl = "/images/elkurru.jpg", EntryFee = 2500, OpeningHours = "8:00 صباحاً - 4:00 مساءً" },
            new TouristAttraction { Id = 5, Name = "النقعة", City = "الخرطوم", Description = "موقع أثري يحتوي على معبد الأسد ومعبد آمون، يظهر روعة الفن الكوشي", Category = "آثار تاريخية", ImageUrl = "/images/naqa.jpg", EntryFee = 2000, OpeningHours = "8:00 صباحاً - 5:00 مساءً" },
            new TouristAttraction { Id = 6, Name = "المصورات الصفراء", City = "شندي", Description = "موقع أثري غامض يحتوي على معابد ومنحوتات تمثل الحضارة المروية", Category = "آثار تاريخية", ImageUrl = "/images/musawwarat.jpg", EntryFee = 2000, OpeningHours = "8:00 صباحاً - 5:00 مساءً" },
            new TouristAttraction { Id = 7, Name = "المتحف القومي السوداني", City = "الخرطوم", Description = "أكبر متحف في السودان يحتوي على كنوز أثرية من مختلف الحضارات السودانية القديمة", Category = "متاحف", ImageUrl = "/images/national-museum.jpg", EntryFee = 1000, OpeningHours = "9:00 صباحاً - 6:00 مساءً" },
            new TouristAttraction { Id = 8, Name = "بيت الخليفة", City = "أم درمان", Description = "متحف تاريخي في منزل الخليفة عبد الله التعايشي، يعرض فترة الدولة المهدية", Category = "متاحف", ImageUrl = "/images/khalifa-house.jpg", EntryFee = 500, OpeningHours = "9:00 صباحاً - 5:00 مساءً" },
            new TouristAttraction { Id = 9, Name = "البحر الأحمر", City = "بورتسودان", Description = "شواطئ ساحرة ومواقع غوص عالمية مع شعاب مرجانية نادرة وأسماك استوائية ملونة", Category = "طبيعة", ImageUrl = "/images/red-sea.jpg", EntryFee = null, OpeningHours = "طوال اليوم" },
            new TouristAttraction { Id = 10, Name = "ملتقى النيلين", City = "الخرطوم", Description = "نقطة التقاء النيل الأزرق بالنيل الأبيض، منظر طبيعي فريد في قلب العاصمة", Category = "طبيعة", ImageUrl = "/images/nile-confluence.jpg", EntryFee = null, OpeningHours = "طوال اليوم" },
            new TouristAttraction { Id = 11, Name = "محمية الدندر القومية", City = "سنار", Description = "محمية طبيعية ضخمة تضم أسوداً وفيلة وزرافاً وأكثر من 160 نوع طيور", Category = "طبيعة", ImageUrl = "/images/dinder.jpg", EntryFee = 3000, OpeningHours = "موسمي" },
            new TouristAttraction { Id = 12, Name = "سوق أم درمان", City = "أم درمان", Description = "أكبر سوق تقليدي في السودان للحرف اليدوية والتوابل والأقمشة", Category = "ثقافة", ImageUrl = "/images/omdurman-market.jpg", EntryFee = null, OpeningHours = "8:00 صباحاً - 6:00 مساءً" }
        );
    }
}
