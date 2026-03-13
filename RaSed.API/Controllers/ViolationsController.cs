using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Violations;
using RaSed.Application.Interfaces;

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
    }
}
