using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Infrastructure.Services.Authantication;

namespace RaSed.API.Controllers.Authantication
{
    [Route("api/employees")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin")]
    public class EmployeesController : ControllerBase
    {
        private readonly IEmployeeService _employeeService;

        public EmployeesController(IEmployeeService _employeeService)
        {
            this._employeeService = _employeeService;
        }

        // Create Employee
        [HttpPost]
        public async Task<IActionResult> CreateEmployee([FromBody] CreateEmployeeDto dto)
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

                var result = await _employeeService.CreateEmployeeAsync(dto);

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
                    email = result.Employee.Email,
                    fullName = result.Employee.FullName,
                    initialPassword = result.Employee.InitialPassword


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


        // Get All Employees with Pagination
        [HttpGet]
        public async Task<IActionResult> GetAllEmployees(
                 [FromQuery] int page = 1,
                 [FromQuery] int pageSize = 10)
        {
            try
            {
                var result = await _employeeService.GetAllEmployeesAsync(page, pageSize);

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
                return StatusCode(500, new { message = "An error occurred while retrieving employees", error = ex.Message });
            }
        }




        // Activate or Disactivate Employee
        [HttpPut]
        public async Task<IActionResult> ActivateOrDisactivateEmployee([FromBody] EmployeeEditDto adminEdit)
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

                var result = await _employeeService.ActivateOrDisactivateEmployeeAsync(id, isActive);
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

        // Delete Employees by Ids
        [HttpDelete]
        public async Task<IActionResult> DeleteEmployee([FromBody] List<int> ids)
        {
            try
            {
                var result = await _employeeService.DeleteEmplyeesByIdsAsync(ids);

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


        [HttpGet("global/search")]
        public async Task<IActionResult> GetAllEmployeesBy([FromQuery] QueryDto query)
        {
            try
            {
                var result = await _employeeService.GetFilteredEmployeesAsync(query);

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
        public async Task<IActionResult> GetEmployeeById(int id)
        {
            try
            {
                var result = await _employeeService.GetEmployeeByIdAsync(id);


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
                    data = result.Employee,
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
