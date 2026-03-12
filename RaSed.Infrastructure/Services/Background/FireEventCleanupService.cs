using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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

        // ── Configuration ─────────────────────────────────────────────────────

        /// <summary>How many days to keep fire events before deletion</summary>
        private const int RETENTION_DAYS = 30;

        /// <summary>How often to run cleanup (24 hours)</summary>
        private static readonly TimeSpan CLEANUP_INTERVAL = TimeSpan.FromHours(24);

        /// <summary>Delay before first cleanup (1 minute after startup)</summary>
        private static readonly TimeSpan INITIAL_DELAY = TimeSpan.FromMinutes(1);

        public FireEventCleanupService(
            IServiceScopeFactory scopeFactory,
            ILogger<FireEventCleanupService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        // ── ExecuteAsync ──────────────────────────────────────────────────────

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "🧹 Fire Event Cleanup Service starting — Retention: {Days} days, Interval: {Interval}",
                RETENTION_DAYS, CLEANUP_INTERVAL);

            // Wait before first run (let the app fully start)
            await Task.Delay(INITIAL_DELAY, stoppingToken);

            // Infinite loop — runs until app shutdown
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await PerformCleanupAsync(stoppingToken);

                    // Wait until next cleanup
                    _logger.LogInformation(
                        "⏰ Next cleanup in {Hours} hours",
                        CLEANUP_INTERVAL.TotalHours);

                    await Task.Delay(CLEANUP_INTERVAL, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    // App is shutting down
                    _logger.LogInformation("🛑 Fire Event Cleanup Service stopping (shutdown requested)");
                    break;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Error in cleanup service — will retry in {Hours} hours", CLEANUP_INTERVAL.TotalHours);

                    // Continue running despite error — retry on next interval
                    await Task.Delay(CLEANUP_INTERVAL, stoppingToken);
                }
            }
        }

        // ── PerformCleanupAsync ───────────────────────────────────────────────

        /// <summary>
        /// Executes the cleanup operation.
        /// 
        /// STEPS:
        ///   1. Create a new scope (for scoped services like DbContext)
        ///   2. Get IUnitOfWork
        ///   3. Call DeleteOldFireEventsAsync
        ///   4. Log results
        /// </summary>
        private async Task PerformCleanupAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation(
                "🧹 Starting fire event cleanup — deleting events older than {Days} days",
                RETENTION_DAYS);

            try
            {
                // Create new scope for scoped services (DbContext is scoped)
                using var scope = _scopeFactory.CreateScope();
                var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

                // Execute cleanup
                var deletedCount = await unitOfWork._fireEventRepository
                    .DeleteOldFireEventsAsync(RETENTION_DAYS);

                if (deletedCount > 0)
                {
                    _logger.LogInformation(
                        "✅ Cleanup complete — deleted {Count} old fire event(s)",
                        deletedCount);
                }
                else
                {
                    _logger.LogInformation(
                        "✅ Cleanup complete — no old events to delete");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during fire event cleanup");
                throw;
            }
        }

        // ── StopAsync ─────────────────────────────────────────────────────────

        public override Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("🛑 Fire Event Cleanup Service stopped");
            return base.StopAsync(cancellationToken);
        }
    }
}