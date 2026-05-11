using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Notifications;
using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.DTOs.Realtime;
using RaSed.Application.DTOs.Violations;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Enums;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Hubs;
using RaSed.Infrastructure.Repositories;
using static RaSed.Infrastructure.Services.Realtime.FcmService;


namespace RaSed.Infrastructure.Services.Realtime
{
    public class RealtimeNotificationService : IRealtimeNotificationService
    {
        private readonly IHubContext<SensorHub> _hubContext;
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly IFcmService _fcmService;
        private readonly ILogger<RealtimeNotificationService> _logger;
        private readonly IUnitOfWork _unitOfWork;

        public RealtimeNotificationService(
            IHubContext<SensorHub> hubContext,
            IHubContext<NotificationHub> notificationHubContext,
            IFcmService fcmService,
            IUnitOfWork unitOfWork,
            ILogger<RealtimeNotificationService> logger )
        {
            _hubContext = hubContext;
            _notificationHubContext = notificationHubContext;
            _fcmService = fcmService;
            _unitOfWork = unitOfWork;
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
                await _notificationHubContext.Clients.All.SendAsync("ReceiveIssueNotification", notification);

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
                await _notificationHubContext.Clients.All.SendAsync("ReceiveViolationNotification", notification);

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

        // ── General Notification ──────────────────────────────────────────────

        /// <summary>
        /// Sends a general notification to a SPECIFIC user via SignalR.
        /// 
        /// SIGNALR EVENT: "ReceiveGeneralNotification"
        /// TARGET: Specific user (by userId)
        /// 
        /// IMPORTANT:
        ///   - Uses Clients.User(userId.ToString())
        ///   - User must be connected to SignalR with userId as identifier
        ///   - If user is offline, they'll see notification in history when they login
        /// </summary>
        public async Task SendGeneralNotificationAsync(int userId, GeneralNotificationDto notification)
        {
            try
            {
                // Send to specific user only (not all clients)
                await _notificationHubContext.Clients.User(userId.ToString())
                    .SendAsync("ReceiveGeneralNotification", notification);

                _logger.LogInformation(
                    "📢 General notification sent — UserId={UserId}, Type={Type}",
                    userId, notification.Type);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ Error sending general notification — UserId={UserId}, Type={Type}",
                    userId, notification.Type);

                // Don't rethrow — SignalR failure should not crash the operation
            }
        }


        // ── Specific Employee Violation Notification ──────────────────────────────────────────────
        public async Task SendViolationWarningToEmployeeAsync(ViolationNotificationDto notification)
        {
            if (notification.EmployeeId <= 0)
            {
                _logger.LogWarning(
                    "⚠️ Skipping employee violation warning — EmployeeId is unknown (Violation ID: {ViolationId})",
                    notification.ViolationId);
                return;
            }

            try
            {
                // ── CHANNEL 1: SignalR (if employee is connected) ─────────────────
                await _notificationHubContext.Clients
                    .User(notification.EmployeeId.ToString())
                    .SendAsync("ReceiveViolationWarning", notification);

                _logger.LogInformation(
                    "📱 SignalR violation warning sent — EmployeeId: {EmployeeId}, ViolationId: {ViolationId}",
                    notification.EmployeeId, notification.ViolationId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ SignalR violation warning failed — EmployeeId: {EmployeeId}", notification.EmployeeId);
            }

            try
            {
                // ── CHANNEL 2: FCM (push when app is closed) ──────────────────────
                var tokens = await _unitOfWork._fcmDeviceTokenRepository
                    .GetByEmployeeIdAsync(notification.EmployeeId);

                var tokenList = tokens.ToList();

                if (!tokenList.Any())
                {
                    _logger.LogInformation(
                        "ℹ️ No FCM tokens found for Employee {EmployeeId} — skipping push",
                        notification.EmployeeId);
                    return;
                }

                var title = "⚠️ Safety Violation Detected";
                var body = $"A {notification.ViolationType.Replace("_", " ")} violation was recorded for you.";

                var data = new Dictionary<string, string>
        {
            { "type",         "violation_warning"                    },
            { "violationId",  notification.ViolationId.ToString()    },
            { "violationType",notification.ViolationType             },
            { "timestamp",    notification.Timestamp.ToString("o")   },
            { "imageUrl",     notification.ImageUrl ?? string.Empty  }
        };

                foreach (var token in tokenList)
                {
                    try
                    {
                        await _fcmService.SendToDeviceAsync(
                            deviceToken: token.Token,
                            title: title,
                            body: body,
                            data: data,
                            priority: FcmNotificationPriority.High,
                            color: "#FF6B35"
                        );

                        _logger.LogInformation(
                            "📤 FCM violation warning sent — EmployeeId: {EmployeeId}, Platform: {Platform}",
                            notification.EmployeeId, token.Platform);
                    }
                    catch (InvalidFcmTokenException)
                    {
                        // Token is stale — delete it silently
                        await _unitOfWork._fcmDeviceTokenRepository
                            .DeleteByTokenAsync(token.Token);
                        await _unitOfWork.SaveChangesAsync();

                        _logger.LogWarning(
                            "🗑️ Stale FCM token removed — EmployeeId: {EmployeeId}", notification.EmployeeId);
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "❌ FCM violation warning failed — EmployeeId: {EmployeeId}", notification.EmployeeId);
                // Never rethrow — FCM failure must not affect anything else
            }
        }

    }
}
