using RaSed.Application.DTOs.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Realtime
{
    public interface IRealtimeNotificationService
    {
        Task SendSensorReadingAsync(SensorReadingDto reading);
        Task SendAlertAsync(AlertDto alert);
        Task SendChartUpdateAsync(ChartDataDto chartData);
    }
}
