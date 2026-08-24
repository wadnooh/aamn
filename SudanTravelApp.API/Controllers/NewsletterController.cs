using System.Collections.Concurrent;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

public record NewsletterRequest(string? Email, string? Language);

[ApiController]
[Route("api/newsletter")]
public class NewsletterController : ControllerBase
{
    private static readonly ConcurrentBag<string> Emails = new();
    private readonly IWebHostEnvironment _env;
    private readonly IAdminNotificationService _notify;
    private readonly ILogger<NewsletterController> _logger;

    public NewsletterController(
        IWebHostEnvironment env,
        IAdminNotificationService notify,
        ILogger<NewsletterController> logger)
    {
        _env = env;
        _notify = notify;
        _logger = logger;
    }

    [HttpPost]
    public async Task<IActionResult> Subscribe([FromBody] NewsletterRequest req, CancellationToken ct)
    {
        var email = (req.Email ?? string.Empty).Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || !email.Contains('@') || email.Length > 200)
            return BadRequest(new { message = "Invalid email" });

        Emails.Add(email);
        _logger.LogInformation("WNC newsletter subscribe: {Email} lang={Lang}", email, req.Language);

        try
        {
            var dir = Path.Combine(_env.ContentRootPath, "App_Data");
            Directory.CreateDirectory(dir);
            var file = Path.Combine(dir, "newsletter.json");
            var list = Emails.OrderBy(x => x).ToList();
            await System.IO.File.WriteAllTextAsync(file, JsonSerializer.Serialize(list), ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Could not persist newsletter email to disk");
        }

        try
        {
            await _notify.NotifyNewsletterAsync(email, req.Language, ct);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Admin notify failed for newsletter {Email}", email);
        }

        return Ok(new { ok = true, message = "subscribed" });
    }
}
