using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.Interfaces;
using System.Security.Claims;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // All routes on this controller require a valid JWT
    public class PerformanceController : ControllerBase
    {
        private readonly IPerformanceService _performanceService;
        private readonly ILogger<PerformanceController> _logger;

        public PerformanceController(
            IPerformanceService performanceService,
            ILogger<PerformanceController> logger)
        {
            _performanceService = performanceService;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/performance/me
        //
        // Returns the logged-in employee's performance rate.
        // Intended for the MOBILE APP home screen.
        //
        // HOW THE EMPLOYEE ID IS RESOLVED:
        //   The ID is read from the JWT NameIdentifier claim — the same claim
        //   that every other endpoint in this project uses.  The employee never
        //   passes their own ID in the URL, so they cannot forge a request for
        //   a different employee by changing the route parameter.
        //
        // RESPONSES:
        //   200 OK         — performance DTO with rate, rating, count, window
        //   401 Unauthorized — missing/invalid/expired JWT, or role ≠ Employee
        //   404 Not Found  — employee ID from the token not found in DB
        //   500 Internal   — unexpected error (logged server-side)
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("me")]
        [Authorize(Roles = "Employee")]
        public async Task<IActionResult> GetMyPerformance()
        {
            // Resolve the caller's identity from the JWT claim
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                // This should never happen if JWT middleware is configured correctly,
                // but we guard defensively.
                return Unauthorized(new
                {
                    isSuccessful = false,
                    message = "Invalid or missing authentication token."
                });
            }

            try
            {
                var result = await _performanceService.GetEmployeePerformanceAsync(userId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        isSuccessful = false,
                        message = "Employee profile not found."
                    });
                }

                return Ok(new
                {
                    isSuccessful = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error calculating performance for employee {UserId}", userId);

                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while calculating performance."
                });
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/performance/employee/{employeeId}
        //
        // Admin / SuperAdmin only — lets the admin dashboard show any
        // employee's performance card (e.g. when drilling into an employee
        // profile page or generating a team-wide report).
        //
        // RESPONSES:
        //   200 OK         — performance DTO
        //   401 Unauthorized — missing/invalid JWT, or role ≠ Admin / SuperAdmin
        //   404 Not Found  — employeeId not found in DB
        //   500 Internal   — unexpected error (logged server-side)
        // ──────────────────────────────────────────────────────────────────────
        [HttpGet("employee/{employeeId:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetEmployeePerformance(int employeeId)
        {
            try
            {
                var result = await _performanceService.GetEmployeePerformanceAsync(employeeId);

                if (result == null)
                {
                    return NotFound(new
                    {
                        isSuccessful = false,
                        message = $"Employee with ID {employeeId} was not found."
                    });
                }

                return Ok(new
                {
                    isSuccessful = true,
                    data = result
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error calculating performance for employee {EmployeeId}", employeeId);

                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while calculating performance."
                });
            }
        }
    }
}
