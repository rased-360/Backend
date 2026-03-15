using RaSed.Application.DTOs.Notifications;

namespace RaSed.Application.Interfaces
{
    /// <summary>
    /// Service for managing unified notifications across all types.
    /// Combines violations, issues, and general notifications.
    /// </summary>
    public interface INotificationService
    {
        /// <summary>
        /// Gets all notifications for Admin (Desktop).
        /// Returns: Violations (all employees) + Issues (all employees) + General (own account)
        /// Sorted by timestamp descending.
        /// </summary>
        Task<IEnumerable<UnifiedNotificationDto>> GetAdminNotificationsAsync(
            int adminId,
            bool unreadOnly = false);

        /// <summary>
        /// Gets all notifications for Employee (Mobile).
        /// Returns: Violations (my violations) + General (own account)
        /// Sorted by timestamp descending.
        /// </summary>
        Task<IEnumerable<UnifiedNotificationDto>> GetEmployeeNotificationsAsync(
           int employeeId,
           bool unreadOnly = false);


        /// <summary>
        /// Gets unread notification count for badge.
        /// </summary>
        Task<int> GetUnreadCountAsync(int userId, bool isAdmin);

        /// <summary>
        /// Creates a general notification after password/phone change.
        /// Also sends SignalR notification.
        /// </summary>
        Task CreateGeneralNotificationAsync(int userId, string type, string message);
    }
}