using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefreshTokenCleanupService> _logger;
        private readonly TimeSpan _interval = TimeSpan.FromHours(24); // Run daily

        public RefreshTokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<RefreshTokenCleanupService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Refresh Token Cleanup Service started");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await CleanupExpiredTokensAsync();
                    await Task.Delay(_interval, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in Refresh Token Cleanup Service");
                    await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
                }
            }
        }

        private async Task CleanupExpiredTokensAsync()
        {
            using var scope = _serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var cutoffDate = DateTime.UtcNow.AddDays(-30); // Delete tokens older than 30 days

            var expiredTokens = await context.RefreshTokens
                .Where(t => t.Expires < cutoffDate ||
                           (t.Revoked != null && t.Revoked < cutoffDate))
                .ToListAsync();

            if (expiredTokens.Any())
            {
                context.RefreshTokens.RemoveRange(expiredTokens);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "Cleaned up {Count} expired refresh tokens",
                    expiredTokens.Count);
            }
        }
    }
}
