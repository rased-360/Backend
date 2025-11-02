using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/admin/auth")]
    [ApiController]
    public class AdminAuthController : ControllerBase
    {
        private readonly IAdminAuthService _adminAuthService;
        public AdminAuthController(IAdminAuthService _adminAuthService)
        {
            this._adminAuthService = _adminAuthService;
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

            var result = await _adminAuthService.LoginAsync(dto,ipAddress);
            if (!result.IsSuccessful)
            {
                return Unauthorized(new
                {
                    isSuccessful = result.IsSuccessful,
                    token = result.AccessToken,
                    refreshToken = result.RefreshToken,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            return Ok(new
            {
                isSuccessful = result.IsSuccessful,
                token = result.AccessToken,
                refreshToken = result.RefreshToken,
                message = result.Message,
                errors = (List<string>)null,
                data = new
                {
                    isSuperAdmin = result.IsSuperAdmin,
                    mustChangePassword = result.MustChangePassword,
                    admin = result.Admin
                }
            });
        }
        
        [Authorize]
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

            var result = await _adminAuthService.RefreshTokenAsync(dto.RefreshToken, ipAddress);

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
                    isSuperAdmin = result.IsSuperAdmin,
                    mustChangePassword = result.MustChangePassword,
                    admin = result.Admin
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

            // 2. Get IP Address
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "Unknown";

            // 3. Call logout service
            var result = await _adminAuthService.LogoutAsync(dto.RefreshToken, ipAddress);

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

    }
}
