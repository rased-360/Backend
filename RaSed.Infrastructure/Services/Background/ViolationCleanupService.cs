using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Background
{
    /// <summary>
    /// Hosted background service that runs daily and deletes violations
    /// older than the configured retention period (default: 60 days = 2 months).
    ///
    /// Registered in Program.cs via builder.Services.AddHostedService&lt;ViolationCleanupService&gt;()
    /// </summary>
    public class ViolationCleanupService : BackgroundService
    {
        private readonly ILogger<ViolationCleanupService> _logger;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly int _retentionDays;
        private readonly TimeSpan _interval;

        public ViolationCleanupService(
            ILogger<ViolationCleanupService> logger,
            IServiceScopeFactory scopeFactory,
            IOptions<CleanupSettings> settings)       
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _retentionDays = settings.Value.Violations.RetentionDays;
            _interval = TimeSpan.FromHours(settings.Value.Violations.IntervalHours);

            _logger.LogInformation(
                "🧹 ViolationCleanupService configured — RetentionDays: {Retention}, Interval: {Interval}h",
                _retentionDays, _interval.TotalHours);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🧹 ViolationCleanupService started");

            // Run cleanup immediately on startup, then on the configured interval
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunCleanupAsync();

                try
                {
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (OperationCanceledException)
                {
                    break; // Graceful shutdown
                }
            }

            _logger.LogInformation("🛑 ViolationCleanupService stopped");
        }

        private async Task RunCleanupAsync()
        {
            try
            {
                _logger.LogInformation(
                    "🧹 Starting violation cleanup — removing records older than {Days} days", _retentionDays);

                // IViolationService is scoped — must create a scope per run
                using var scope = _scopeFactory.CreateScope();
                var violationService = scope.ServiceProvider.GetRequiredService<IViolationService>();

                var deleted = await violationService.DeleteOldViolationsAsync(_retentionDays);

                _logger.LogInformation(
                    "✅ Cleanup complete — {Count} violation(s) removed", deleted);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error during violation cleanup");
                // Do NOT rethrow — we never want cleanup to crash the host process
            }
        }
    }
}
