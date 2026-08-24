namespace SudanTravelApp.API.Models;

/// <summary>Per-member saved lecture / محاضرة محفوظة.</summary>
public class MemberLecture
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public ApplicationUser? User { get; set; }

    public string TitleAr { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? Subject { get; set; }
    public string? SpecialtyId { get; set; }
    public string? CourseId { get; set; }
    public string? LessonId { get; set; }
    public string? Notes { get; set; }
    public DateTime? LectureDate { get; set; }
    public int? DurationMinutes { get; set; }

    /// <summary>JSON array of { url, filename, type }.</summary>
    public string? AttachmentsJson { get; set; }

    /// <summary>JSON string array of tags.</summary>
    public string? TagsJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;
}
