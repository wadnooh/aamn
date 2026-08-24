using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _users;
    private readonly IJwtTokenService _jwt;
    private readonly IMembershipService _membership;
    private readonly IAdminNotificationService _notify;
    private readonly IEmailVerificationService _emailVerify;
    private readonly IHostEnvironment _env;
    private readonly ILogger<AuthController> _logger;

    public AuthController(
        UserManager<ApplicationUser> users,
        IJwtTokenService jwt,
        IMembershipService membership,
        IAdminNotificationService notify,
        IEmailVerificationService emailVerify,
        IHostEnvironment env,
        ILogger<AuthController> logger)
    {
        _users = users;
        _jwt = jwt;
        _membership = membership;
        _notify = notify;
        _emailVerify = emailVerify;
        _env = env;
        _logger = logger;
    }

    [HttpPost("register")]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            return BadRequest(new { message = "البريد وكلمة المرور مطلوبان" });
        if (string.IsNullOrWhiteSpace(request.FullName))
            return BadRequest(new { message = "الاسم الكامل مطلوب" });
        if (request.Password.Length < 6)
            return BadRequest(new { message = "كلمة المرور يجب ألا تقل عن 6 أحرف" });

        var existing = await _users.FindByEmailAsync(request.Email.Trim());
        if (existing != null)
            return Conflict(new { message = "البريد مسجّل مسبقاً" });

        var user = new ApplicationUser
        {
            UserName = request.Email.Trim(),
            Email = request.Email.Trim(),
            FullName = request.FullName.Trim(),
            PhoneNumber = request.Phone?.Trim(),
            PassportNumber = request.PassportNumber?.Trim(),
            EmailConfirmed = false
        };

        var result = await _users.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return BadRequest(new { message = string.Join(" · ", result.Errors.Select(e => e.Description)) });

        EmailVerificationIssueResult? issued = null;
        try
        {
            issued = await _emailVerify.IssueAndSendAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Verification issue failed after register for {Email}", user.Email);
        }

        try
        {
            await _notify.NotifyUserRegisteredAsync(user);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Admin notify failed after register for {Email}", user.Email);
        }

        var membership = await _membership.GetActiveMembershipAsync(user.Id);
        var roles = await _users.GetRolesAsync(user);
        var response = _jwt.CreateToken(user, membership, roles);
        response.VerificationEmailSent = issued?.EmailSent ?? false;
        if (_env.IsDevelopment() && issued != null && !issued.EmailSent)
        {
            response.DevVerifyUrl = issued.VerifyUrl;
            response.DevVerifyCode = issued.Code;
        }

        return Ok(response);
    }

    [HttpPost("login")]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var user = await _users.FindByEmailAsync(request.Email?.Trim() ?? string.Empty);
        if (user == null || !await _users.CheckPasswordAsync(user, request.Password ?? string.Empty))
            return Unauthorized(new { message = "بيانات الدخول غير صحيحة" });

        if (await _users.IsLockedOutAsync(user))
            return Unauthorized(new { message = "الحساب مقفل — تواصل مع الإدارة" });

        var membership = await _membership.GetActiveMembershipAsync(user.Id);
        var roles = await _users.GetRolesAsync(user);
        return Ok(_jwt.CreateToken(user, membership, roles));
    }

    /// <summary>Confirm via query token (link from email).</summary>
    [HttpGet("confirm-email")]
    [HttpPost("confirm-email")]
    public async Task<ActionResult> ConfirmEmail([FromQuery] string? token, [FromBody] VerifyEmailRequest? body = null)
    {
        var t = token ?? body?.Token;
        var (ok, message) = await _emailVerify.ConfirmByTokenAsync(t ?? string.Empty);
        if (!ok) return BadRequest(new { message });
        return Ok(new { message, emailConfirmed = true });
    }

    /// <summary>Confirm via 6-digit code or token in body.</summary>
    [HttpPost("verify-email")]
    public async Task<ActionResult> VerifyEmail([FromBody] VerifyEmailRequest request)
    {
        if (!string.IsNullOrWhiteSpace(request.Token))
        {
            var (okT, msgT) = await _emailVerify.ConfirmByTokenAsync(request.Token);
            if (!okT) return BadRequest(new { message = msgT });
            return Ok(new { message = msgT, emailConfirmed = true });
        }

        var (ok, message) = await _emailVerify.ConfirmByCodeAsync(
            request.Email ?? string.Empty,
            request.Code ?? string.Empty);
        if (!ok) return BadRequest(new { message });
        return Ok(new { message, emailConfirmed = true });
    }

    [HttpPost("resend-verification")]
    public async Task<ActionResult> ResendVerification([FromBody] ResendVerificationRequest? request)
    {
        string? email = request?.Email?.Trim();
        ApplicationUser? user = null;

        if (User.Identity?.IsAuthenticated == true)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(userId))
                user = await _users.FindByIdAsync(userId);
        }

        if (user == null && !string.IsNullOrWhiteSpace(email))
            user = await _users.FindByEmailAsync(email);

        // Always return a generic OK to avoid email enumeration.
        if (user == null)
            return Ok(new { message = "إن وُجد الحساب فسيصلك رابط التأكيد إن لم يكن مؤكداً بعد" });

        if (user.EmailConfirmed)
            return Ok(new { message = "البريد مؤكد مسبقاً", emailConfirmed = true });

        var issued = await _emailVerify.IssueAndSendAsync(user);
        var payload = new Dictionary<string, object?>
        {
            ["message"] = issued.EmailSent
                ? "أُرسل رابط التأكيد إلى بريدك"
                : "تعذر إرسال البريد — تحقق لاحقاً أو استخدم الرمز إن ظهر",
            ["emailSent"] = issued.EmailSent,
            ["smtpConfigured"] = issued.SmtpConfigured
        };

        if (_env.IsDevelopment() && !issued.EmailSent)
        {
            payload["devVerifyUrl"] = issued.VerifyUrl;
            payload["devVerifyCode"] = issued.Code;
        }

        return Ok(payload);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<UserProfileDto>> Me()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        var user = await _users.FindByIdAsync(userId);
        if (user == null) return Unauthorized();

        var membership = await _membership.GetActiveMembershipAsync(user.Id);
        var roles = await _users.GetRolesAsync(user);
        return Ok(new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Phone = user.PhoneNumber,
            PassportNumber = user.PassportNumber,
            EmailConfirmed = user.EmailConfirmed,
            Membership = membership,
            Roles = roles.ToList()
        });
    }
}
