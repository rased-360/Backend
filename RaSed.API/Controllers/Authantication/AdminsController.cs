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

        // Create Admin 
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
                    email = result.Admin.Email,
                    fullName = result.Admin.FullName,
                    initialPassword = result.Admin.InitialPassword

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

        // Get All Admins with Pagination
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


        // Activate or Disactivate Admin
        [HttpPut]
        public async Task<IActionResult> ActivateOrDisactivateAdmin([FromBody] AdminEditDto adminEdit)
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

                var id = adminEdit.userId;
                var isActive = adminEdit.IsActive;

                var result = await _adminService.ActivateOrDisactivateAdminAsync(id, isActive);
                if (!result.IsSuccessful)
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = result.Message,
                        error = result.Errors
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


        // Delete Admins by Ids
        [HttpDelete]
        public async Task<IActionResult> DeleteAdmins([FromBody] List<int> ids)
        {
            try
            {
                var result = await _adminService.DeleteAdminsByIdsAsync(ids);

                if (!result.IsSuccessful)
                {
                    return BadRequest(new
                    {
                        isSuccessful = false,
                        message = result.Message,
                        error = result.Errors
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
                    error = "An unexpected error occurred while deleting admins.",
                    details = ex.Message
                });
            }
        }


        [HttpGet("global/search")]
        public async Task<IActionResult> GetAllAdminsBy([FromQuery] AdminQueryDto query)
        {
            try
            {
                var result = await _adminService.GetFilteredAdminsAsync(query);

                return Ok(new
                {
                    data = result.Items,
                    totalCount = result.TotalCount,
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalPages = result.TotalPages,
                    hasPreviousPage = result.HasPreviousPage,
                    hasNextPage = result.HasNextPage,

                    filters = new
                    {
                        searchTerm = query.SearchTerm,
                        isActive = query.IsActive,
                        sortOrder = query.SortOrder
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    message = "An error occurred while retrieving admins",
                    error = ex.Message
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


    }
}
