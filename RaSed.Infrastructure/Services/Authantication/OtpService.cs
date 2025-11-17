using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
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
    public class OtpService : IOtpService
    {
        private readonly IEmailService _emailService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IConfiguration _configuration;
        public OtpService(IConfiguration configuration, IEmailService emailService, IUnitOfWork unitOfWork, UserManager<ApplicationUser> userManager)
        {
            _emailService = emailService;
            _unitOfWork = unitOfWork;
            _configuration = configuration;
            _userManager = userManager;
        }


        public async Task<ServerOperationResult> SendOtpAsync(SendOtpRequest request)
        {
            try
            {
                // Validate email
                if (string.IsNullOrEmpty(request.Email))
                {
                    return ServerOperationResult.Failure("email is required");
                }

                // Check if user exists
                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {                    
                    return ServerOperationResult.Failure("User with the provided email does not exist.");
                }

                // Get user ID
                var userId = user.Id;

                // Check recent OTP requests to prevent spamming
                var recentOtpCount = await _unitOfWork._otpRepository.CountRecentOtpAsync(request.Email, 10);

                // Get max long term attempts from configuration
                var maxLongTermAttempts = _configuration.GetValue<int>("OtpSetting:MaxLongTermAttempts", 5);
                
                if (recentOtpCount >= maxLongTermAttempts)
                {
                    return ServerOperationResult.Failure("Too many OTP requests. Please try again after 10 minutes.");
                }

                // Get the latest OTP for the email
                var latestOtp = await _unitOfWork._otpRepository.GetLatestOtpAsync(request.Email);

                var resendDelayMinutesString = _configuration["OtpSetting:ResendDelayMinutes"];
                var resendDelayMinutes = int.TryParse(resendDelayMinutesString, out var delayResult) ? delayResult : 1;
                
                // Check if we need to enforce a delay before resending new OTP
                if (latestOtp != null)
                {
                    var timeSinceLastOtp = DateTime.UtcNow - latestOtp.CreatedAt;

                    if (timeSinceLastOtp.TotalMinutes < resendDelayMinutes)
                    {
                        var remainingSeconds = (int)Math.Ceiling((resendDelayMinutes * 60) - timeSinceLastOtp.TotalSeconds);
                        return ServerOperationResult.Failure($"Please wait {remainingSeconds} seconds before requesting a new code.");
                    }
                }

                // Mark previous OTP as used
                if (latestOtp != null && !latestOtp.IsUsed)
                {
                    await _unitOfWork._otpRepository.InvalidateUserOtpsAsync(userId);
                    await _unitOfWork.SaveChangesAsync();
                }

                // Generate new OTP
                var otp = await GenerateOtpAsync();

                var expirationMinutesString = _configuration["OtpSetting:ExpiryMinutes"];
                var expirationMinutes = int.TryParse(expirationMinutesString, out var expirationResult) ? expirationResult : 10;

                // Save new OTP to database
                var otpEntity = new Otp
                {
                    UserID = userId,
                    Email = request.Email,
                    Code = otp,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = DateTime.UtcNow.AddMinutes(expirationMinutes),
                    IsUsed = false,
                    IsVerified = false,
                    FailedAttempts = 0
                };

                await _unitOfWork._otpRepository.AddAsync(otpEntity);
                await _unitOfWork.SaveChangesAsync();

                // Send OTP via email
                var subject = "Your One-Time Password (OTP)";
                var body = $@"
                    Your OTP code is: {otp}
                    
                    This code will expire in {expirationMinutes} minutes.
                    
                ⚠️ Do not share this code with anyone!
                ";
                var emailSend=await _emailService.SendEmailAsync(request.Email, subject, body);

                if (!emailSend.IsSuccessful)
                {
                    return ServerOperationResult.Failure("Failed to send OTP. Please try again.");
                }

                return ServerOperationResult.Success("OTP sent successfully to your email.");
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

        public async Task<ServerOperationResult> VerifyOtpAsync(OtpVerifyRequest request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Code))
                {
                    return ServerOperationResult.Failure("Email and code are required.");
                }

                var user = await _userManager.FindByEmailAsync(request.Email);

                if (user == null)
                {
                    return ServerOperationResult.Failure("User with the provided email does not exist.");
                }

                // Get max failed attempts from configuration
                var maxAttempts = _configuration.GetValue<int>("OtpSetting:MaxFailedAttempts", 3);

                // Retrieve the valid OTP record from the database
                var otpRecord = await _unitOfWork._otpRepository.GetValidOtpAsync(request.Email, request.Code,maxAttempts);
                //
                // If no valid OTP found, increment failed attempts for the latest OTP
                if (otpRecord == null)
                {
                    var anyOtp = await _unitOfWork._otpRepository.GetLatestOtpAsync(request.Email);
                    if (anyOtp != null && !anyOtp.IsUsed && anyOtp.ExpiresAt > DateTime.UtcNow)
                    {
                        anyOtp.FailedAttempts++;

                        //  Auto-invalidate after max attempts
                        if (anyOtp.FailedAttempts >= maxAttempts)
                        {
                            anyOtp.IsUsed = true;
                            anyOtp.UsedAt = DateTime.UtcNow;
                        }
                        _unitOfWork._otpRepository.Update(anyOtp);
                        await _unitOfWork.SaveChangesAsync();
                    }

                    return ServerOperationResult.Failure("Invalid or expired OTP code. Please request a new one.");
                }

                // Check if the OTP is expired
                if (DateTime.UtcNow > otpRecord.ExpiresAt)
                {
                    return new ServerOperationResult
                    {
                        IsSuccessful = false,
                        Message = "OTP has expired."
                    };
                }

                // Mark OTP as verified and used
                otpRecord.IsVerified = true;
                otpRecord.VerifiedAt = DateTime.UtcNow;
                _unitOfWork._otpRepository.Update(otpRecord);
                await _unitOfWork.SaveChangesAsync();
                    
                return ServerOperationResult.Success("OTP verified successfully.");
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


        #region
        public async Task<string> GenerateOtpAsync()
        {
            var random = new Random();
            var otp = random.Next(100000, 999999).ToString();
            return otp;
        }
        #endregion
    }
}
