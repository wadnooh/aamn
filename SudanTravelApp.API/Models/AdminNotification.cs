namespace SudanTravelApp.API.Models;

public static class AdminNotificationTypes
{
    public const string UserRegistered = "user_registered";
    public const string Newsletter = "newsletter";
    public const string Contact = "contact";
}

public class AdminNotification
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
