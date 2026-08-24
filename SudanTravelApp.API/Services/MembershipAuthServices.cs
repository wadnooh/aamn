using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public interface IJwtTokenService
{
    AuthResponse CreateToken(ApplicationUser user, MembershipInfoDto? membership = null, IEnumerable<string>? roles = null);
}

public class JwtTokenService : IJwtTokenService
{
    private readonly JwtOptions _options;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
    }

    public AuthResponse CreateToken(ApplicationUser user, MembershipInfoDto? membership = null, IEnumerable<string>? roles = null)
    {
        var roleList = roles?.ToList() ?? [];
        var expires = DateTime.UtcNow.AddHours(Math.Max(1, _options.ExpiresHours));
        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id),
            new(JwtRegisteredClaimNames.Email, user.Email ?? string.Empty),
            new(ClaimTypes.NameIdentifier, user.Id),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email ?? string.Empty)
        };
        foreach (var role in roleList)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Key));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            expires: expires,
            signingCredentials: creds);

        return new AuthResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            ExpiresAtUtc = expires,
            User = new UserProfileDto
            {
                Id = user.Id,
                FullName = user.FullName,
                Email = user.Email ?? string.Empty,
                Phone = user.PhoneNumber,
                PassportNumber = user.PassportNumber,
                EmailConfirmed = user.EmailConfirmed,
                Membership = membership,
                Roles = roleList
            }
        };
    }
}

public interface IMembershipService
{
    Task<MembershipInfoDto> GetActiveMembershipAsync(string userId, CancellationToken ct = default);
    Task<decimal> GetDiscountPercentAsync(string? userId, CancellationToken ct = default);
    Task SeedPlansAsync(CancellationToken ct = default);
}

public class MembershipService : IMembershipService
{
    private readonly TravelDbContext _db;

    public MembershipService(TravelDbContext db)
    {
        _db = db;
    }

    public async Task SeedPlansAsync(CancellationToken ct = default)
    {
        if (await _db.MembershipPlans.AnyAsync(ct)) return;

        _db.MembershipPlans.AddRange(
            new MembershipPlan
            {
                Code = "basic",
                NameAr = "أساسية",
                NameEn = "Basic",
                DescriptionAr = "خصم 5% على الحجوزات لمدة سنة",
                DescriptionEn = "5% off bookings for one year",
                Price = 29,
                Currency = "USD",
                DiscountPercent = 5,
                DurationDays = 365,
                SortOrder = 1
            },
            new MembershipPlan
            {
                Code = "silver",
                NameAr = "فضية",
                NameEn = "Silver",
                DescriptionAr = "خصم 10% على الحجوزات + أولوية دعم",
                DescriptionEn = "10% off bookings + priority support",
                Price = 79,
                Currency = "USD",
                DiscountPercent = 10,
                DurationDays = 365,
                SortOrder = 2
            },
            new MembershipPlan
            {
                Code = "gold",
                NameAr = "ذهبية",
                NameEn = "Gold",
                DescriptionAr = "خصم 15% على الحجوزات + مزايا VIP",
                DescriptionEn = "15% off bookings + VIP benefits",
                Price = 149,
                Currency = "USD",
                DiscountPercent = 15,
                DurationDays = 365,
                SortOrder = 3
            });

        await _db.SaveChangesAsync(ct);
    }

    public async Task<MembershipInfoDto> GetActiveMembershipAsync(string userId, CancellationToken ct = default)
    {
        var now = DateTime.UtcNow;
        var active = await _db.UserMemberships
            .Include(m => m.Plan)
            .Where(m => m.UserId == userId && m.Status == "Active" && m.EndUtc >= now)
            .OrderByDescending(m => m.EndUtc)
            .FirstOrDefaultAsync(ct);

        if (active?.Plan == null)
        {
            return new MembershipInfoDto();
        }

        return new MembershipInfoDto
        {
            PlanCode = active.Plan.Code,
            PlanNameAr = active.Plan.NameAr,
            PlanNameEn = active.Plan.NameEn,
            DiscountPercent = active.Plan.DiscountPercent,
            Status = active.Status,
            EndUtc = active.EndUtc
        };
    }

    public async Task<decimal> GetDiscountPercentAsync(string? userId, CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(userId)) return 0;
        var info = await GetActiveMembershipAsync(userId, ct);
        return info.DiscountPercent;
    }
}
