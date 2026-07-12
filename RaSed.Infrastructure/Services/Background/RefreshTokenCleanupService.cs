using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Background
{
    public class RefreshTokenCleanupService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<RefreshTokenCleanupService> _logger;
        private readonly TimeSpan _interval;
        private readonly int _expiredRetentionDays;
        private readonly int _revokedRetentionDays;

        public RefreshTokenCleanupService(
            IServiceProvider serviceProvider,
            ILogger<RefreshTokenCleanupService> logger,
            IOptions<CleanupSettings> settings)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
            _interval = TimeSpan.FromHours(settings.Value.RefreshTokens.IntervalHours);
            _expiredRetentionDays = settings.Value.RefreshTokens.ExpiredRetentionDays;
            _revokedRetentionDays = settings.Value.RefreshTokens.RevokedRetentionDays;

            _logger.LogInformation(
                "🧹 RefreshTokenCleanupService — ExpiredRetention: {Expired}d, RevokedRetention: {Revoked}d",
                _expiredRetentionDays, _revokedRetentionDays);
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

            var expiredCutoff = DateTime.UtcNow.AddDays(-_expiredRetentionDays);
            var revokedCutoff = DateTime.UtcNow.AddDays(-_revokedRetentionDays);

            var tokensToDelete = await context.RefreshTokens
                .Where(t =>
                    // Expired tokens — safe to delete quickly, they can't be used
                    (t.Revoked == null && t.Expires < expiredCutoff) ||

                    // Revoked tokens — keep longer for reuse detection audit trail
                    // Only delete after RevokedRetentionDays have passed since revocation
                    (t.Revoked != null && t.Revoked < revokedCutoff)
                )
                .ToListAsync();

            if (tokensToDelete.Any())
            {
                context.RefreshTokens.RemoveRange(tokensToDelete);
                await context.SaveChangesAsync();

                _logger.LogInformation(
                    "🧹 Cleaned up {Count} refresh token(s)", tokensToDelete.Count);
            }
            else
            {
                _logger.LogInformation("🧹 No refresh tokens to clean up");
            }
        }
    }
}
