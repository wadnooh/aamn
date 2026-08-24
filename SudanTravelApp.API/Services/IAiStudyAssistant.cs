using SudanTravelApp.API.Models.Dtos;

namespace SudanTravelApp.API.Services;

public interface IAiStudyAssistant
{
    Task<AiStudyResponse> StudyAsync(AiStudyRequest request, CancellationToken ct = default);
    Task<List<BookResourceDto>> SearchBooksAsync(string query, string? language = null, int limit = 8, CancellationToken ct = default);
}
