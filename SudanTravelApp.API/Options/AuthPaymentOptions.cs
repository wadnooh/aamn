namespace SudanTravelApp.API.Options;

public class JwtOptions
{
    public const string SectionName = "Jwt";
    public string Key { get; set; } = "WadNouhTech_ChangeMe_SuperSecretKey_2026!";
    public string Issuer { get; set; } = "wadnooh.com";
    public string Audience { get; set; } = "wadnooh.com";
    public int ExpiresHours { get; set; } = 72;
}

public class PaymentOptions
{
    public const string SectionName = "Payment";
    public string Provider { get; set; } = "Auto";
    public string Currency { get; set; } = "USD";
    public string StripeSecretKey { get; set; } = string.Empty;
    public string StripeWebhookSecret { get; set; } = string.Empty;
    public string StripePublishableKey { get; set; } = string.Empty;
    public string SuccessUrl { get; set; } = "https://wadnooh.com/?paid=1";
    public string CancelUrl { get; set; } = "https://wadnooh.com/?paid=0";
}
