using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Domain.Interfaces;

namespace RaSed.Infrastructure.Services.Background
{
    /// <summary>
    /// Background service that deletes old fire events from the database.
    /// 
    /// SCHEDULE:
    ///   - Runs every 24 hours
    ///   - First run: 1 minute after startup (configurable)
    ///   - Subsequent runs: every 24 hours
    /// 
    /// CONFIGURATION:
    ///   - Retention period: 30 days (configurable)
    ///   - Only deletes RESOLVED events
    ///   - Never deletes ACTIVE events
    /// 
    /// WHY:
    ///   - Prevents database from growing indefinitely
    ///   - Old fire events are only needed for historical analysis
    ///   - Automated cleanup = no manual maintenance
    /// 
    /// </summary>
    public class FireEventCleanupService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<FireEventCleanupService> _logger;
        private readonly int _retentionDays;
        private readonly TimeSpan _interval;
        private readonly TimeSpan _initialDelay;

        public FireEventCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<FireEventCleanupService> logger,
            IOptions<CleanupSettings> settings)     
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
            _retentionDays = settings.Value.FireEvents.RetentionDays;
            _interval = TimeSpan.FromHours(settings.Value.FireEvents.IntervalHours);
            _initialDelay = TimeSpan.FromMinutes(settings.Value.FireEvents.InitialDelayMinutes);

            _logger.LogInformation(
                "🧹 FireEventCleanupService configured — RetentionDays: {Retention}, Interval: {Interval}h",
                _retentionDays, _interval.TotalHours);
        }

        // ── ExecuteAsync ──────────────────────────────────────────────────────

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "🧹 Fire Event Cleanup Service starting — Retention: {Days} days, Interval: {Interval}",
                _retentionDays, _interval);

            await Task.Delay(_initialDelay, stoppingToken);  // ← was hardcoded

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanupAsync(stoppingToken);
                    _logger.LogInformation("⏰ Next cleanup in {Hours} hours", _interval.TotalHours);
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    _logger.LogInformation("🛑 Fire Event Cleanup Service stopping");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in cleanup — retrying in {Hours}h", _interval.TotalHours);
                    await Task.Delay(_interval, stoppingToken);
                }
            }
        }

        // PerformCleanupAsync — replace the hardcoded RETENTION_DAYS with _retentionDays:
        private async Task PerformCleanupAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "🧹 Starting fire event cleanup — deleting events older than {Days} days", _retentionDays);

            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var deletedCount = await unitOfWork._fireEventRepository
                .DeleteOldFireEventsAsync(_retentionDays);   // ← was RETENTION_DAYS constant

            _logger.LogInformation(
                deletedCount > 0
                    ? "✅ Cleanup complete — deleted {Count} old fire event(s)"
                    : "✅ Cleanup complete — no old events to delete",
                deletedCount);
        }

        // ── StopAsync ─────────────────────────────────────────────────────────

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Fire Event Cleanup Service stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}