using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Infrastructure.Services.Authantication;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminController(IAdminService _adminService)
        {
            this._adminService = _adminService;
        }


        [HttpPost("Create_Admine")]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    // نجمع كل الأخطاء ونرجعها كـ BadRequest
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();

                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                var result = await _adminService.CreateAdminAsync(dto);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An unexpected error occurred while creating the admin.", details = ex.Message });
            }
        }
    }
}
