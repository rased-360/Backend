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
    public class SensorReadingRepository : ISensorReadingRepository
    {
        private readonly AppDbContext _dbContext;
        public SensorReadingRepository(AppDbContext context)
        {
            _dbContext = context;
        }
        public async Task AddAsync(SensorReading reading)
        {
            await _dbContext.SensorReadings.AddAsync(reading);
        }

        public async Task<IEnumerable<SensorReading>> GetLast24HoursAsync()
        {
            var cutoffTime = DateTime.UtcNow.AddHours(-24);

            return await _dbContext.SensorReadings
                .Where(r => r.Timestamp >= cutoffTime)
                .OrderBy(r => r.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<SensorReading>> GetCurrentMonthAsync()
        {
            var startOfMonth = new DateTime(
                DateTime.UtcNow.Year,
                DateTime.UtcNow.Month,
                1
            );

            return await _dbContext.SensorReadings
                .Where(r => r.Timestamp >= startOfMonth)
                .OrderBy(r => r.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<SensorReading>> GetTodayReadingsAsync()
        {
            var startOfDay = DateTime.UtcNow.Date;

            return await _dbContext.SensorReadings
                .Where(r => r.Timestamp >= startOfDay)
                .OrderBy(r => r.Timestamp)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<SensorReading?> GetLatestAsync()
        {
            return await _dbContext.SensorReadings
                .OrderByDescending(r => r.Timestamp)
                .AsNoTracking()
                .FirstOrDefaultAsync();
        }
    }
}
