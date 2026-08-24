namespace SudanTravelApp.API.Models;

public class UserMembership
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }
    public int PlanId { get; set; }
    public MembershipPlan? Plan { get; set; }
    public string Status { get; set; } = "PendingPayment";
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? PaymentId { get; set; }
    public Payment? Payment { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
