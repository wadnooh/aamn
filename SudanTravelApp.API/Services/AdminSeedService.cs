using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public interface IAdminSeedService
{
    Task SeedAsync(CancellationToken ct = default);
}

public class AdminSeedService : IAdminSeedService
{
    public const string AdminRole = "Admin";

    private readonly UserManager<ApplicationUser> _users;
    private readonly RoleManager<IdentityRole> _roles;
    private readonly AdminOptions _options;
    private readonly ILogger<AdminSeedService> _logger;

    public AdminSeedService(
        UserManager<ApplicationUser> users,
        RoleManager<IdentityRole> roles,
        IOptions<AdminOptions> options,
        ILogger<AdminSeedService> logger)
    {
        _users = users;
        _roles = roles;
        _options = options.Value;
        _logger = logger;
    }

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (!await _roles.RoleExistsAsync(AdminRole))
        {
            var roleResult = await _roles.CreateAsync(new IdentityRole(AdminRole));
            if (!roleResult.Succeeded)
            {
                _logger.LogError("Failed to create Admin role: {Errors}",
                    string.Join(", ", roleResult.Errors.Select(e => e.Description)));
                return;
            }
        }

        var email = string.IsNullOrWhiteSpace(_options.Email) ? "admin@wadnooh.com" : _options.Email.Trim();
        var password = string.IsNullOrWhiteSpace(_options.Password) ? "Admin@123456" : _options.Password;
        var fullName = string.IsNullOrWhiteSpace(_options.FullName) ? "مدير الشركة" : _options.FullName.Trim();

        var user = await _users.FindByEmailAsync(email);
        if (user == null)
        {
            user = new ApplicationUser
            {
                UserName = email,
                Email = email,
                FullName = fullName,
                EmailConfirmed = true,
                CreatedAtUtc = DateTime.UtcNow
            };
            var create = await _users.CreateAsync(user, password);
            if (!create.Succeeded)
            {
                _logger.LogError("Failed to create admin user: {Errors}",
                    string.Join(", ", create.Errors.Select(e => e.Description)));
                return;
            }
            _logger.LogInformation("Admin user created: {Email}", email);
        }

        if (!await _users.IsInRoleAsync(user, AdminRole))
        {
            var add = await _users.AddToRoleAsync(user, AdminRole);
            if (!add.Succeeded)
            {
                _logger.LogError("Failed to assign Admin role: {Errors}",
                    string.Join(", ", add.Errors.Select(e => e.Description)));
            }
        }
    }
}
