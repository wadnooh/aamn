using Microsoft.AspNetCore.Mvc;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

public record ContactRequest(string? Name, string? Email, string? Phone, string? Message);

[ApiController]
[Route("api/contact")]
public class ContactController : ControllerBase
{
    private readonly IAdminNotificationService _notify;
    private readonly ILogger<ContactController> _logger;

    public ContactController(IAdminNotificationService notify, ILogger<ContactController> logger)
    {
        _notify = notify;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Submit([FromBody] ContactRequest req, CancellationToken ct)
    {
        var name = (req.Name ?? string.Empty).Trim();
        var email = (req.Email ?? string.Empty).Trim();
        var message = (req.Message ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(name) || name.Length > 120)
            return BadRequest(new { message = "الاسم مطلوب" });
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length > 200)
            return BadRequest(new { message = "بريد غير صالح" });
        if (string.IsNullOrWhiteSpace(message) || message.Length > 4000)
            return BadRequest(new { message = "الرسالة مطلوبة" });

        try
        {
            await _notify.NotifyContactAsync(name, email, req.Phone?.Trim(), message, ct);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to persist contact notification");
            return StatusCode(500, new { message = "تعذر حفظ الرسالة" });
        }

        return Ok(new { ok = true, message = "تم الإرسال" });
    }
}
