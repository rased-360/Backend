using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface ISensorReadingRepository
    {
        Task AddAsync(SensorReading reading);
        Task<IEnumerable<SensorReading>> GetLast24HoursAsync();
        Task<IEnumerable<SensorReading>> GetCurrentMonthAsync();
        Task<IEnumerable<SensorReading>> GetTodayReadingsAsync();
        Task<SensorReading?> GetLatestAsync();
        Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
    }
}
