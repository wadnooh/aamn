namespace SudanTravelApp.API.Models;

public class EmailConfirmationToken
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    /// <summary>Opaque one-time link token (URL-safe).</summary>
    public string Token { get; set; } = string.Empty;

    /// <summary>6-digit code alternative for manual entry.</summary>
    public string Code { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime ExpiresAtUtc { get; set; }
    public DateTime? UsedAtUtc { get; set; }
}
