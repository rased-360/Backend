using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
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
    public class FireEventRepository : GenericRepository<FireEvent>, IFireEventRepository
    {
        public FireEventRepository(AppDbContext context) : base(context)
        {
            
        }

        public async Task<FireEvent?> GetActiveFireEventAsync(string deviceId)
        {
            return await _context.FireEvents
                .Where(e => e.DeviceId == deviceId && e.Status == "Active")
                .OrderByDescending(e => e.StartTime)
                .FirstOrDefaultAsync();
        }
        public async Task<int> DeleteOldFireEventsAsync(int olderThanDays)
        {
            var cutoffDate = DateTime.UtcNow.AddDays(-olderThanDays);

            return await _context.FireEvents
                .Where(e => e.Status == "Resolved" && e.StartTime < cutoffDate)
                .ExecuteDeleteAsync();  // Bulk delete — efficient
        }
    }
}
