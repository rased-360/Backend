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
    public class IssueRepository : GenericRepository<Issue>, IIssueRepository
    {
        public IssueRepository(AppDbContext context) : base(context)
        {
        }

        // Gets all issues with employee and section details
        // Ordered by most recent first for notification display
        public async Task<IEnumerable<Issue>> GetAllIssuesAsync()
        {
            return await _context.Issues
                .Include(i => i.Employee)
                    .ThenInclude(e => e.Section)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        // Gets a single issue with all related data for detail view
        public async Task<Issue?> GetIssueWithDetailsAsync(int id)
        {
            return await _context.Issues
                .Include(i => i.Employee)
                    .ThenInclude(e => e.Section)
                .FirstOrDefaultAsync(i => i.Id == id);
        }

        /// <summary>
        /// Marks an issue as read.
        /// Returns false if issue doesn't exist.
        /// 
        /// SECURITY: Only Admin can mark issues as read (issues are admin-only feature).
        /// </summary>
        public async Task<bool> MarkAsReadAsync(int issueId)
        {
            var issue = await _context.Issues.FirstOrDefaultAsync(i => i.Id == issueId);

            if (issue == null)
                return false;

            issue.IsRead = true;
            // SaveChanges called by UnitOfWork

            return true;
        }

      

        /// <summary>
        /// Gets unread issue count for admin.
        /// Used for admin notification badge.
        /// </summary>
        public async Task<int> GetUnreadCountForAdminAsync()
        {
            return await _context.Issues
                .Where(i => !i.IsRead)
                .CountAsync();
        }

        // ✅ NEW: Get Unread Only

        /// <summary>
        /// Gets all UNREAD issues for admin.
        /// Used when filtering notifications to show only unread.
        /// </summary>
        public async Task<IEnumerable<Issue>> GetUnreadForAdminAsync()
        {
            return await _context.Issues
                .Include(i => i.Employee)
                    .ThenInclude(e => e.Section)
                .Where(i => !i.IsRead)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }
    }
}
