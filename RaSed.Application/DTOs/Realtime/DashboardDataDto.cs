using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    public class DashboardDataDto
    {
        public SensorReadingDto? LatestReading { get; set; }
        public ChartDataDto? TodayChart { get; set; }
        public List<AlertDto> ActiveAlerts { get; set; } = new();
    }
}
