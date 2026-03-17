using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Violations;
using RaSed.Application.Interfaces;
using System.Security.Claims;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ViolationsController : ControllerBase
    {
        private readonly IViolationService _violationService;
        private readonly ILogger<ViolationsController> _logger;

        public ViolationsController(IViolationService violationService, ILogger<ViolationsController> logger)
        {
            _violationService = violationService;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────
        // POST /api/violations
        // Called by the AI camera model when it detects one or more violations.
        // No [Authorize] here — the AI team calls this from a trusted internal service.
        // Production recommendation: secure with an API key header (see TODO below).
        // ──────────────────────────────────────────────────────────────────────
        [HttpPost]
        [AllowAnonymous] // TODO: Replace with [ApiKey] attribute when the AI team provides a key
        public async Task<IActionResult> ReportViolations([FromBody] ViolationDetectedDto dto)
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

                _logger.LogInformation(
                    "🚨 Violation report received from AI — {Count} violation(s) at {Timestamp}",
                    dto.Violations.Count, dto.Timestamp);

                var saved = await _violationService.ProcessViolationsAsync(dto);
                var list = saved.ToList();

                return Ok(new
                {
                    isSuccessful = true,
                    message = $"{list.Count} violation(s) saved and admin notified.",
                    count = list.Count,
                    data = list
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error processing violation report");
                return StatusCode(500, new { isSuccessful = false, message = ex.Message });
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/violations/{id}
        // Gets a single violation by ID
        // ✅ AUTOMATICALLY marks violation as read when user views it
        // SECURITY: Employee can only view their own violations
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> GetViolationById(int id)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        isSuccessful = false,
                        message = "Invalid authentication token."
                    });
                }

                var userRole = GetCurrentUserRole();
                var isAdmin = userRole == "Admin" || userRole == "SuperAdmin";

                // Get violation with security check
                var violation = await _violationService.GetViolationByIdAsync(id, userId.Value, isAdmin);

                if (violation == null)
                {
                    return NotFound(new
                    {
                        isSuccessful = false,
                        message = $"Violation with ID {id} not found or you don't have permission to view it."
                    });
                }

                // ✅ AUTOMATICALLY mark as read when viewing details
                await _violationService.MarkViolationAsReadAsync(id, userId.Value, isAdmin);

                return Ok(new
                {
                    isSuccessful = true,
                    data = violation
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving violation {ViolationId}", id);
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while retrieving the violation."
                });
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/violations/employee/{employeeId}
        // Gets all violations for a specific employee
        // If violationId is provided, marks THAT violation as read
        // Used when admin clicks notification → opens list + highlights specific violation
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet("employee/{employeeId}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetEmployeeViolations(int employeeId,[FromQuery] int? violationId = null)  
        {
            try
            {
                var violations = await _violationService.GetViolationsByEmployeeIdAsync(employeeId);

                // If violationId provided, mark it as read
                if (violationId.HasValue)
                {
                    var userId = GetCurrentUserId();
                    if (userId.HasValue)
                    {
                        var isAdmin = GetCurrentUserRole() == "Admin";
                        await _violationService.MarkViolationAsReadAsync(violationId.Value, userId.Value, isAdmin);

                        _logger.LogInformation(
                            "✅ Violation {ViolationId} marked as read when viewing employee {EmployeeId} violations",
                            violationId.Value, employeeId);
                    }
                }

                return Ok(new
                {
                    isSuccessful = true,
                    count = violations.Count(),
                    highlightId = violationId,
                    data = violations
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving violations for employee {EmployeeId}", employeeId);
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while retrieving violations."
                });
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // Helper Methods
        // ──────────────────────────────────────────────────────────────────────

        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }

        private string GetCurrentUserRole()
        {
            return User.FindFirst(ClaimTypes.Role)?.Value ?? "Employee";
        }
    }
}
