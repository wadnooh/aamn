using System.ComponentModel.DataAnnotations;

namespace SudanTravelApp.API.Models.Dtos;

public class LectureAttachmentDto
{
    public string? Url { get; set; }
    public string? Filename { get; set; }
    public string? Type { get; set; }
}

public class MemberLectureDto
{
    public int Id { get; set; }
    public string TitleAr { get; set; } = string.Empty;
    public string? TitleEn { get; set; }
    public string? Subject { get; set; }
    public string? SpecialtyId { get; set; }
    public string? CourseId { get; set; }
    public string? LessonId { get; set; }
    public string? Notes { get; set; }
    public DateTime? LectureDate { get; set; }
    public int? DurationMinutes { get; set; }
    public List<LectureAttachmentDto> Attachments { get; set; } = [];
    public List<string> Tags { get; set; } = [];
    public DateTime CreatedAtUtc { get; set; }
    public DateTime UpdatedAtUtc { get; set; }
}

public class MemberLectureUpsertRequest
{
    [Required]
    [MaxLength(300)]
    public string TitleAr { get; set; } = string.Empty;

    [MaxLength(300)]
    public string? TitleEn { get; set; }

    [MaxLength(200)]
    public string? Subject { get; set; }

    [MaxLength(80)]
    public string? SpecialtyId { get; set; }

    [MaxLength(80)]
    public string? CourseId { get; set; }

    [MaxLength(80)]
    public string? LessonId { get; set; }

    [MaxLength(8000)]
    public string? Notes { get; set; }

    public DateTime? LectureDate { get; set; }
    public int? DurationMinutes { get; set; }
    public List<LectureAttachmentDto>? Attachments { get; set; }
    public List<string>? Tags { get; set; }
}
