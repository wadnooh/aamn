namespace SudanTravelApp.API.Models;

public class TouristAttraction
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string ImageUrl { get; set; } = string.Empty;
    public decimal? EntryFee { get; set; }
    public string OpeningHours { get; set; } = string.Empty;
}
