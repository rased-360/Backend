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

        // Gets issue by ID with full details (Admin only - Desktop app)
        // Called when admin clicks on a notification to view details
        // Shows: Title, Description, Image, Employee info, Phone, Section, Time

        [HttpGet("{id:int}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> GetIssueById(int id)
        {
            try
            {
                var issue = await _issueService.GetIssueByIdAsync(id);

                if (issue == null)
                {
                    return NotFound(new { message = $"Issue with ID {id} not found" });
                }

                return Ok(new { data = issue });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error retrieving issue {IssueId}", id);
                return BadRequest(new { message = ex.Message });
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
