using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class DataAggregationService: BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DataAggregationService> _logger;

        // every hour
        private static readonly TimeSpan AGGREGATION_INTERVAL = TimeSpan.FromHours(1);

        public DataAggregationService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DataAggregationService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🔄 Data Aggregation Service started");

            // wait 5 minutes before starting the first aggregation
            await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await AggregateLastHourDataAsync();

                    _logger.LogInformation("✅ Data aggregation completed. Next run in {Minutes} minutes",
                        AGGREGATION_INTERVAL.TotalMinutes);

                    await Task.Delay(AGGREGATION_INTERVAL, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during data aggregation. Retrying in 10 minutes...");
                    await Task.Delay(TimeSpan.FromMinutes(10), stoppingToken);
                }
            }

            _logger.LogInformation("🛑 Data Aggregation Service stopped");
        }

        private async Task AggregateLastHourDataAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var endTime = DateTime.UtcNow;
            var startTime = endTime.AddHours(-1);

            _logger.LogInformation("📊 Aggregating data from {Start} to {End}",
                startTime.ToString("yyyy-MM-dd HH:mm"),
                endTime.ToString("yyyy-MM-dd HH:mm"));

            //get readings from last hour
            var readings = (await unitOfWork._sensorReadingRepository.GetLast24HoursAsync())
                .Where(r => r.Timestamp >= startTime && r.Timestamp < endTime)
                .ToList();

            if (!readings.Any())
            {
                _logger.LogInformation("⚠️ No readings found for the last hour");
                return;
            }

            //calculate aggregates
            var aggregatedData = new AggregatedSensorData
            {
                StartTime = startTime,
                EndTime = endTime,
                ReadingCount = readings.Count,
                AlertCount = readings.Count(r => r.HasAlert),

                // Temperature
                AvgTemperature = readings.Average(r => r.Temperature),
                MinTemperature = readings.Min(r => r.Temperature),
                MaxTemperature = readings.Max(r => r.Temperature),

                // Humidity
                AvgHumidity = readings.Average(r => r.Humidity),
                MinHumidity = readings.Min(r => r.Humidity),
                MaxHumidity = readings.Max(r => r.Humidity),

                // Pressure
                AvgPressure = readings.Average(r => r.Pressure),
                MinPressure = readings.Min(r => r.Pressure),
                MaxPressure = readings.Max(r => r.Pressure),

                // Hydrogen
                AvgHydrogen = (int)readings.Average(r => r.Hydrogen),
                MinHydrogen = readings.Min(r => r.Hydrogen),
                MaxHydrogen = readings.Max(r => r.Hydrogen),

                // Ethanol
                AvgEthanol = (int)readings.Average(r => r.Ethanol),
                MinEthanol = readings.Min(r => r.Ethanol),
                MaxEthanol = readings.Max(r => r.Ethanol)
            };

            // store aggregated data
            await unitOfWork._aggregatedSensorDataRepository.AddAsync(aggregatedData);
            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "✅ Aggregated {Count} readings - Avg Temp: {Temp}°C, Avg Humidity: {Hum}%, Alerts: {Alerts}",
                readings.Count,
                aggregatedData.AvgTemperature,
                aggregatedData.AvgHumidity,
                aggregatedData.AlertCount);
        }
    }
}
