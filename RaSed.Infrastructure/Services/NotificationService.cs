using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Notifications;
using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.DTOs.Violations;
using RaSed.Application.Interfaces;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;

namespace RaSed.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotificationService _realtimeService;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(
            IUnitOfWork unitOfWork,
            IRealtimeNotificationService realtimeService,
            ILogger<NotificationService> logger)
        {
            _unitOfWork = unitOfWork;
            _realtimeService = realtimeService;
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET ADMIN NOTIFICATIONS (Desktop)
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets all notifications for Admin (Desktop).
        /// Supports filtering by unread only.
        /// Returns ALL notifications (no pagination).
        /// </summary>
        public async Task<IEnumerable<UnifiedNotificationDto>> GetAdminNotificationsAsync(
            int adminId,
            bool unreadOnly = false)
        {
            var notifications = new List<UnifiedNotificationDto>();

            // 1. Violations (all employees)
            var violations = unreadOnly
                ? await _unitOfWork._violationRepository.GetUnreadForAdminAsync()
                : await _unitOfWork._violationRepository.GetAllWithDetailsAsync();

            foreach (var v in violations)
            {
                notifications.Add(new UnifiedNotificationDto
                {
                    Type = "violation",
                    Timestamp = v.Timestamp,
                    IsRead = v.IsRead,
                    Data = new ViolationNotificationDto
                    {
                        ViolationId = v.Id,
                        ViolationType = v.ViolationType,
                        Timestamp = v.Timestamp,
                        EmployeeId = v.EmployeeId ?? 0,
                        EmployeeName = v.Employee?.FullName ?? "Unknown",
                        SectionName = v.Employee?.Section?.Name ?? "Unknown",
                        ImageUrl = v.ImageUrl
                    }
                });
            }

            // 2. Issues (all employees)
            var issues = unreadOnly
                ? await _unitOfWork._issueRepository.GetUnreadForAdminAsync()
                : await _unitOfWork._issueRepository.GetAllIssuesAsync();

            foreach (var i in issues)
            {
                notifications.Add(new UnifiedNotificationDto
                {
                    Type = "issue",
                    Timestamp = i.ReportedAt,
                    IsRead = i.IsRead,
                    Data = new IssueNotificationPreviewDto
                    {
                        IssueId = i.Id,
                        Title = i.Title,
                        ReportedAt = i.ReportedAt,
                        EmployeeName = i.Employee.FullName,
                        SectionName = i.Employee.Section.Name
                    }
                });
            }

            // 3. General (own account changes only)
            var generalQuery = unreadOnly
                ? (await _unitOfWork._generalNotificationRepository.GetUserNotificationsAsync(adminId))
                    .Where(g => !g.IsRead)
                : await _unitOfWork._generalNotificationRepository.GetUserNotificationsAsync(adminId);

            foreach (var g in generalQuery)
            {
                notifications.Add(new UnifiedNotificationDto
                {
                    Type = "general",
                    Timestamp = g.Timestamp,
                    IsRead = g.IsRead,
                    Data = new GeneralNotificationDto
                    {
                        Id = g.Id,
                        Type = g.Type,
                        Message = g.Message,
                        Timestamp = g.Timestamp,
                        IsRead = g.IsRead
                    }
                });
            }

            // Sort by timestamp descending (most recent first)
            return notifications
                .OrderByDescending(n => n.Timestamp)
                .ToList();
        }

        // ──────────────────────────────────────────────────────────────────────
        // GET EMPLOYEE NOTIFICATIONS (Mobile)
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets all notifications for Employee (Mobile).
        /// Supports filtering by unread only.
        /// Returns ALL notifications (no pagination).
        /// </summary>
        public async Task<IEnumerable<UnifiedNotificationDto>> GetEmployeeNotificationsAsync(
            int employeeId,
            bool unreadOnly = false)
        {
            var notifications = new List<UnifiedNotificationDto>();

            // 1. Violations (my violations only)
            var violations = unreadOnly
                ? await _unitOfWork._violationRepository.GetUnreadByEmployeeIdAsync(employeeId)
                : await _unitOfWork._violationRepository.GetViolationsByEmployeeIdAsync(employeeId);

            foreach (var v in violations)
            {
                notifications.Add(new UnifiedNotificationDto
                {
                    Type = "violation",
                    Timestamp = v.Timestamp,
                    IsRead = v.IsRead,
                    Data = new EmployeeViolationDto
                    {
                        ViolationId = v.Id,
                        ViolationType = v.ViolationType,
                        Timestamp = v.Timestamp,
                        ImageUrl = v.ImageUrl
                    }
                });
            }

            // 2. General (own account changes only)
            var generalQuery = unreadOnly
                ? (await _unitOfWork._generalNotificationRepository.GetUserNotificationsAsync(employeeId))
                    .Where(g => !g.IsRead)
                : await _unitOfWork._generalNotificationRepository.GetUserNotificationsAsync(employeeId);

            foreach (var g in generalQuery)
            {
                notifications.Add(new UnifiedNotificationDto
                {
                    Type = "general",
                    Timestamp = g.Timestamp,
                    IsRead = g.IsRead,
                    Data = new GeneralNotificationDto
                    {
                        Id = g.Id,
                        Type = g.Type,
                        Message = g.Message,
                        Timestamp = g.Timestamp,
                        IsRead = g.IsRead
                    }
                });
            }

            // Sort by timestamp descending (most recent first)
            return notifications
                .OrderByDescending(n => n.Timestamp)
                .ToList();
        }

        // ──────────────────────────────────────────────────────────────────────
        // UNREAD COUNT
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets unread notification count for badge.
        /// Counts: Violations + Issues (admin only) + General
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int userId, bool isAdmin)
        {
            int count = 0;

            // 1. Violations
            if (isAdmin)
                count += await _unitOfWork._violationRepository.GetUnreadCountForAdminAsync();
            else
                count += await _unitOfWork._violationRepository.GetUnreadCountByEmployeeIdAsync(userId);

            // 2. Issues (Admin only)
            if (isAdmin)
                count += await _unitOfWork._issueRepository.GetUnreadCountForAdminAsync();

            // 3. General (own account)
            count += await _unitOfWork._generalNotificationRepository.GetUnreadCountAsync(userId);

            return count;
        }

        // Add this method to NotificationService.cs

        /// <summary>
        /// Marks a general notification as read.
        /// Called automatically by frontend when user views notification.
        /// SECURITY: User can only mark their own notifications.
        /// </summary>
        public async Task<bool> MarkGeneralNotificationAsReadAsync(int notificationId, int userId)
        {
            var success = await _unitOfWork._generalNotificationRepository.MarkAsReadAsync(notificationId, userId);

            if (success)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation(
                    "✅ General notification {NotificationId} marked as read by user {UserId}",
                    notificationId, userId);
            }

            return success;
        }

        // ──────────────────────────────────────────────────────────────────────
        // CREATE GENERAL NOTIFICATION
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Creates a general notification (password change, phone change).
        /// Saves to DB and sends SignalR notification.
        /// </summary>
        public async Task CreateGeneralNotificationAsync(int userId, string type, string message)
        {
            try
            {
                // 1. Save to database
                var notification = new GeneralNotification
                {
                    UserId = userId,
                    Type = type,
                    Message = message,
                    Timestamp = DateTime.UtcNow,
                    IsRead = false
                };

                await _unitOfWork._generalNotificationRepository.AddAsync(notification);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation(
                    "✅ General notification created — UserId={UserId}, Type={Type}",
                    userId, type);

                // 2. Send SignalR notification (real-time)
                var dto = new GeneralNotificationDto
                {
                    Id = notification.Id,
                    Type = notification.Type,
                    Message = notification.Message,
                    Timestamp = notification.Timestamp,
                    IsRead = notification.IsRead
                };

                await _realtimeService.SendGeneralNotificationAsync(userId, dto);

                _logger.LogInformation(
                    "📢 SignalR notification sent — UserId={UserId}, Type={Type}",
                    userId, type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error creating general notification — UserId={UserId}, Type={Type}",
                    userId, type);

                // Don't rethrow — notification failure should not crash password/phone change
            }
        }
    }
}