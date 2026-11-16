using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SmartFoodBox.Api.Data;

namespace SmartFoodBox.Api.Controllers;

[ApiController]
[Route("api/boxes")]
public class BoxesController : ControllerBase
{
    private readonly AppDbContext _db;

    public BoxesController(AppDbContext db)
    {
        _db = db;
    }

    [HttpGet]
    public async Task<IActionResult> GetAllBoxes(CancellationToken ct)
    {
        var boxes = await _db.FoodBoxes
            .AsNoTracking()
            .Select(b => new
            {
                b.Id,
                b.Name,
                b.DeviceCode,
                b.ConnectionStatus,
                b.BatteryLevel,
                b.LastSeenAt,
                Sensors = b.Sensors.Select(s => new
                {
                    s.Id,
                    s.SensorType,
                    s.Unit,
                    LatestReading = s.Readings
                        .OrderByDescending(r => r.RecordedAt)
                        .Select(r => new { r.Value, r.Status, r.RecordedAt })
                        .FirstOrDefault()
                })
            })
            .ToListAsync(ct);

        return Ok(boxes);
    }

    [HttpGet("{deviceCode}/readings")]
    public async Task<IActionResult> GetReadings(
        string deviceCode,
        [FromQuery] string? sensorType,
        [FromQuery] int limit = 50,
        CancellationToken ct = default)
    {
        var query = _db.SensorReadings
            .AsNoTracking()
            .Where(r => r.Sensor!.Box!.DeviceCode == deviceCode);

        if (!string.IsNullOrWhiteSpace(sensorType))
        {
            query = query.Where(r => r.Sensor!.SensorType.ToLower() == sensorType.ToLower());
        }

        var readings = await query
            .OrderByDescending(r => r.RecordedAt)
            .Take(Math.Clamp(limit, 1, 500))
            .Select(r => new
            {
                r.Id,
                SensorType = r.Sensor!.SensorType,
                Unit = r.Sensor.Unit,
                r.Value,
                r.Status,
                r.RecordedAt
            })
            .ToListAsync(ct);

        return Ok(readings);
    }

    [HttpGet("{deviceCode}/alerts")]
    public async Task<IActionResult> GetAlerts(
        string deviceCode,
        [FromQuery] bool unreadOnly = false,
        CancellationToken ct = default)
    {
        var query = _db.Alerts
            .AsNoTracking()
            .Where(a => a.Box!.DeviceCode == deviceCode);

        if (unreadOnly)
        {
            query = query.Where(a => !a.IsRead);
        }

        var alerts = await query
            .OrderByDescending(a => a.CreatedAt)
            .Take(100)
            .Select(a => new
            {
                a.Id,
                a.Message,
                a.Severity,
                a.IsRead,
                a.CreatedAt
            })
            .ToListAsync(ct);

        return Ok(alerts);
    }
}