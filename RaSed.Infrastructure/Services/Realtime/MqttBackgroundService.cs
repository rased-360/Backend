using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using RaSed.Application.Configuration;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.Interfaces.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class MqttBackgroundService: BackgroundService
    {
        private readonly ILogger<MqttBackgroundService> _logger;
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly MqttSettings _mqttSettings;
        private IMqttClient? _mqttClient;

        public MqttBackgroundService(
           ILogger<MqttBackgroundService> logger,
           IServiceScopeFactory serviceScopeFactory,
            IOptions<MqttSettings> mqttSettings)
        {
            _logger = logger;
            _serviceScopeFactory = serviceScopeFactory;
            _mqttSettings = mqttSettings.Value;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚀 MQTT Background Service is starting...");

            var factory = new MqttClientFactory();
            _mqttClient = factory.CreateMqttClient();

            var options = new MqttClientOptionsBuilder()
               .WithTcpServer(_mqttSettings.Broker, _mqttSettings.Port)
               .WithClientId($"{_mqttSettings.ClientIdPrefix}_{Guid.NewGuid()}")
               .WithCleanSession()
               .Build();

            _mqttClient.ApplicationMessageReceivedAsync += async e =>
            {
                try
                {
                    var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                    _logger.LogInformation("📨 Received MQTT message: {Payload}", payload);

                    var sensorData = JsonSerializer.Deserialize<MqttSensorData>(payload,
                        new JsonSerializerOptions
                        {
                            PropertyNameCaseInsensitive = true
                        });

                    if (sensorData != null)
                    {
                        var reading = new SensorReadingDto
                        {
                            Hydrogen = sensorData.H,
                            Ethanol = sensorData.C,
                            Temperature = sensorData.Temp,
                            Humidity = sensorData.Hum,
                            Pressure = sensorData.Pres,
                            Timestamp = DateTime.UtcNow
                        };

                        using var scope = _serviceScopeFactory.CreateScope();
                        var sensorService = scope.ServiceProvider
                            .GetRequiredService<ISensorDataService>();

                        await sensorService.ProcessAndStoreReadingAsync(reading);

                        _logger.LogInformation("✅ Successfully processed sensor reading - Temp: {Temp}°C",
                            reading.Temperature);
                    }
                }
                catch (JsonException jsonEx)
                {
                    _logger.LogError(jsonEx, "❌ Error parsing JSON message");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error processing MQTT message");
                }
            };

            _mqttClient.ConnectedAsync += async e =>
            {
                _logger.LogInformation("✅ Connected to MQTT broker!");

                await _mqttClient.SubscribeAsync(
                    new MqttTopicFilterBuilder()
                        .WithTopic(_mqttSettings.Topic)
                        .Build());

                _logger.LogInformation("✅ Subscribed to topic: {Topic}", _mqttSettings.Topic);
            };

            _mqttClient.DisconnectedAsync += async e =>
            {
                _logger.LogWarning("⚠️ Disconnected from MQTT broker. Reason: {Reason}",
                    e.Reason);

                if (!stoppingToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(5), stoppingToken);
                }
            };
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mqttClient.IsConnected)
                    {
                        _logger.LogInformation("🔄 Connecting to MQTT broker {Broker}:{Port}...",
                            _mqttSettings.Broker, _mqttSettings.Port);

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
                    _logger.LogError(ex, "❌ Error in MQTT service. Retrying in 10 seconds...");
                    await Task.Delay(TimeSpan.FromSeconds(10), stoppingToken);
                }
            }

        }
        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 MQTT Background Service is stopping...");

            if (_mqttClient?.IsConnected == true)
            {
                await _mqttClient.DisconnectAsync(
                    cancellationToken: cancellationToken);
            }

            _mqttClient?.Dispose();
            await base.StopAsync(cancellationToken);
        }

    }
    internal class MqttSensorData
    {
        public int H { get; set; }
        public int C { get; set; }
        public decimal Temp { get; set; }
        public decimal Hum { get; set; }
        public decimal Pres { get; set; }
    }
}
