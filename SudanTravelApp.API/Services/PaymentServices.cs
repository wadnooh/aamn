using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Stripe;
using Stripe.Checkout;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public interface IPaymentService
{
    string ActiveProvider { get; }
    Task<CheckoutResponse> CreateCheckoutAsync(Payment payment, string successUrl, string cancelUrl, CancellationToken ct = default);
    Task<Payment?> CompleteDemoAsync(int paymentId, CancellationToken ct = default);
    Task<Payment?> HandleStripeWebhookAsync(string json, string signatureHeader, CancellationToken ct = default);
}

public interface IBookingFulfillmentService
{
    Task FulfillPaymentAsync(Payment payment, CancellationToken ct = default);
}

public class PaymentGatewayService : IPaymentService
{
    private readonly TravelDbContext _db;
    private readonly PaymentOptions _options;
    private readonly IBookingFulfillmentService _fulfillment;
    private readonly ILogger<PaymentGatewayService> _logger;

    public PaymentGatewayService(
        TravelDbContext db,
        IOptions<PaymentOptions> options,
        IBookingFulfillmentService fulfillment,
        ILogger<PaymentGatewayService> logger)
    {
        _db = db;
        _options = options.Value;
        _fulfillment = fulfillment;
        _logger = logger;
    }

    public string ActiveProvider
    {
        get
        {
            if (_options.Provider.Equals("demo", StringComparison.OrdinalIgnoreCase))
                return "demo";
            if (_options.Provider.Equals("stripe", StringComparison.OrdinalIgnoreCase))
                return "stripe";
            return string.IsNullOrWhiteSpace(_options.StripeSecretKey) ? "demo" : "stripe";
        }
    }

    public async Task<CheckoutResponse> CreateCheckoutAsync(
        Payment payment,
        string successUrl,
        string cancelUrl,
        CancellationToken ct = default)
    {
        payment.Provider = ActiveProvider;
        payment.Status = "Pending";
        payment.CreatedAtUtc = DateTime.UtcNow;

        if (payment.Id == 0)
        {
            _db.Payments.Add(payment);
            await _db.SaveChangesAsync(ct);
        }

        if (ActiveProvider == "stripe")
        {
            StripeConfiguration.ApiKey = _options.StripeSecretKey;
            var sessionService = new SessionService();
            var session = await sessionService.CreateAsync(new SessionCreateOptions
            {
                Mode = "payment",
                SuccessUrl = AppendQuery(successUrl, $"paymentId={payment.Id}&paid=1"),
                CancelUrl = AppendQuery(cancelUrl, $"paymentId={payment.Id}&paid=0"),
                ClientReferenceId = payment.Id.ToString(),
                Metadata = new Dictionary<string, string>
                {
                    ["paymentId"] = payment.Id.ToString(),
                    ["purpose"] = payment.Purpose
                },
                LineItems =
                [
                    new SessionLineItemOptions
                    {
                        Quantity = 1,
                        PriceData = new SessionLineItemPriceDataOptions
                        {
                            Currency = payment.Currency.ToLowerInvariant(),
                            UnitAmount = (long)Math.Round(payment.Amount * 100),
                            ProductData = new SessionLineItemPriceDataProductDataOptions
                            {
                                Name = string.IsNullOrWhiteSpace(payment.Description)
                                    ? "Wad Nooh Payment"
                                    : payment.Description
                            }
                        }
                    }
                ]
            }, cancellationToken: ct);

            payment.ExternalId = session.Id;
            payment.CheckoutUrl = session.Url;
            await _db.SaveChangesAsync(ct);
        }
        else
        {
            payment.ExternalId = $"demo_{payment.Id}_{Guid.NewGuid():N}"[..32];
            payment.CheckoutUrl = $"/?demoPay={payment.Id}";
            await _db.SaveChangesAsync(ct);
        }

        return new CheckoutResponse
        {
            PaymentId = payment.Id,
            Provider = payment.Provider,
            CheckoutUrl = payment.CheckoutUrl,
            Status = payment.Status,
            Amount = payment.Amount,
            Currency = payment.Currency
        };
    }

    public async Task<Payment?> CompleteDemoAsync(int paymentId, CancellationToken ct = default)
    {
        var payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        if (payment == null) return null;
        if (payment.Status == "Paid") return payment;
        if (!string.Equals(payment.Provider, "demo", StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException("هذا الدفع ليس تجريبياً");

        payment.Status = "Paid";
        payment.PaidAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync(ct);
        await _fulfillment.FulfillPaymentAsync(payment, ct);
        return payment;
    }

    public async Task<Payment?> HandleStripeWebhookAsync(string json, string signatureHeader, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(_options.StripeWebhookSecret))
        {
            _logger.LogWarning("Stripe webhook secret missing");
            return null;
        }

        var stripeEvent = EventUtility.ConstructEvent(json, signatureHeader, _options.StripeWebhookSecret);
        if (stripeEvent.Type != EventTypes.CheckoutSessionCompleted)
            return null;

        var session = stripeEvent.Data.Object as Session;
        if (session == null) return null;

        Payment? payment = null;
        if (session.Metadata != null &&
            session.Metadata.TryGetValue("paymentId", out var idText) &&
            int.TryParse(idText, out var paymentId))
        {
            payment = await _db.Payments.FirstOrDefaultAsync(p => p.Id == paymentId, ct);
        }

        payment ??= await _db.Payments.FirstOrDefaultAsync(p => p.ExternalId == session.Id, ct);
        if (payment == null) return null;
        if (payment.Status == "Paid") return payment;

        payment.Status = "Paid";
        payment.PaidAtUtc = DateTime.UtcNow;
        payment.ExternalId = session.Id;
        await _db.SaveChangesAsync(ct);
        await _fulfillment.FulfillPaymentAsync(payment, ct);
        return payment;
    }

    private static string AppendQuery(string url, string query)
    {
        if (string.IsNullOrWhiteSpace(url)) return "?" + query;
        return url.Contains('?', StringComparison.Ordinal) ? $"{url}&{query}" : $"{url}?{query}";
    }
}

public class BookingFulfillmentService : IBookingFulfillmentService
{
    private readonly TravelDbContext _db;
    private readonly IFlightProvider _provider;
    private readonly IFlightOfferCache _cache;
    private readonly ILogger<BookingFulfillmentService> _logger;

    public BookingFulfillmentService(
        TravelDbContext db,
        IFlightProvider provider,
        IFlightOfferCache cache,
        ILogger<BookingFulfillmentService> logger)
    {
        _db = db;
        _provider = provider;
        _cache = cache;
        _logger = logger;
    }

    public async Task FulfillPaymentAsync(Payment payment, CancellationToken ct = default)
    {
        if (payment.Purpose == "membership" && payment.MembershipPlanId.HasValue && !string.IsNullOrWhiteSpace(payment.UserId))
        {
            await ActivateMembershipAsync(payment, ct);
            return;
        }

        if (payment.Purpose == "booking" && payment.BookingType == "flight" && payment.BookingId.HasValue)
        {
            await ConfirmFlightBookingAsync(payment, ct);
            return;
        }

        if (payment.Purpose == "booking" && payment.BookingType == "hotel" && payment.BookingId.HasValue)
        {
            await ConfirmHotelBookingAsync(payment, ct);
        }
    }

    private async Task ActivateMembershipAsync(Payment payment, CancellationToken ct)
    {
        var plan = await _db.MembershipPlans.FindAsync([payment.MembershipPlanId!.Value], ct);
        if (plan == null) return;

        var now = DateTime.UtcNow;
        var existing = await _db.UserMemberships
            .Where(m => m.UserId == payment.UserId && m.Status == "Active")
            .ToListAsync(ct);
        foreach (var m in existing)
        {
            m.Status = "Expired";
        }

        _db.UserMemberships.Add(new UserMembership
        {
            UserId = payment.UserId!,
            PlanId = plan.Id,
            Status = "Active",
            StartUtc = now,
            EndUtc = now.AddDays(plan.DurationDays),
            PaymentId = payment.Id
        });
        await _db.SaveChangesAsync(ct);
    }

    private async Task ConfirmFlightBookingAsync(Payment payment, CancellationToken ct)
    {
        var booking = await _db.FlightBookings
            .Include(b => b.Flight)
            .FirstOrDefaultAsync(b => b.Id == payment.BookingId, ct);
        if (booking == null || booking.Status == "Confirmed") return;

        var offer = _cache.GetByOfferId(booking.OfferId);
        if (offer != null)
        {
            var request = new LiveBookingRequest
            {
                OfferId = booking.OfferId,
                PassengerName = booking.PassengerName,
                PassengerEmail = booking.PassengerEmail,
                PassengerPhone = booking.PassengerPhone,
                PassportNumber = booking.PassportNumber,
                NumberOfSeats = booking.NumberOfSeats
            };
            var result = await _provider.BookAsync(request, offer, ct);
            if (result.Success)
            {
                booking.ExternalOrderId = result.ExternalOrderId;
                booking.AirlineConfirmation = result.ExternalOrderId;
                booking.Provider = result.Source;
                booking.BookingReference = string.IsNullOrWhiteSpace(result.BookingReference)
                    ? booking.BookingReference
                    : result.BookingReference;
            }
            else
            {
                _logger.LogWarning("Airline book after payment failed for {Id}: {Msg}", booking.Id, result.Message);
                // Keep paid booking confirmed locally even if airline provider fails in demo.
            }
        }

        booking.Status = "Confirmed";
        booking.PaymentStatus = "Paid";
        booking.PaymentId = payment.Id;
        await _db.SaveChangesAsync(ct);
    }

    private async Task ConfirmHotelBookingAsync(Payment payment, CancellationToken ct)
    {
        var booking = await _db.HotelBookings
            .Include(b => b.Hotel)
            .FirstOrDefaultAsync(b => b.Id == payment.BookingId, ct);
        if (booking == null || booking.Status == "Confirmed") return;

        if (booking.Hotel != null && booking.Status == "PendingPayment")
        {
            if (booking.Hotel.AvailableRooms < booking.NumberOfRooms)
            {
                booking.Status = "Failed";
                booking.PaymentStatus = "Paid";
                await _db.SaveChangesAsync(ct);
                return;
            }
            booking.Hotel.AvailableRooms -= booking.NumberOfRooms;
        }

        booking.Status = "Confirmed";
        booking.PaymentStatus = "Paid";
        booking.PaymentId = payment.Id;
        await _db.SaveChangesAsync(ct);
    }
}
