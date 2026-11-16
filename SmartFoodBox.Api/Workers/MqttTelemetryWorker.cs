using System.Text.Json;
using MQTTnet;
using SmartFoodBox.Api.DTOs;
using SmartFoodBox.Api.Services;

namespace SmartFoodBox.Api.Workers;

public class MqttTelemetryWorker : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly IConfiguration _config;
    private readonly ILogger<MqttTelemetryWorker> _logger;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public MqttTelemetryWorker(
        IServiceScopeFactory scopeFactory,
        IConfiguration config,
        ILogger<MqttTelemetryWorker> logger)
    {
        _scopeFactory = scopeFactory;
        _config = config;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        var brokerHost = _config["MqttSettings:BrokerHost"] ?? "localhost";
        var brokerPort = _config.GetValue<int>("MqttSettings:BrokerPort", 1883);
        var topic = _config["MqttSettings:Topic"] ?? "smartbox/+/telemetry";

        var factory = new MqttClientFactory();
        using var mqttClient = factory.CreateMqttClient();

        var clientOptions = new MqttClientOptionsBuilder()
            .WithTcpServer(brokerHost, brokerPort)
            .WithClientId($"smartfoodbox-api-{Guid.NewGuid():N}")
            .WithCleanSession()
            .Build();

        mqttClient.ApplicationMessageReceivedAsync += async e =>
        {
            try
            {
                // Expected topic format: smartbox/{deviceCode}/telemetry
                var segments = e.ApplicationMessage.Topic.Split('/', StringSplitOptions.RemoveEmptyEntries);
                if (segments.Length < 3) return;

                var deviceCode = segments[1];
                var rawPayload = e.ApplicationMessage.ConvertPayloadToString();
                var dto = JsonSerializer.Deserialize<TelemetryPayloadDto>(rawPayload, JsonOptions);

                if (dto is null) return;

                using var scope = _scopeFactory.CreateScope();
                var telemetryService = scope.ServiceProvider.GetRequiredService<TelemetryService>();
                await telemetryService.ProcessTelemetryAsync(deviceCode, dto, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process incoming MQTT message on topic '{Topic}'.", e.ApplicationMessage.Topic);
            }
        };

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                if (!mqttClient.IsConnected)
                {
                    await mqttClient.ConnectAsync(clientOptions, stoppingToken);
                    var subscribeOptions = factory.CreateSubscribeOptionsBuilder()
                        .WithTopicFilter(topic)
                        .Build();

                    await mqttClient.SubscribeAsync(subscribeOptions, stoppingToken);
                    _logger.LogInformation("Connected to MQTT broker at {Host}:{Port} and subscribed to '{Topic}'.", brokerHost, brokerPort, topic);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning("MQTT broker connection failed ({Message}). Retrying in 5 seconds...", ex.Message);
            }

            await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
        }
    }
}