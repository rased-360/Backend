using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Realtime;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class SensorCacheService
    {
        private readonly IMemoryCache _memoryCache;
        private readonly ILogger<SensorCacheService> _logger;

        // In-Memory Storage to compare last reading
        private static readonly ConcurrentDictionary<string, SensorReadingDto> _lastRawReadings = new();

        // Cache Keys
        private const string LATEST_READING_KEY = "LatestSensorReading";
        private const string TODAY_CHART_KEY = "TodayChartData";

        //  Cache Durations
        private static readonly TimeSpan LATEST_READING_CACHE_DURATION = TimeSpan.FromSeconds(30);
        private static readonly TimeSpan CHART_CACHE_DURATION = TimeSpan.FromMinutes(1);

        // Thresholds 
        private const decimal TEMPERATURE_THRESHOLD = 0.2m;
        private const decimal HUMIDITY_THRESHOLD = 0.1m;
        private const decimal PRESSURE_THRESHOLD = 0.5m;
        private const int HYDROGEN_THRESHOLD = 10;
        private const int ETHANOL_THRESHOLD = 10;

        public SensorCacheService(
            IMemoryCache memoryCache,
            ILogger<SensorCacheService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger;
        }

        /// <summary>
        /// make sure the new reading has significant change before storing
        /// </summary>
        public bool ShouldStoreReading(SensorReadingDto newReading, string sensorId = "default")
        {
            if (!_lastRawReadings.TryGetValue(sensorId, out var lastReading))
            {
                //Store the first reading
                _lastRawReadings[sensorId] = newReading;
                _logger.LogDebug("🆕 First reading for sensor {SensorId} - storing", sensorId);
                return true;
            }

            //compare with last reading
            bool hasSignificantChange =
                Math.Abs(newReading.Temperature - lastReading.Temperature) >= TEMPERATURE_THRESHOLD ||
                Math.Abs(newReading.Humidity - lastReading.Humidity) >= HUMIDITY_THRESHOLD ||
                Math.Abs(newReading.Pressure - lastReading.Pressure) >= PRESSURE_THRESHOLD ||
                Math.Abs(newReading.Hydrogen - lastReading.Hydrogen) >= HYDROGEN_THRESHOLD ||
                Math.Abs(newReading.Ethanol - lastReading.Ethanol) >= ETHANOL_THRESHOLD;

            if (hasSignificantChange)
            {
                _lastRawReadings[sensorId] = newReading;
                _logger.LogDebug("📊 Significant change detected - storing reading");
                return true;
            }

            _logger.LogDebug("🔄 No significant change - skipping storage");
            return false;
        }

        /// <summary>
        ///Cache for the latest reading
        /// </summary>
        public void CacheLatestReading(SensorReadingDto reading)
        {
            _memoryCache.Set(LATEST_READING_KEY, reading, LATEST_READING_CACHE_DURATION);
            _logger.LogDebug("💾 Latest reading cached for {Duration}s",
                LATEST_READING_CACHE_DURATION.TotalSeconds);
        }

        /// <summary>
        ///Cache to get the latest reading
        /// </summary>
        public SensorReadingDto? GetLatestReading()
        {
            return _memoryCache.Get<SensorReadingDto>(LATEST_READING_KEY);
        }

        /// <summary>
        /// Set Cache for Chart 
        /// </summary>
        public void CacheTodayChart(ChartDataDto chartData)
        {
            _memoryCache.Set(TODAY_CHART_KEY, chartData, CHART_CACHE_DURATION);
            _logger.LogDebug("📈 Chart data cached for {Duration}m",
                CHART_CACHE_DURATION.TotalMinutes);
        }

        /// <summary>
        /// Cache to get Chart Data
        /// </summary>
        public ChartDataDto? GetTodayChart()
        {
            return _memoryCache.Get<ChartDataDto>(TODAY_CHART_KEY);
        }

        /// <summary>
        /// Clear Cache (Invalidation)
        /// </summary>
        public void InvalidateCache()
        {
            _memoryCache.Remove(LATEST_READING_KEY);
            _memoryCache.Remove(TODAY_CHART_KEY);
            _logger.LogInformation("🗑️ Cache invalidated");
        }

        /// <summary>
        /// clear the in-memory last readings
        /// </summary>
        public void ClearRawReadings()
        {
            _lastRawReadings.Clear();
            _logger.LogInformation("🗑️ Raw readings cache cleared");
        }
    }
}
