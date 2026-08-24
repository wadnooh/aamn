using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    private readonly TravelDbContext _db;
    private readonly IPaymentService _payments;
    private readonly PaymentOptions _options;
    private readonly IConfiguration _config;
    private readonly ICurrencyService _fx;

    public PaymentsController(
        TravelDbContext db,
        IPaymentService payments,
        IOptions<PaymentOptions> options,
        IConfiguration config,
        ICurrencyService fx)
    {
        _db = db;
        _payments = payments;
        _options = options.Value;
        _config = config;
        _fx = fx;
    }

    [HttpGet("provider")]
    public ActionResult GetProvider() => Ok(new
    {
        provider = _payments.ActiveProvider,
        currency = _options.Currency,
        stripePublishableKey = _options.StripePublishableKey
    });

    [Authorize]
    [HttpPost("checkout")]
    public async Task<ActionResult<CheckoutResponse>> Checkout([FromBody] CheckoutRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

        if (string.Equals(request.Purpose, "membership", StringComparison.OrdinalIgnoreCase))
        {
            return BadRequest(new { message = "استخدم /api/membership/subscribe للاشتراك" });
        }

        if (!request.BookingId.HasValue || string.IsNullOrWhiteSpace(request.BookingType))
            return BadRequest(new { message = "بيانات الحجز مطلوبة" });

        decimal amount;
        string currency;
        string description;

        if (request.BookingType == "flight")
        {
            var booking = await _db.FlightBookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, ct);
            if (booking == null) return NotFound(new { message = "الحجز غير موجود" });
            if (booking.UserId != null && booking.UserId != userId)
                return Forbid();
            amount = booking.TotalPrice;
            currency = string.IsNullOrWhiteSpace(booking.Currency) ? "SDG" : booking.Currency;
            if (!currency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                amount = Math.Max(1, _fx.Convert(booking.TotalPrice, currency, "USD"));
                currency = "USD";
            }
            description = $"Flight booking {booking.BookingReference}";
            booking.UserId ??= userId;
        }
        else if (request.BookingType == "hotel")
        {
            var booking = await _db.HotelBookings.FirstOrDefaultAsync(b => b.Id == request.BookingId, ct);
            if (booking == null) return NotFound(new { message = "الحجز غير موجود" });
            if (booking.UserId != null && booking.UserId != userId)
                return Forbid();
            amount = booking.TotalPrice;
            currency = string.IsNullOrWhiteSpace(booking.Currency) ? "SDG" : booking.Currency;
            if (!currency.Equals("USD", StringComparison.OrdinalIgnoreCase))
            {
                amount = Math.Max(1, _fx.Convert(booking.TotalPrice, currency, "USD"));
                currency = "USD";
            }
            description = $"Hotel booking {booking.BookingReference}";
            booking.UserId ??= userId;
        }
        else
        {
            return BadRequest(new { message = "نوع الحجز غير صالح" });
        }

        var payment = new Payment
        {
            UserId = userId,
            Amount = amount,
            Currency = currency,
            Purpose = "booking",
            BookingType = request.BookingType,
            BookingId = request.BookingId,
            Description = description
        };

        var publicBase = _config["PublicBaseUrl"] ?? "https://wadnooh.com";
        var success = string.IsNullOrWhiteSpace(_options.SuccessUrl) ? $"{publicBase}/?paid=1" : _options.SuccessUrl;
        var cancel = string.IsNullOrWhiteSpace(_options.CancelUrl) ? $"{publicBase}/?paid=0" : _options.CancelUrl;

        var checkout = await _payments.CreateCheckoutAsync(payment, success, cancel, ct);

        if (request.BookingType == "flight")
        {
            var booking = await _db.FlightBookings.FindAsync([request.BookingId.Value], ct);
            if (booking != null)
            {
                booking.PaymentId = checkout.PaymentId;
                booking.PaymentStatus = "Pending";
                await _db.SaveChangesAsync(ct);
            }
        }
        else if (request.BookingType == "hotel")
        {
            var booking = await _db.HotelBookings.FindAsync([request.BookingId.Value], ct);
            if (booking != null)
            {
                booking.PaymentId = checkout.PaymentId;
                booking.PaymentStatus = "Pending";
                await _db.SaveChangesAsync(ct);
            }
        }

        return Ok(checkout);
    }

    [HttpPost("demo/complete")]
    public async Task<ActionResult> CompleteDemo([FromBody] DemoCompleteRequest request, CancellationToken ct)
    {
        if (request.PaymentId <= 0) return BadRequest(new { message = "paymentId مطلوب" });
        try
        {
            var payment = await _payments.CompleteDemoAsync(request.PaymentId, ct);
            if (payment == null) return NotFound(new { message = "الدفع غير موجود" });
            return Ok(new
            {
                payment.Id,
                payment.Status,
                payment.Purpose,
                payment.BookingType,
                payment.BookingId,
                payment.Amount,
                payment.Currency,
                message = "تم الدفع التجريبي بنجاح"
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("webhook")]
    public async Task<IActionResult> StripeWebhook(CancellationToken ct)
    {
        var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync(ct);
        var signature = Request.Headers["Stripe-Signature"].ToString();
        try
        {
            await _payments.HandleStripeWebhookAsync(json, signature, ct);
            return Ok();
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [Authorize]
    [HttpGet("{id:int}")]
    public async Task<ActionResult> GetPayment(int id, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (payment == null) return NotFound();
        if (payment.UserId != null && payment.UserId != userId) return Forbid();
        return Ok(payment);
    }
}

public class DemoCompleteRequest
{
    public int PaymentId { get; set; }
}
