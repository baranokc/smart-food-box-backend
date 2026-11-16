using Microsoft.EntityFrameworkCore;
using SmartFoodBox.Api.Entities;

namespace SmartFoodBox.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<FoodBox> FoodBoxes => Set<FoodBox>();
    public DbSet<Sensor> Sensors => Set<Sensor>();
    public DbSet<SensorReading> SensorReadings => Set<SensorReading>();
    public DbSet<FoodCategory> FoodCategories => Set<FoodCategory>();
    public DbSet<FoodItem> FoodItems => Set<FoodItem>();
    public DbSet<AlertRule> AlertRules => Set<AlertRule>();
    public DbSet<Alert> Alerts => Set<Alert>();
    public DbSet<Notification> Notifications => Set<Notification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasIndex(u => u.Email).IsUnique();
            e.HasIndex(u => u.UserName).IsUnique();
        });

        modelBuilder.Entity<FoodBox>(e =>
        {
            e.HasIndex(b => b.DeviceCode).IsUnique();
        });

        modelBuilder.Entity<SensorReading>(e =>
        {
            e.HasIndex(r => new { r.SensorId, r.RecordedAt });
        });

        // Initial seed data for development and verification
        var seedDate = new DateTime(2026, 11, 1, 0, 0, 0, DateTimeKind.Utc);
        var userId = Guid.Parse("11111111-1111-1111-1111-111111111111");
        var boxId = Guid.Parse("22222222-2222-2222-2222-222222222222");
        var tempSensorId = Guid.Parse("33333333-3333-3333-3333-333333333331");
        var humSensorId = Guid.Parse("33333333-3333-3333-3333-333333333332");
        var ruleId = Guid.Parse("44444444-4444-4444-4444-444444444441");
        var categoryId = Guid.Parse("55555555-5555-5555-5555-555555555551");

        modelBuilder.Entity<User>().HasData(new User
        {
            Id = userId,
            FullName = "Suleyman Baran Okcuoglu",
            Email = "baran@smartfoodbox.local",
            UserName = "baran",
            PasswordHash = "demo_hash",
            CreatedAt = seedDate,
            UpdatedAt = seedDate
        });

        modelBuilder.Entity<FoodBox>().HasData(new FoodBox
        {
            Id = boxId,
            UserId = userId,
            Name = "Main Kitchen Box",
            DeviceCode = "box-01",
            SerialNumber = "SFB-2026-001",
            MacAddress = "AA:BB:CC:DD:EE:01",
            CreatedAt = seedDate
        });

        modelBuilder.Entity<Sensor>().HasData(
            new Sensor { Id = tempSensorId, BoxId = boxId, SensorType = "Temperature", SerialNumber = "TMP-01", Unit = "C", CreatedAt = seedDate },
            new Sensor { Id = humSensorId, BoxId = boxId, SensorType = "Humidity", SerialNumber = "HUM-01", Unit = "%", CreatedAt = seedDate }
        );

        modelBuilder.Entity<AlertRule>().HasData(new AlertRule
        {
            Id = ruleId,
            BoxId = boxId,
            SensorId = tempSensorId,
            RuleType = "TEMP_HIGH",
            ThresholdValue = 8.0,
            ComparisonOperator = ">",
            CreatedAt = seedDate
        });

        modelBuilder.Entity<FoodCategory>().HasData(new FoodCategory
        {
            Id = categoryId,
            Name = "Dairy & Breakfast",
            Description = "Perishable items requiring cold chain storage"
        });
    }
}