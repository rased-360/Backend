using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class DataCleanupService: BackgroundService
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;
        private readonly ILogger<DataCleanupService> _logger;

        // set to run once every 24 hours
        private static readonly TimeSpan CLEANUP_INTERVAL = TimeSpan.FromHours(24);

        // constants for data retention
        private const int RAW_DATA_RETENTION_DAYS = 7;
        private const int AGGREGATED_DATA_RETENTION_DAYS = 90;

        public DataCleanupService(
            IServiceScopeFactory serviceScopeFactory,
            ILogger<DataCleanupService> logger)
        {
            _serviceScopeFactory = serviceScopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧹 Data Cleanup Service started");

            // wait 1 hour before starting the first cleanup
            await Task.Delay(TimeSpan.FromHours(1), stoppingToken);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupOldDataAsync();

                    _logger.LogInformation("✅ Data cleanup completed. Next run in {Hours} hours",
                        CLEANUP_INTERVAL.TotalHours);

                    await Task.Delay(CLEANUP_INTERVAL, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error during data cleanup. Retrying in 1 hour...");
                    await Task.Delay(TimeSpan.FromHours(1), stoppingToken);
                }
            }

            _logger.LogInformation("🛑 Data Cleanup Service stopped");
        }

        private async Task CleanupOldDataAsync()
        {
            using var scope = _serviceScopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            _logger.LogInformation("🧹 Starting data cleanup process...");

            //  delete raw data Older than 7 days
            var rawDataCutoff = DateTime.UtcNow.AddDays(-RAW_DATA_RETENTION_DAYS);
            var deletedRawCount = await CleanupRawDataAsync(unitOfWork, rawDataCutoff);

            // delete aggregated data older than 90 days
            var aggregatedCutoff = DateTime.UtcNow.AddDays(-AGGREGATED_DATA_RETENTION_DAYS);
            var deletedAggregatedCount = await CleanupAggregatedDataAsync(unitOfWork, aggregatedCutoff);

            await unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "✅ Cleanup completed: {RawCount} raw readings deleted, {AggCount} aggregated records deleted",
                deletedRawCount,
                deletedAggregatedCount);
        }

        private async Task<int> CleanupRawDataAsync(IUnitOfWork unitOfWork, DateTime cutoffDate)
        {
            var deletedCount = await unitOfWork._sensorReadingRepository
                .DeleteOlderThanAsync(cutoffDate);

            return deletedCount;
        }

        private async Task<int> CleanupAggregatedDataAsync(IUnitOfWork unitOfWork, DateTime cutoffDate)
        {
            _logger.LogInformation("🗑️ Deleting aggregated data older than {Date}",
                cutoffDate.ToString("yyyy-MM-dd"));

            var deletedCount = await unitOfWork._aggregatedSensorDataRepository
                .DeleteOlderThanAsync(cutoffDate);

            if (deletedCount == 0)
            {
                _logger.LogInformation("✅ No old aggregated data to delete");
            }
            else
            {
                _logger.LogInformation("✅ Deleted {Count} old aggregated records", deletedCount);
            }

            return deletedCount;
        }
    }
}
