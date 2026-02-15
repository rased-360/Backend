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
        // ── Telemetry ─────────────────────────────────────────────────────────
        /// <summary>SignalR event: "ReceiveSensorReading"</summary>
        Task SendSensorReadingAsync(SensorReadingDto reading);

        // ── Threshold Alerts ──────────────────────────────────────────────────
        /// <summary>SignalR event: "ReceiveAlert"</summary>
        Task SendAlertAsync(AlertDto alert);

        // ── Device State ──────────────────────────────────────────────────────
        /// <summary>SignalR event: "ReceiveDeviceState"</summary>
        Task SendDeviceStateAsync(DeviceStateDto state);

        //Method for sending issue notifications to admin desktop
        Task SendIssueNotificationAsync(IssueNotificationPreviewDto notification);

    }
}
