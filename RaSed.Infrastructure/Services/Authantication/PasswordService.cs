using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class PasswordService : IPasswordService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PasswordService> _logger;
        private readonly IOtpService _otpService;
        public PasswordService(
            UserManager<ApplicationUser> userManager,
            IUnitOfWork unitOfWork,
            ILogger<PasswordService> logger,
            IOtpService otpService)
        {
            _userManager = userManager;
            _unitOfWork = unitOfWork;
            _logger = logger;
            _otpService = otpService;
        }
        public async Task<ServerOperationResult> ChangePasswordAsync(int userId, ChangePasswordDto dto)
        {
            try
            {
                _logger.LogInformation("Change password attempt for user ID: {UserId}", userId);

                // 1. Find user
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return ServerOperationResult.Failure("User not found.");
                }

                // 2. Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted password change: {UserId}", userId);
                    return ServerOperationResult.Failure("Account is deactivated.");
                }

                // 3. Verify old password
                var isOldPasswordValid = await _userManager.CheckPasswordAsync(user, dto.OldPassword);
                if (!isOldPasswordValid)
                {
                    _logger.LogWarning("Invalid old password for user: {UserId}", userId);
                    return ServerOperationResult.Failure("Current password is incorrect.");
                }

                // 4. Check if new password is same as old password
                var isSamePassword = await _userManager.CheckPasswordAsync(user, dto.NewPassword);
                if (isSamePassword)
                {
                    _logger.LogWarning("User tried to use same password: {UserId}", userId);
                    return ServerOperationResult.Failure("New password must be different from current password.");
                }

                // 5. Change password using Identity
                var result = await _userManager.ChangePasswordAsync(user, dto.OldPassword, dto.NewPassword);

                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Password change failed for user {UserId}: {Errors}",
                        userId, string.Join(", ", errors));
                    return ServerOperationResult.Failure(errors, "Failed to change password.");
                }

                // 6. Update Admin-specific fields (if user is Admin)
                if (user is Admin admin)
                {
                    admin.MustChangePassword = false;
                    admin.PasswordChangedAt = DateTime.UtcNow;

                    // Save changes
                    await _unitOfWork.SaveChangesAsync();
                }

                // 7. Revoke all refresh tokens (force re-login for security)
                await _unitOfWork._refreshTokenRepository.RevokeAllUserTokensAsync(userId);

                _logger.LogInformation("Password changed successfully for user: {UserId}", userId);

                return ServerOperationResult.Success(
                    "Password changed successfully. Please login again with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user: {UserId}", userId);
                return ServerOperationResult.Failure("An error occurred while changing password.");
            }
        }

        //Reset Password Method
        public async Task<ServerOperationResult> ResetPasswordAsync(int userId, ResetPasswordDto dto)
        {
            try
            {
                _logger.LogInformation("Reset password attempt for user ID: {UserId}", userId);
                
                // Find user
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    _logger.LogWarning("User not found: {UserId}", userId);
                    return ServerOperationResult.Failure("User not found.");
                }

                // Validate new password and confirmation
                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    _logger.LogWarning("Password confirmation does not match for user: {UserId}", userId);
                    return ServerOperationResult.Failure("Password confirmation does not match.");
                }

                // Check if user is active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Inactive user attempted password reset: {UserId}", userId);
                    return ServerOperationResult.Failure("Account is deactivated.");
                }

                // Reset password using Identity
                var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, resetToken, dto.NewPassword);
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    _logger.LogWarning("Password reset failed for user {UserId}: {Errors}",
                        userId, string.Join(", ", errors));
                    return ServerOperationResult.Failure(errors, "Failed to reset password.");
                }
                
                // Update Admin-specific fields (if user is Admin)
                if (user is Admin admin)
                {
                    admin.MustChangePassword = false;
                    admin.PasswordChangedAt = DateTime.UtcNow;
                    // Save changes
                    await _unitOfWork.SaveChangesAsync();
                }
                
                // Revoke all refresh tokens (force re-login for security)
                await _unitOfWork._refreshTokenRepository.RevokeAllUserTokensAsync(userId);
                _logger.LogInformation("Password reset successfully for user: {UserId}", userId);
                return ServerOperationResult.Success("Password reset successfully. Please login again with your new password.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user: {UserId}", userId);
                return ServerOperationResult.Failure("An error occurred while resetting password.");
            }
        }
    }
}
