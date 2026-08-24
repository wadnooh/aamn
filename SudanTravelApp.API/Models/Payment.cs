namespace SudanTravelApp.API.Models;

public class Payment
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Purpose { get; set; } = "booking"; // booking | membership
    public string Status { get; set; } = "Pending"; // Pending | Paid | Failed | Cancelled
    public string Provider { get; set; } = "demo"; // stripe | demo
    public string ExternalId { get; set; } = string.Empty;
    public string CheckoutUrl { get; set; } = string.Empty;
    public string? BookingType { get; set; } // flight | hotel | membership
    public int? BookingId { get; set; }
    public int? MembershipPlanId { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? PaidAtUtc { get; set; }
}
