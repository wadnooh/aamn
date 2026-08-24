using Microsoft.AspNetCore.Identity;

namespace SudanTravelApp.API.Models;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? PassportNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
