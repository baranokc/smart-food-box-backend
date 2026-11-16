using Microsoft.EntityFrameworkCore;
using SmartFoodBox.Api.Data;
using SmartFoodBox.Api.DTOs;
using SmartFoodBox.Api.Entities;

namespace SmartFoodBox.Api.Services;

public class TelemetryService
{
    private readonly AppDbContext _db;
    private readonly ILogger<TelemetryService> _logger;

    public TelemetryService(AppDbContext db, ILogger<TelemetryService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task ProcessTelemetryAsync(string deviceCode, TelemetryPayloadDto payload, CancellationToken ct = default)
    {
        var box = await _db.FoodBoxes
            .Include(b => b.Sensors)
            .Include(b => b.AlertRules)
            .FirstOrDefaultAsync(b => b.DeviceCode == deviceCode && b.IsActive, ct);

        if (box is null)
        {
            _logger.LogWarning("Telemetry ignored: unknown or inactive device code '{DeviceCode}'.", deviceCode);
            return;
        }

        var now = DateTime.UtcNow;
        box.ConnectionStatus = "Online";
        box.LastSeenAt = now;
        if (payload.BatteryLevel.HasValue)
        {
            box.BatteryLevel = Math.Clamp(payload.BatteryLevel.Value, 0, 100);
        }

        var measurements = new Dictionary<string, double?>
        {
            ["Temperature"] = payload.Temperature,
            ["Humidity"] = payload.Humidity,
            ["Weight"] = payload.Weight
        };

        foreach (var (sensorType, value) in measurements)
        {
            if (!value.HasValue) continue;

            var sensor = box.Sensors.FirstOrDefault(s =>
                s.IsActive && s.SensorType.Equals(sensorType, StringComparison.OrdinalIgnoreCase));

            if (sensor is null) continue;

            var breachedRule = box.AlertRules
                .Where(r => r.IsEnabled && r.SensorId == sensor.Id)
                .FirstOrDefault(r => EvaluateRule(value.Value, r.ComparisonOperator, r.ThresholdValue));

            var reading = new SensorReading
            {
                SensorId = sensor.Id,
                Value = value.Value,
                RecordedAt = now,
                Status = breachedRule is not null ? "Critical" : "Normal"
            };
            _db.SensorReadings.Add(reading);

            if (breachedRule is not null)
            {
                var alertMessage = $"{box.Name} ({box.DeviceCode}): {sensor.SensorType} reading {value.Value}{sensor.Unit} breached rule ({breachedRule.ComparisonOperator} {breachedRule.ThresholdValue}{sensor.Unit}).";

                var alert = new Alert
                {
                    BoxId = box.Id,
                    SensorId = sensor.Id,
                    AlertRuleId = breachedRule.Id,
                    Message = alertMessage,
                    Severity = "High",
                    IsRead = false,
                    CreatedAt = now
                };
                _db.Alerts.Add(alert);
                await _db.SaveChangesAsync(ct);

                _db.Notifications.Add(new Notification
                {
                    Id = Guid.NewGuid(),
                    UserId = box.UserId,
                    AlertId = alert.Id,
                    Title = $"Alert: {breachedRule.RuleType}",
                    Message = alertMessage,
                    Type = "Alert",
                    CreatedAt = now
                });

                _logger.LogWarning("Alert triggered for {DeviceCode}: {Message}", deviceCode, alertMessage);
            }
        }

        await _db.SaveChangesAsync(ct);
    }

    private static bool EvaluateRule(double actualValue, string op, double threshold) => op switch
    {
        ">" => actualValue > threshold,
        ">=" => actualValue >= threshold,
        "<" => actualValue < threshold,
        "<=" => actualValue <= threshold,
        "==" => Math.Abs(actualValue - threshold) < 0.0001,
        _ => false
    };
}