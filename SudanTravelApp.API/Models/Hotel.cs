namespace SudanTravelApp.API.Models;

public class Hotel
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int StarRating { get; set; }
    public decimal PricePerNight { get; set; }
    public List<string> Amenities { get; set; } = new();
    public int AvailableRooms { get; set; }
    public string ImageUrl { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
}
