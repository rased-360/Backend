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
    }
}
