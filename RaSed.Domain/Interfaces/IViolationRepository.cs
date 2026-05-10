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


        /// <summary>
        /// Returns the number of violations for <paramref name="employeeId"/>
        /// whose Timestamp is &gt;= <paramref name="from"/> (i.e. inside the
        /// rolling window).
        ///
        /// WHY a dedicated COUNT method instead of reusing GetViolationsByEmployeeIdAsync?
        ///   GetViolationsByEmployeeIdAsync loads full entity objects plus two
        ///   navigation properties (Employee + Section) into memory before
        ///   counting.  For performance rate we only need a scalar integer, so
        ///   this method translates to a single  SELECT COUNT(*)  with no JOINs.
        ///   Zero objects are materialised — much cheaper at scale.
        /// </summary>
        /// <param name="employeeId">The employee whose violations to count.</param>
        /// <param name="from">
        ///     Inclusive lower bound — typically DateTime.UtcNow minus WindowDays.
        /// </param>
        Task<int> CountViolationsByEmployeeInWindowAsync(int employeeId, DateTime from);


        /// <summary>
        /// Gets violations for a specific employee grouped by date for a given month.
        /// Used for calendar view.
        /// </summary>
        Task<IEnumerable<Violation>> GetViolationsByEmployeeAndMonthAsync(
            int employeeId,
            int year,
            int month);


        /// <summary>
        /// Gets violations for a specific employee on a specific date.
        /// Used when employee clicks on a day in the calendar.
        /// </summary>
        Task<IEnumerable<Violation>> GetViolationsByEmployeeAndDateAsync(
            int employeeId,
            DateTime date);
    }
}
