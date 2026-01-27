
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaSed.Application.Interfaces;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using RaSed.Infrastructure.Data.Seed;
using RaSed.Infrastructure.Repositories;
using RaSed.Infrastructure.Services.Authantication;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using RaSed.API.Extensions;
using RaSed.Infrastructure.Hubs;


namespace RaSed.API
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container
            builder.Services.AddControllers()
            .ConfigureApiBehaviorOptions(options =>
            {
                //  Disable automatic 400 responses for model validation errors
                options.SuppressModelStateInvalidFilter = true;
            });

            // Configure services using extension methods
            builder.Services.AddDatabaseServices(builder.Configuration);
            builder.Services.AddIdentityServices();
            builder.Services.AddRepositories();
            builder.Services.AddApplicationServices();
            builder.Services.AddConfigurationSettings(builder.Configuration);
            builder.Services.AddBackgroundServices();
            builder.Services.AddJwtAuthentication(builder.Configuration);
            builder.Services.AddSwaggerWithJwt();
            builder.Services.AddCustomRateLimiter();
            builder.Services.AddMemoryCache();

            // Register the OtpCleanUpService as a hosted service
            builder.Services.AddHostedService<OtpCleanUpService>();
            builder.Services.AddHostedService<RefreshTokenCleanupService>();

            // ? Add SignalR
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = builder.Environment.IsDevelopment();
                options.KeepAliveInterval = TimeSpan.FromSeconds(10);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            });

            // CORS Configuration - Improved
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("DynamicCorsPolicy", policy =>
                {
                    var allowedOrigins = builder.Configuration
                        .GetSection("AllowedOrigins")
                        .Get<string[]>() ?? Array.Empty<string>();

                    if (allowedOrigins.Length > 0)
                    {
                        // Production with known domains
                        policy.WithOrigins(allowedOrigins)
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                    else if (builder.Environment.IsDevelopment())
                    {
                        // Development mode - Allow all origins
                        policy.AllowAnyOrigin()
                              .AllowAnyHeader()
                              .AllowAnyMethod();
                    }
                    else
                    { 
                        // Production without AllowedOrigins - Restrict to API domain only
                        policy.WithOrigins("https://rasedapi.runasp.net")
                              .AllowAnyHeader()
                              .AllowAnyMethod()
                              .AllowCredentials();
                    }
                });
            });



            var app = builder.Build();

            // Seed default data (Roles + SuperAdmin)
            using (var scope = app.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                try
                {
                    var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
                    var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
                    var context = services.GetRequiredService<AppDbContext>();

                    // Seed SuperAdmin
                    await AdminSuperSeeder.SeedSuperAdminAsync(userManager, roleManager);

                    // Seed Sections
                    await SectionSeeder.SeedSectionsAsync(context);
                }
                catch (Exception ex)
                {
                    var logger = services.GetRequiredService<ILogger<Program>>();
                    logger.LogError(ex, "An error occurred while seeding the database.");
                }
            }

            // Configure the HTTP request pipeline
            if (app.Environment.IsDevelopment())
            {
                app.MapOpenApi();
                app.UseSwaggerUI(options =>
                {
                    options.SwaggerEndpoint("/openapi/v1.json", "api");
                });
            }

            app.UseHttpsRedirection();
            app.UseCors("DynamicCorsPolicy");
            app.UseAuthentication();
            app.UseAuthorization();
            app.UseRateLimiter();
            app.MapControllers();
            // Map SignalR Hub
            app.MapHub<SensorHub>("/sensorHub");

            app.Run();
        }
    }
}
