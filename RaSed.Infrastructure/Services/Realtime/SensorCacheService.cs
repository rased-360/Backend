using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Realtime;

namespace RaSed.Infrastructure.Services.Realtime
{
    /// <summary>
    /// In-memory cache for:
    ///   1. Latest sensor telemetry reading
    ///   2. Latest device state (fan / pump)
    ///
    /// TODO (next sprint): add fire state cache when fire logic is implemented.
    /// </summary>
    public class SensorCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<SensorCacheService> _logger;

        // ── Cache Keys ────────────────────────────────────────────────────────
        private const string LATEST_READING_KEY = "LatestSensorReading";
        private const string DEVICE_STATE_KEY = "LatestDeviceState";
        private const string FIRE_STATE_KEY = "FireState";

        // ── Cache Durations ───────────────────────────────────────────────────
        private static readonly TimeSpan LATEST_READING_DURATION = TimeSpan.FromMinutes(5);
        private static readonly TimeSpan DEVICE_STATE_DURATION = TimeSpan.FromHours(1);
        private static readonly TimeSpan FIRE_STATE_DURATION = TimeSpan.FromHours(24); /// long — fire state must survive


        public SensorCacheService(
            IMemoryCache memoryCache,
            ILogger<SensorCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        // ── Latest Sensor Reading ─────────────────────────────────────────────

        public void CacheLatestReading(SensorReadingDto reading)
        {
            _memoryCache.Set(LATEST_READING_KEY, reading, LATEST_READING_DURATION);
            _logger.LogDebug("💾 Latest reading cached (DeviceId={DeviceId})", reading.DeviceId);
        }

        public SensorReadingDto? GetLatestReading()
        {
            return _memoryCache.TryGetValue(LATEST_READING_KEY, out SensorReadingDto? reading)
                ? reading : null;
        }

        // ── Device State (fan / pump) ─────────────────────────────────────────

        public void CacheDeviceState(DeviceStateDto state)
        {
            _memoryCache.Set(DEVICE_STATE_KEY, state, DEVICE_STATE_DURATION);
            _logger.LogDebug("💾 Device state cached — Fan={Fan}, Pump={Pump}", state.Fan, state.Pump);
        }

        public DeviceStateDto? GetDeviceState()
        {
            return _memoryCache.TryGetValue(DEVICE_STATE_KEY, out DeviceStateDto? state)
                ? state : null;
        }

        /// <summary>
        /// Returns true when fan or pump value changed.
        /// Prevents redundant SignalR updates for identical state messages.
        /// </summary>
        public bool HasStateChanged(DeviceStateDto newState)
        {
            var cached = GetDeviceState();
            if (cached == null) return true; // first message → always emit
            return cached.Fan != newState.Fan || cached.Pump != newState.Pump;
        }

        // ── Fire State ──────────────────────────────────────────────────

        public void CacheFireState(FireStateDto fireState)
        {
            _memoryCache.Set(FIRE_STATE_KEY, fireState, FIRE_STATE_DURATION);
            _logger.LogDebug("💾 Fire state cached — FireAlarm={FireAlarm}", fireState.FireAlarm);
        }

        public FireStateDto? GetFireState() =>
            _memoryCache.TryGetValue(FIRE_STATE_KEY, out FireStateDto? f) ? f : null;

        /// <summary>
        /// Returns true if fire_alarm value changed.
        ///
        /// 0 → 1 : true  (fire started)
        /// 1 → 0 : true  (fire cleared)
        /// null  : true  (first message ever → treat as changed)
        /// </summary>
        public bool HasFireStateChanged(int newFireAlarm)
        {
            var cached = GetFireState();
            if (cached == null) return true;

            bool changed = cached.FireAlarm != newFireAlarm;
            if (changed)
                _logger.LogInformation("🔥 Fire state changed: {Old} → {New}", cached.FireAlarm, newFireAlarm);

            return changed;
        }

        // ── Invalidation ──────────────────────────────────────────────────────

        public void InvalidateAll()
        {
            _memoryCache.Remove(LATEST_READING_KEY);
            _memoryCache.Remove(DEVICE_STATE_KEY);
            _memoryCache.Remove(FIRE_STATE_KEY);
            _logger.LogInformation("🗑️ Sensor cache invalidated");
        }
    }
}
