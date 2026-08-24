using Microsoft.AspNetCore.Mvc;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Services;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/ai")]
public class AiController : ControllerBase
{
    private readonly IAiTechAssistant _assistant;
    private readonly IAiStudyAssistant _study;

    public AiController(IAiTechAssistant assistant, IAiStudyAssistant study)
    {
        _assistant = assistant;
        _study = study;
    }

    [HttpPost("assist")]
    public async Task<ActionResult<AiAssistResponse>> Assist(
        [FromBody] AiAssistRequest request,
        CancellationToken ct)
    {
        var result = await _assistant.AssistAsync(request, ct);
        return Ok(result);
    }

    [HttpGet("recommend")]
    public async Task<ActionResult<AiAssistResponse>> Recommend(
        [FromQuery] string? from,
        [FromQuery] string? to,
        [FromQuery] string? budget,
        CancellationToken ct)
    {
        var msg = string.IsNullOrWhiteSpace(from) && string.IsNullOrWhiteSpace(to)
            ? "اقترح أرخص الخدمات المتاحة"
            : $"اقترح أفضل خدمة من {from} إلى {to} {budget}";
        return await Assist(new AiAssistRequest { Message = msg }, ct);
    }

    /// <summary>Study assistant: explains topics via Wikipedia + OpenAI (optional) and fetches books from Open Library.</summary>
    [HttpPost("study")]
    public async Task<ActionResult<AiStudyResponse>> Study(
        [FromBody] AiStudyRequest request,
        CancellationToken ct)
    {
        var result = await _study.StudyAsync(request, ct);
        return Ok(result);
    }

    /// <summary>Search public books on Open Library.</summary>
    [HttpGet("books")]
    public async Task<ActionResult<List<BookResourceDto>>> Books(
        [FromQuery] string q,
        [FromQuery] string? lang,
        [FromQuery] int limit = 8,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(q))
            return BadRequest(new { message = "query q is required" });

        var books = await _study.SearchBooksAsync(q, lang, limit, ct);
        return Ok(books);
    }
}
