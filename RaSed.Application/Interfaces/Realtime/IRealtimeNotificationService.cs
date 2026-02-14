using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.DTOs.Realtime;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Realtime
{
    public interface IRealtimeNotificationService
    {
        // Existing methods for sensor data
        Task SendSensorReadingAsync(SensorReadingDto reading);
        Task SendAlertAsync(AlertDto alert);
        Task SendChartUpdateAsync(ChartDataDto chartData);

        //Method for sending issue notifications to admin desktop
        Task SendIssueNotificationAsync(IssueNotificationPreviewDto notification);

    }
}
