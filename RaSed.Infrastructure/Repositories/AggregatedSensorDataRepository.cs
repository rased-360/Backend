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
    public class AggregatedSensorDataRepository: IAggregatedSensorDataRepository
    {
        private readonly AppDbContext _context;

        public AggregatedSensorDataRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AggregatedSensorData data)
        {
            await _context.AggregatedSensorData.AddAsync(data);
        }

        public async Task<IEnumerable<AggregatedSensorData>> GetByDateRangeAsync(
            DateTime startDate,
            DateTime endDate)
        {
            return await _context.AggregatedSensorData
                .Where(a => a.StartTime >= startDate && a.EndTime <= endDate)
                .OrderBy(a => a.StartTime)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> DeleteOlderThanAsync(DateTime cutoffDate)
        {
            var oldRecords = await _context.AggregatedSensorData
                .Where(a => a.EndTime < cutoffDate)
                .ToListAsync();

            _context.AggregatedSensorData.RemoveRange(oldRecords);

            return oldRecords.Count;
        }
    }
}
