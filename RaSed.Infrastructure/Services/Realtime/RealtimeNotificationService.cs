using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Entities;
using RaSed.Infrastructure.Hubs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Realtime
{
    public class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly IHubContext<IssueHub> _issueHubContext;
        private readonly ILogger<RealtimeNotificationService> _logger;

        public RealtimeNotificationService(
            IHubContext<SensorHub> hubContext,
            IHubContext<IssueHub> issueHubContext,
            ILogger<RealtimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _issueHubContext = issueHubContext;
            _logger = logger;
        }

        // ── Telemetry ─────────────────────────────────────────────────────────

        /// <summary>SignalR event: "ReceiveSensorReading"</summary>
        public async Task SendSensorReadingAsync(SensorReadingDto reading)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveSensorReading", reading);
                _logger.LogDebug(
                    "📡 Sent reading — Temp={Temp}°C, Hum={Hum}%, H={H}, C={C}, PM2.5={Pm25}",
                    reading.Temperature, reading.Humidity,
                    reading.Hydrogen, reading.Ethanol, reading.Pm2_5);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending sensor reading via SignalR");
                throw;
            }
        }

        // ── Threshold Alerts ──────────────────────────────────────────────────

        /// <summary>SignalR event: "ReceiveAlert"</summary>
        public async Task SendAlertAsync(AlertDto alert)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveAlert", alert);
                _logger.LogDebug("🚨 Sent alert — Type={Type}, Severity={Severity}",
                    alert.Type, alert.Severity);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending alert via SignalR");
                throw;
            }
        }

        // ── Device State ──────────────────────────────────────────────────────

        /// <summary>SignalR event: "ReceiveDeviceState"</summary>
        public async Task SendDeviceStateAsync(DeviceStateDto state)
        {
            try
            {
                await _hubContext.Clients.All.SendAsync("ReceiveDeviceState", state);
                _logger.LogInformation("🔧 Sent device state — Fan={Fan}, Pump={Pump}",
                    state.Fan, state.Pump);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending device state via SignalR");
                throw;
            }
        }
        // Issue notification method
        public async Task SendIssueNotificationAsync(IssueNotificationPreviewDto notification)
        {
            try
            {
                // Send to all connected admin desktops
                await _issueHubContext.Clients.All.SendAsync("ReceiveIssueNotification", notification);

                _logger.LogInformation(
                    "📢 Issue notification sent - ID: {IssueId}, Title: {Title}, Employee: {EmployeeName}, Section: {SectionName}, Time: {ReportedAt}",
                    notification.IssueId,
                    notification.Title,
                    notification.EmployeeName,
                    notification.SectionName,
                    notification.ReportedAt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending issue notification via SignalR - Issue ID: {IssueId}", notification.IssueId);
            }
        }
    }
}
