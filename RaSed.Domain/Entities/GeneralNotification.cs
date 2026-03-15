using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    /// <summary>
    /// Represents a general system notification (password change, phone change, etc.)
    /// 
    /// TYPES:
    ///   - PASSWORD_CHANGED: User changed their password
    ///   - PHONE_CHANGED: User changed their phone number
    /// 
    /// USAGE:
    ///   - Desktop (Admin): Shows in notification badge
    ///   - Mobile (Employee): Shows in notification badge
    /// 
    /// </summary>
    public class GeneralNotification
    {
        public int Id { get; set; }

        /// <summary>
        /// User who performed the action (Admin or Employee)
        /// SECURITY: Always get from JWT token, never from URL parameter
        /// </summary>
        public int UserId { get; set; }

        /// <summary>
        /// Notification type
        /// Valid values: "PASSWORD_CHANGED" | "PHONE_CHANGED"
        /// </summary>
        public string Type { get; set; } = null!;

        /// <summary>
        /// Human-readable message
        /// Examples:
        ///   - "Your password was changed successfully."
        ///   - "Your phone number was changed to +20 123 456 7890."
        /// </summary>
        public string Message { get; set; } = null!;

        /// <summary>
        /// When the action occurred (UTC)
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Whether user has seen this notification
        /// Used for notification badge count
        /// </summary>
        public bool IsRead { get; set; }

        // ── Navigation Properties ─────────────────────────────────────────────

        /// <summary>
        /// The user who owns this notification
        /// </summary>
        public ApplicationUser User { get; set; } = null!;
    }
}