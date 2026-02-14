using Microsoft.EntityFrameworkCore;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Application.Interfaces;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using RaSed.Infrastructure.Repositories;
using RaSed.Infrastructure.Services.Authantication;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using RaSed.Infrastructure.Services;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Infrastructure.Services.Realtime;
using RaSed.Application.Configuration;

namespace RaSed.API.Extensions
{
    public static class ServiceExtensions
    {
        public static IServiceCollection AddDatabaseServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddDbContext<AppDbContext>(options =>
                options.UseNpgsql(configuration.GetConnectionString("DefaultConnection")));

            return services;
        }
        public static IServiceCollection AddConfigurationSettings(this IServiceCollection services, IConfiguration configuration)
        {
            // Bind MqttSettings from appsettings.json
            services.Configure<MqttSettings>(configuration.GetSection("MqttSettings"));

            // Bind AlertThresholds from appsettings.json
            services.Configure<AlertThresholds>(configuration.GetSection("AlertThresholds"));

            return services;
        }

        public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                // Global rate limiter rejection behavior
                options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

                options.OnRejected = async (context, cancellationToken) =>
                {
                    context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                    context.HttpContext.Response.ContentType = "application/json";

                    var retryAfterSeconds = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? (int)retryAfter.TotalSeconds
                        : 60; // Default to 60 seconds

                    context.HttpContext.Response.Headers["Retry-After"] = retryAfterSeconds.ToString();

                    var response = new
                    {
                        error = "Rate limit exceeded. Too many requests.",
                        retryAfter = $"{retryAfterSeconds} seconds"
                    };

                    await context.HttpContext.Response.WriteAsJsonAsync(response, cancellationToken);
                };

                // Login rate limiter
                options.AddFixedWindowLimiter("login", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 5;
                    opt.QueueLimit = 0;
                    opt.AutoReplenishment = true; // ✅ Important: auto-reset after window expires
                });

                //  OTP Send
                options.AddFixedWindowLimiter("otp-send-limit", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(10);
                    opt.PermitLimit = 5;  // Max 3 OTP requests per 10 minutes per IP
                    opt.QueueLimit = 0;
                });

                // OTP Verify (prevent brute force)
                options.AddFixedWindowLimiter("otp-verify-limit", opt =>
                {
                    opt.Window = TimeSpan.FromMinutes(5);
                    opt.PermitLimit = 5;  // Max 5 verification attempts per 5 minutes per IP
                    opt.QueueLimit = 0;
                });
            });

            return services;
        }

        public static IServiceCollection AddRepositories(this IServiceCollection services)
        {
            services.AddScoped<IUnitOfWork, UnitOfWork>();
            services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            services.AddScoped<IAdminRepository, AdminRepository>();
            services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
            services.AddScoped<IOtpRepository, OtpRepository>();
            services.AddScoped<IEmployeeRepository, EmployeeRepository>();
            services.AddScoped<ISectionRepository, SectionRepository>();
            services.AddScoped<ICloudinaryService, CloudinaryService>();
            services.AddScoped<IProfileService, ProfileService>();
            services.AddScoped<ISensorReadingRepository, SensorReadingRepository>();
            services.AddScoped<IAggregatedSensorDataRepository, AggregatedSensorDataRepository>(); 
            services.AddScoped<IIssueRepository, IssueRepository>();

            return services;
        }

        public static IServiceCollection AddApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<IAdminAuthService, AdminAuthService>();
            services.AddScoped<ITokenService, TokenService>();
            services.AddScoped<IAdminService, AdminService>();
            services.AddScoped<IPasswordService, PasswordService>();
            services.AddScoped<IPhoneService, PhoneService>();
            services.AddScoped<IOtpService, OtpService>();
            services.AddScoped<IEmailService, EmailService>();
            services.AddScoped<IEmployeeAuthService, EmployeeAuthService>();
            services.AddScoped<IEmployeeService, EmployeeService>();
            services.AddScoped<ISectionService, SectionService>();
            services.AddScoped<ISensorDataService, SensorDataService>();
            services.AddScoped<ISensorDataProcessor, SensorDataProcessor>();
            services.AddScoped<IRealtimeNotificationService, RealtimeNotificationService>();
            services.AddScoped<IIssueService, IssueService>();
            services.AddSingleton<SensorCacheService>();

            return services;
        }
        public static IServiceCollection AddBackgroundServices(this IServiceCollection services)
        {        
            services.AddHostedService<MqttBackgroundService>();
            services.AddHostedService<DataAggregationService>();
            services.AddHostedService<DataCleanupService>(); 
            return services;
        }
    }
}
