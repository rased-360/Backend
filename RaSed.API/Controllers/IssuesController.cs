using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.Interfaces;
using System.Security.Claims;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class IssuesController : ControllerBase
    {
        private readonly IIssueService _issueService;
        private readonly ILogger<IssuesController> _logger;

        public IssuesController(IIssueService issueService, ILogger<IssuesController> logger)
        {
            _issueService = issueService;
            _logger = logger;
        }

        // Creates a new issue (Employee only - Mobile app)
        // Automatically sends real-time notification to admin desktop

        [HttpPost]
        [Authorize(Roles = "Employee")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> CreateIssue([FromForm] CreateIssueDto createIssueDto)
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

                //Get UserId from Jwt token
                var userId = GetCurrentUserId();
                if (userId == null)
                {
                    return Unauthorized(new
                    {
                        isSuccessful = false,
                        message = "Invalid authentication token."
                    });
                }

                _logger.LogInformation("📝 Employee {EmployeeId} creating new issue", userId.Value);

                var issue = await _issueService.CreateIssueAsync(createIssueDto, userId.Value);

                return CreatedAtAction(
                    nameof(GetIssueById),
                    new { id = issue.Id },
                    new
                    {
                        message = "Issue reported successfully. Admin has been notified.",
                        data = issue
                    }
                );
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating issue");
                return BadRequest(new { message = ex.Message });
            }
        }

        // Gets all issues (Admin only - Desktop app)
        // Used to populate the notification list
        // Ordered by most recent first

        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetAllIssues()
        {
            try
            {
                var issues = await _issueService.GetAllIssuesAsync();

                return Ok(new
                {
                    count = issues.Count(),
                    data = issues
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving all issues");
                return BadRequest(new { message = ex.Message });
            }
        }


        // ──────────────────────────────────────────────────────────────────────
        // GET /api/issues/{id}
        // Gets issue by ID with full details (Admin only - Desktop app)
        // ✅ AUTOMATICALLY marks issue as read when admin views it
        // Called when admin clicks on a notification to view details
        // ──────────────────────────────────────────────────────────────────────

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetIssueById(int id)
        {
            try
            {
                var issue = await _issueService.GetIssueByIdAsync(id);

                if (issue == null)
                {
                    return NotFound(new
                    {
                        isSuccessful = false,
                        message = $"Issue with ID {id} not found"
                    });
                }

                // ✅ AUTOMATICALLY mark as read when admin views details
                await _issueService.MarkIssueAsReadAsync(id);

                return Ok(new
                {
                    isSuccessful = true,
                    data = issue
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving issue {IssueId}", id);
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while retrieving the issue."
                });
            }
        }


        #region
        private int? GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
            {
                return null;
            }
            return userId;
        }
        #endregion
    }
}
