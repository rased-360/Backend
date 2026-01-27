using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IAggregatedSensorDataRepository
    {
        Task AddAsync(AggregatedSensorData data);
        Task<IEnumerable<AggregatedSensorData>> GetByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<int> DeleteOlderThanAsync(DateTime cutoffDate);
    }
}
