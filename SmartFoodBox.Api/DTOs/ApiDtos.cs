namespace SmartFoodBox.Api.DTOs;

public record TelemetryPayloadDto(
    double? Temperature,
    double? Humidity,
    double? Weight,
    int? BatteryLevel
);

public record CreateFoodItemDto(
    Guid CategoryId,
    string Name,
    double Quantity,
    string QuantityUnit,
    DateTime ExpectedExpirationAt,
    string? Notes
);

public record UpdateFoodItemStatusDto(
    string Status // "Fresh", "Consumed", "Expired"
);