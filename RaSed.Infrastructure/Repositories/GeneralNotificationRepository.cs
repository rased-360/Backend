using Microsoft.EntityFrameworkCore;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;

namespace RaSed.Infrastructure.Repositories
{
    public class GeneralNotificationRepository : GenericRepository<GeneralNotification>, IGeneralNotificationRepository
    {
        public GeneralNotificationRepository(AppDbContext context) : base(context) { }

        /// <summary>
        /// Gets all notifications for a user, ordered by most recent first.
        /// </summary>
        public async Task<IEnumerable<GeneralNotification>> GetUserNotificationsAsync(int userId)
        {
            return await _context.GeneralNotifications
                .Where(n => n.UserId == userId)
                .OrderByDescending(n => n.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Marks a single notification as read.
        /// Returns false if notification doesn't exist or doesn't belong to user.
        /// 
        /// SECURITY: Always check UserId to prevent marking other users' notifications.
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int notificationId, int userId)
        {
            var notification = await _context.GeneralNotifications
                .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

            if (notification == null)
                return false;

            notification.IsRead = true;
            // SaveChanges called by UnitOfWork

            return true;
        }

        /// <summary>
        /// Marks all notifications as read for a user.
        /// Returns count of updated notifications.
        /// </summary>
        public async Task<int> MarkAllAsReadAsync(int userId)
        {
            return await _context.GeneralNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(setters => setters.SetProperty(n => n.IsRead, true));
        }

        /// <summary>
        /// Gets count of unread notifications for badge display.
        /// </summary>
        public async Task<int> GetUnreadCountAsync(int userId)
        {
            return await _context.GeneralNotifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .CountAsync();
        }

        /// <summary>
        /// Deletes notifications older than specified days.
        /// Used by cleanup background service.
        /// </summary>
        public async Task<int> DeleteOldNotificationsAsync(int olderThanDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

            return await _context.GeneralNotifications
                .Where(n => n.Timestamp < cutoffDate)
                .ExecuteDeleteAsync();
        }
    }
}