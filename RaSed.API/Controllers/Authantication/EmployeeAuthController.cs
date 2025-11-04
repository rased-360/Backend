using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Infrastructure.Services.Authantication;
using System.Security.Claims;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/employee/auth")]
    [ApiController]
    public class EmployeeAuthController : ControllerBase
    {
        private readonly IEmployeeAuthService _employeeAuthService;
        public EmployeeAuthController(IEmployeeAuthService _employeeAuthService)
        {
            this._employeeAuthService = _employeeAuthService;
        }

        [HttpPost("login")]
        [EnableRateLimiting("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = "Invalid model state.",
                    errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList(),
                    data = (object)null
                });
            }

            // Get IP Address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _employeeAuthService.LoginAsync(dto, ipAddress);
            if (!result.IsSuccessful)
            {
                return Unauthorized(new
                {
                    isSuccessful = result.IsSuccessful,
                    token = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    message = result.Message ?? result.Errors?.FirstOrDefault() ?? "Invalid email or password.",
                    errors = result.Errors
                });
            }
            return Ok(new
            {
                isSuccessful = result.IsSuccessful,
                message = result.Message,
                errors = (List<string>)null,
                data = new
                {
                    token = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    mustChangePassword = result.MustChangePassword,
                    employee = result.LoginResponse
                }
            });
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrEmpty(dto.RefreshToken))
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = "Refresh token is required.",
                    errors = new List<string> { "Refresh token is required." },
                    data = (object)null
                });
            }

            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            var result = await _employeeAuthService.RefreshTokenAsync(dto.RefreshToken, ipAddress);

            if (!result.IsSuccessful)
            {
                return Unauthorized(new
                {
                    isSuccessful = result.IsSuccessful,
                    message = result.Message,
                    errors = result.Errors,
                    data = (object)null
                });
            }

            return Ok(new
            {
                isSuccessful = result.IsSuccessful,
                message = result.Message,
                errors = (List<string>)null,
                data = new
                {
                    accessToken = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    message = result.Message
                }
            });
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> Logout([FromBody] RefreshTokenDto dto)
        {
            // 1. Validate input
            if (string.IsNullOrEmpty(dto?.RefreshToken))
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = "Refresh token is required.",
                    errors = new List<string> { "Refresh token is required." },
                    data = (object)null
                });
            }

            // ✅Get userId from JWT claims
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new
                {
                    isSuccessful = false,
                    message = "Invalid authentication.",
                    errors = new List<string> { "User not authenticated." }
                });
            }
            // 2. Get IP Address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // 3. Call logout service
            var result = await _employeeAuthService.LogoutAsync(dto.RefreshToken, userId, ipAddress);

            // 4. Handle failure
            if (!result.IsSuccessful)
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = result.Message,
                    errors = result.Errors,
                    data = (object)null
                });
            }

            // 5. Return success
            return Ok(new
            {
                isSuccessful = true,
                message = result.Message,
                errors = (List<string>)null,
                data = (object)null
            });
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeToken([FromBody] RefreshTokenDto dto)
        {
            if (string.IsNullOrEmpty(dto?.RefreshToken))
            {
                return BadRequest(new { isSuccessful = false, message = "Refresh token is required." });
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // ✅ Add userId validation in RevokeTokenAsync too
            var success = await _employeeAuthService.RevokeTokenAsync(dto.RefreshToken, userId, ipAddress);

            if (!success)
            {
                return BadRequest(new { isSuccessful = false, message = "Failed to revoke token. Token not found or already revoked." });
            }

            return Ok(new { isSuccessful = true, message = "Token revoked successfully." });
        }
    }
}
