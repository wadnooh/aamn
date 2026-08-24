namespace SudanTravelApp.API.Models.Dtos;

public class AdminStatsDto
{
    public int UsersCount { get; set; }
    public int FlightBookingsCount { get; set; }
    public int HotelBookingsCount { get; set; }
    public int PendingPaymentsCount { get; set; }
    public int PaidPaymentsCount { get; set; }
    public decimal PaidRevenue { get; set; }
    public string RevenueCurrency { get; set; } = "USD";
    public int ActiveMembershipsCount { get; set; }
    public int ConfirmedFlightsCount { get; set; }
    public int ConfirmedHotelsCount { get; set; }
    public int PendingBookingsCount { get; set; }
    public int HotelsCount { get; set; }
    public int AttractionsCount { get; set; }
}

public class PagedResult<T>
{
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int Total { get; set; }
    public IReadOnlyList<T> Items { get; set; } = [];
}

public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? PassportNumber { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool Locked { get; set; }
    public bool EmailConfirmed { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
    public string MembershipPlan { get; set; } = "free";
    public int FlightBookings { get; set; }
    public int HotelBookings { get; set; }
    public int PaymentsCount { get; set; }
}

public class AdminPaymentDto
{
    public int Id { get; set; }
    public string? UserId { get; set; }
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string Purpose { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? BookingType { get; set; }
    public int? BookingId { get; set; }
    public int? MembershipPlanId { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ExternalId { get; set; } = string.Empty;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? PaidAtUtc { get; set; }
}

public class AdminFlightBookingDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string PassengerName { get; set; } = string.Empty;
    public string PassengerEmail { get; set; } = string.Empty;
    public string PassengerPhone { get; set; } = string.Empty;
    public int NumberOfSeats { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal OriginalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string Provider { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int? PaymentId { get; set; }
    public DateTime BookingDate { get; set; }
    public string? Airline { get; set; }
    public string? FlightNumber { get; set; }
    public string? Route { get; set; }
}

public class AdminHotelBookingDto
{
    public int Id { get; set; }
    public string BookingReference { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string PaymentStatus { get; set; } = string.Empty;
    public string GuestName { get; set; } = string.Empty;
    public string GuestEmail { get; set; } = string.Empty;
    public string GuestPhone { get; set; } = string.Empty;
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfRooms { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public decimal DiscountPercent { get; set; }
    public string Currency { get; set; } = string.Empty;
    public string? UserId { get; set; }
    public int? PaymentId { get; set; }
    public DateTime BookingDate { get; set; }
    public string? HotelName { get; set; }
    public string? City { get; set; }
}

public class AdminMembershipDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string? UserEmail { get; set; }
    public string? UserName { get; set; }
    public string PlanCode { get; set; } = string.Empty;
    public string PlanNameAr { get; set; } = string.Empty;
    public string PlanNameEn { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public decimal DiscountPercent { get; set; }
    public DateTime StartUtc { get; set; }
    public DateTime EndUtc { get; set; }
    public int? PaymentId { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class AdminStatusUpdateRequest
{
    public string Status { get; set; } = string.Empty;
}

public class AdminLockUserRequest
{
    public bool Locked { get; set; }
}

public class AdminNotificationDto
{
    public int Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string PayloadJson { get; set; } = "{}";
    public object? Payload { get; set; }
    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}

public class AdminMemberDto
{
    public string Id { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public bool EmailConfirmed { get; set; }
    public IReadOnlyList<string> Roles { get; set; } = [];
}
