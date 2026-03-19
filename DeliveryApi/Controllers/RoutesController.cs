using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryApi.Data;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RoutesController : ControllerBase
{
    private readonly AppDbContext _db;
    public RoutesController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Routes.Include(r => r.Packages).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var route = await _db.Routes.Include(r => r.Packages)
                                    .FirstOrDefaultAsync(r => r.Id == id);
        return route == null ? NotFound() : Ok(route);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Models.Route route)
    {
        _db.Routes.Add(route);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = route.Id }, route);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Models.Route updated)
    {
        var existing = await _db.Routes.FindAsync(id);
        if (existing == null) return NotFound();
        existing.Name = updated.Name;
        existing.StartPoint = updated.StartPoint;
        existing.EndPoint = updated.EndPoint;
        existing.DistanceKm = updated.DistanceKm;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var route = await _db.Routes.FindAsync(id);
        if (route == null) return NotFound();
        _db.Routes.Remove(route);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}