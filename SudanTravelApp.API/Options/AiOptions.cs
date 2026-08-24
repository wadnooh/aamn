namespace SudanTravelApp.API.Options;

public class AiOptions
{
    public const string SectionName = "AI";

    /// <summary>openai | local</summary>
    public string Provider { get; set; } = "local";

    public string OpenAiApiKey { get; set; } = string.Empty;
    public string OpenAiModel { get; set; } = "gpt-4o-mini";
    public string OpenAiBaseUrl { get; set; } = "https://api.openai.com/v1";
}
