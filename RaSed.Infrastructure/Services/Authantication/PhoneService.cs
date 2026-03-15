using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces;
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
    public class PhoneService : IPhoneService
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly INotificationService _notificationService;  
        private readonly IUnitOfWork _unitOfWork;
        public PhoneService(UserManager<ApplicationUser> userManager, INotificationService notificationService, IUnitOfWork unitOfWork)
        {
            _userManager = userManager;
            _notificationService = notificationService;
            _unitOfWork = unitOfWork;
        }

        // Verify Password to confirm user's identity before changing phone number
        public async Task<ServerOperationResult> VerifyPasswordAsync(int userId, string password)
        {
            try
            {
                //Get user by ID
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return ServerOperationResult.Failure("User not found.");
                }
                if (!user.IsActive)
                {
                    return ServerOperationResult.Failure("Account is deactivated.");
                }
                // Check the password
                var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
                if (!isPasswordValid)
                {
                    return ServerOperationResult.Failure("Current password is incorrect.");
                }

                return ServerOperationResult.Success("Password verified successfully.");
            }
            catch (Exception ex)
            {
                return ServerOperationResult.Failure("An error occurred.");
            }
        }

        public async Task<ServerOperationResult> ConfirmPhoneChangeRequest(int userId, string newPhoneNumber)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return ServerOperationResult.Failure("User not found.");
                }

                if (user.PhoneNumber == newPhoneNumber)
                {
                    return ServerOperationResult.Failure("New phone number must be different from current one.");
                }

                var existingUser = await _userManager.Users
                   .FirstOrDefaultAsync(u => u.PhoneNumber == newPhoneNumber);

                if (existingUser != null && existingUser.Id != user.Id)
                {
                    return ServerOperationResult.Failure("This phone number already exists for another user");

                }

                return ServerOperationResult.Success("Phone number is available for use.");
            }
            catch (Exception ex)
            {
                return ServerOperationResult.Failure("An error occurred in changing number.");

            }
        }

        public async Task<ServerOperationResult> ChangePhoneNumberAsync(int userId, string password, string newPhoneNumber)
        {
            
            try
            {
                // First, verify the password
                var verificationResult = await VerifyPasswordAsync(userId, password);
                if (!verificationResult.IsSuccessful)
                {
                    return verificationResult; // Return the failure result
                }
                // Get user by ID
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return ServerOperationResult.Failure("User not found.");
                }

                var oldPhoneNumber = user.PhoneNumber;

                user.PhoneNumber = newPhoneNumber;
                var result = await _userManager.UpdateAsync(user);
                // Change phone number
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServerOperationResult.Failure(errors, "Failed to change phone number.");
                }


                // ✅ CREATE GENERAL NOTIFICATION (NEW)
                await _notificationService.CreateGeneralNotificationAsync(
                    userId: userId,
                    type: "PHONE_CHANGED",
                    message: $"Your phone number was changed from {oldPhoneNumber} to {newPhoneNumber}."
                );

                return ServerOperationResult.Success("Phone number changed successfully.");
            }
            catch (Exception ex)
            {
                return new ServerOperationResult
                {
                    IsSuccessful = false,
                    Message = $"An error occurred: {ex.Message}"
                };
            }
        }
    }
}
