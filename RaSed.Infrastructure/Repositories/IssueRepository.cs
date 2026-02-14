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
        public async Task<IEnumerable<Issue>> GetAllIssuesWithDetailsAsync()
        {
            return await _context.Set<Issue>()
                .Include(i => i.Employee)
                    .ThenInclude(e => e.Section)
                .OrderByDescending(i => i.ReportedAt)
                .ToListAsync();
        }

        // Gets a single issue with all related data for detail view
        public async Task<Issue?> GetIssueWithDetailsAsync(int id)
        {
            return await _context.Set<Issue>()
                .Include(i => i.Employee)
                    .ThenInclude(e => e.Section)
                .FirstOrDefaultAsync(i => i.Id == id);
        }
    }
}
