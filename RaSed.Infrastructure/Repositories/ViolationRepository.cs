using Microsoft.EntityFrameworkCore;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Repositories
{
    public class ViolationRepository : GenericRepository<Violation>, IViolationRepository
    {
        public ViolationRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<Violation>> GetAllWithDetailsAsync()
        {
            return await _context.Violations
                .Include(v => v.Employee)
                    .ThenInclude(e => e!.Section)
                .OrderByDescending(v => v.Timestamp)
                .ToListAsync();
        }

        public async Task<Violation?> GetByIdWithDetailsAsync(int id)
        {
            return await _context.Violations
                .Include(v => v.Employee)
                    .ThenInclude(e => e!.Section)
                .FirstOrDefaultAsync(v => v.Id == id);
        }

        public async Task<IEnumerable<Violation>> GetViolationsOlderThanAsync(DateTime cutoff)
        {
            return await _context.Violations
                .Where(v => v.Timestamp < cutoff)
                .ToListAsync();
        }

        /// <summary>
        /// ExecuteDeleteAsync is EF 7+ bulk-delete — zero round trips per row.
        /// Falls back to RemoveRange for older EF versions.
        /// </summary>
        public async Task DeleteRangeAsync(IEnumerable<Violation> violations)
        {
            _context.Violations.RemoveRange(violations);
            // SaveChanges is called by UnitOfWork — do NOT call it here
        }


        //---------------------------- METHODS FOR NOTIFICATIONS ----------------------------

        /// <summary>
        /// Gets all violations for a specific employee.
        /// Used for employee notification history (Mobile app).
        /// 
        /// ✅ NEW: Added for employee-specific notifications
        /// </summary>
        public async Task<IEnumerable<Violation>> GetViolationsByEmployeeIdAsync(int employeeId)
        {
            return await _context.Violations
                .Include(v => v.Employee)
                    .ThenInclude(e => e!.Section)
                .Where(v => v.EmployeeId == employeeId)
                .OrderByDescending(v => v.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Marks a violation as read.
        /// Returns false if violation doesn't exist or doesn't belong to user.
        /// 
        /// SECURITY:
        ///   - Employee: Can only mark their own violations
        ///   - Admin: Can mark any violation
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int violationId, int userId, bool isAdmin)
        {
            var query = _context.Violations.AsQueryable();

            // Security: Employee can only mark their own violations
            if (!isAdmin)
                query = query.Where(v => v.EmployeeId == userId);

            var violation = await query.FirstOrDefaultAsync(v => v.Id == violationId);

            if (violation == null)
                return false;

            violation.IsRead = true;
            // SaveChanges called by UnitOfWork

            return true;
        }


        /// <summary>
        /// Gets unread violation count for a specific employee.
        /// Used for employee notification badge.
        /// </summary>
        public async Task<int> GetUnreadCountByEmployeeIdAsync(int employeeId)
        {
            return await _context.Violations
                .Where(v => v.EmployeeId == employeeId && !v.IsRead)
                .CountAsync();
        }

        /// <summary>
        /// Gets unread violation count for admin (all employees).
        /// Used for admin notification badge.
        /// </summary>
        public async Task<int> GetUnreadCountForAdminAsync()
        {
            return await _context.Violations
                .Where(v => !v.IsRead)
                .CountAsync();
        }

        /// <summary>
        /// Gets all UNREAD violations for admin.
        /// Used when filtering notifications to show only unread.
        /// </summary>
        public async Task<IEnumerable<Violation>> GetUnreadForAdminAsync()
        {
            return await _context.Violations
                .Include(v => v.Employee)
                    .ThenInclude(e => e!.Section)
                .Where(v => !v.IsRead)
                .OrderByDescending(v => v.Timestamp)
                .ToListAsync();
        }

        /// <summary>
        /// Gets all UNREAD violations for a specific employee.
        /// Used when filtering notifications to show only unread.
        /// </summary>
        public async Task<IEnumerable<Violation>> GetUnreadByEmployeeIdAsync(int employeeId)
        {
            return await _context.Violations
                .Include(v => v.Employee)
                    .ThenInclude(e => e!.Section)
                .Where(v => v.EmployeeId == employeeId && !v.IsRead)
                .OrderByDescending(v => v.Timestamp)
                .ToListAsync();
        }

        /// </summary>
        /// <param name="employeeId">The employee to count violations for.</param>
        /// <param name="from">
        ///     Inclusive lower-bound timestamp.
        ///     Caller passes DateTime.UtcNow.AddDays(-WindowDays).
        /// </param>
        public async Task<int> CountViolationsByEmployeeInWindowAsync(
            int employeeId,
            DateTime from)
        {
            return await _context.Violations
                .Where(v => v.EmployeeId == employeeId && v.Timestamp >= from)
                .CountAsync();
        }
    }
}
