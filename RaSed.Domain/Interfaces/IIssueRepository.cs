using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IIssueRepository : IGenericRepository<Issue>
    {
        // Gets all issues with employee and section details
        // Ordered by most recent first
        Task<IEnumerable<Issue>> GetAllIssuesAsync();

        // Gets issue by ID with related employee and section data
        Task<Issue?> GetIssueWithDetailsAsync(int id);

        /// <summary>
        /// Marks an issue as read.
        /// Returns false if issue doesn't exist.
        /// 
        /// SECURITY: Only Admin can mark issues as read (issues are admin-only feature).
        /// </summary>
        Task<bool> MarkAsReadAsync(int issueId);

        /// <summary>
        /// Gets unread issue count for admin.
        /// Used for admin notification badge.
        /// </summary>
        Task<int> GetUnreadCountForAdminAsync();


        /// <summary>
        /// Gets all UNREAD issues for admin.
        /// Used when filtering notifications to show only unread.
        /// </summary>
        Task<IEnumerable<Issue>> GetUnreadForAdminAsync();
    }
}
