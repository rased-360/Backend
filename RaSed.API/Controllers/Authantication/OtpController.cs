using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using System.Security.Claims;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/otp")]
    [ApiController]
    public class OtpController : ControllerBase
    {
        private readonly IOtpService _otpService;
        public OtpController(IOtpService otpService)
        {
            _otpService = otpService;
        }

        // Send OTP Endpoint
        [HttpPost("send")]
        [EnableRateLimiting("otp-send-limit")]
        public async Task<IActionResult> SendOtp([FromBody] SendOtpRequest? request)
        {
            // Get UserId and Email from JWT token
            // Case 1: User is Logged In (JWT exists)
            if (User.Identity?.IsAuthenticated == true)
            {
                var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(emailClaim))
                {
                    return Unauthorized(new
                    {
                        isSuccessful = false,
                        message = "Email not found in token."
                    });
                }

                request.Email = emailClaim;
            }

            // Case 2: User is NOT Logged In (Forgot Password from Login page)
            else
            {
                if (request == null || string.IsNullOrEmpty(request.Email))
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = "Email is required when not authenticated."
                    });
                }

            }

            var result = await _otpService.SendOtpAsync(request);

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

        // Verify OTP Endpoint
        [HttpPost("verify")]
        [EnableRateLimiting("otp-verify-limit")]
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

                string email;

                // Case 1: User is Logged In (JWT exists)
                if (User.Identity?.IsAuthenticated == true)
                {
                    var emailClaim = User.FindFirst(ClaimTypes.Email)?.Value;

                    if (string.IsNullOrEmpty(emailClaim))
                    {
                        return Unauthorized(new
                        {
                            isSuccessful = false,
                            message = "Invalid authentication token."
                        });
                    }

                    email = emailClaim;
                }
                // Case 2: User is NOT Logged In
                else
                {
                    if (string.IsNullOrEmpty(dto.Email))
                    {
                        return BadRequest(new
                        {
                            isSuccessful = false,
                            message = "Email is required when not authenticated."
                        });
                    }

                    email = dto.Email;
                }

                var verifyOtpDto = new OtpVerifyRequest
                {
                    Email = email,
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
