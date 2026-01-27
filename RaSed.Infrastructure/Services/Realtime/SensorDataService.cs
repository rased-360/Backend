using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class SensorDataService: ISensorDataService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISensorDataProcessor _processor;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly SensorCacheService _cacheService;
        private readonly ILogger<SensorDataService> _logger;

        public SensorDataService(
            IUnitOfWork unitOfWork,
            ISensorDataProcessor processor,
            IRealtimeNotificationService notificationService,
            SensorCacheService cacheService,
            ILogger<SensorDataService> logger)
        {
            _unitOfWork = unitOfWork;
            _processor = processor;
            _notificationService = notificationService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public async Task ProcessAndStoreReadingAsync(SensorReadingDto readingDto)
        {
            try
            {
                _logger.LogDebug("🔄 Processing sensor reading...");

                // Step 1: Process the reading (apply formulas, validation)
                var processedReading = _processor.ProcessReading(readingDto);

                // Step 2: Check for alerts
                var alerts = _processor.CheckForAlerts(processedReading);

                // Step 3: check if should store in Database
                bool shouldStore = _cacheService.ShouldStoreReading(processedReading);

                if (shouldStore)
                {
                   
                    var entity = MapToEntity(processedReading);
                    await _unitOfWork._sensorReadingRepository.AddAsync(entity);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "💾 Saved reading to database: ID={Id}, Temp={Temp}°C, Hum={Hum}%, H={H}, Alert={HasAlert}",
                        entity.Id,
                        entity.Temperature,
                        entity.Humidity,
                        entity.Hydrogen,
                        entity.HasAlert
                    );

                    // Update Cache
                    _cacheService.InvalidateCache();
                }
                else
                {
                    _logger.LogDebug("⏭️ Skipped database storage (no significant change)");
                }

                // Step 4: update the latest reading in Cache
                _cacheService.CacheLatestReading(processedReading);

                // Step 5: send the processed reading via Notification Service
                await _notificationService.SendSensorReadingAsync(processedReading);

                // Step 6:send alerts if any
                if (alerts.Any())
                {
                    _logger.LogWarning("🚨 {Count} alert(s) detected", alerts.Count);

                    foreach (var alert in alerts)
                    {
                        await _notificationService.SendAlertAsync(alert);
                        _logger.LogWarning("📢 Alert sent: {Type} - {Message}",
                            alert.Type, alert.Message);
                    }
                }

                // Step 7:update today's chart if stored
                if (shouldStore)
                {
                    var chartData = await GetTodayChartDataAsync();
                    await _notificationService.SendChartUpdateAsync(chartData);
                }

                _logger.LogDebug("✅ Sensor reading processing completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing sensor reading");
                throw;
            }
        }

        public async Task<DashboardDataDto> GetDashboardDataAsync()
        {
            try
            {
                // try to get the latest reading from Cache first
                var latestReading = _cacheService.GetLatestReading();

                if (latestReading == null)
                {
                    // get from Database if not in Cache
                    var latestEntity = await _unitOfWork._sensorReadingRepository.GetLatestAsync();
                    latestReading = latestEntity != null ? MapToDto(latestEntity) : null;

                    //store in Cache
                    if (latestReading != null)
                    {
                        _cacheService.CacheLatestReading(latestReading);
                    }
                }

                var todayChart = await GetTodayChartDataAsync();

                var dashboard = new DashboardDataDto
                {
                    LatestReading = latestReading,
                    TodayChart = todayChart,
                    ActiveAlerts = new List<AlertDto>()
                };

                _logger.LogInformation("📊 Dashboard data retrieved successfully");
                return dashboard;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving dashboard data");
                throw;
            }
        }

        public async Task<ChartDataDto> GetTodayChartDataAsync()
        {
            try
            {
                //try to get from Cache first
                var cachedChart = _cacheService.GetTodayChart();
                if (cachedChart != null)
                {
                    _logger.LogDebug("📈 Chart data retrieved from cache");
                    return cachedChart;
                }

                // get aggregated data for the last 24 hours
                var endTime = DateTime.UtcNow;
                var startTime = endTime.AddHours(-24);

                var aggregatedData = await _unitOfWork._aggregatedSensorDataRepository
                    .GetByDateRangeAsync(startTime, endTime);

                var dataList = aggregatedData.OrderBy(d => d.StartTime).ToList();

                // group into 3-hour intervals and calculate averages
                var grouped = dataList
                    .GroupBy(d => new
                    {
                        //round down to nearest 3-hour block
                        Hour = d.StartTime.Hour / 3 * 3,
                        d.StartTime.Date
                    })
                    .Select(g => new
                    {
                        Time = new DateTime(g.Key.Date.Year, g.Key.Date.Month, g.Key.Date.Day, g.Key.Hour, 0, 0, DateTimeKind.Utc),
                        AvgTemp = g.Average(d => d.AvgTemperature),
                        AvgHum = g.Average(d => d.AvgHumidity)
                    })
                    .OrderBy(g => g.Time)
                    .ToList();

                ChartDataDto chartData;

                // ✨ use raw data as fallback if no aggregated data
                if (!grouped.Any())
                {
                    _logger.LogWarning("⚠️ No aggregated data found, using raw data as fallback");

                    var rawReadings = await _unitOfWork._sensorReadingRepository.GetTodayReadingsAsync();

                    chartData = new ChartDataDto
                    {
                        TemperatureData = rawReadings.Select(r => new ChartPointDto
                        {
                            Time = r.Timestamp,
                            Value = r.Temperature
                        }).ToList(),

                        HumidityData = rawReadings.Select(r => new ChartPointDto
                        {
                            Time = r.Timestamp,
                            Value = r.Humidity
                        }).ToList()
                    };

                    _logger.LogDebug("📈 Chart data from raw readings: {Count} points", rawReadings.Count());
                }
                else
                {
                    // use aggregated data
                    chartData = new ChartDataDto
                    {
                        TemperatureData = grouped.Select(g => new ChartPointDto
                        {
                            Time = g.Time,
                            Value = g.AvgTemp
                        }).ToList(),

                        HumidityData = grouped.Select(g => new ChartPointDto
                        {
                            Time = g.Time,
                            Value = g.AvgHum
                        }).ToList()
                    };

                    _logger.LogDebug("📈 Chart data from aggregated data: {Count} points (3-hour intervals)",
                        grouped.Count);
                }

                // store in Cache
                _cacheService.CacheTodayChart(chartData);

                return chartData;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving chart data");
                throw;
            }
        }

        private SensorReading MapToEntity(SensorReadingDto dto)
        {
            return new SensorReading
            {
                Hydrogen = dto.Hydrogen,
                Ethanol = dto.Ethanol,
                Temperature = dto.Temperature,
                Humidity = dto.Humidity,
                Pressure = dto.Pressure,
                Timestamp = dto.Timestamp,
                HeatIndex = dto.HeatIndex,
                HasAlert = dto.HasAlert,
                AlertType = dto.AlertType,
                AlertMessage = dto.AlertMessage
            };
        }

        private SensorReadingDto MapToDto(SensorReading entity)
        {
            return new SensorReadingDto
            {
                Hydrogen = entity.Hydrogen,
                Ethanol = entity.Ethanol,
                Temperature = entity.Temperature,
                Humidity = entity.Humidity,
                Pressure = entity.Pressure,
                Timestamp = entity.Timestamp,
                HeatIndex = entity.HeatIndex,
                HasAlert = entity.HasAlert,
                AlertType = entity.AlertType,
                AlertMessage = entity.AlertMessage
            };
        }

    }
}
