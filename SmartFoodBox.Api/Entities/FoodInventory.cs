namespace SmartFoodBox.Api.Entities;

public class FoodCategory
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;

    public ICollection<FoodItem> FoodItems { get; set; } = new List<FoodItem>();
}

public class FoodItem
{
    public Guid Id { get; set; }
    public Guid BoxId { get; set; }
    public Guid CategoryId { get; set; }
    public string Name { get; set; } = string.Empty;
    public double Quantity { get; set; }
    public string QuantityUnit { get; set; } = "g";
    public DateTime StoredAt { get; set; } = DateTime.UtcNow;
    public DateTime ExpectedExpirationAt { get; set; }
    public string Status { get; set; } = "Fresh"; // "Fresh", "ExpiringSoon", "Expired"
    public string? Notes { get; set; }

    public FoodBox? Box { get; set; }
    public FoodCategory? Category { get; set; }
}