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
        private readonly ILogger<SensorDataService> _logger;

        public SensorDataService(
            IUnitOfWork unitOfWork,
            ISensorDataProcessor processor,
            IRealtimeNotificationService notificationService,
            ILogger<SensorDataService> logger)
        {
            _unitOfWork = unitOfWork;
            _processor = processor;
            _notificationService = notificationService;
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

                // Step 3: Map to entity and save to database
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

                // Step 4: Send real-time update to connected clients
                await _notificationService.SendSensorReadingAsync(processedReading);

                // Step 5: Send alerts if any exist
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

                // Step 6: Update chart data
                var chartData = await GetTodayChartDataAsync();
                await _notificationService.SendChartUpdateAsync(chartData);

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
                var latestReading = await _unitOfWork._sensorReadingRepository.GetLatestAsync();
                var todayChart = await GetTodayChartDataAsync();

                var dashboard = new DashboardDataDto
                {
                    LatestReading = latestReading != null
                        ? MapToDto(latestReading)
                        : null,
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
                var readings = await _unitOfWork._sensorReadingRepository.GetTodayReadingsAsync();

                var chartData = new ChartDataDto
                {
                    TemperatureData = readings.Select(r => new ChartPointDto
                    {
                        Time = r.Timestamp,
                        Value = r.Temperature
                    }).ToList(),

                    HumidityData = readings.Select(r => new ChartPointDto
                    {
                        Time = r.Timestamp,
                        Value = r.Humidity
                    }).ToList(),
                };

                _logger.LogDebug("📈 Chart data retrieved: {Count} readings", readings.Count());
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
