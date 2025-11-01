using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using System.Security.Claims;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/[controller]")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        // Send OTP Endpoint
        [HttpPost("send-otp")]
        [Authorize]
        public async Task<IActionResult> SendOtp()
        {
            try
            {
                // Get UserId and Email from JWT token
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

                // Validate UserId and Email
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new
                    {
                        isSuccessful = false,
                        message = "Invalid authentication token."
                    });
                }

                if (string.IsNullOrEmpty(emailClaim))
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = "Email not found in authentication token."
                    });
                }

                var result = await _otpService.SendOtpAsync(userId, emailClaim);

                if (!result.IsSuccessful)
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    isSuccessful = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An unexpected error occurred."
                });
            }
        }

        // Verify OTP Endpoint
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] OtpCodeRequest dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = "Invalid input.",
                        errors = ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage)
                            .ToList()
                    });
                }

                // Get Email from JWT token
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized(new
                    {
                        isSuccessful = false,
                        message = "Invalid authentication token."
                    });
                }

                var verifyOtpDto = new OtpVerifyRequestDto
                {
                    Email = emailClaim,
                    Code = dto.Code
                };

                var result = await _otpService.VerifyOtpAsync(verifyOtpDto);

                if (!result.IsSuccessful)
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    isSuccessful = true,
                    message = result.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An unexpected error occurred."
                });
            }
        }
    }
}
