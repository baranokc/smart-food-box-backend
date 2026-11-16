namespace SmartFoodBox.Api.Entities;

public class AlertRule
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public Guid? SensorId { get; set; }
    public string RuleType { get; set; } = string.Empty; // "TEMP_HIGH", "HUMIDITY_HIGH"
    public double ThresholdValue { get; set; }
    public string ComparisonOperator { get; set; } = ">"; // ">", "<", ">="
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FoodBox? Box { get; set; }
    public Sensor? Sensor { get; set; }
}

public class Alert
{
    public long Id { get; set; }
    public Guid BoxId { get; set; }
    public Guid? SensorId { get; set; }
    public Guid? AlertRuleId { get; set; }
    public string Message { get; set; } = string.Empty;
    public string Severity { get; set; } = "Medium"; // "Low", "Medium", "High"
    public bool IsRead { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FoodBox? Box { get; set; }
    public Sensor? Sensor { get; set; }
    public AlertRule? AlertRule { get; set; }
}

public class Notification
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public long? AlertId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime? ReadAt { get; set; }
    public string Type { get; set; } = "Alert";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public User? User { get; set; }
    public Alert? Alert { get; set; }
}