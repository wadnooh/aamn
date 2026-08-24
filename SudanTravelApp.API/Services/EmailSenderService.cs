using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public record EmailVerificationIssueResult(
    string Token,
    string Code,
    string VerifyUrl,
    bool EmailSent,
    bool SmtpConfigured);

public interface IEmailSender
{
    bool IsConfigured { get; }
    Task SendAsync(string to, string subject, string bodyText, string? bodyHtml = null, CancellationToken ct = default);
}

public interface IEmailVerificationService
{
    Task<EmailVerificationIssueResult> IssueAndSendAsync(ApplicationUser user, CancellationToken ct = default);
    Task<(bool Ok, string Message)> ConfirmByTokenAsync(string token, CancellationToken ct = default);
    Task<(bool Ok, string Message)> ConfirmByCodeAsync(string email, string code, CancellationToken ct = default);
}

public class SmtpEmailSender : IEmailSender
{
    private readonly IOptions<SmtpOptions> _smtp;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(IOptions<SmtpOptions> smtp, ILogger<SmtpEmailSender> logger)
    {
        _smtp = smtp;
        _logger = logger;
    }

    public bool IsConfigured => !string.IsNullOrWhiteSpace(_smtp.Value.Host);

    public async Task SendAsync(string to, string subject, string bodyText, string? bodyHtml = null, CancellationToken ct = default)
    {
        var smtp = _smtp.Value;
        if (string.IsNullOrWhiteSpace(smtp.Host))
            throw new InvalidOperationException("SMTP host is not configured");

        var from = string.IsNullOrWhiteSpace(smtp.From)
            ? (string.IsNullOrWhiteSpace(smtp.UserName) ? "noreply@wadnooh.com" : smtp.UserName)
            : smtp.From;

        using var client = new SmtpClient(smtp.Host, smtp.Port)
        {
            EnableSsl = smtp.EnableSsl,
            DeliveryMethod = SmtpDeliveryMethod.Network,
            Timeout = 20000
        };
        if (!string.IsNullOrWhiteSpace(smtp.UserName))
            client.Credentials = new NetworkCredential(smtp.UserName, smtp.Password);

        using var message = new MailMessage(from, to.Trim(), subject, bodyText);
        if (!string.IsNullOrWhiteSpace(bodyHtml))
        {
            message.Body = string.Empty;
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(bodyText, Encoding.UTF8, "text/plain"));
            message.AlternateViews.Add(AlternateView.CreateAlternateViewFromString(bodyHtml, Encoding.UTF8, "text/html"));
        }

        await client.SendMailAsync(message, ct);
        _logger.LogInformation("Email sent to {To}: {Subject}", to, subject);
    }
}

public class EmailVerificationService : IEmailVerificationService
{
    private static readonly TimeSpan TokenLifetime = TimeSpan.FromHours(48);

    private readonly TravelDbContext _db;
    private readonly UserManager<ApplicationUser> _users;
    private readonly IEmailSender _email;
    private readonly IConfiguration _config;
    private readonly ILogger<EmailVerificationService> _logger;

    public EmailVerificationService(
        TravelDbContext db,
        UserManager<ApplicationUser> users,
        IEmailSender email,
        IConfiguration config,
        ILogger<EmailVerificationService> logger)
    {
        _db = db;
        _users = users;
        _email = email;
        _config = config;
        _logger = logger;
    }

    public async Task<EmailVerificationIssueResult> IssueAndSendAsync(ApplicationUser user, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var pending = await _db.EmailConfirmationTokens
            .Where(t => t.UserId == user.Id && t.UsedAtUtc == null && t.ExpiresAtUtc > now)
            .ToListAsync(ct);
        foreach (var old in pending)
            old.UsedAtUtc = now;

        var token = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32))
            .TrimEnd('=').Replace('+', '-').Replace('/', '_');
        var code = RandomNumberGenerator.GetInt32(100000, 1000000).ToString();

        var row = new EmailConfirmationToken
        {
            UserId = user.Id,
            Token = token,
            Code = code,
            CreatedAtUtc = now,
            ExpiresAtUtc = now.Add(TokenLifetime)
        };
        _db.EmailConfirmationTokens.Add(row);
        await _db.SaveChangesAsync(ct);

        var publicBase = (_config["PublicBaseUrl"] ?? "https://wadnooh.com").TrimEnd('/');
        var verifyUrl = $"{publicBase}/verify.html?token={Uri.EscapeDataString(token)}";

        var emailSent = false;
        var subject = "تأكيد بريدك · Confirm your email — Wadnooh";
        var bodyText =
            $"مرحباً {user.FullName},\n\n" +
            $"أكد بريدك عبر الرابط:\n{verifyUrl}\n\n" +
            $"أو أدخل الرمز: {code}\n\n" +
            $"صالح لمدة 48 ساعة.\n\n" +
            $"Hello {user.FullName},\n\n" +
            $"Confirm your email:\n{verifyUrl}\n\n" +
            $"Or enter code: {code}\n\n" +
            $"Valid for 48 hours.\n\n— Wadnooh Software & Computer";

        var bodyHtml =
            $"<p dir=\"rtl\">مرحباً <strong>{WebUtility.HtmlEncode(user.FullName)}</strong>،</p>" +
            $"<p dir=\"rtl\">أكد بريدك بالضغط على الرابط أو بإدخال الرمز <strong>{code}</strong>.</p>" +
            $"<p><a href=\"{WebUtility.HtmlEncode(verifyUrl)}\">{WebUtility.HtmlEncode(verifyUrl)}</a></p>" +
            $"<hr/><p>Hello <strong>{WebUtility.HtmlEncode(user.FullName)}</strong>,</p>" +
            $"<p>Confirm via link or code <strong>{code}</strong> (48h).</p>" +
            $"<p>— Wadnooh Software & Computer</p>";

        if (_email.IsConfigured && !string.IsNullOrWhiteSpace(user.Email))
        {
            try
            {
                await _email.SendAsync(user.Email!, subject, bodyText, bodyHtml, ct);
                emailSent = true;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to send verification email to {Email}", user.Email);
            }
        }

        if (!emailSent)
        {
            _logger.LogWarning(
                "EMAIL VERIFY (SMTP unavailable) user={Email} code={Code} url={Url} token={Token}",
                user.Email, code, verifyUrl, token);
            try
            {
                var logDir = Path.Combine(AppContext.BaseDirectory, "App_Data");
                Directory.CreateDirectory(logDir);
                var line = $"{now:u}\t{user.Email}\t{code}\t{verifyUrl}\t{token}{Environment.NewLine}";
                await File.AppendAllTextAsync(Path.Combine(logDir, "email-verify.log"), line, ct);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Could not write email-verify.log");
            }
        }

        return new EmailVerificationIssueResult(token, code, verifyUrl, emailSent, _email.IsConfigured);
    }

    public async Task<(bool Ok, string Message)> ConfirmByTokenAsync(string token, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(token))
            return (false, "رمز التأكيد مفقود");

        var now = DateTime.UtcNow;
        var row = await _db.EmailConfirmationTokens
            .Include(t => t.User)
            .FirstOrDefaultAsync(t => t.Token == token.Trim(), ct);

        if (row == null)
            return (false, "رابط التأكيد غير صالح");
        if (row.UsedAtUtc != null)
            return (false, "تم استخدام هذا الرابط مسبقاً");
        if (row.ExpiresAtUtc < now)
            return (false, "انتهت صلاحية رابط التأكيد");

        var user = row.User ?? await _users.FindByIdAsync(row.UserId);
        if (user == null)
            return (false, "المستخدم غير موجود");

        user.EmailConfirmed = true;
        row.UsedAtUtc = now;
        await InvalidateOthersAsync(user.Id, row.Id, now, ct);
        await _users.UpdateAsync(user);
        await _db.SaveChangesAsync(ct);
        return (true, "تم تأكيد البريد بنجاح");
    }

    public async Task<(bool Ok, string Message)> ConfirmByCodeAsync(string email, string code, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(code))
            return (false, "البريد والرمز مطلوبان");

        var user = await _users.FindByEmailAsync(email.Trim());
        if (user == null)
            return (false, "بيانات التأكيد غير صحيحة");

        var now = DateTime.UtcNow;
        var normalized = code.Trim();
        var row = await _db.EmailConfirmationTokens
            .Where(t => t.UserId == user.Id && t.UsedAtUtc == null && t.ExpiresAtUtc > now)
            .OrderByDescending(t => t.CreatedAtUtc)
            .FirstOrDefaultAsync(ct);

        if (row == null || !string.Equals(row.Code, normalized, StringComparison.Ordinal))
            return (false, "رمز التأكيد غير صالح أو منتهٍ");

        user.EmailConfirmed = true;
        row.UsedAtUtc = now;
        await InvalidateOthersAsync(user.Id, row.Id, now, ct);
        await _users.UpdateAsync(user);
        await _db.SaveChangesAsync(ct);
        return (true, "تم تأكيد البريد بنجاح");
    }

    private async Task InvalidateOthersAsync(string userId, int keepId, DateTime now, CancellationToken ct)
    {
        var others = await _db.EmailConfirmationTokens
            .Where(t => t.UserId == userId && t.Id != keepId && t.UsedAtUtc == null)
            .ToListAsync(ct);
        foreach (var o in others)
            o.UsedAtUtc = now;
    }
}

public static class EmailConfirmationSchema
{
    public static async Task EnsureAsync(TravelDbContext db, CancellationToken ct = default)
    {
        await db.Database.ExecuteSqlRawAsync("""
            CREATE TABLE IF NOT EXISTS "EmailConfirmationTokens" (
                "Id" INTEGER NOT NULL CONSTRAINT "PK_EmailConfirmationTokens" PRIMARY KEY AUTOINCREMENT,
                "UserId" TEXT NOT NULL,
                "Token" TEXT NOT NULL,
                "Code" TEXT NOT NULL,
                "CreatedAtUtc" TEXT NOT NULL,
                "ExpiresAtUtc" TEXT NOT NULL,
                "UsedAtUtc" TEXT NULL,
                CONSTRAINT "FK_EmailConfirmationTokens_AspNetUsers_UserId"
                    FOREIGN KEY ("UserId") REFERENCES "AspNetUsers" ("Id") ON DELETE CASCADE
            );
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE UNIQUE INDEX IF NOT EXISTS "IX_EmailConfirmationTokens_Token"
                ON "EmailConfirmationTokens" ("Token");
            """, ct);
        await db.Database.ExecuteSqlRawAsync("""
            CREATE INDEX IF NOT EXISTS "IX_EmailConfirmationTokens_UserId"
                ON "EmailConfirmationTokens" ("UserId");
            """, ct);
    }
}
