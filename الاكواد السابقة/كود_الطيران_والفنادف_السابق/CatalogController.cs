using Microsoft.AspNetCore.Mvc;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/catalog")]
public class CatalogController : ControllerBase
{
    private static readonly HashSet<string> Allowed = new(StringComparer.OrdinalIgnoreCase)
    {
        "departments", "courses", "articles", "projects", "library", "faq", "universities", "events", "osh-sources"
    };

    private readonly IWebHostEnvironment _env;

    public CatalogController(IWebHostEnvironment env) => _env = env;

    /// <summary>Serves Phase-1 JSON catalog files from wwwroot/data.</summary>
    [HttpGet("{resource}")]
    public IActionResult Get(string resource)
    {
        if (!Allowed.Contains(resource))
            return NotFound(new { message = "Unknown catalog resource" });

        var path = Path.Combine(_env.WebRootPath ?? Path.Combine(_env.ContentRootPath, "wwwroot"), "data", $"{resource}.json");
        if (!System.IO.File.Exists(path))
            return NotFound(new { message = "Catalog file missing" });

        return PhysicalFile(path, "application/json");
    }
}
