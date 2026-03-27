using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Hubs
{
    /// <summary>
    /// Unified SignalR hub for ALL admin notifications.
    /// Handles: Violations, Issues, General Notifications.
    /// 
    /// EVENTS SENT TO CLIENTS:
    ///   - ReceiveViolationNotification
    ///   - ReceiveIssueNotification
    ///   - ReceiveGeneralNotification
    ///   
    ///Next sprint: can be extended to also push to employees (by user group / connection group).
    ///
    /// SECURITY: Admin
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class NotificationHub : Hub
    {
        private static readonly HashSet<string> _connectedAdmins = new();
        private static readonly object _lock = new();
        private readonly ILogger<NotificationHub> _logger;

        public NotificationHub(ILogger<NotificationHub> logger)
        {
            _logger = logger;
        }

        // ──────────────────────────────────────────────────────────────────────
        // Connection Lifecycle
        // ──────────────────────────────────────────────────────────────────────

        public override async Task OnConnectedAsync()
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                lock (_lock)
                {
                    _connectedAdmins.Add(userId);
                }

                _logger.LogInformation(
                    "✅ [NotificationHub] Admin connected — UserId: {UserId}, ConnectionId: {ConnectionId}, Total: {Count}",
                    userId, connectionId, _connectedAdmins.Count);
            }

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(userId))
            {
                lock (_lock)
                {
                    _connectedAdmins.Remove(userId);
                }

                if (exception != null)
                {
                    _logger.LogWarning(exception,
                        "⚠️ [NotificationHub] Admin disconnected with error — UserId: {UserId}, ConnectionId: {ConnectionId}",
                        userId, connectionId);
                }
                else
                {
                    _logger.LogInformation(
                        "⚠️ [NotificationHub] Admin disconnected — UserId: {UserId}, ConnectionId: {ConnectionId}, Total: {Count}",
                        userId, connectionId, _connectedAdmins.Count);
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // ──────────────────────────────────────────────────────────────────────
        // Client Methods
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Keep-alive ping to prevent connection timeout.
        /// Desktop app should call this every 15-20 seconds.
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new
            {
                timestamp = DateTime.UtcNow,
                connectionId = Context.ConnectionId
            });
        }

        // ──────────────────────────────────────────────────────────────────────
        // Static Helpers
        // ──────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Gets count of currently connected admins.
        /// Used for monitoring/debugging.
        /// </summary>
        public static int GetConnectedAdminsCount()
        {
            lock (_lock)
            {
                return _connectedAdmins.Count;
            }
        }
    }
}
