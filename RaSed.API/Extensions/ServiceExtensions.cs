using Microsoft.EntityFrameworkCore;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Application.Interfaces;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using RaSed.Infrastructure.Repositories;
using RaSed.Infrastructure.Services.Authantication;
using Microsoft.AspNetCore.RateLimiting;

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

        public static IServiceCollection AddCustomRateLimiter(this IServiceCollection services)
        {
            services.AddRateLimiter(options =>
            {
                options.AddFixedWindowLimiter("login", opt =>
                {

                    opt.Window = TimeSpan.FromMinutes(1);
                    opt.PermitLimit = 5; 
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

            return services;
        }
    }
}
