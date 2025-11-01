using Microsoft.AspNetCore.Identity;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
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

        public PhoneService(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
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

        public async Task<ServerOperationResult> ChangePhoneNumberAsync(int userId, string password, string newPhoneNumber)
        {
            // First, verify the password
            var verificationResult = await VerifyPasswordAsync(userId, password);
            if (!verificationResult.IsSuccessful)
            {
                return verificationResult; // Return the failure result
            }
            try
            {
                // Get user by ID
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                {
                    return ServerOperationResult.Failure("User not found.");
                }

                if (user.PhoneNumber == newPhoneNumber)
                {
                    return ServerOperationResult.Failure("New phone number must be different from current one.");
                }
                user.PhoneNumber = newPhoneNumber;
                var result = await _userManager.UpdateAsync(user);
                // Change phone number
                if (!result.Succeeded)
                {
                    var errors = result.Errors.Select(e => e.Description).ToList();
                    return ServerOperationResult.Failure(errors, "Failed to change phone number.");
                }

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
