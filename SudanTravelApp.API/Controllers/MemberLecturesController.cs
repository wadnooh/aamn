using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;
using SudanTravelApp.API.Models.Dtos;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Authorize]
[Route("api/me/lectures")]
public class MemberLecturesController : ControllerBase
{
    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private readonly TravelDbContext _db;

    public MemberLecturesController(TravelDbContext db) => _db = db;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<MemberLectureDto>>> List([FromQuery] string? q, [FromQuery] string? tag)
    {
        var userId = RequireUserId();
        if (userId is null) return Unauthorized();

        var query = _db.MemberLectures.AsNoTracking().Where(x => x.UserId == userId);

        if (!string.IsNullOrWhiteSpace(q))
        {
            var term = q.Trim().ToLowerInvariant();
            query = query.Where(x =>
                x.TitleAr.ToLower().Contains(term) ||
                (x.TitleEn != null && x.TitleEn.ToLower().Contains(term)) ||
                (x.Subject != null && x.Subject.ToLower().Contains(term)) ||
                (x.Notes != null && x.Notes.ToLower().Contains(term)) ||
                (x.TagsJson != null && x.TagsJson.ToLower().Contains(term)));
        }

        if (!string.IsNullOrWhiteSpace(tag))
        {
            var t = tag.Trim().ToLowerInvariant();
            query = query.Where(x => x.TagsJson != null && x.TagsJson.ToLower().Contains(t));
        }

        var rows = await query.OrderByDescending(x => x.UpdatedAtUtc).ToListAsync();
        return Ok(rows.Select(ToDto));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<MemberLectureDto>> Get(int id)
    {
        var userId = RequireUserId();
        if (userId is null) return Unauthorized();

        var row = await _db.MemberLectures.AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (row is null) return NotFound(new { message = "المحاضرة غير موجودة" });
        return Ok(ToDto(row));
    }

    [HttpPost]
    public async Task<ActionResult<MemberLectureDto>> Create([FromBody] MemberLectureUpsertRequest request)
    {
        var userId = RequireUserId();
        if (userId is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.TitleAr))
            return BadRequest(new { message = "عنوان المحاضرة مطلوب" });

        var now = DateTime.UtcNow;
        var entity = new MemberLecture
        {
            UserId = userId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };
        Apply(entity, request);
        _db.MemberLectures.Add(entity);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(Get), new { id = entity.Id }, ToDto(entity));
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult<MemberLectureDto>> Update(int id, [FromBody] MemberLectureUpsertRequest request)
    {
        var userId = RequireUserId();
        if (userId is null) return Unauthorized();
        if (string.IsNullOrWhiteSpace(request.TitleAr))
            return BadRequest(new { message = "عنوان المحاضرة مطلوب" });

        var entity = await _db.MemberLectures.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity is null) return NotFound(new { message = "المحاضرة غير موجودة" });

        Apply(entity, request);
        entity.UpdatedAtUtc = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return Ok(ToDto(entity));
    }

    [HttpDelete("{id:int}")]
    public async Task<ActionResult> Delete(int id)
    {
        var userId = RequireUserId();
        if (userId is null) return Unauthorized();

        var entity = await _db.MemberLectures.FirstOrDefaultAsync(x => x.Id == id && x.UserId == userId);
        if (entity is null) return NotFound(new { message = "المحاضرة غير موجودة" });

        _db.MemberLectures.Remove(entity);
        await _db.SaveChangesAsync();
        return NoContent();
    }

    private string? RequireUserId() => User.FindFirstValue(ClaimTypes.NameIdentifier);

    private static void Apply(MemberLecture entity, MemberLectureUpsertRequest request)
    {
        entity.TitleAr = request.TitleAr.Trim();
        entity.TitleEn = string.IsNullOrWhiteSpace(request.TitleEn) ? null : request.TitleEn.Trim();
        entity.Subject = string.IsNullOrWhiteSpace(request.Subject) ? null : request.Subject.Trim();
        entity.SpecialtyId = string.IsNullOrWhiteSpace(request.SpecialtyId) ? null : request.SpecialtyId.Trim();
        entity.CourseId = string.IsNullOrWhiteSpace(request.CourseId) ? null : request.CourseId.Trim();
        entity.LessonId = string.IsNullOrWhiteSpace(request.LessonId) ? null : request.LessonId.Trim();
        entity.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();
        entity.LectureDate = request.LectureDate;
        entity.DurationMinutes = request.DurationMinutes is > 0 ? request.DurationMinutes : null;

        var attachments = (request.Attachments ?? [])
            .Where(a => !string.IsNullOrWhiteSpace(a.Url) || !string.IsNullOrWhiteSpace(a.Filename))
            .Select(a => new LectureAttachmentDto
            {
                Url = a.Url?.Trim(),
                Filename = a.Filename?.Trim(),
                Type = a.Type?.Trim()
            })
            .Take(20)
            .ToList();
        entity.AttachmentsJson = attachments.Count == 0
            ? null
            : JsonSerializer.Serialize(attachments, JsonOpts);

        var tags = (request.Tags ?? [])
            .Select(t => t?.Trim())
            .Where(t => !string.IsNullOrWhiteSpace(t))
            .Select(t => t!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Take(20)
            .ToList();
        entity.TagsJson = tags.Count == 0 ? null : JsonSerializer.Serialize(tags, JsonOpts);
    }

    private static MemberLectureDto ToDto(MemberLecture x) => new()
    {
        Id = x.Id,
        TitleAr = x.TitleAr,
        TitleEn = x.TitleEn,
        Subject = x.Subject,
        SpecialtyId = x.SpecialtyId,
        CourseId = x.CourseId,
        LessonId = x.LessonId,
        Notes = x.Notes,
        LectureDate = x.LectureDate,
        DurationMinutes = x.DurationMinutes,
        Attachments = DeserializeAttachments(x.AttachmentsJson),
        Tags = DeserializeTags(x.TagsJson),
        CreatedAtUtc = x.CreatedAtUtc,
        UpdatedAtUtc = x.UpdatedAtUtc
    };

    private static List<LectureAttachmentDto> DeserializeAttachments(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<LectureAttachmentDto>>(json, JsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static List<string> DeserializeTags(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<List<string>>(json, JsonOpts) ?? [];
        }
        catch
        {
            return [];
        }
    }
}
