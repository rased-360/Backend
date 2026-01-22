using RaSed.Application.DTOs.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Realtime
{
    public interface ISensorDataService
    {
        Task ProcessAndStoreReadingAsync(SensorReadingDto reading);
        Task<DashboardDataDto> GetDashboardDataAsync();
        Task<ChartDataDto> GetTodayChartDataAsync();
    }
}
