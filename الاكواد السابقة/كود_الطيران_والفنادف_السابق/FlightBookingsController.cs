using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FlightBookingsController : ControllerBase
{
    private readonly TravelDbContext _context;
    private readonly IFlightProvider _provider;
    private readonly IFlightOfferCache _cache;
    private readonly IMembershipService _membership;

    public FlightBookingsController(
        TravelDbContext context,
        IFlightProvider provider,
        IFlightOfferCache cache,
        IMembershipService membership)
    {
        _context = context;
        _provider = provider;
        _cache = cache;
        _membership = membership;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FlightBooking>>> GetFlightBookings()
    {
        return await _context.FlightBookings.Include(b => b.Flight).ToListAsync();
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<FlightBooking>>> GetMyFlightBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return await _context.FlightBookings
            .Include(b => b.Flight)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<FlightBooking>> GetFlightBooking(int id)
    {
        var booking = await _context.FlightBookings
            .Include(b => b.Flight)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        return booking;
    }

    [HttpGet("reference/{reference}")]
    public async Task<ActionResult<FlightBooking>> GetFlightBookingByReference(string reference)
    {
        var booking = await _context.FlightBookings
            .Include(b => b.Flight)
            .FirstOrDefaultAsync(b => b.BookingReference == reference);

        if (booking == null) return NotFound();
        return booking;
    }

    /// <summary>Create pending booking — confirm after payment.</summary>
    [HttpPost]
    public async Task<ActionResult<object>> CreateFlightBooking(
        [FromBody] LiveBookingRequest request,
        CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(request.OfferId))
            return BadRequest(new { message = "معرّف العرض مطلوب" });
        if (string.IsNullOrWhiteSpace(request.PassengerName))
            return BadRequest(new { message = "اسم المسافر مطلوب" });

        var offer = _cache.GetByOfferId(request.OfferId);
        if (offer == null)
            return BadRequest(new { message = "انتهى العرض أو لم يعد متاحاً. أعد البحث واختر خدمة جديدة." });

        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var discount = await _membership.GetDiscountPercentAsync(userId, ct);
        var seats = Math.Max(1, request.NumberOfSeats);
        var original = offer.Price * seats;
        var total = Math.Round(original * (1 - discount / 100m), 2);

        var booking = new FlightBooking
        {
            OfferId = offer.OfferId,
            ExternalOrderId = string.Empty,
            Provider = offer.Source,
            PassengerName = request.PassengerName,
            PassengerEmail = request.PassengerEmail,
            PassengerPhone = request.PassengerPhone,
            PassportNumber = request.PassportNumber,
            NumberOfSeats = seats,
            OriginalPrice = original,
            DiscountPercent = discount,
            TotalPrice = total,
            Currency = offer.Currency,
            BookingDate = DateTime.Now,
            Status = "PendingPayment",
            PaymentStatus = "Unpaid",
            BookingReference = $"WN{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}",
            UserId = userId,
            Flight = new Flight
            {
                OfferId = offer.OfferId,
                FlightNumber = offer.FlightNumber,
                Airline = offer.Airline,
                AirlineCode = offer.AirlineCode,
                DepartureCity = offer.DepartureCity,
                ArrivalCity = offer.ArrivalCity,
                DepartureAirport = offer.DepartureAirport,
                ArrivalAirport = offer.ArrivalAirport,
                DepartureTime = offer.DepartureTime,
                ArrivalTime = offer.ArrivalTime,
                Price = offer.Price,
                Currency = offer.Currency,
                AvailableSeats = offer.AvailableSeats,
                AircraftType = offer.AircraftType,
                Stops = offer.Stops,
                CabinClass = offer.CabinClass,
                Source = offer.Source,
                LastUpdatedUtc = offer.LastUpdatedUtc,
                BookableDirect = true
            }
        };

        _context.FlightBookings.Add(booking);
        await _context.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetFlightBooking), new { id = booking.Id }, new
        {
            booking.Id,
            booking.BookingReference,
            booking.Status,
            booking.PaymentStatus,
            booking.Provider,
            booking.TotalPrice,
            booking.OriginalPrice,
            booking.DiscountPercent,
            booking.Currency,
            booking.PassengerName,
            requiresPayment = true,
            message = "تم إنشاء الحجز — أكمل الدفع للتأكيد",
            flight = offer
        });
    }

    [HttpPost("local")]
    public async Task<ActionResult<FlightBooking>> CreateLocalBooking(FlightBooking booking)
    {
        booking.BookingDate = DateTime.Now;
        booking.BookingReference = $"FLT{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        booking.Status = "PendingPayment";
        booking.PaymentStatus = "Unpaid";
        booking.Provider = "local";
        booking.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        _context.FlightBookings.Add(booking);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetFlightBooking), new { id = booking.Id }, booking);
    }

    [HttpPut("{id:int}/cancel")]
    public async Task<IActionResult> CancelFlightBooking(int id, CancellationToken ct)
    {
        var booking = await _context.FlightBookings
            .Include(b => b.Flight)
            .FirstOrDefaultAsync(b => b.Id == id, ct);

        if (booking == null) return NotFound();
        if (booking.Status == "Cancelled") return BadRequest("الحجز ملغى مسبقاً");

        if (!string.IsNullOrWhiteSpace(booking.ExternalOrderId))
        {
            await _provider.CancelAsync(booking.ExternalOrderId, ct);
        }

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync(ct);
        return NoContent();
    }
}
