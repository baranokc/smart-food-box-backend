namespace SmartFoodBox.Api.Entities;

public class FoodBox
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string DeviceCode { get; set; } = string.Empty; // e.g., "box-01"
    public string SerialNumber { get; set; } = string.Empty;
    public string FirmwareVersion { get; set; } = "1.0.0";
    public string MacAddress { get; set; } = string.Empty;
    public string ConnectionStatus { get; set; } = "Offline";
    public int BatteryLevel { get; set; } = 100;
    public DateTime? LastSeenAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    public User? User { get; set; }
    public ICollection<Sensor> Sensors { get; set; } = new List<Sensor>();
    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
    public ICollection<AlertRule> AlertRules { get; set; } = new List<AlertRule>();
    public ICollection<Alert> Alerts { get; set; } = new List<Alert>();
}

public class Sensor
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public string SensorType { get; set; } = string.Empty; // "Temperature", "Humidity", "Weight"
    public string SerialNumber { get; set; } = string.Empty;
    public string Unit { get; set; } = string.Empty; // "C", "%", "g"
    public bool IsActive { get; set; } = true;
    public DateTime? LastCalibrationAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public FoodBox? Box { get; set; }
    public ICollection<SensorReading> Readings { get; set; } = new List<SensorReading>();
}

public class SensorReading
{
    public long Id { get; set; }
    public Guid SensorId { get; set; }
    public double Value { get; set; }
    public DateTime RecordedAt { get; set; } = DateTime.UtcNow;
    public string Status { get; set; } = "Normal"; // "Normal", "Warning", "Critical"

    public Sensor? Sensor { get; set; }
}