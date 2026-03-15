using System;

namespace RaSed.Application.DTOs.Notifications
{
    /// <summary>
    /// Unified notification DTO that combines violations, issues, and general notifications.
    /// Used to return all notifications in a single API call, sorted by timestamp.
    /// 
    /// DESKTOP (Admin):
    ///   - Violations (all employees)
    ///   - Issues (all employees)
    ///   - General (own account changes)
    /// 
    /// MOBILE (Employee):
    ///   - Violations (my violations)
    ///   - General (own account changes)
    /// </summary>
    public class UnifiedNotificationDto
    {
        /// <summary>
        /// Notification type: "violation" | "issue" | "general"
        /// Lowercase for consistency with frontend.
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// When this notification occurred (UTC)
        /// Used for sorting across all notification types.
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Whether user has seen this notification
        /// </summary>
        public bool IsRead { get; set; }

        /// <summary>
        /// Notification-specific data.
        /// Type depends on notification type:
        ///   - violation → ViolationNotificationDto
        ///   - issue → IssueNotificationPreviewDto
        ///   - general → GeneralNotificationDto
        /// </summary>
        public object Data { get; set; } = null!;
    }
}