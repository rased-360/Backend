using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Hubs
{
    public class SensorHub :Hub
    {
        private readonly ILogger<SensorHub> _logger;

        public SensorHub(ILogger<SensorHub> logger)
        {
            _logger = logger;
        }

        public override async Task OnConnectedAsync()
        {
            _logger.LogInformation("🔌 Client connected: {ConnectionId} from {UserAgent}",
                Context.ConnectionId,
                Context.GetHttpContext()?.Request.Headers["User-Agent"].ToString() ?? "Unknown");

            await Clients.Caller.SendAsync("Connected", new
            {
                connectionId = Context.ConnectionId,
                message = "Successfully connected to SensorHub",
                timestamp = DateTime.UtcNow
            });

            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            if (exception != null)
            {
                _logger.LogWarning(exception,
                    "🔌 Client disconnected with error: {ConnectionId}",
                    Context.ConnectionId);
            }
            else
            {
                _logger.LogInformation(
                    "🔌 Client disconnected normally: {ConnectionId}",
                    Context.ConnectionId);
            }

            await base.OnDisconnectedAsync(exception);
        }

        public async Task JoinAlertGroup()
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, "AlertSubscribers");
            _logger.LogInformation(
                "🚨 Client {ConnectionId} joined alert group",
                Context.ConnectionId);
        }

        public async Task LeaveAlertGroup()
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, "AlertSubscribers");
            _logger.LogInformation(
                "🚨 Client {ConnectionId} left alert group",
                Context.ConnectionId);
        }

        /// <summary>
        /// Ping/Pong for connection health check
        /// </summary>
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", new
            {
                timestamp = DateTime.UtcNow,
                serverTime = DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss")
            });
        }
    }
}
