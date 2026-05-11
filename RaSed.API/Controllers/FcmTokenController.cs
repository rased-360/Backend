using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.FcmTokens;
using RaSed.Application.Interfaces.Realtime;
using System.Security.Claims;

namespace RaSed.API.Controllers
{
    [Route("api/fcm-tokens")]
    [ApiController]
    [Authorize(Roles = "Employee")]
    public class FcmTokenController : ControllerBase
    {
        private readonly IFcmTokenService _fcmTokenService;
        private readonly ILogger<FcmTokenController> _logger;

        public FcmTokenController(IFcmTokenService fcmTokenService, ILogger<FcmTokenController> logger)
        {
            _fcmTokenService = fcmTokenService;
            _logger = logger;
        }

        // POST /api/fcm-tokens
        // Called by mobile app: on login, on app start, when FCM refreshes the token
        [HttpPost]
        public async Task<IActionResult> RegisterToken([FromBody] RegisterFcmTokenDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { isSuccessful = false, message = "Validation failed", errors });
                }

                var userId = GetCurrentUserId();
                if (userId == null)
                    return Unauthorized(new { isSuccessful = false, message = "Invalid token." });

                await _fcmTokenService.RegisterTokenAsync(userId.Value, dto);

                return Ok(new { isSuccessful = true, message = "Device token registered successfully." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error registering FCM token");
                return StatusCode(500, new { isSuccessful = false, message = ex.Message });
            }
        }

        // DELETE /api/fcm-tokens
        // Called by mobile app on logout
        [HttpDelete]
        public async Task<IActionResult> RemoveToken([FromBody] RemoveFcmTokenDto dto)
        {
            try
            {
                await _fcmTokenService.RemoveTokenAsync(dto.Token);
                return Ok(new { isSuccessful = true, message = "Device token removed." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error removing FCM token");
                return StatusCode(500, new { isSuccessful = false, message = ex.Message });
            }
        }

        private int? GetCurrentUserId()
        {
            var claim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return int.TryParse(claim, out var id) ? id : null;
        }
    }
}