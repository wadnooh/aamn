using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SudanTravelApp.API.Data;
using SudanTravelApp.API.Models;

namespace SudanTravelApp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TouristAttractionsController : ControllerBase
{
    private readonly TravelDbContext _context;

    public TouristAttractionsController(TravelDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<TouristAttraction>>> GetTouristAttractions()
    {
        return await _context.TouristAttractions.ToListAsync();
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<TouristAttraction>> GetTouristAttraction(int id)
    {
        var attraction = await _context.TouristAttractions.FindAsync(id);

        if (attraction == null)
        {
            return NotFound();
        }

        return attraction;
    }

    [HttpGet("search")]
    public async Task<ActionResult<IEnumerable<TouristAttraction>>> SearchTouristAttractions(
        [FromQuery] string? city,
        [FromQuery] string? category)
    {
        var query = _context.TouristAttractions.AsQueryable();

        if (!string.IsNullOrEmpty(city))
        {
            query = query.Where(a => a.City.Contains(city));
        }

        if (!string.IsNullOrEmpty(category))
        {
            query = query.Where(a => a.Category.Contains(category));
        }

        return await query.ToListAsync();
    }

    [HttpGet("city/{city}")]
    public async Task<ActionResult<IEnumerable<TouristAttraction>>> GetAttractionsByCity(string city)
    {
        var attractions = await _context.TouristAttractions
            .Where(a => a.City.Contains(city))
            .ToListAsync();

        return attractions;
    }

    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<TouristAttraction>>> GetAttractionsByCategory(string category)
    {
        var attractions = await _context.TouristAttractions
            .Where(a => a.Category.Contains(category))
            .ToListAsync();

        return attractions;
    }

    [HttpPost]
    public async Task<ActionResult<TouristAttraction>> CreateTouristAttraction(TouristAttraction attraction)
    {
        _context.TouristAttractions.Add(attraction);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetTouristAttraction), new { id = attraction.Id }, attraction);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTouristAttraction(int id, TouristAttraction attraction)
    {
        if (id != attraction.Id)
        {
            return BadRequest();
        }

        _context.Entry(attraction).State = EntityState.Modified;

        try
        {
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!TouristAttractionExists(id))
            {
                return NotFound();
            }
            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTouristAttraction(int id)
    {
        var attraction = await _context.TouristAttractions.FindAsync(id);
        if (attraction == null)
        {
            return NotFound();
        }

        _context.TouristAttractions.Remove(attraction);
        await _context.SaveChangesAsync();

        return NoContent();
    }

    private bool TouristAttractionExists(int id)
    {
        return _context.TouristAttractions.Any(e => e.Id == id);
    }
}
