using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.FcmTokens;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class FcmTokenService : IFcmTokenService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<FcmTokenService> _logger;

        public FcmTokenService(IUnitOfWork unitOfWork, ILogger<FcmTokenService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task RegisterTokenAsync(int userId, RegisterFcmTokenDto dto)
        {
            // Check if token already exists
            var existing = await _unitOfWork._fcmDeviceTokenRepository
                .GetByTokenAsync(dto.Token);

            if (existing != null)
            {
                // Token exists — just update LastUsedAt
                existing.LastUsedAt = DateTime.UtcNow;
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "🔄 FCM token refreshed — UserId: {UserId}, Platform: {Platform}",
                    userId, dto.Platform);
                return;
            }

            // New token — save it
            var token = new FcmDeviceToken
            {
                EmployeeId = userId,
                Token = dto.Token,
                Platform = dto.Platform.ToLower(),
                RegisteredAt = DateTime.UtcNow,
                LastUsedAt = DateTime.UtcNow
            };

            await _unitOfWork._fcmDeviceTokenRepository.AddAsync(token);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "✅ FCM token registered — UserId: {UserId}, Platform: {Platform}",
                userId, dto.Platform);
        }

        public async Task RemoveTokenAsync(string token)
        {
            var deleted = await _unitOfWork._fcmDeviceTokenRepository
                .DeleteByTokenAsync(token);

            if (deleted)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("🗑️ FCM token removed");
            }
        }
    }
}
