using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Options;
using SudanTravelApp.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders =
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto |
        Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedHost;
    options.KnownIPNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApi();
builder.Services.AddHttpClient();
builder.Services.AddHttpClient("openai");
builder.Services.AddHttpClient("openlibrary", c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd("WadNoohAcademy/1.0 (educational; contact@wadnooh.com)");
    c.Timeout = TimeSpan.FromSeconds(20);
});
builder.Services.AddHttpClient("wikipedia", c =>
{
    c.DefaultRequestHeaders.UserAgent.ParseAdd("WadNoohAcademy/1.0 (educational; contact@wadnooh.com)");
    c.Timeout = TimeSpan.FromSeconds(15);
});

builder.Services.Configure<FlightProviderOptions>(
    builder.Configuration.GetSection(FlightProviderOptions.SectionName));
builder.Services.Configure<AiOptions>(
    builder.Configuration.GetSection(AiOptions.SectionName));
builder.Services.Configure<JwtOptions>(
    builder.Configuration.GetSection(JwtOptions.SectionName));
builder.Services.Configure<PaymentOptions>(
    builder.Configuration.GetSection(PaymentOptions.SectionName));
builder.Services.Configure<AdminOptions>(
    builder.Configuration.GetSection(AdminOptions.SectionName));
builder.Services.Configure<SmtpOptions>(
    builder.Configuration.GetSection(SmtpOptions.SectionName));
builder.Services.Configure<CurrencyOptions>(
    builder.Configuration.GetSection(CurrencyOptions.SectionName));

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Data Source=SudanTravel.db";
builder.Services.AddDbContext<TravelDbContext>(options =>
    options.UseSqlite(connectionString));

builder.Services
    .AddIdentityCore<ApplicationUser>(options =>
    {
        options.Password.RequiredLength = 6;
        options.Password.RequireDigit = false;
        options.Password.RequireLowercase = false;
        options.Password.RequireUppercase = false;
        options.Password.RequireNonAlphanumeric = false;
        options.User.RequireUniqueEmail = true;
        options.Lockout.AllowedForNewUsers = true;
        options.Lockout.MaxFailedAccessAttempts = 10;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<TravelDbContext>()
    .AddDefaultTokenProviders();

var jwt = builder.Configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>() ?? new JwtOptions();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwt.Issuer,
            ValidAudience = jwt.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Key)),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = ClaimTypes.Name
        };
    });
builder.Services.AddAuthorization();

builder.Services.AddSingleton<IFlightOfferCache, FlightOfferCache>();
builder.Services.AddSingleton<DemoFlightProvider>();
builder.Services.AddHttpClient("duffel");
builder.Services.AddSingleton<IFlightProvider, DuffelFlightProvider>();
builder.Services.AddSingleton<IAiTechAssistant, AiTechAssistant>();
builder.Services.AddSingleton<IAiStudyAssistant, AiStudyAssistant>();
builder.Services.AddHostedService<FlightInventorySyncService>();
builder.Services.AddScoped<IJwtTokenService, JwtTokenService>();
builder.Services.AddScoped<IMembershipService, MembershipService>();
builder.Services.AddScoped<IBookingFulfillmentService, BookingFulfillmentService>();
builder.Services.AddScoped<IPaymentService, PaymentGatewayService>();
builder.Services.AddScoped<IAdminSeedService, AdminSeedService>();
builder.Services.AddScoped<IAdminNotificationService, AdminNotificationService>();
builder.Services.AddSingleton<ICurrencyService, CurrencyService>();
builder.Services.AddScoped<IEmailSender, SmtpEmailSender>();
builder.Services.AddScoped<IEmailVerificationService, EmailVerificationService>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseForwardedHeaders();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHsts();
}

if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("DISABLE_HTTPS_REDIRECT")))
{
    app.UseHttpsRedirection();
}
app.UseDefaultFiles();
app.UseStaticFiles();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

var publicBaseUrl = builder.Configuration["PublicBaseUrl"] ?? "https://wadnooh.com";

app.MapGet("/api/info", (IFlightProvider flights, IFlightOfferCache cache, IPaymentService payments) => new
{
    name = "ود نــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــوح للسفر والتقنية",
    domain = "wadnooh.com",
    publicUrl = publicBaseUrl,
    message = "مرحباً بك في ود نــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــــوح للسفر والتقنية",
    description = "Membership · Payments · Global flights · AI",
    audience = "worldwide",
    languages = new[] { "ar", "en" },
    version = "5.0",
    paymentProvider = payments.ActiveProvider,
    flightProvider = new
    {
        name = flights.Name,
        live = flights.IsLive,
        lastSyncUtc = cache.LastSyncUtc,
        cachedOffers = cache.GetAll().Count
    },
    endpoints = new[]
    {
        "/api/auth/register",
        "/api/auth/login",
        "/api/auth/confirm-email",
        "/api/auth/verify-email",
        "/api/auth/resend-verification",
        "/api/me/lectures",
        "/api/membership/plans",
        "/api/payments/checkout",
        "/api/currency/rates",
        "/api/admin/stats",
        "/api/admin/notifications",
        "/api/admin/members",
        "/api/flights",
        "/api/flightbookings",
        "/api/hotels",
        "/api/ai/assist"
    }
}).WithName("Info").WithTags("General");

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<TravelDbContext>();
        context.Database.EnsureCreated();
        await AdminNotificationSchema.EnsureAsync(context);
        await EmailConfirmationSchema.EnsureAsync(context);
        // EnsureCreated does not add new tables to an existing SQLite file — create if missing.
        await context.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "MemberLectures" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_MemberLectures" PRIMARY KEY AUTOINCREMENT,
                "UserId" TEXT NOT NULL,
                "TitleAr" TEXT NOT NULL,
                "TitleEn" TEXT NULL,
                "Subject" TEXT NULL,
                "SpecialtyId" TEXT NULL,
                "CourseId" TEXT NULL,
                "LessonId" TEXT NULL,
                "Notes" TEXT NULL,
                "LectureDate" TEXT NULL,
                "DurationMinutes" INTEGER NULL,
                "AttachmentsJson" TEXT NULL,
                "TagsJson" TEXT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "UpdatedAtUtc" TEXT NOT NULL,
                CONSTRAINT "FK_MemberLectures_AspNetUsers_UserId" FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """);
        await context.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_MemberLectures_UserId" ON "MemberLectures" ("UserId");
            """);
        await context.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_MemberLectures_UserId_UpdatedAtUtc" ON "MemberLectures" ("UserId", "UpdatedAtUtc");
            """);
        await services.GetRequiredService<IMembershipService>().SeedPlansAsync();
        await services.GetRequiredService<IAdminSeedService>().SeedAsync();

        var flightOpts = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<FlightProviderOptions>>().Value;
        var aiOpts = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AiOptions>>().Value;
        var pay = services.GetRequiredService<IPaymentService>();
        var adminOpts = services.GetRequiredService<Microsoft.Extensions.Options.IOptions<AdminOptions>>().Value;

        Console.WriteLine("");
        Console.WriteLine("╔════════════════════════════════════════════════════════════════╗");
        Console.WriteLine("║     ود نوح للبرمجيات والكمبيوتر  v5 · Admin Panel                    ║");
        Console.WriteLine("║     Membership · Payments · Services · Company Control          ║");
        Console.WriteLine("╚════════════════════════════════════════════════════════════════╝");
        Console.WriteLine("");
        Console.WriteLine($"مزود الخدمات: {(string.IsNullOrWhiteSpace(flightOpts.DuffelApiKey) ? "demo" : "Duffel NDC")}");
        Console.WriteLine($"الذكاء الاصطناعي: {(aiOpts.Provider == "openai" && !string.IsNullOrWhiteSpace(aiOpts.OpenAiApiKey) ? "OpenAI" : "محلي")}");
        Console.WriteLine($"السداد: {pay.ActiveProvider}");
        Console.WriteLine($"فنادق: {context.Hotels.Count()} | باقات عضوية: {context.MembershipPlans.Count()}");
        Console.WriteLine($"لوحة الأدمن: /admin.html  ·  {adminOpts.Email}");
        Console.WriteLine("");
        Console.WriteLine("https://wadnooh.com");
        Console.WriteLine("http://localhost:5162");
        Console.WriteLine("");
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "حدث خطأ أثناء إنشاء قاعدة البيانات");
    }
}

app.Run();
