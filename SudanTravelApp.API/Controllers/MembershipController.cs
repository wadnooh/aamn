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
[Route("api/membership")]
public class MembershipController : ControllerBase
{
    private readonly TravelDbContext _db;
    private readonly IMembershipService _membership;
    private readonly IPaymentService _payments;
    private readonly PaymentOptions _paymentOptions;
    private readonly IConfiguration _config;

    public MembershipController(
        TravelDbContext db,
        IMembershipService membership,
        IPaymentService payments,
        IOptions<PaymentOptions> paymentOptions,
        IConfiguration config)
    {
        _db = db;
        _membership = membership;
        _payments = payments;
        _paymentOptions = paymentOptions.Value;
        _config = config;
    }

    [HttpGet("plans")]
    public async Task<ActionResult> GetPlans()
    {
        var plans = await _db.MembershipPlans
            .Where(p => p.IsActive)
            .OrderBy(p => p.SortOrder)
            .Select(p => new
            {
                p.Id,
                p.Code,
                p.NameAr,
                p.NameEn,
                p.DescriptionAr,
                p.DescriptionEn,
                p.Price,
                p.Currency,
                p.DiscountPercent,
                p.DurationDays
            })
            .ToListAsync();
        return Ok(plans);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<ActionResult<MembershipInfoDto>> GetMyMembership()
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        return Ok(await _membership.GetActiveMembershipAsync(userId));
    }

    [Authorize]
    [HttpPost("subscribe")]
    public async Task<ActionResult<CheckoutResponse>> Subscribe([FromBody] CheckoutRequest request)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();
        if (!request.MembershipPlanId.HasValue)
            return BadRequest(new { message = "اختر باقة العضوية" });

        var plan = await _db.MembershipPlans.FirstOrDefaultAsync(p => p.Id == request.MembershipPlanId && p.IsActive);
        if (plan == null) return NotFound(new { message = "الباقة غير موجودة" });

        var payment = new Payment
        {
            UserId = userId,
            Amount = plan.Price,
            Currency = plan.Currency,
            Purpose = "membership",
            MembershipPlanId = plan.Id,
            BookingType = "membership",
            Description = $"Wad Nooh {plan.NameEn} Membership"
        };

        var publicBase = _config["PublicBaseUrl"] ?? "https://wadnooh.com";
        var success = string.IsNullOrWhiteSpace(_paymentOptions.SuccessUrl) ? $"{publicBase}/?paid=1" : _paymentOptions.SuccessUrl;
        var cancel = string.IsNullOrWhiteSpace(_paymentOptions.CancelUrl) ? $"{publicBase}/?paid=0" : _paymentOptions.CancelUrl;

        var checkout = await _payments.CreateCheckoutAsync(payment, success, cancel);
        return Ok(checkout);
    }
}
