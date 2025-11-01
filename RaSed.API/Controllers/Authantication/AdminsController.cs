using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Infrastructure.Services.Authantication;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/admins")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]

    public class AdminsController : ControllerBase
    {
        private readonly IAdminService _adminService;

        public AdminsController(IAdminService _adminService)
        {
            this._adminService = _adminService;
        }


        [HttpPost]
        public async Task<IActionResult> CreateAdmin([FromBody] CreateAdminDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
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
                    isSuperAdmin = result.IsSuperAdmin,
                    mustChangePassword = result.MustChangePassword


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

        [HttpGet("{id:int}")]
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
        [HttpGet("email/{email}")]
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
        [HttpGet]
        public async Task<IActionResult> GetAllAdmins(
                 [FromQuery] int page = 1,
                 [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _adminService.GetAllAdminsAsync(page, pageSize);

                return Ok(new
                {
                    data = result.Items,
                    totalCount = result.TotalCount,
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while retrieving admins", error = ex.Message });
            }
        }
        [HttpPut("{id:int}")]
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

        [HttpDelete("{id:int}")]
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
