using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.DTOs.Violations;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Enums;
using RaSed.Infrastructure.Hubs;


namespace RaSed.Infrastructure.Services.Realtime
{
    public class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly IHubContext<IssueHub> _issueHubContext;
        private readonly IHubContext<ViolationHub> _violationHubContext;
        private readonly IFcmService _fcmService;
        private readonly ILogger<RealtimeNotificationService> _logger;

        public RealtimeNotificationService(
            IHubContext<SensorHub> hubContext,
            IHubContext<IssueHub> issueHubContext,
            IFcmService fcmService,
            IHubContext<ViolationHub> violationHubContext,
            ILogger<RealtimeNotificationService> logger)
        {
            _hubContext = hubContext;
            _issueHubContext = issueHubContext;
            _fcmService = fcmService;
            _violationHubContext = violationHubContext;
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


        // ── Fire Alert ────────────────────────────────────────────────────────

        /// <summary>
        /// Broadcasts "ReceiveFireAlert" to ALL connected clients (desktop + mobile).
        ///
        /// Triggered ONLY when fire_alarm state changes (0→1 or 1→0).
        /// Never called for 0→0 or 1→1 (handled by SensorCacheService.HasFireStateChanged).
        ///
        /// The payload contains content for BOTH client types:
        ///   - Desktop reads: DesktopTitle + DesktopBody
        ///   - Mobile reads:  MobileTitle  + MobileBody
        ///   - Both read:     Type + Status + Timestamp  (for logic/routing)
        /// </summary>
        public async Task SendFireAlertAsync(FireAlertDto fireAlert)
        {
            try
            {
                // ── CHANNEL 1: SignalR ────────────────────────────────────────
                // Broadcasts to ALL connected clients (Desktop + Mobile if app is running) 
                await _hubContext.Clients.All.SendAsync("ReceiveFireAlert", fireAlert);

                _logger.LogCritical(
                    "🔥 FIRE ALERT BROADCAST — Type={Type}, Status={Status}, " +
                    "DesktopTitle={DesktopTitle}, MobileTitle={MobileTitle}",
                    fireAlert.Type,
                    fireAlert.Status,
                    fireAlert.DesktopTitle,
                    fireAlert.MobileTitle);

                // ── CHANNEL 2: FCM ────────────────────────────────────────────
                // Sends push notification to Mobile (even if app is closed)
                // Uses MobileTitle + MobileBody from FireAlertDto
                await _fcmService.SendToTopicAsync(
                    topic: "fire-alerts",
                    title: fireAlert.MobileTitle,
                    body: fireAlert.MobileBody,
                    data: new Dictionary<string, string>
                    {
                        { "type", fireAlert.Type.ToLower() },           // "fire_started" or "fire_cleared"
                        { "status", fireAlert.Status.ToLower() },       // "active" or "resolved"
                        { "device_id", fireAlert.DeviceId },
                        { "timestamp", fireAlert.Timestamp.ToString("o") }
                    },
                    priority: fireAlert.Status == "Active"
                        ? FcmNotificationPriority.Critical  // Fire started — urgent
                        : FcmNotificationPriority.High,     // Fire cleared — important but not urgent
                    color: fireAlert.Status == "Active"
                        ? "#FF0000"  // Red for fire
                        : "#00FF00"  // Green for cleared
                );

                _logger.LogCritical(
                    "📤 FCM sent — Topic=fire-alerts, Type={Type}, Priority={Priority}",
                    fireAlert.Type,
                    fireAlert.Status == "Active" ? "Critical" : "High");

                _logger.LogCritical(
                    "✅ Fire alert broadcast complete — SignalR ✅, FCM ✅");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error sending fire alert via SignalR");
                // Do NOT rethrow — a SignalR failure must never crash the MQTT processing pipeline
            }
        }



        //── Issue Notification ────────────────────────────────────────────────
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

        // ── Violation Notification ────────────────────────────────────────────────
        public async Task SendViolationNotificationAsync(ViolationNotificationDto notification)
        {
            try
            {
                // Broadcast to all connected admin desktops
                await _violationHubContext.Clients.All.SendAsync("ReceiveViolationNotification", notification);

                _logger.LogInformation(
                    "🚨 Violation notification sent — ID: {ViolationId}, Type: {Type}, Employee: {EmployeeName}, Section: {SectionName}",
                    notification.ViolationId,
                    notification.ViolationType,
                    notification.EmployeeName,
                    notification.SectionName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error sending violation notification via SignalR — Violation ID: {ViolationId}",
                    notification.ViolationId);
            }
        }




    }
}
