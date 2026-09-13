using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HotelBookingsController : ControllerBase
{
    private readonly TravelDbContext _context;
    private readonly IMembershipService _membership;

    public HotelBookingsController(TravelDbContext context, IMembershipService membership)
    {
        _context = context;
        _membership = membership;
    }

    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<ActionResult<IEnumerable<HotelBooking>>> GetHotelBookings()
    {
        return await _context.HotelBookings.Include(b => b.Hotel).ToListAsync();
    }

    [Authorize]
    [HttpGet("mine")]
    public async Task<ActionResult<IEnumerable<HotelBooking>>> GetMyHotelBookings()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return await _context.HotelBookings
            .Include(b => b.Hotel)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.BookingDate)
            .ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<HotelBooking>> GetHotelBooking(int id)
    {
        var booking = await _context.HotelBookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        return booking;
    }

    [HttpGet("reference/{reference}")]
    public async Task<ActionResult<HotelBooking>> GetHotelBookingByReference(string reference)
    {
        var booking = await _context.HotelBookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.BookingReference == reference);

        if (booking == null) return NotFound();
        return booking;
    }

    [HttpPost]
    public async Task<ActionResult<object>> CreateHotelBooking(HotelBooking booking)
    {
        var hotel = await _context.Hotels.FindAsync(booking.HotelId);
        if (hotel == null) return BadRequest(new { message = "الفندق غير موجود" });
        if (hotel.AvailableRooms < booking.NumberOfRooms)
            return BadRequest(new { message = "عدد الغرف المتاحة غير كافٍ" });

        var nights = Math.Max(1, (booking.CheckOutDate - booking.CheckInDate).Days);
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var discount = await _membership.GetDiscountPercentAsync(userId);
        var original = hotel.PricePerNight * booking.NumberOfRooms * nights;
        var total = Math.Round(original * (1 - discount / 100m), 2);

        booking.BookingDate = DateTime.Now;
        booking.OriginalPrice = original;
        booking.DiscountPercent = discount;
        booking.TotalPrice = total;
        booking.Currency = "SDG";
        booking.BookingReference = $"HTL{DateTime.Now:yyyyMMddHHmmss}{Random.Shared.Next(1000, 9999)}";
        booking.Status = "PendingPayment";
        booking.PaymentStatus = "Unpaid";
        booking.UserId = userId;

        // Rooms reserved only after payment confirmation
        _context.HotelBookings.Add(booking);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetHotelBooking), new { id = booking.Id }, new
        {
            booking.Id,
            booking.BookingReference,
            booking.Status,
            booking.PaymentStatus,
            booking.TotalPrice,
            booking.OriginalPrice,
            booking.DiscountPercent,
            booking.Currency,
            requiresPayment = true,
            message = "تم إنشاء الحجز — أكمل الدفع للتأكيد"
        });
    }

    [HttpPut("{id}/cancel")]
    public async Task<IActionResult> CancelHotelBooking(int id)
    {
        var booking = await _context.HotelBookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == id);

        if (booking == null) return NotFound();
        if (booking.Status == "Cancelled") return BadRequest("الحجز ملغى مسبقاً");

        if (booking.Status == "Confirmed" && booking.Hotel != null)
        {
            booking.Hotel.AvailableRooms += booking.NumberOfRooms;
        }

        booking.Status = "Cancelled";
        await _context.SaveChangesAsync();
        return NoContent();
    }
}
