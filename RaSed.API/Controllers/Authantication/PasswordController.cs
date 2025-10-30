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
    public class PasswordController : ControllerBase
    {
        private IPasswordService _passwordService;

        public PasswordController(IPasswordService passwordService)
        {
            _passwordService = passwordService;
        }

        [Authorize]
        [HttpPost("change")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto dto)
        {
            if(!ModelState.IsValid)
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
            //Get UserId from Jwt token
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId)) 
            {
                return Unauthorized(new
                {
                    isSuccessful = false,
                    message = "Invalid authentication token."
                });
            }
            var result = await _passwordService.ChangePasswordAsync(userId, dto);
            if(!result.IsSuccessful)
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = result.Message,
                    errors = result.Errors
                });
            }
            return Ok(new
            {
                isSuccessful = true,
                message = result.Message
            });
        }
    }
}
