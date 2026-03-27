using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.Interfaces;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class EmployeeDashboardController : ControllerBase
    {
        private readonly IEmployeeDashboardService _dashboardService;
        private readonly ILogger<EmployeeDashboardController> _logger;

        public EmployeeDashboardController(
            IEmployeeDashboardService dashboardService,
            ILogger<EmployeeDashboardController> logger)
        {
            _dashboardService = dashboardService;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/employeedashboard/section/{sectionId}
        // 
        // Employee Dashboard for Admin Desktop.
        // Shows all employees in a section with performance metrics.
        // Sorted by most recent violation (worst performers first).
        // 
        // QUERY PARAMETERS:
        //   - searchTerm (optional): Filter by employee name
        // 
        // RESPONSES:
        //   200 OK — List of employees with performance stats
        //   401 Unauthorized — Not authenticated
        //   403 Forbidden — Not Admin/SuperAdmin
        //   500 Internal Server Error — Unexpected error
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet("section/{sectionId:int}")]
        public async Task<IActionResult> GetSectionDashboard(
            int sectionId,
            [FromQuery] string? searchTerm = null)
        {
            try
            {
                _logger.LogInformation(
                    "📊 Admin requesting dashboard — Section: {SectionId}, Search: {Search}",
                    sectionId, searchTerm ?? "None");

                var employees = await _dashboardService.GetSectionEmployeesAsync(
                    sectionId,
                    searchTerm);

                var employeeList = employees.ToList();

                return Ok(new
                {
                    isSuccessful = true,
                    sectionId = sectionId,
                    count = employeeList.Count,
                    data = employeeList
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error loading dashboard for Section {SectionId}", sectionId);

                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while loading the dashboard."
                });
            }
        }
    }
}
