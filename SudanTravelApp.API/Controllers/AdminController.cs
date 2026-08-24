using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = AdminSeedService.AdminRole)]
public class AdminController : ControllerBase
{
    private static readonly JsonSerializerOptions PayloadJsonOpts = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private readonly TravelDbContext _db;
    private readonly UserManager<ApplicationUser> _users;

    public AdminController(TravelDbContext db, UserManager<ApplicationUser> users)
    {
        _db = db;
        _users = users;
    }

    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> Stats(CancellationToken ct)
    {
        var now = DateTime.UtcNow;
        var paid = await _db.Payments.Where(p => p.Status == "Paid").ToListAsync(ct);
        return Ok(new AdminStatsDto
        {
            UsersCount = await _users.Users.CountAsync(ct),
            FlightBookingsCount = await _db.FlightBookings.CountAsync(ct),
            HotelBookingsCount = await _db.HotelBookings.CountAsync(ct),
            PendingPaymentsCount = await _db.Payments.CountAsync(p => p.Status == "Pending", ct),
            PaidPaymentsCount = paid.Count,
            PaidRevenue = paid.Sum(p => p.Amount),
            RevenueCurrency = "USD",
            ActiveMembershipsCount = await _db.UserMemberships.CountAsync(
                m => m.Status == "Active" && m.EndUtc >= now, ct),
            ConfirmedFlightsCount = await _db.FlightBookings.CountAsync(b => b.Status == "Confirmed", ct),
            ConfirmedHotelsCount = await _db.HotelBookings.CountAsync(b => b.Status == "Confirmed", ct),
            PendingBookingsCount =
                await _db.FlightBookings.CountAsync(b => b.Status == "PendingPayment", ct) +
                await _db.HotelBookings.CountAsync(b => b.Status == "PendingPayment", ct),
            HotelsCount = await _db.Hotels.CountAsync(ct),
            AttractionsCount = await _db.TouristAttractions.CountAsync(ct)
        });
    }

    [HttpGet("notifications")]
    public async Task<ActionResult<PagedResult<AdminNotificationDto>>> Notifications(
        [FromQuery] bool? unreadOnly,
        [FromQuery] string? type,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.AdminNotifications.AsQueryable();
        if (unreadOnly == true)
            q = q.Where(n => !n.IsRead);
        if (!string.IsNullOrWhiteSpace(type))
            q = q.Where(n => n.Type == type.Trim());

        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(n => n.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = rows.Select(MapNotification).ToList();
        return Ok(new PagedResult<AdminNotificationDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpGet("notifications/unread-count")]
    public async Task<ActionResult> UnreadNotificationsCount(CancellationToken ct)
    {
        var count = await _db.AdminNotifications.CountAsync(n => !n.IsRead, ct);
        return Ok(new { unread = count });
    }

    [HttpPost("notifications/{id:int}/read")]
    public async Task<ActionResult> MarkNotificationRead(int id, CancellationToken ct)
    {
        var row = await _db.AdminNotifications.FindAsync([id], ct);
        if (row == null) return NotFound(new { message = "الإشعار غير موجود" });
        row.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return Ok(new { id = row.Id, isRead = true });
    }

    [HttpPost("notifications/read-all")]
    public async Task<ActionResult> MarkAllNotificationsRead(CancellationToken ct)
    {
        var unread = await _db.AdminNotifications.Where(n => !n.IsRead).ToListAsync(ct);
        foreach (var n in unread) n.IsRead = true;
        await _db.SaveChangesAsync(ct);
        return Ok(new { marked = unread.Count });
    }

    /// <summary>Lightweight member list (email, name, created, roles).</summary>
    [HttpGet("members")]
    public async Task<ActionResult<PagedResult<AdminMemberDto>>> Members(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _users.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(u =>
                (u.Email != null && u.Email.Contains(s)) ||
                u.FullName.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var users = await q.OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var items = new List<AdminMemberDto>();
        foreach (var u in users)
        {
            var roles = await _users.GetRolesAsync(u);
            items.Add(new AdminMemberDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.PhoneNumber,
                CreatedAtUtc = u.CreatedAtUtc,
                EmailConfirmed = u.EmailConfirmed,
                Roles = roles.ToList()
            });
        }

        return Ok(new PagedResult<AdminMemberDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpGet("users")]
    public async Task<ActionResult<PagedResult<AdminUserDto>>> Users(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _users.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(u =>
                (u.Email != null && u.Email.Contains(s)) ||
                u.FullName.Contains(s) ||
                (u.PhoneNumber != null && u.PhoneNumber.Contains(s)));
        }

        var total = await q.CountAsync(ct);
        var users = await q.OrderByDescending(u => u.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var now = DateTime.UtcNow;
        var userIds = users.Select(u => u.Id).ToList();
        var memberships = await _db.UserMemberships
            .Include(m => m.Plan)
            .Where(m => userIds.Contains(m.UserId) && m.Status == "Active" && m.EndUtc >= now)
            .ToListAsync(ct);
        var flightCounts = await _db.FlightBookings
            .Where(b => b.UserId != null && userIds.Contains(b.UserId))
            .GroupBy(b => b.UserId!)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var hotelCounts = await _db.HotelBookings
            .Where(b => b.UserId != null && userIds.Contains(b.UserId))
            .GroupBy(b => b.UserId!)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync(ct);
        var payCounts = await _db.Payments
            .Where(p => p.UserId != null && userIds.Contains(p.UserId))
            .GroupBy(p => p.UserId!)
            .Select(g => new { UserId = g.Key, Count = g.Count() })
            .ToListAsync(ct);

        var items = new List<AdminUserDto>();
        foreach (var u in users)
        {
            var roles = await _users.GetRolesAsync(u);
            var mem = memberships.FirstOrDefault(m => m.UserId == u.Id);
            items.Add(new AdminUserDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email ?? string.Empty,
                Phone = u.PhoneNumber,
                PassportNumber = u.PassportNumber,
                CreatedAtUtc = u.CreatedAtUtc,
                Locked = u.LockoutEnd.HasValue && u.LockoutEnd > DateTimeOffset.UtcNow,
                EmailConfirmed = u.EmailConfirmed,
                Roles = roles.ToList(),
                MembershipPlan = mem?.Plan?.Code ?? "free",
                FlightBookings = flightCounts.FirstOrDefault(x => x.UserId == u.Id)?.Count ?? 0,
                HotelBookings = hotelCounts.FirstOrDefault(x => x.UserId == u.Id)?.Count ?? 0,
                PaymentsCount = payCounts.FirstOrDefault(x => x.UserId == u.Id)?.Count ?? 0
            });
        }

        return Ok(new PagedResult<AdminUserDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpPost("users/{id}/lock")]
    public async Task<ActionResult> LockUser(string id, [FromBody] AdminLockUserRequest request)
    {
        var user = await _users.FindByIdAsync(id);
        if (user == null) return NotFound(new { message = "المستخدم غير موجود" });

        if (await _users.IsInRoleAsync(user, AdminSeedService.AdminRole) && request.Locked)
            return BadRequest(new { message = "لا يمكن قفل حساب الأدمن" });

        if (request.Locked)
        {
            await _users.SetLockoutEnabledAsync(user, true);
            await _users.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));
        }
        else
        {
            await _users.SetLockoutEndDateAsync(user, null);
        }

        return Ok(new { message = request.Locked ? "تم قفل الحساب" : "تم فتح الحساب", locked = request.Locked });
    }

    [HttpGet("payments")]
    public async Task<ActionResult<PagedResult<AdminPaymentDto>>> Payments(
        [FromQuery] string? status,
        [FromQuery] string? purpose,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.Payments.AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(p => p.Status == status);
        if (!string.IsNullOrWhiteSpace(purpose))
            q = q.Where(p => p.Purpose == purpose);

        var total = await q.CountAsync(ct);
        var payments = await q.OrderByDescending(p => p.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = payments.Where(p => p.UserId != null).Select(p => p.UserId!).Distinct().ToList();
        var users = await _users.Users.Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.FullName })
            .ToListAsync(ct);
        var map = users.ToDictionary(u => u.Id);

        var items = payments.Select(p =>
        {
            map.TryGetValue(p.UserId ?? "", out var u);
            return new AdminPaymentDto
            {
                Id = p.Id,
                UserId = p.UserId,
                UserEmail = u?.Email,
                UserName = u?.FullName,
                Amount = p.Amount,
                Currency = p.Currency,
                Purpose = p.Purpose,
                Status = p.Status,
                Provider = p.Provider,
                BookingType = p.BookingType,
                BookingId = p.BookingId,
                MembershipPlanId = p.MembershipPlanId,
                Description = p.Description,
                ExternalId = p.ExternalId,
                CreatedAtUtc = p.CreatedAtUtc,
                PaidAtUtc = p.PaidAtUtc
            };
        }).ToList();

        return Ok(new PagedResult<AdminPaymentDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpGet("bookings/flights")]
    public async Task<ActionResult<PagedResult<AdminFlightBookingDto>>> FlightBookings(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.FlightBookings.Include(b => b.Flight).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(b => b.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(b =>
                b.BookingReference.Contains(s) ||
                b.PassengerName.Contains(s) ||
                b.PassengerEmail.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new AdminFlightBookingDto
            {
                Id = b.Id,
                BookingReference = b.BookingReference,
                Status = b.Status,
                PaymentStatus = b.PaymentStatus,
                PassengerName = b.PassengerName,
                PassengerEmail = b.PassengerEmail,
                PassengerPhone = b.PassengerPhone,
                NumberOfSeats = b.NumberOfSeats,
                TotalPrice = b.TotalPrice,
                OriginalPrice = b.OriginalPrice,
                DiscountPercent = b.DiscountPercent,
                Currency = b.Currency,
                Provider = b.Provider,
                UserId = b.UserId,
                PaymentId = b.PaymentId,
                BookingDate = b.BookingDate,
                Airline = b.Flight != null ? b.Flight.Airline : null,
                FlightNumber = b.Flight != null ? b.Flight.FlightNumber : null,
                Route = b.Flight != null
                    ? b.Flight.DepartureCity + " → " + b.Flight.ArrivalCity
                    : null
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminFlightBookingDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpGet("bookings/hotels")]
    public async Task<ActionResult<PagedResult<AdminHotelBookingDto>>> HotelBookings(
        [FromQuery] string? status,
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HotelBookings.Include(b => b.Hotel).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(b => b.Status == status);
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            q = q.Where(b =>
                b.BookingReference.Contains(s) ||
                b.GuestName.Contains(s) ||
                b.GuestEmail.Contains(s));
        }

        var total = await q.CountAsync(ct);
        var items = await q.OrderByDescending(b => b.BookingDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(b => new AdminHotelBookingDto
            {
                Id = b.Id,
                BookingReference = b.BookingReference,
                Status = b.Status,
                PaymentStatus = b.PaymentStatus,
                GuestName = b.GuestName,
                GuestEmail = b.GuestEmail,
                GuestPhone = b.GuestPhone,
                CheckInDate = b.CheckInDate,
                CheckOutDate = b.CheckOutDate,
                NumberOfRooms = b.NumberOfRooms,
                NumberOfGuests = b.NumberOfGuests,
                TotalPrice = b.TotalPrice,
                DiscountPercent = b.DiscountPercent,
                Currency = b.Currency,
                UserId = b.UserId,
                PaymentId = b.PaymentId,
                BookingDate = b.BookingDate,
                HotelName = b.Hotel != null ? b.Hotel.Name : null,
                City = b.Hotel != null ? b.Hotel.City : null
            })
            .ToListAsync(ct);

        return Ok(new PagedResult<AdminHotelBookingDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpPut("bookings/flights/{id}/status")]
    public async Task<ActionResult> UpdateFlightStatus(int id, [FromBody] AdminStatusUpdateRequest request, CancellationToken ct)
    {
        var booking = await _db.FlightBookings.FindAsync([id], ct);
        if (booking == null) return NotFound(new { message = "الحجز غير موجود" });
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(new { message = "الحالة مطلوبة" });

        booking.Status = request.Status.Trim();
        if (booking.Status.Equals("Cancelled", StringComparison.OrdinalIgnoreCase))
            booking.PaymentStatus = booking.PaymentStatus == "Paid" ? "RefundPending" : booking.PaymentStatus;
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "تم تحديث حالة حجز الطيران", booking.Id, booking.Status });
    }

    [HttpPut("bookings/hotels/{id}/status")]
    public async Task<ActionResult> UpdateHotelStatus(int id, [FromBody] AdminStatusUpdateRequest request, CancellationToken ct)
    {
        var booking = await _db.HotelBookings.FindAsync([id], ct);
        if (booking == null) return NotFound(new { message = "الحجز غير موجود" });
        if (string.IsNullOrWhiteSpace(request.Status))
            return BadRequest(new { message = "الحالة مطلوبة" });

        booking.Status = request.Status.Trim();
        await _db.SaveChangesAsync(ct);
        return Ok(new { message = "تم تحديث حالة حجز الفندق", booking.Id, booking.Status });
    }

    [HttpGet("memberships")]
    public async Task<ActionResult<PagedResult<AdminMembershipDto>>> Memberships(
        [FromQuery] string? status,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.UserMemberships.Include(m => m.Plan).AsQueryable();
        if (!string.IsNullOrWhiteSpace(status))
            q = q.Where(m => m.Status == status);

        var total = await q.CountAsync(ct);
        var rows = await q.OrderByDescending(m => m.CreatedAtUtc)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

        var userIds = rows.Select(m => m.UserId).Distinct().ToList();
        var users = await _users.Users.Where(u => userIds.Contains(u.Id))
            .Select(u => new { u.Id, u.Email, u.FullName })
            .ToListAsync(ct);
        var map = users.ToDictionary(u => u.Id);

        var items = rows.Select(m =>
        {
            map.TryGetValue(m.UserId, out var u);
            return new AdminMembershipDto
            {
                Id = m.Id,
                UserId = m.UserId,
                UserEmail = u?.Email,
                UserName = u?.FullName,
                PlanCode = m.Plan?.Code ?? "",
                PlanNameAr = m.Plan?.NameAr ?? "",
                PlanNameEn = m.Plan?.NameEn ?? "",
                Status = m.Status,
                DiscountPercent = m.Plan?.DiscountPercent ?? 0,
                StartUtc = m.StartUtc,
                EndUtc = m.EndUtc,
                PaymentId = m.PaymentId,
                CreatedAtUtc = m.CreatedAtUtc
            };
        }).ToList();

        return Ok(new PagedResult<AdminMembershipDto>
        {
            Page = page,
            PageSize = pageSize,
            Total = total,
            Items = items
        });
    }

    [HttpGet("hotels")]
    public async Task<ActionResult> Hotels(CancellationToken ct)
    {
        var hotels = await _db.Hotels
            .OrderBy(h => h.City)
            .ThenBy(h => h.Name)
            .Select(h => new
            {
                h.Id,
                h.Name,
                h.City,
                h.Address,
                h.StarRating,
                h.PricePerNight,
                h.AvailableRooms,
                h.Description
            })
            .ToListAsync(ct);
        return Ok(hotels);
    }

    [HttpPut("hotels/{id}")]
    public async Task<ActionResult> UpdateHotel(int id, [FromBody] Hotel update, CancellationToken ct)
    {
        var hotel = await _db.Hotels.FindAsync([id], ct);
        if (hotel == null) return NotFound(new { message = "الفندق غير موجود" });

        if (!string.IsNullOrWhiteSpace(update.Name)) hotel.Name = update.Name.Trim();
        if (!string.IsNullOrWhiteSpace(update.City)) hotel.City = update.City.Trim();
        if (!string.IsNullOrWhiteSpace(update.Address)) hotel.Address = update.Address.Trim();
        if (update.StarRating > 0) hotel.StarRating = update.StarRating;
        if (update.PricePerNight > 0) hotel.PricePerNight = update.PricePerNight;
        if (update.AvailableRooms >= 0) hotel.AvailableRooms = update.AvailableRooms;
        if (update.Description != null) hotel.Description = update.Description;

        await _db.SaveChangesAsync(ct);
        return Ok(hotel);
    }

    private static AdminNotificationDto MapNotification(AdminNotification n)
    {
        object? payload = null;
        try
        {
            if (!string.IsNullOrWhiteSpace(n.PayloadJson))
                payload = JsonSerializer.Deserialize<JsonElement>(n.PayloadJson, PayloadJsonOpts);
        }
        catch
        {
            payload = n.PayloadJson;
        }

        return new AdminNotificationDto
        {
            Id = n.Id,
            Type = n.Type,
            Title = n.Title,
            PayloadJson = n.PayloadJson,
            Payload = payload,
            IsRead = n.IsRead,
            CreatedAtUtc = n.CreatedAtUtc
        };
    }
}
