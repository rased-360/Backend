using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.Interfaces;
using System.Security.Claims;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly IProfileService _profileService;

        public ProfileController(IProfileService profileService)
        {
            _profileService = profileService;
        }
        //Get Current user profile
        [HttpGet]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var profile = await _profileService.GetProfileAsync(userId);
                return Ok(new
                {
                    isSuccessful = true,
                    data = profile
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = ex.Message
                });
            }
        }
        //Upload or update profile photo
        [HttpPost("photo")]
        public async Task<IActionResult> UpdateProfilePhoto([FromForm] IFormFile photo)
        {
            try
            {
                if (photo == null || photo.Length == 0)
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = "Please select a photo"
                    });
                }

                var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var result = await _profileService.UpdateProfilePhotoAsync(userId, photo);

                return Ok(new
                {
                    isSuccessful = true,
                    message = result.Message,
                    data = new { profilePictureUrl = result.ProfilePictureUrl }
                });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    isSuccessful = false,
                    message = ex.Message
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while uploading the photo"
                });
            }
        }


    }
}
