using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using RaSed.Application.Configuration;
using RaSed.Application.Interfaces.Realtime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class MqttBackgroundService : BackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly MqttSettings _settings;
        private IMqttClient? _mqttClient;

        private static readonly JsonSerializerOptions _jsonOpts = new()
        {
            PropertyNameCaseInsensitive = true
        };

        public MqttBackgroundService(
            ILogger<MqttBackgroundService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<MqttSettings> settings)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
        }

        // ── ExecuteAsync ──────────────────────────────────────────────────────

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 MQTT Background Service starting…");

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
                .WithTcpServer(_settings.Broker, _settings.Port)
                .WithClientId($"{_settings.ClientIdPrefix}_{Guid.NewGuid()}")
                .WithCleanSession()
                .Build();

            // ── Message handler ──────────────────────────────────────────────
            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                _logger.LogDebug("📨 MQTT [{Topic}] ← {Payload}", topic, payload);

                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var sensorService = scope.ServiceProvider.GetRequiredService<ISensorDataService>();

                    if (topic.EndsWith("/telemetry"))
                    {
                        var msg = JsonSerializer.Deserialize<TelemetryMessage>(payload, _jsonOpts);
                        if (msg?.Telemetry != null)
                            await sensorService.ProcessTelemetryAsync(msg.DeviceId, msg.Timestamp, msg.Telemetry);
                    }
                    else if (topic.EndsWith("/state"))
                    {
                        var msg = JsonSerializer.Deserialize<StateMessage>(payload, _jsonOpts);
                        if (msg?.State != null)
                            await sensorService.ProcessDeviceStateAsync(msg.DeviceId, msg.Timestamp, msg.State);
                    }
                    else if (topic.EndsWith("/alert"))
                    {
                        // TODO (next sprint): deserialize and call sensorService.ProcessFireAlertAsync()
                        _logger.LogDebug("🔔 Alert topic received — fire logic coming next sprint. Payload: {Payload}", payload);
                    }
                    else
                    {
                        _logger.LogWarning("⚠️ Unknown topic: {Topic}", topic);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ JSON parse error on topic {Topic}", topic);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing MQTT message on topic {Topic}", topic);
                }
            };

            // ── Connected handler ────────────────────────────────────────────
            _mqttClient.ConnectedAsync += async _ =>
            {
                _logger.LogInformation("✅ Connected to MQTT broker {Broker}:{Port}",
                    _settings.Broker, _settings.Port);
                await SubscribeToAllTopicsAsync();
            };

            // ── Disconnected handler ─────────────────────────────────────────
            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("⚠️ Disconnected from MQTT. Reason: {Reason}", e.Reason);
                if (!stoppingToken.IsCancellationRequested)
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
            };

            // ── Connection loop ──────────────────────────────────────────────
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        _logger.LogInformation("🔄 Connecting to MQTT broker…");
                        await _mqttClient.ConnectAsync(options, stoppingToken);
                    }
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ MQTT connection error. Retrying in 10 s…");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }
        }

        private async Task SubscribeToAllTopicsAsync()
        {
            var deviceId = _settings.DeviceId;

            var subscribeOptions = new MqttClientSubscribeOptionsBuilder()
                .WithTopicFilter($"rased/{deviceId}/telemetry")
                .WithTopicFilter($"rased/{deviceId}/state")
                .WithTopicFilter($"rased/{deviceId}/alert")
                .Build();

            await _mqttClient!.SubscribeAsync(subscribeOptions);

            _logger.LogInformation(
                "✅ Subscribed to: telemetry | state | alert  (device: {DeviceId})", deviceId);
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 MQTT Background Service stopping…");
            if (_mqttClient?.IsConnected == true)
                await _mqttClient.DisconnectAsync(cancellationToken: cancellationToken);
            _mqttClient?.Dispose();
            await base.StopAsync(cancellationToken);
        }
    }

    // ── Internal MQTT payload models ──────────────────────────────────────────

    internal class TelemetryMessage
    {
        [JsonPropertyName("deviceId")] public string DeviceId { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("telemetry")] public TelemetryPayload? Telemetry { get; set; }
    }

    internal class TelemetryPayload
    {
        [JsonPropertyName("C")] public int C { get; set; }   // Ethanol
        [JsonPropertyName("H")] public int H { get; set; }   // Hydrogen
        [JsonPropertyName("temp")] public decimal Temp { get; set; }
        [JsonPropertyName("hum")] public decimal Hum { get; set; }
        [JsonPropertyName("pres")] public decimal Pres { get; set; }
        [JsonPropertyName("pm2_5")] public decimal Pm25 { get; set; }
        [JsonPropertyName("pm1_0")] public decimal Pm10 { get; set; }
        [JsonPropertyName("tvoc")] public int Tvoc { get; set; }
        [JsonPropertyName("eco2")] public int Eco2 { get; set; }
    }

    internal class StateMessage
    {
        [JsonPropertyName("deviceId")] public string DeviceId { get; set; } = string.Empty;
        [JsonPropertyName("timestamp")] public DateTime Timestamp { get; set; }
        [JsonPropertyName("state")] public StatePayload? State { get; set; }
    }

    internal class StatePayload
    {
        [JsonPropertyName("fan")] public int Fan { get; set; }
        [JsonPropertyName("pump")] public int Pump { get; set; }
    }

    // TODO (next sprint): add AlertMessage + AlertPayload models here for fire logic.
}