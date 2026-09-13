namespace SudanTravelApp.API.Models;

public class HotelBooking
{
    public int Id { get; set; }
    public int HotelId { get; set; }
    public Hotel? Hotel { get; set; }
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string BookingReference { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int? PaymentId { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public decimal DiscountPercent { get; set; }
    public decimal OriginalPrice { get; set; }
    public string Currency { get; set; } = "SDG";
}
