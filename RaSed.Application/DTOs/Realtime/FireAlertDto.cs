using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    /// <summary>
    /// Sent via SignalR "ReceiveFireAlert" to ALL connected clients (desktop + mobile).
    /// 
    /// The payload contains content for BOTH client types:
    ///   - Desktop clients use: DesktopTitle + DesktopBody
    ///   - Mobile clients use:  MobileTitle  + MobileBody
    /// 
    /// The REST endpoint GET /api/sensor/fire/status uses the X-Client-Type header
    /// to return only the relevant fields for the requesting client.
    /// </summary>
    public class FireAlertDto
    {
        // ── Core fields (used by both clients for logic) ──────────────────────

        public string DeviceId { get; set; }

        /// <summary>"FireStarted" or "FireCleared"</summary>
        public string Type { get; set; } = string.Empty;

        /// <summary>"Active" or "Resolved"</summary>
        public string Status { get; set; } = string.Empty;

        public DateTime Timestamp { get; set; }

        // ── Desktop-specific content (English, formal) ────────────────────────

        public string DesktopTitle { get; set; } = string.Empty;
        public string DesktopBody { get; set; } = string.Empty;

        // ── Mobile-specific content ───────────────────────────────────────────

        public string MobileTitle { get; set; } = string.Empty;
        public string MobileBody { get; set; } = string.Empty;
    }
}
