namespace SudanTravelApp.API.Options;

public class SmtpOptions
{
    public const string SectionName = "Smtp";

    public string Host { get; set; } = string.Empty;
    public int Port { get; set; } = 587;
    public bool EnableSsl { get; set; } = true;
    public string UserName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string From { get; set; } = string.Empty;
    /// <summary>Admin inbox for alerts; falls back to Admin:Email when empty.</summary>
    public string AdminEmail { get; set; } = string.Empty;
}
