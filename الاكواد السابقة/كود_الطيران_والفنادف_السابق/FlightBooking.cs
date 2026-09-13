namespace SudanTravelApp.API.Models;

public class FlightBooking
{
    public int Id { get; set; }
    public int? FlightId { get; set; }
    public Flight? Flight { get; set; }
    public string OfferId { get; set; } = string.Empty;
    public string ExternalOrderId { get; set; } = string.Empty;
    public string Provider { get; set; } = "local";
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "SDG";
    public DateTime BookingDate { get; set; }
    public string Status { get; set; } = "Pending";
    public string BookingReference { get; set; } = string.Empty;
    public string AirlineConfirmation { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int? PaymentId { get; set; }
    public string PaymentStatus { get; set; } = "Unpaid";
    public decimal DiscountPercent { get; set; }
    public decimal OriginalPrice { get; set; }
}
