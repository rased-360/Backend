using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Hubs
{
    [Authorize(Roles = "Admin")]
    public class IssueHub : Hub
    {
        private static readonly HashSet<string> _connectedAdmins = new();
        private static readonly object _lock = new object();

        // Called when admin connects to the hub
        // Automatically connects when desktop app starts
        public override async Task OnConnectedAsync()
        {
            var adminId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(adminId))
            {
                lock (_lock)
                {
                    _connectedAdmins.Add(adminId);
                }

                Console.WriteLine($"[IssueHub] ✅ Admin connected: {adminId} (Connection: {connectionId}) - Total admins online: {_connectedAdmins.Count}");
            }

            await base.OnConnectedAsync();
        }

        /// Called when admin disconnects
        /// Automatically attempts to reconnect
        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var adminId = Context.UserIdentifier;
            var connectionId = Context.ConnectionId;

            if (!string.IsNullOrEmpty(adminId))
            {
                lock (_lock)
                {
                    _connectedAdmins.Remove(adminId);
                }

                if (exception != null)
                {
                    Console.WriteLine($"[IssueHub] ⚠️ Admin disconnected with error: {adminId} (Connection: {connectionId}) - Error: {exception.Message}");
                }
                else
                {
                    Console.WriteLine($"[IssueHub] ⚠️ Admin disconnected: {adminId} (Connection: {connectionId}) - Total admins online: {_connectedAdmins.Count}");
                }
            }

            await base.OnDisconnectedAsync(exception);
        }

        // Gets the count of currently connected admins
        public static int GetConnectedAdminsCount()
        {
            lock (_lock)
            {
                return _connectedAdmins.Count;
            }
        }

        // Ping method to keep connection alive
        // Desktop app can call this periodically to ensure connection stays active
        public async Task Ping()
        {
            await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
        }

    }
}
