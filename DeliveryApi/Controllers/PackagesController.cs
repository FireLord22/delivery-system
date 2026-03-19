using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using System.Text.Json;
using DeliveryApi.Data;
using DeliveryApi.Models;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PackagesController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly IDatabase _redis;

    public PackagesController(AppDbContext db, IConnectionMultiplexer redis)
    {
        _db = db;
        _redis = redis.GetDatabase();
    }

    // GET /api/packages — с кэшированием
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        const string cacheKey = "packages:all";
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return Ok(JsonSerializer.Deserialize<List<Package>>((string)cached!));

        var packages = await _db.Packages
            .Include(p => p.Client)
            .Include(p => p.Courier)
            .Include(p => p.Route)
            .ToListAsync();

        await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(packages), TimeSpan.FromMinutes(5));
        return Ok(packages);
    }

    // GET /api/packages/{id} — с кэшированием
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        string cacheKey = $"packages:{id}";
        var cached = await _redis.StringGetAsync(cacheKey);
        if (cached.HasValue)
            return Ok(JsonSerializer.Deserialize<Package>((string)cached!));

        var package = await _db.Packages
            .Include(p => p.Client)
            .Include(p => p.Courier)
            .Include(p => p.Route)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (package == null) return NotFound();

        await _redis.StringSetAsync(cacheKey, JsonSerializer.Serialize(package), TimeSpan.FromMinutes(5));
        return Ok(package);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Package package)
    {
        _db.Packages.Add(package);
        await _db.SaveChangesAsync();
        // Инвалидация кэша
        await _redis.KeyDeleteAsync("packages:all");
        return CreatedAtAction(nameof(GetById), new { id = package.Id }, package);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Package updated)
    {
        var existing = await _db.Packages.FindAsync(id);
        if (existing == null) return NotFound();

        existing.Status = updated.Status;
        existing.WeightKg = updated.WeightKg;
        existing.CourierId = updated.CourierId;
        existing.RouteId = updated.RouteId;
        await _db.SaveChangesAsync();

        // Инвалидация кэша
        await _redis.KeyDeleteAsync("packages:all");
        await _redis.KeyDeleteAsync($"packages:{id}");
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var package = await _db.Packages.FindAsync(id);
        if (package == null) return NotFound();

        _db.Packages.Remove(package);
        await _db.SaveChangesAsync();

        await _redis.KeyDeleteAsync("packages:all");
        await _redis.KeyDeleteAsync($"packages:{id}");
        return NoContent();
    }
}