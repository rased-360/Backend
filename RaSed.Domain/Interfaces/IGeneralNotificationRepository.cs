using RaSed.Domain.Entities;

namespace RaSed.Domain.Interfaces
{
    /// <summary>
    /// Repository for general notifications (password change, phone change, etc.)
    /// </summary>
    public interface IGeneralNotificationRepository : IGenericRepository<GeneralNotification>
    {
        /// <summary>
        /// Gets all notifications for a specific user, ordered by most recent first.
        /// Used for notification history page.
        /// </summary>
        Task<IEnumerable<GeneralNotification>> GetUserNotificationsAsync(int userId);

        /// <summary>
        /// Marks a notification as read.
        /// Returns false if notification doesn't exist or doesn't belong to the user.
        /// </summary>
        Task<bool> MarkAsReadAsync(int notificationId, int userId);

        /// <summary>
        /// Marks all notifications as read for a user.
        /// Used when user opens notification page.
        /// </summary>
        Task<int> MarkAllAsReadAsync(int userId);

        /// <summary>
        /// Gets unread notification count for a user.
        /// Used for notification badge count.
        /// </summary>
        Task<int> GetUnreadCountAsync(int userId);

        /// <summary>
        /// Deletes notifications older than specified days.
        /// Used by cleanup background service.
        /// </summary>
        Task<int> DeleteOldNotificationsAsync(int olderThanDays);
    }
}