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
    public class OtpCleanUpService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly ILogger<OtpCleanUpService> _logger;
        private readonly TimeSpan _period = TimeSpan.FromHours(1);

        public OtpCleanUpService(
            IServiceProvider serviceProvider,
            ILogger<OtpCleanUpService> logger)
        {
            _serviceProvider = serviceProvider;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // wait until the application is fully started
            using PeriodicTimer timer = new PeriodicTimer(_period);

            // start the first run immediately
            await DoWorkAsync(stoppingToken);

            // then continue on the defined period
            while (!stoppingToken.IsCancellationRequested &&
                   await timer.WaitForNextTickAsync(stoppingToken))
            {
                await DoWorkAsync(stoppingToken);
            }
        }

        private async Task DoWorkAsync(CancellationToken cancellationToken)
        {
            try
            {
                _logger.LogInformation("Starting OTP cleanup job at {Time}", DateTime.UtcNow);

                using var scope = _serviceProvider.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // remove expired OTPs
                var deletedCount = await dbContext.Otps
                    .Where(otp => otp.ExpiresAt <= DateTime.UtcNow)
                    .ExecuteDeleteAsync(cancellationToken);

                _logger.LogInformation(
                    "Cleaned up {Count} expired OTPs at {Time}",
                    deletedCount,
                    DateTime.UtcNow);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("OTP cleanup job was cancelled");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while cleaning up OTPs");
            }
        }

        public override async Task StopAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("OTP Cleanup Service is stopping");
            await base.StopAsync(cancellationToken);
        }
    }

}
