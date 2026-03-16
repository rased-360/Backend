using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RaSed.Application.Interfaces;
using System.Security.Claims;

namespace RaSed.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;
        private readonly ILogger<NotificationsController> _logger;

        public NotificationsController(
            INotificationService notificationService,
            ILogger<NotificationsController> logger)
        {
            _notificationService = notificationService;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/notifications
        // Returns unified notifications (violations + issues + general)
        // 
        // DESKTOP (Admin): Violations (all) + Issues (all) + General (own)
        // MOBILE (Employee): Violations (my) + General (own)
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets all notifications for the authenticated user.
        /// 
        /// DESKTOP (Admin):
        ///   - Violations (all employees)
        ///   - Issues (all employees)
        ///   - General (own account changes)
        /// 
        /// MOBILE (Employee):
        ///   - Violations (my violations)
        ///   - General (own account changes)
        /// 
        /// SECURITY:
        ///   - UserId from JWT token (not URL parameter)
        ///   - Admin sees all violations/issues
        ///   - Employee sees only own violations
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetNotifications([FromQuery] bool unreadOnly = false)
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

                _logger.LogInformation(
                    "📋 Getting notifications for user {UserId} (Role: {Role})",
                    userId.Value, userRole);

                var notifications = isAdmin
                    ? await _notificationService.GetAdminNotificationsAsync(userId.Value, unreadOnly)
                    : await _notificationService.GetEmployeeNotificationsAsync(userId.Value, unreadOnly);

                return Ok(new
                {
                    isSuccessful = true,
                    count = notifications.Count(),
                    data = notifications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting notifications");
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred while retrieving notifications."
                });
            }
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET /api/notifications/unread-count
        // Returns unread notification count for badge
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets unread notification count for badge display.
        /// 
        /// NOTE: Currently only counts unread general notifications.
        /// Violations and Issues don't have IsRead state (always shown).
        /// </summary>
        [HttpGet("unread-count")]
        public async Task<IActionResult> GetUnreadCount()
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

                var count = await _notificationService.GetUnreadCountAsync(userId.Value, isAdmin);

                return Ok(new
                {
                    isSuccessful = true,
                    unreadCount = count
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error getting unread count");
                return StatusCode(500, new
                {
                    isSuccessful = false,
                    message = "An error occurred."
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