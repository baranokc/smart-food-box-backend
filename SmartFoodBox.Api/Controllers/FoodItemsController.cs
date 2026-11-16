using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFoodBox.Api.Data;
using SmartFoodBox.Api.DTOs;
using SmartFoodBox.Api.Entities;

namespace SmartFoodBox.Api.Controllers;

[ApiController]
[Route("api")]
public class FoodItemsController : ControllerBase
{
    private readonly AppDbContext _db;

    public FoodItemsController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories(CancellationToken ct)
    {
        var categories = await _db.FoodCategories
            .AsNoTracking()
            .Where(c => c.IsActive)
            .Select(c => new { c.Id, c.Name, c.Description })
            .ToListAsync(ct);

        return Ok(categories);
    }

    [HttpGet("boxes/{deviceCode}/items")]
    public async Task<IActionResult> GetBoxItems(string deviceCode, CancellationToken ct)
    {
        var items = await _db.FoodItems
            .AsNoTracking()
            .Where(i => i.Box!.DeviceCode == deviceCode)
            .OrderBy(i => i.ExpectedExpirationAt)
            .Select(i => new
            {
                i.Id,
                i.Name,
                Category = i.Category!.Name,
                i.Quantity,
                i.QuantityUnit,
                i.StoredAt,
                i.ExpectedExpirationAt,
                i.Status,
                i.Notes
            })
            .ToListAsync(ct);

        return Ok(items);
    }

    [HttpPost("boxes/{deviceCode}/items")]
    public async Task<IActionResult> AddFoodItem(
        string deviceCode,
        [FromBody] CreateFoodItemDto dto,
        CancellationToken ct)
    {
        var box = await _db.FoodBoxes.FirstOrDefaultAsync(b => b.DeviceCode == deviceCode, ct);
        if (box is null) return NotFound(new { error = $"Box '{deviceCode}' not found." });

        var item = new FoodItem
        {
            Id = Guid.NewGuid(),
            BoxId = box.Id,
            CategoryId = dto.CategoryId,
            Name = dto.Name,
            Quantity = dto.Quantity,
            QuantityUnit = dto.QuantityUnit,
            StoredAt = DateTime.UtcNow,
            ExpectedExpirationAt = DateTime.SpecifyKind(dto.ExpectedExpirationAt, DateTimeKind.Utc),
            Status = "Fresh",
            Notes = dto.Notes
        };

        _db.FoodItems.Add(item);
        await _db.SaveChangesAsync(ct);

        return CreatedAtAction(nameof(GetBoxItems), new { deviceCode }, new { item.Id, item.Name, item.Status });
    }

    [HttpPatch("items/{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(
        Guid id,
        [FromBody] UpdateFoodItemStatusDto dto,
        CancellationToken ct)
    {
        var item = await _db.FoodItems.FindAsync([id], ct);
        if (item is null) return NotFound();

        item.Status = dto.Status;
        await _db.SaveChangesAsync(ct);

        return Ok(new { item.Id, item.Status });
    }
}