using System;

namespace RaSed.Application.DTOs.Notifications
{
    /// <summary>
    /// DTO for general notification (password change, phone change, etc.)
    /// Used in unified notification API response.
    /// </summary>
    public class GeneralNotificationDto
    {
        public int Id { get; set; }

        /// <summary>
        /// Notification type: "PASSWORD_CHANGED" | "PHONE_CHANGED"
        /// </summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>
        /// Human-readable message
        /// Example: "Your password was changed successfully."
        /// </summary>
        public string Message { get; set; } = string.Empty;

        /// <summary>
        /// When the action occurred (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Whether user has seen this notification
        /// </summary>
        public bool IsRead { get; set; }
    }
}