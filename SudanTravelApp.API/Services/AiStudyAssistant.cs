using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Options;
using SudanTravelApp.API.Models.Dtos;
using SudanTravelApp.API.Options;

namespace SudanTravelApp.API.Services;

public class AiStudyAssistant : IAiStudyAssistant
{
    private static readonly JsonSerializerOptions JsonOpts = new() { PropertyNameCaseInsensitive = true };

    private readonly AiOptions _options;
    private readonly IHttpClientFactory _httpFactory;
    private readonly ILogger<AiStudyAssistant> _logger;

    public AiStudyAssistant(
        IOptions<AiOptions> options,
        IHttpClientFactory httpFactory,
        ILogger<AiStudyAssistant> logger)
    {
        _options = options.Value;
        _httpFactory = httpFactory;
        _logger = logger;
    }

    public async Task<AiStudyResponse> StudyAsync(AiStudyRequest request, CancellationToken ct = default)
    {
        var message = (request.Message ?? string.Empty).Trim();
        var lang = string.Equals(request.Language, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "ar";

        if (string.IsNullOrWhiteSpace(message))
        {
            return new AiStudyResponse
            {
                Reply = lang == "en"
                    ? "Ask about a topic, request a summary, or search for books — e.g. HTML basics or English for beginners."
                    : "اسأل عن موضوع، أو اطلب ملخصاً، أو ابحث عن كتب — مثل: أساسيات HTML أو الإنجليزية للمبتدئين.",
                Intent = "help",
                Provider = EffectiveProvider
            };
        }

        var intent = DetectIntent(message);
        var topic = ExtractTopic(message, request.Topic);
        var bookQuery = string.IsNullOrWhiteSpace(topic) ? message : topic;

        var infoTask = FetchTopicInfoAsync(bookQuery, lang, ct);
        var booksTask = SearchBooksAsync(bookQuery, lang, 8, ct);
        await Task.WhenAll(infoTask, booksTask);

        var info = await infoTask;
        var books = await booksTask;

        string reply;
        if (HasOpenAi)
        {
            try
            {
                reply = await GenerateStudyReplyAsync(message, lang, info, books, ct)
                        ?? BuildLocalReply(message, lang, intent, info, books);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "OpenAI study reply failed; using local + Open Library");
                reply = BuildLocalReply(message, lang, intent, info, books);
            }
        }
        else
        {
            reply = BuildLocalReply(message, lang, intent, info, books);
        }

        return new AiStudyResponse
        {
            Reply = reply,
            Intent = intent,
            Topic = topic,
            Sources = info is null ? [] : [info],
            Books = books,
            Provider = EffectiveProvider
        };
    }

    public async Task<List<BookResourceDto>> SearchBooksAsync(
        string query,
        string? language = null,
        int limit = 8,
        CancellationToken ct = default)
    {
        query = (query ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(query))
            return [];

        limit = Math.Clamp(limit, 1, 20);
        var lang = string.Equals(language, "en", StringComparison.OrdinalIgnoreCase) ? "en" : "ar";
        var englishQuery = ToEnglishSearchQuery(query);

        try
        {
            var client = _httpFactory.CreateClient("openlibrary");
            var url =
                $"https://openlibrary.org/search.json?q={Uri.EscapeDataString(englishQuery)}&limit={limit}&fields=key,title,author_name,first_publish_year,cover_i,edition_count,language,subject,ia,ebook_access";

            using var response = await client.GetAsync(url, ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Open Library search failed: {Status}", response.StatusCode);
                return FallbackBooks(englishQuery, lang);
            }

            await using var stream = await response.Content.ReadAsStreamAsync(ct);
            using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
            if (!doc.RootElement.TryGetProperty("docs", out var docs) || docs.ValueKind != JsonValueKind.Array)
                return FallbackBooks(englishQuery, lang);

            var books = new List<BookResourceDto>();
            foreach (var d in docs.EnumerateArray())
            {
                var title = d.TryGetProperty("title", out var t) ? t.GetString() : null;
                if (string.IsNullOrWhiteSpace(title)) continue;

                var key = d.TryGetProperty("key", out var k) ? k.GetString() : null;
                var authors = d.TryGetProperty("author_name", out var an) && an.ValueKind == JsonValueKind.Array
                    ? string.Join(", ", an.EnumerateArray()
                        .Select(x => x.GetString())
                        .Where(x => !string.IsNullOrWhiteSpace(x))
                        .Take(3)
                        .Select(x => x!))
                    : "";
                int? year = d.TryGetProperty("first_publish_year", out var y) && y.TryGetInt32(out var yi) ? yi : null;
                int? cover = d.TryGetProperty("cover_i", out var c) && c.TryGetInt32(out var ci) ? ci : null;
                var ia = d.TryGetProperty("ia", out var iaEl) && iaEl.ValueKind == JsonValueKind.Array
                    ? iaEl.EnumerateArray().Select(x => x.GetString()).FirstOrDefault(x => !string.IsNullOrWhiteSpace(x))
                    : null;

                var openLibUrl = string.IsNullOrWhiteSpace(key)
                    ? $"https://openlibrary.org/search?q={Uri.EscapeDataString(title)}"
                    : $"https://openlibrary.org{key}";

                books.Add(new BookResourceDto
                {
                    Title = title!,
                    Authors = authors,
                    Year = year,
                    CoverUrl = cover is null ? null : $"https://covers.openlibrary.org/b/id/{cover}-M.jpg",
                    OpenLibraryUrl = openLibUrl,
                    ReadUrl = string.IsNullOrWhiteSpace(ia)
                        ? openLibUrl
                        : $"https://archive.org/details/{ia}",
                    Source = "openlibrary"
                });
            }

            return books.Count > 0 ? books : FallbackBooks(englishQuery, lang);
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Open Library request error");
            return FallbackBooks(englishQuery, lang);
        }
    }

    private async Task<KnowledgeSourceDto?> FetchTopicInfoAsync(string topic, string lang, CancellationToken ct)
    {
        topic = topic.Trim();
        if (string.IsNullOrWhiteSpace(topic)) return null;

        var candidates = new List<(string WikiLang, string Title)>();
        if (lang == "ar")
        {
            candidates.Add(("ar", topic));
            candidates.Add(("en", ToEnglishSearchQuery(topic)));
            candidates.Add(("en", topic));
        }
        else
        {
            candidates.Add(("en", topic));
            candidates.Add(("en", ToEnglishSearchQuery(topic)));
        }

        var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        foreach (var (wikiLang, title) in candidates)
        {
            var key = $"{wikiLang}:{title}";
            if (!seen.Add(key) || string.IsNullOrWhiteSpace(title)) continue;

            try
            {
                var client = _httpFactory.CreateClient("wikipedia");
                var url =
                    $"https://{wikiLang}.wikipedia.org/api/rest_v1/page/summary/{Uri.EscapeDataString(title.Replace(' ', '_'))}";
                using var response = await client.GetAsync(url, ct);
                if (!response.IsSuccessStatusCode) continue;

                await using var stream = await response.Content.ReadAsStreamAsync(ct);
                using var doc = await JsonDocument.ParseAsync(stream, cancellationToken: ct);
                var root = doc.RootElement;
                var type = root.TryGetProperty("type", out var ty) ? ty.GetString() : "";
                if (string.Equals(type, "disambiguation", StringComparison.OrdinalIgnoreCase))
                    continue;

                var extract = root.TryGetProperty("extract", out var ex) ? ex.GetString() : null;
                if (string.IsNullOrWhiteSpace(extract)) continue;

                var pageTitle = root.TryGetProperty("title", out var pt) ? pt.GetString() ?? title : title;
                var pageUrl = root.TryGetProperty("content_urls", out var cu)
                              && cu.TryGetProperty("desktop", out var desk)
                              && desk.TryGetProperty("page", out var page)
                    ? page.GetString()
                    : $"https://{wikiLang}.wikipedia.org/wiki/{Uri.EscapeDataString(pageTitle.Replace(' ', '_'))}";

                return new KnowledgeSourceDto
                {
                    Title = pageTitle,
                    Summary = extract!,
                    Url = pageUrl ?? "",
                    Source = "wikipedia"
                };
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Wikipedia lookup failed for {Title}", title);
            }
        }

        return null;
    }

    private async Task<string?> GenerateStudyReplyAsync(
        string message,
        string lang,
        KnowledgeSourceDto? info,
        List<BookResourceDto> books,
        CancellationToken ct)
    {
        var client = _httpFactory.CreateClient("openai");
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", _options.OpenAiApiKey);

        var bookLines = books.Take(5)
            .Select((b, i) => $"{i + 1}. {b.Title} — {b.Authors} ({b.Year?.ToString() ?? "n/a"})")
            .ToList();

        var context = new StringBuilder();
        if (info != null)
        {
            context.AppendLine("Wikipedia summary:");
            context.AppendLine(info.Summary);
            context.AppendLine($"Source: {info.Url}");
        }
        if (bookLines.Count > 0)
        {
            context.AppendLine("Related books from Open Library:");
            foreach (var line in bookLines) context.AppendLine(line);
        }

        var system = lang == "en"
            ? """
              You are the Engineering Copilot for Wadnooh Software & Computer (education + research + innovation + jobs).
              Help with: step-by-step equations, code review (correctness/style/safety), circuits, risk analysis (hazard ID, L×S matrix, hierarchy of controls, PTW/LOTO), references, summaries, graduation-project critique, and short quiz generation (3–5 MCQs with answers).
              Cover engineering + OSH/HSE (ISO 45001, PPE, fire, electrical, confined space, incident RCA) when relevant.
              Prefer framing across specialties (circuits, signals, software, civil/mechanical, petroleum, chemical, aviation, mining, biomedical, control, IoT, security, renewables, OSH).
              Use the provided Wikipedia summary and book list when relevant.
              Reply in English. Keep answers concise (under 260 words). Mention 1-2 book/official titles if useful.
              For university questions, briefly note strengths and typical fields; point to official sites — do not invent admissions or rankings.
              Do not invent book URLs, DOI claims, or copyrighted full texts. If context is thin, still give a solid engineering/OSH overview.
              """
            : """
              أنت المساعد التقني (Engineering Copilot) في ود نوح للبرمجيات والكمبيوتر (تعليم + بحث + ابتكار + وظائف).
              ساعد في: معادلات خطوة بخطوة، مراجعة كود (صحة/أسلوب/سلامة)، دوائر، تحليل مخاطر (تحديد أخطار، مصفوفة احتمال×شدة، تسلسل هرمي للتحكم، PTW/LOTO)، مراجع، تلخيص، نقد مشروع تخرج، وتوليد اختبار قصير (٣–٥ أسئلة مع إجابات).
              غطِّ التقنية + السلامة والصحة المهنية OSH/HSE (ISO 45001، PPE، حريق، كهرباء، أماكن محصورة، تحقيق حوادث) عند المناسبة.
              فضّل الإطار عبر التخصصات (دوائر، إشارات، برمجيات، مدني/ميكانيك، بترول، كيميائية، طيران، تعدين، طبية حيوية، تحكم، IoT، أمن، طاقة متجددة، سلامة مهنية).
              استخدم ملخص ويكيبيديا وقائمة الكتب عند توفرها.
              أجب بالعربية الفصحى المبسطة. اجعل الرد مختصراً (أقل من 260 كلمة). اذكر عنوان كتاب/مرجع رسمي أو اثنين إن أمكن.
              لأسئلة الجامعات: اذكر باختصار نقاط القوة والمجالات، وأشر للموقع الرسمي — لا تخترع قبولًا أو ترتيبًا.
              لا تخترع روابط كتب أو DOI أو نصوصاً محمية كاملة. إن كان السياق ضعيفاً فقدّم نظرة تقنية/سلامة مفيدة.
              """;

        var userContent = $"""
            Student question: {message}

            Context:
            {context}
            """;

        var payload = new
        {
            model = _options.OpenAiModel,
            temperature = 0.4,
            messages = new object[]
            {
                new { role = "system", content = system },
                new { role = "user", content = userContent }
            }
        };

        using var content = new StringContent(JsonSerializer.Serialize(payload), Encoding.UTF8, "application/json");
        using var response = await client.PostAsync($"{_options.OpenAiBaseUrl.TrimEnd('/')}/chat/completions", content, ct);
        var body = await response.Content.ReadAsStringAsync(ct);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("OpenAI study HTTP {Status}: {Body}", response.StatusCode, body[..Math.Min(body.Length, 240)]);
            return null;
        }

        using var doc = JsonDocument.Parse(body);
        return doc.RootElement
            .GetProperty("choices")[0]
            .GetProperty("message")
            .GetProperty("content")
            .GetString()
            ?.Trim();
    }

    private static string BuildLocalReply(
        string message,
        string lang,
        string intent,
        KnowledgeSourceDto? info,
        List<BookResourceDto> books)
    {
        var sb = new StringBuilder();

        if (info != null)
        {
            sb.AppendLine(lang == "en"
                ? $"About «{info.Title}»:"
                : $"حول «{info.Title}»:");
            sb.AppendLine(Truncate(info.Summary, 520));
            sb.AppendLine();
        }
        else
        {
            sb.AppendLine(lang == "en"
                ? $"Here is a study starting point for: {message}"
                : $"نقطة انطلاق للتعلّم حول: {message}");
            sb.AppendLine(lang == "en"
                ? "I fetched related public books below. Open a title to read more on Open Library / Internet Archive."
                : "جلبت كتباً عامة مرتبطة أدناه. افتح عنواناً للقراءة عبر Open Library أو Internet Archive.");
            sb.AppendLine();
        }

        if (books.Count > 0)
        {
            sb.AppendLine(lang == "en" ? "Recommended books:" : "كتب مقترحة:");
            foreach (var b in books.Take(4))
            {
                var year = b.Year?.ToString() ?? "—";
                sb.AppendLine($"• {b.Title} — {b.Authors} ({year})");
            }
        }

        if (intent == "books" && books.Count == 0)
        {
            sb.AppendLine(lang == "en"
                ? "No books matched that query. Try a broader topic in English, e.g. “computer basics”."
                : "لم تُعثر على كتب مطابقة. جرّب موضوعاً أوسع بالإنجليزية مثل computer basics.");
        }

        return sb.ToString().Trim();
    }

    private static List<BookResourceDto> FallbackBooks(string query, string lang)
    {
        // Curated educational fallbacks when Open Library is unreachable
        var pool = new List<BookResourceDto>
        {
            new()
            {
                Title = "Think Python",
                Authors = "Allen B. Downey",
                Year = 2015,
                OpenLibraryUrl = "https://openlibrary.org/works/OL17869954W/Think_Python",
                ReadUrl = "https://greenteapress.com/wp/think-python-2e/",
                Source = "curated"
            },
            new()
            {
                Title = "Eloquent JavaScript",
                Authors = "Marijn Haverbeke",
                Year = 2018,
                OpenLibraryUrl = "https://openlibrary.org/works/OL17888215W",
                ReadUrl = "https://eloquentjavascript.net/",
                Source = "curated"
            },
            new()
            {
                Title = "English Grammar in Use",
                Authors = "Raymond Murphy",
                Year = 2019,
                OpenLibraryUrl = "https://openlibrary.org/search?q=English+Grammar+in+Use",
                ReadUrl = "https://openlibrary.org/search?q=English+Grammar+in+Use",
                Source = "curated"
            },
            new()
            {
                Title = "Getting Things Done",
                Authors = "David Allen",
                Year = 2015,
                OpenLibraryUrl = "https://openlibrary.org/search?q=Getting+Things+Done",
                ReadUrl = "https://openlibrary.org/search?q=Getting+Things+Done",
                Source = "curated"
            }
        };

        var q = query.ToLowerInvariant();
        IEnumerable<BookResourceDto> filtered = pool;
        if (q.Contains("html") || q.Contains("css") || q.Contains("javascript") || q.Contains("web") || q.Contains("برمجة") || q.Contains("ويب"))
            filtered = pool.Where(b => b.Title.Contains("JavaScript", StringComparison.OrdinalIgnoreCase) || b.Title.Contains("Python", StringComparison.OrdinalIgnoreCase));
        else if (q.Contains("english") || q.Contains("إنجليز") || q.Contains("grammar"))
            filtered = pool.Where(b => b.Title.Contains("English", StringComparison.OrdinalIgnoreCase));
        else if (q.Contains("time") || q.Contains("productivity") || q.Contains("وقت") || q.Contains("إنتاج"))
            filtered = pool.Where(b => b.Title.Contains("Getting Things", StringComparison.OrdinalIgnoreCase));

        var list = filtered.ToList();
        return list.Count > 0 ? list : pool.Take(3).ToList();
    }

    private static string DetectIntent(string message)
    {
        var m = message.ToLowerInvariant();
        if (Regex.IsMatch(m, @"كتاب|كتب|book|books|اقرأ|reading list")) return "books";
        if (Regex.IsMatch(m, @"جامعة|جامعات|university|college|كلية|opencourseware|تعليم مفتوح|openlearn|mit ocw")) return "university";
        if (Regex.IsMatch(m, @"ملخص|summary|لخّص|لخص|explain|اشرح|ما هو|what is|تعريف")) return "explain";
        if (Regex.IsMatch(m, @"مسار|path|من أين أبدأ|roadmap|مبتدئ")) return "path";
        return "study";
    }

    private static string ExtractTopic(string message, string? explicitTopic)
    {
        if (!string.IsNullOrWhiteSpace(explicitTopic))
            return explicitTopic.Trim();

        var cleaned = Regex.Replace(message, @"^(اشرح|اشرح لي|ما هو|ما هي|ملخص|لخص|لخّص|كتب عن|كتاب عن|أريد كتب|find books|explain|what is|summary of|books about)\s+", "", RegexOptions.IgnoreCase).Trim();
        cleaned = Regex.Replace(cleaned, @"[؟?!.]+$", "").Trim();
        return string.IsNullOrWhiteSpace(cleaned) ? message.Trim() : cleaned;
    }

    private static string ToEnglishSearchQuery(string query)
    {
        var map = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["أساسيات الحاسوب"] = "computer basics",
            ["الحاسوب"] = "computer",
            ["حاسوب"] = "computer",
            ["الإنجليزية"] = "English language learning",
            ["إنجليزية"] = "English grammar",
            ["العربية"] = "Arabic language",
            ["مهارات التواصل"] = "communication skills",
            ["التواصل"] = "communication skills",
            ["برمجة"] = "programming",
            ["الويب"] = "web development",
            ["ويب"] = "web development HTML CSS",
            ["إدارة الوقت"] = "time management",
            ["الإنتاجية"] = "productivity",
            ["سيرة ذاتية"] = "CV resume writing",
            ["مقابلة عمل"] = "job interview",
            ["جافاسكريبت"] = "JavaScript",
            ["بايثون"] = "Python programming",
            ["جامعة أكسفورد"] = "University of Oxford",
            ["أكسفورد"] = "University of Oxford",
            ["جامعة كامبريدج"] = "University of Cambridge",
            ["كامبريدج"] = "University of Cambridge",
            ["جامعة هارفارد"] = "Harvard University",
            ["هارفارد"] = "Harvard University",
            ["معهد ماساتشوستس"] = "Massachusetts Institute of Technology",
            ["جامعة الخرطوم"] = "University of Khartoum",
            ["تعليم مفتوح"] = "open university online learning",
            ["الجامعة المفتوحة"] = "The Open University",
            ["السلامة والصحة المهنية"] = "occupational safety and health",
            ["السلامة المهنية"] = "occupational safety",
            ["الصحة المهنية"] = "occupational health",
            ["تقييم المخاطر"] = "risk assessment workplace",
            ["مصفوفة المخاطر"] = "risk matrix hazard",
            ["معدات الوقاية"] = "personal protective equipment PPE",
            ["تصريح عمل"] = "permit to work",
            ["الإغلاق والتعليق"] = "lockout tagout LOTO",
            ["أماكن محصورة"] = "confined space entry",
            ["العمل على الارتفاع"] = "working at height fall protection",
            ["السلامة من الحرائق"] = "fire safety extinguisher",
            ["السلامة الكهربائية"] = "electrical safety",
            ["تحقيق الحوادث"] = "incident investigation root cause",
            ["تقنية البترول"] = "petroleum engineering",
            ["التقنية الكيميائية"] = "chemical engineering",
            ["تقنية الطيران"] = "aerospace engineering",
            ["تقنية التعدين"] = "mining engineering",
            ["التقنية الطبية"] = "biomedical engineering"
        };

        foreach (var kv in map)
        {
            if (query.Contains(kv.Key, StringComparison.OrdinalIgnoreCase))
                return kv.Value;
        }

        // If mostly Arabic letters, append "education book" for Open Library recall
        var arabicChars = query.Count(c => c is >= '\u0600' and <= '\u06FF');
        if (arabicChars >= 3)
            return query + " education";

        return query;
    }

    private static string Truncate(string text, int max)
    {
        if (string.IsNullOrEmpty(text) || text.Length <= max) return text;
        var cut = text[..max];
        var last = cut.LastIndexOf(' ');
        if (last > max / 2) cut = cut[..last];
        return cut.TrimEnd() + "…";
    }

    private bool HasOpenAi =>
        _options.Provider.Equals("openai", StringComparison.OrdinalIgnoreCase)
        && !string.IsNullOrWhiteSpace(_options.OpenAiApiKey);

    private string EffectiveProvider =>
        HasOpenAi ? "openai+openlibrary" : "openlibrary";
}
