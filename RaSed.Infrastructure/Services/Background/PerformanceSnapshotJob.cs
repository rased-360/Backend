using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Background
{
    public class PerformanceSnapshotJob : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<PerformanceSnapshotJob> _logger;

        // Settings are read once at construction time via IOptions<T> snapshot.
        // If you need live-reload use IOptionsMonitor<T> instead.
        private readonly PerformanceSettings _settings;

        public PerformanceSnapshotJob(
            IServiceScopeFactory scopeFactory,
            IOptions<PerformanceSettings> settings,
            ILogger<PerformanceSnapshotJob> logger)
        {
            _scopeFactory = scopeFactory;
            _settings = settings.Value;
            _logger = logger;
        }

        // ── Entry point ───────────────────────────────────────────────────────

        /// <summary>
        /// Called once by the ASP.NET Core host when the application starts.
        /// Runs the snapshot loop until the host requests cancellation
        /// (i.e. the application is shutting down).
        /// </summary>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation(
                "📊 PerformanceSnapshotJob started — interval: every {Hours}h, " +
                "window: {Days} days, penalty: {Penalty} pts/violation",
                _settings.JobIntervalHours, _settings.WindowDays, _settings.PenaltyPerViolation);

            // Run once immediately on startup so the table is populated before
            // the first API request arrives, then repeat on the configured interval.
            while (!stoppingToken.IsCancellationRequested)
            {
                await RunJobAsync(stoppingToken);

                // Wait for the next interval — or exit immediately if cancellation
                // is requested during the delay (graceful shutdown).
                try
                {
                    await Task.Delay(
                        TimeSpan.FromHours(_settings.JobIntervalHours),
                        stoppingToken);
                }
                catch (TaskCanceledException)
                {
                    // Application is shutting down — exit the loop cleanly.
                    break;
                }
            }

            _logger.LogInformation("📊 PerformanceSnapshotJob stopped.");
        }

        // ── Core job logic ────────────────────────────────────────────────────

        /// <summary>
        /// One full execution cycle:
        ///   - Opens a fresh DI scope (required for scoped services like IUnitOfWork)
        ///   - Fetches all employee IDs
        ///   - Upserts a snapshot for each one
        ///   - Logs a completion summary
        /// </summary>
        private async Task RunJobAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("📊 PerformanceSnapshotJob — run starting at {Time}", DateTime.UtcNow);

            // Create a new DI scope for this run.
            // This gives us a fresh DbContext / UnitOfWork so there are no
            // stale tracked entities from a previous run.
            using var scope = _scopeFactory.CreateScope();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var windowStart = DateTime.UtcNow.AddDays(-_settings.WindowDays);
            var employeeIds = (await unitOfWork._performanceSnapshotRepository.GetAllEmployeeIdsAsync()).ToList();

            if (employeeIds.Count == 0)
            {
                _logger.LogInformation("📊 PerformanceSnapshotJob — no employees found, skipping run.");
                return;
            }

            int successCount = 0;
            int errorCount = 0;

            foreach (var employeeId in employeeIds)
            {
                // Check for shutdown between employees so the job exits quickly
                // on application stop without leaving the DB in a partial state.
                if (cancellationToken.IsCancellationRequested)
                    break;

                try
                {
                    // ── Count violations in the rolling window ────────────────
                    // Pure COUNT(*) — no entities loaded into memory.
                    var violationCount = await unitOfWork._violationRepository
                        .CountViolationsByEmployeeInWindowAsync(employeeId, windowStart);

                    // ── Calculate score ───────────────────────────────────────
                    var rate = Math.Max(0, 100 - (violationCount * _settings.PenaltyPerViolation));

                    // ── Build snapshot ────────────────────────────────────────
                    var snapshot = new PerformanceSnapshot
                    {
                        EmployeeId = employeeId,
                        PerformanceRate = Math.Round(rate, 2),
                        Rating = ResolveRating(rate),
                        ViolationCount = violationCount,
                        WindowDays = _settings.WindowDays,
                        LastCalculatedAt = DateTime.UtcNow
                    };

                    // ── Upsert + save ─────────────────────────────────────────
                    // SaveChangesAsync is called per employee (not once at the end)
                    // so a failure on one employee does not roll back all others.
                    await unitOfWork._performanceSnapshotRepository.UpsertAsync(snapshot);
                    await unitOfWork.SaveChangesAsync();

                    successCount++;
                }
                catch (Exception ex)
                {
                    errorCount++;
                    _logger.LogError(ex,
                        "❌ PerformanceSnapshotJob — failed to process employee {EmployeeId}", employeeId);
                    // Continue to the next employee — one failure must not stop the whole run.
                }
            }

            _logger.LogInformation(
                "📊 PerformanceSnapshotJob — run complete. " +
                "✅ {Success} updated | ❌ {Errors} failed | ⏱ {Time}",
                successCount, errorCount, DateTime.UtcNow);
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        /// <summary>
        /// Maps a numeric score to one of the four rating tiers.
        /// Must stay in sync with the same method in PerformanceService.
        /// </summary>
        private static string ResolveRating(double rate) => rate switch
        {
            >= 90 => "Excellent",
            >= 75 => "VeryGood",
            >= 50 => "Good",
            _ => "Bad"
        };
    }
}
