using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IViolationRepository : IGenericRepository<Violation>
    {
        /// <summary>Gets violations older than the given date (used by cleanup service)</summary>
        Task<IEnumerable<Violation>> GetViolationsOlderThanAsync(DateTime cutoff);

        /// <summary>Bulk delete — more efficient than deleting one by one</summary>
        Task DeleteRangeAsync(IEnumerable<Violation> violations);

        /// <summary>All violations with employee + section, newest first</summary>
        Task<IEnumerable<Violation>> GetAllWithDetailsAsync();

        /// <summary>Single violation with full navigation properties</summary>
        Task<Violation?> GetByIdWithDetailsAsync(int id);

        //------------------------------ METHODS FOR NOTIFICATIONS ----------------------------
        /// <summary>
        /// Gets all violations for a specific employee.
        /// Used for employee notification history (Mobile app).
        /// 
        /// </summary>
        Task<IEnumerable<Violation>> GetViolationsByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// Marks a violation as read.
        /// Returns false if violation doesn't exist or doesn't belong to user.
        /// 
        /// SECURITY:
        ///   - Employee: Can only mark their own violations
        ///   - Admin: Can mark any violation
        /// </summary>

        Task<bool> MarkAsReadAsync(int violationId, int userId, bool isAdmin);

        /// <summary>
        /// Gets unread violation count for a specific employee.
        /// Used for employee notification badge.
        /// </summary>
        Task<int> GetUnreadCountByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// Gets unread violation count for admin (all employees).
        /// Used for admin notification badge.
        /// </summary>
        Task<int> GetUnreadCountForAdminAsync();

        /// <summary>
        /// Gets all UNREAD violations for admin.
        /// Used when filtering notifications to show only unread.
        /// </summary>
        Task<IEnumerable<Violation>> GetUnreadForAdminAsync();

        /// <summary>
        /// Gets all UNREAD violations for a specific employee.
        /// Used when filtering notifications to show only unread.
        /// </summary>
        Task<IEnumerable<Violation>> GetUnreadByEmployeeIdAsync(int employeeId);

    }
}
