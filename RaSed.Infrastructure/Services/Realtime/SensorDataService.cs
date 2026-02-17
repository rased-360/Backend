using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Interfaces;


namespace RaSed.Infrastructure.Services.Realtime
{
    public class SensorDataService : ISensorDataService
    {
        private readonly ISensorDataProcessor _processor;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly SensorCacheService _cacheService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<SensorDataService> _logger;
        private readonly string _deviceId;

        public SensorDataService(
            ISensorDataProcessor processor,
            IRealtimeNotificationService notificationService,
            SensorCacheService cacheService,
            IUnitOfWork unitOfWork,
            ILogger<SensorDataService> logger)
        {
            _processor = processor;
            _notificationService = notificationService;
            _cacheService = cacheService;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // ─────────────────────────────────────────────────────────────────────
        // 1.  TELEMETRY  (rased/{deviceId}/telemetry)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Steps:
        ///   1. Map MQTT payload → SensorReadingDto
        ///   2. Compute HeatIndex  (SensorDataProcessor)
        ///   3. Check threshold-based alerts
        ///   4. Update in-memory cache  (NO DB write)
        ///   5. Broadcast reading + any alerts via SignalR
        /// </summary>
        public async Task ProcessTelemetryAsync(string deviceId, DateTime timestamp, object rawPayload)
        {
            try
            {
                var payload = (dynamic)rawPayload;

                var reading = new SensorReadingDto
                {
                    DeviceId = deviceId,
                    Timestamp = timestamp,
                    Hydrogen = (int)payload.H,
                    Ethanol = (int)payload.C,
                    Temperature = (decimal)payload.Temp,
                    Humidity = (decimal)payload.Hum,
                    Pressure = (decimal)payload.Pres,
                    Pm2_5 = (decimal)payload.Pm25,
                    Pm1_0 = (decimal)payload.Pm10,
                    Tvoc = (int)payload.Tvoc,
                    Eco2 = (int)payload.Eco2
                };

                var processed = _processor.ProcessReading(reading);
                var alerts = _processor.CheckForAlerts(processed);

                _cacheService.CacheLatestReading(processed);

                await _notificationService.SendSensorReadingAsync(processed);

                foreach (var alert in alerts)
                {
                    await _notificationService.SendAlertAsync(alert);
                    _logger.LogWarning("📢 Threshold alert: {Type} — {Message}", alert.Type, alert.Message);
                }

                _logger.LogDebug("✅ Telemetry processed — DeviceId={DeviceId}, Temp={Temp}°C, Alerts={Count}",
                    deviceId, processed.Temperature, alerts.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing telemetry for device {DeviceId}", deviceId);
                throw;
            }
        }

        // ─────────────────────────────────────────────────────────────────────
        // 2.  DEVICE STATE  (rased/{deviceId}/state)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Steps:
        ///   1. Map MQTT payload → DeviceStateDto
        ///   2. Compare with cached state — skip if unchanged
        ///   3. Update cache  (NO DB write)
        ///   4. Broadcast "ReceiveDeviceState" via SignalR
        /// </summary>
        public async Task ProcessDeviceStateAsync(string deviceId, DateTime timestamp, object rawPayload)
        {
            try
            {
                var payload = (dynamic)rawPayload;

                var stateDto = new DeviceStateDto
                {
                    DeviceId = deviceId,
                    Timestamp = timestamp,
                    Fan = (int)payload.Fan,
                    Pump = (int)payload.Pump
                };

                if (!_cacheService.HasStateChanged(stateDto))
                {
                    _logger.LogDebug("🔄 Device state unchanged — skipping SignalR update");
                    return;
                }

                _cacheService.CacheDeviceState(stateDto);
                await _notificationService.SendDeviceStateAsync(stateDto);

                _logger.LogInformation("🔧 Device state updated — Fan={Fan}, Pump={Pump}",
                    stateDto.Fan, stateDto.Pump);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing device state for device {DeviceId}", deviceId);
                throw;
            }
        }

        

        // ─────────────────────────────────────────────────────────────────────
        // 3.  DASHBOARD SNAPSHOT  (GET /api/sensor/dashboard)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Returns: latest reading + device state + threshold alerts.
        /// Pure cache — no DB hit.
        /// </summary>
        public Task<DashboardDataDto> GetDashboardDataAsync()
        {
            try
            {
                var latestReading = _cacheService.GetLatestReading();
                var deviceState = _cacheService.GetDeviceState();

                var activeAlerts = new List<AlertDto>();
                if (latestReading != null)
                    activeAlerts = _processor.CheckForAlerts(latestReading);

                var dashboard = new DashboardDataDto
                {
                    LatestReading = latestReading,
                    DeviceState = deviceState,
                    ActiveAlerts = activeAlerts
                };

                _logger.LogInformation(
                    "📊 Dashboard — Reading={HasReading}, State={HasState}, Alerts={AlertCount}",
                    latestReading != null, deviceState != null, activeAlerts.Count);

                return Task.FromResult(dashboard);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error building dashboard");
                throw;
            }
        }

        // TODO (next sprint): add ProcessFireAlertAsync + GetFireStatusAsync here.

        // ─────────────────────────────────────────────────────────────────────
        // 3.  FIRE ALERT
        // ─────────────────────────────────────────────────────────────────────

        public async Task ProcessFireAlertAsync(string deviceId, DateTime timestamp, int fireAlarm)
        {
            try
            {
                // Skip if no state change (0→0 or 1→1)
                if (!_cacheService.HasFireStateChanged(fireAlarm))
                {
                    _logger.LogDebug("🔥 fire_alarm={V} unchanged — skip", fireAlarm);
                    return;
                }

                // Returns the DB EventId only — nothing else needed downstream
                int? eventId = fireAlarm == 1
                    ? await HandleFireStartedAsync(deviceId, timestamp)
                    : await HandleFireClearedAsync(deviceId, timestamp);

                // Update cache
                _cacheService.CacheFireState(new FireStateDto
                {
                    DeviceId = deviceId,
                    FireAlarm = fireAlarm,
                    ActiveEventId = eventId,
                    Timestamp = timestamp
                });

                // Broadcast { fireAlarm: 0 or 1 } to frontend via SignalR
                await _notificationService.SendFireAlertAsync(new FireStatusDto
                {
                    FireAlarm = fireAlarm
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing fire alert for {DeviceId}", deviceId);
                throw;
            }
        }

        // Returns new FireEvent.Id to store in cache (ActiveEventId)
        private async Task<int> HandleFireStartedAsync(string deviceId, DateTime timestamp)
        {
            _logger.LogCritical("🔥🔥🔥 FIRE STARTED — {DeviceId}", deviceId);

            var fireEvent = new Domain.Entities.FireEvent
            {
                DeviceId = deviceId,
                StartTime = timestamp,
                Status = "Active"
            };

            await _unitOfWork._fireEventRepository.AddAsync(fireEvent);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("💾 FireEvent created — Id={Id}", fireEvent.Id);

            return fireEvent.Id;
        }

        private async Task<int?> HandleFireClearedAsync(string deviceId, DateTime timestamp)
        {
            _logger.LogInformation("✅ FIRE CLEARED — {DeviceId}", deviceId);

            var activeEvent = await _unitOfWork._fireEventRepository
                .GetActiveFireEventAsync(deviceId);

            if (activeEvent == null)
            {
                _logger.LogWarning("⚠️ No active FireEvent found for {DeviceId}", deviceId);
                return null;
            }

            activeEvent.EndTime = timestamp;
            activeEvent.DurationSeconds = (int)(timestamp - activeEvent.StartTime).TotalSeconds;
            activeEvent.Status = "Resolved";

            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("💾 FireEvent closed — Id={Id}, Duration={D}s",
                activeEvent.Id, activeEvent.DurationSeconds);

            return activeEvent.Id;
        }
        

        // ─────────────────────────────────────────────────────────────────────
        // 5.  FIRE STATUS  (initial page load only)
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// No param — uses _deviceId from MqttSettings.
        /// Returns fireAlarm: 0 or 1 only.
        /// Called once on frontend connect, then SignalR takes over.
        /// </summary>
        public async Task<FireStatusDto> GetFireStatusAsync()
        {
            try
            {
                var cachedState = _cacheService.GetFireState();

                if (cachedState?.FireAlarm == 1)
                    return new FireStatusDto { FireAlarm = 1 };

                return new FireStatusDto { FireAlarm = 0 };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting fire status");
                throw;
            }
        }
    }
}
