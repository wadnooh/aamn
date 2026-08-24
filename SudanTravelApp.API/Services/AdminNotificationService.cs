using System.Net;
using System.Net.Mail;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public interface IAdminNotificationService
{
    Task<AdminNotification> NotifyAsync(string type, string title, object payload, CancellationToken ct = default);
    Task NotifyUserRegisteredAsync(ApplicationUser user, CancellationToken ct = default);
    Task NotifyNewsletterAsync(string email, string? language, CancellationToken ct = default);
    Task NotifyContactAsync(string name, string email, string? phone, string message, CancellationToken ct = default);
}

public class AdminNotificationService : IAdminNotificationService
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    private readonly TravelDbContext _db;
    private readonly IOptions<SmtpOptions> _smtp;
    private readonly IOptions<AdminOptions> _admin;
    private readonly ILogger<AdminNotificationService> _logger;

    public AdminNotificationService(
        TravelDbContext db,
        IOptions<SmtpOptions> smtp,
        IOptions<AdminOptions> admin,
        ILogger<AdminNotificationService> logger)
    {
        _db = db;
        _smtp = smtp;
        _admin = admin;
        _logger = logger;
    }

    public async Task NotifyUserRegisteredAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var verified = user.EmailConfirmed ? "مؤكد" : "غير مؤكد";
        await NotifyAsync(
            AdminNotificationTypes.UserRegistered,
            $"تسجيل عضو جديد ({verified}): {user.FullName}",
            new
            {
                userId = user.Id,
                fullName = user.FullName,
                email = user.Email,
                phone = user.PhoneNumber,
                emailConfirmed = user.EmailConfirmed,
                createdAtUtc = user.CreatedAtUtc
            },
            ct);
    }

    public async Task NotifyNewsletterAsync(string email, string? language, CancellationToken ct = default)
    {
        await NotifyAsync(
            AdminNotificationTypes.Newsletter,
            $"اشتراك نشرة: {email}",
            new { email, language },
            ct);
    }

    public async Task NotifyContactAsync(string name, string email, string? phone, string message, CancellationToken ct = default)
    {
        await NotifyAsync(
            AdminNotificationTypes.Contact,
            $"رسالة تواصل من {name}",
            new { name, email, phone, message },
            ct);
    }

    public async Task<AdminNotification> NotifyAsync(string type, string title, object payload, CancellationToken ct = default)
    {
        var entity = new AdminNotification
        {
            Type = type,
            Title = title,
            PayloadJson = JsonSerializer.Serialize(payload, JsonOpts),
            IsRead = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        _db.AdminNotifications.Add(entity);
        await _db.SaveChangesAsync(ct);

        try
        {
            await TrySendEmailAsync(title, entity.PayloadJson, type, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Admin email notify failed for {Type} #{Id} — inbox still saved", type, entity.Id);
        }

        return entity;
    }

    private async Task TrySendEmailAsync(string title, string payloadJson, string type, CancellationToken ct)
    {
        var smtp = _smtp.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host))
            return;

        var to = string.IsNullOrWhiteSpace(smtp.AdminEmail) ? _admin.Value.Email : smtp.AdminEmail;
        if (string.IsNullOrWhiteSpace(to))
            return;

        var from = string.IsNullOrWhiteSpace(smtp.From)
            ? (string.IsNullOrWhiteSpace(smtp.UserName) ? "noreply@wadnooh.com" : smtp.UserName)
            : smtp.From;

        // Prefer shared sender when registered; keep inline fallback for isolation.
        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 15000
        };
        if (!string.IsNullOrWhiteSpace(smtp.UserName))
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);

        var body = $"نوع الإشعار: {type}\nالعنوان: {title}\nالوقت (UTC): {DateTime.UtcNow:u}\n\nالبيانات:\n{payloadJson}";
        using var message = new MailMessage(from, to.Trim(), $"[WNC] {title}", body);
        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Admin email sent to {To} for {Type}", to, type);
    }
}

public static class AdminNotificationSchema
{
    public static async Task EnsureAsync(TravelDbContext db, CancellationToken ct = default)
    {
        // EnsureCreated does not add new tables to an existing SQLite file.
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "AdminNotifications" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_AdminNotifications" PRIMARY KEY AUTOINCREMENT,
                "Type" TEXT NOT NULL,
                "Title" TEXT NOT NULL,
                "PayloadJson" TEXT NOT NULL,
                "IsRead" INTEGER NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL
            );
            """, ct);
    }
}
