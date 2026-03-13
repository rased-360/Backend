using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Hubs
{
    /// <summary>
    /// SignalR hub that pushes real-time violation notifications to the admin desktop.
    /// Next sprint: can be extended to also push to employees (by user group / connection group).
    /// </summary>
    [Authorize(Roles = "Admin")]
    public class ViolationHub : Hub
    {
        private static readonly HashSet<string> _connectedAdmins = new();
        private static readonly object _lock = new();

        public override async Task OnConnectedAsync()
        {
            var adminId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(adminId))
            {
                lock (_lock) { _connectedAdmins.Add(adminId); }
                Console.WriteLine($"[ViolationHub] ✅ Admin connected: {adminId} — Total online: {_connectedAdmins.Count}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var adminId = Context.UserIdentifier;
            if (!string.IsNullOrEmpty(adminId))
            {
                lock (_lock) { _connectedAdmins.Remove(adminId); }
                Console.WriteLine($"[ViolationHub] ⚠️ Admin disconnected: {adminId} — Total online: {_connectedAdmins.Count}");
            }
            await base.OnDisconnectedAsync(exception);
        }

        public static int GetConnectedAdminsCount()
        {
            lock (_lock) { return _connectedAdmins.Count; }
        }

        /// <summary>Keep-alive ping — same pattern as IssueHub</summary>
        public async Task Ping() => await Clients.Caller.SendAsync("Pong", DateTime.UtcNow);
    }
}
