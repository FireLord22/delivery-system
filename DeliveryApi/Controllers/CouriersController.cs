using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using DeliveryApi.Data;
using DeliveryApi.Models;

namespace DeliveryApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CouriersController : ControllerBase
{
    private readonly AppDbContext _db;
    public CouriersController(AppDbContext db) => _db = db;

    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await _db.Couriers.Include(c => c.Packages).ToListAsync());

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var courier = await _db.Couriers.Include(c => c.Packages)
                                        .FirstOrDefaultAsync(c => c.Id == id);
        return courier == null ? NotFound() : Ok(courier);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Courier courier)
    {
        _db.Couriers.Add(courier);
        await _db.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = courier.Id }, courier);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, Courier updated)
    {
        var existing = await _db.Couriers.FindAsync(id);
        if (existing == null) return NotFound();
        existing.FullName = updated.FullName;
        existing.Phone = updated.Phone;
        existing.Vehicle = updated.Vehicle;
        existing.IsAvailable = updated.IsAvailable;
        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var courier = await _db.Couriers.FindAsync(id);
        if (courier == null) return NotFound();
        _db.Couriers.Remove(courier);
        await _db.SaveChangesAsync();
        return NoContent();
    }
}