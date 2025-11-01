using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Infrastructure.Services.Authantication;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]

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
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = "Validation failed",
                        errors
                    });
                }

                var result = await _adminService.CreateAdminAsync(dto);

                if (!result.IsSuccessful)
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
                    message = result.Message,
                    data = result.Admin,
                    IsSuperAdmine = result.IsSuperAdmin,
                    MustChangePassword = result.MustChangePassword


                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while creating the result.",
                    details = ex.Message
                });
            }
        }

        [HttpGet("GetById/{id}")]
        public async Task<IActionResult> GetAdminById(int id)
        {
            try
            {
                var result = await _adminService.GetAdminByIdAsync(id);


                if (!result.IsSuccessful)
                {
                    return NotFound(new
                    {
                        isSuccessful = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    isSuccessful = true,
                    message = result.Message,
                    data = result.Admin,
                    IsSuperAdmine = result.IsSuperAdmin,
                    MustChangePassword = result.MustChangePassword
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while retrieving the result.",
                    details = ex.Message
                });
            }
        }
        [HttpGet("GetByEmail/{email}")]
        public async Task<IActionResult> GetAdminByEmail(string email)
        {
            try
            {
                var result = await _adminService.GetAdminByEmailAsync(email);

                if (!result.IsSuccessful)
                {
                    return NotFound(new
                    {
                        isSuccessful = false,
                        message = result.Message
                    });
                }

                return Ok(new
                {
                    isSuccessful = true,
                    message = result.Message,
                    data = result.Admin,
                    IsSuperAdmine = result.IsSuperAdmin,
                    MustChangePassword = result.MustChangePassword
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while retrieving the result.",
                    details = ex.Message
                });
            }
        }
        [HttpGet("GetAll")]
        public async Task<IActionResult> GetAllAdmins()
        {
            try
            {
                var result = await _adminService.GetAllAdminsAsync();
                return Ok(new
                {
                    isSuccessful = true,
                    message = "Gets All Admins",
                    data = result
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while retrieving the result.",
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
                    // نجمع كل الأخطاء ونرجعها كـ BadRequest
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = "Validation failed",
                        errors
                    });
                }
                var result = await _adminService.EditAdminAsync(adminId, dto);
                if (!result.IsSuccessful)
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
                    message = result.Message,
                    data = result.Admin,
                    IsSuperAdmine = result.IsSuperAdmin,
                    MustChangePassword = result.MustChangePassword


                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    error = "An unexpected error occurred while editing the result.",
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
                    error = "An unexpected error occurred while editing the result.",
                    details = ex.Message
                });
            }
        }

    }
}
