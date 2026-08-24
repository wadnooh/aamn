namespace SudanTravelApp.API.Models;

public class MembershipPlan
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string NameAr { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string DescriptionAr { get; set; } = string.Empty;
    public string DescriptionEn { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "USD";
    public decimal DiscountPercent { get; set; }
    public int DurationDays { get; set; } = 365;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
}
