namespace SudanTravelApp.API.Models.Dtos;

public class FlightOfferDto
{
    public string OfferId { get; set; } = string.Empty;
    public string Source { get; set; } = "demo";
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
    /// <summary>Original airline/network amount before company display conversion.</summary>
    public decimal? OriginalPrice { get; set; }
    public string? OriginalCurrency { get; set; }
    public int AvailableSeats { get; set; }
    public string AircraftType { get; set; } = string.Empty;
    public int Stops { get; set; }
    public string CabinClass { get; set; } = "economy";
    public DateTime LastUpdatedUtc { get; set; } = DateTime.UtcNow;
    public bool BookableDirect { get; set; } = true;
}

public class FlightSearchRequest
{
    public string? From { get; set; }
    public string? To { get; set; }
    public DateTime? Date { get; set; }
    public int Passengers { get; set; } = 1;
    public string CabinClass { get; set; } = "economy";
    public bool LiveOnly { get; set; } = true;
}

public class LiveBookingRequest
{
    public string OfferId { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
    public string PassportNumber { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }
    public string Gender { get; set; } = "m";
    public int NumberOfSeats { get; set; } = 1;
}

public class LiveBookingResult
{
    public bool Success { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string ExternalOrderId { get; set; } = string.Empty;
    public string Status { get; set; } = "Confirmed";
    public string Source { get; set; } = "demo";
    public decimal TotalPrice { get; set; }
    public string Currency { get; set; } = "SDG";
    public string Message { get; set; } = string.Empty;
    public FlightOfferDto? Offer { get; set; }
}

public class AiAssistRequest
{
    public string Message { get; set; } = string.Empty;
}

public class AiAssistResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Intent { get; set; } = "general";
    public FlightSearchRequest? SuggestedSearch { get; set; }
    public List<FlightOfferDto> Recommendations { get; set; } = [];
    public string Provider { get; set; } = "local";
}

public class AiStudyRequest
{
    public string Message { get; set; } = string.Empty;
    public string? Topic { get; set; }
    /// <summary>ar | en</summary>
    public string Language { get; set; } = "ar";
}

public class AiStudyResponse
{
    public string Reply { get; set; } = string.Empty;
    public string Intent { get; set; } = "study";
    public string Topic { get; set; } = string.Empty;
    public List<KnowledgeSourceDto> Sources { get; set; } = [];
    public List<BookResourceDto> Books { get; set; } = [];
    public string Provider { get; set; } = "openlibrary";
}

public class KnowledgeSourceDto
{
    public string Title { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string Source { get; set; } = "wikipedia";
}

public class BookResourceDto
{
    public string Title { get; set; } = string.Empty;
    public string Authors { get; set; } = string.Empty;
    public int? Year { get; set; }
    public string? CoverUrl { get; set; }
    public string OpenLibraryUrl { get; set; } = string.Empty;
    public string ReadUrl { get; set; } = string.Empty;
    public string Source { get; set; } = "openlibrary";
}
