namespace SudanTravelApp.API.Models;

public class Flight
{
    public int Id { get; set; }
    public string OfferId { get; set; } = string.Empty;
    public string FlightNumber { get; set; } = string.Empty;
    public string Airline { get; set; } = string.Empty;
    public string AirlineCode { get; set; } = string.Empty;
    public string DepartureCity { get; set; } = string.Empty;
    public string ArrivalCity { get; set; } = string.Empty;
    public string DepartureAirport { get; set; } = string.Empty;
    public string ArrivalAirport { get; set; } = string.Empty;
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public decimal Price { get; set; }
    public string Currency { get; set; } = "SDG";
    public int AvailableSeats { get; set; }
    public string AircraftType { get; set; } = string.Empty;
    public int Stops { get; set; }
    public string CabinClass { get; set; } = "economy";
    public string Source { get; set; } = "local";
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public bool BookableDirect { get; set; }
}
