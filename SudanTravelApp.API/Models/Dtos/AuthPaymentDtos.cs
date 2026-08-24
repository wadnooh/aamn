namespace SudanTravelApp.API.Models.Dtos;

public class RegisterRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? PassportNumber { get; set; }
}

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class AuthResponse
{
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAtUtc { get; set; }
    public UserProfileDto User { get; set; } = new();
    /// <summary>True when a verification email was queued/sent via SMTP.</summary>
    public bool VerificationEmailSent { get; set; }
    /// <summary>Dev-only: absolute verify URL when SMTP is unavailable.</summary>
    public string? DevVerifyUrl { get; set; }
    /// <summary>Dev-only: 6-digit code when SMTP is unavailable.</summary>
    public string? DevVerifyCode { get; set; }
}

public class UserProfileDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? PassportNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public MembershipInfoDto? Membership { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public bool IsAdmin => Roles.Contains("Admin", StringComparer.OrdinalIgnoreCase);
}

public class VerifyEmailRequest
{
    public string? Email { get; set; }
    public string? Code { get; set; }
    public string? Token { get; set; }
}

public class ResendVerificationRequest
{
    public string? Email { get; set; }
}

public class MembershipInfoDto
{
    public string PlanCode { get; set; } = "free";
    public string PlanNameAr { get; set; } = "عضوية مجانية";
    public string PlanNameEn { get; set; } = "Free Member";
    public decimal DiscountPercent { get; set; }
    public string Status { get; set; } = "Active";
    public DateTime? EndUtc { get; set; }
}

public class CheckoutRequest
{
    public string Purpose { get; set; } = "booking"; // booking | membership
    public string? BookingType { get; set; } // flight | hotel
    public int? BookingId { get; set; }
    public int? MembershipPlanId { get; set; }
}

public class CheckoutResponse
{
    public int PaymentId { get; set; }
    public string Provider { get; set; } = "demo";
    public string CheckoutUrl { get; set; } = string.Empty;
    public string Status { get; set; } = "Pending";
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
}
