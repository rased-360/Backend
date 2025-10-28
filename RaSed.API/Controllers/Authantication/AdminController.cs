using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Infrastructure.Services.Authantication;

namespace RaSed.API.Controllers.Authantication
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

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            try
            {
                var admin = await _adminService.GetAdminByIdAsync(id);

                if (admin == null)
                    return NotFound(new { error = "Admin not found" });

                return Ok(admin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while retrieving the admin.",
                    details = ex.Message
                });
            }
        }
        [HttpGet("GetByEmail/{email}")]
        public async Task<IActionResult> GetAdminByEmail(string email)
        {
            try
            {
                var admin = await _adminService.GetAdminByEmailAsync(email);

                if (admin == null)
                    return NotFound(new { error = "Admin not found" });

                return Ok(admin);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while retrieving the admin.",
                    details = ex.Message
                });
            }
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAdmins()
        {
            try
            {
                var admins = await _adminService.GetAllAdminsAsync();
                return Ok(admins);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while retrieving admins.",
                    details = ex.Message
                });
            }
        }
        [HttpPut("Edit/{adminId}")]
        public async Task<IActionResult> EditAdmin(int adminId, [FromBody] AdminEditDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                var result = await _adminService.EditAdminAsync(adminId, dto);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while editing the admin.",
                    details = ex.Message
                });
            }
        }

        [HttpDelete("Delete/{id}")]
        public async Task<IActionResult> DeleteAdmin(int id)
        {
            try
            {
                var result = await _adminService.DeleteAdminByIdAsync(id);

                if (result)
                    return Ok(new { message = "Admin deleted successfully" });

                return BadRequest(new { error = "Failed to delete admin" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { error = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while deleting the admin.",
                    details = ex.Message
                });
            }
        }

    }
}
