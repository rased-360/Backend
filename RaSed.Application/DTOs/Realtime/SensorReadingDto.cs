using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    /// <summary>
    /// Sent to the frontend via SignalR "ReceiveSensorReading"
    /// Contains all telemetry values including new air quality fields.
    /// </summary>
    public class SensorReadingDto
    {
        public string DeviceId { get; set; } = string.Empty;

        // ── Gas sensors ──────────────────────────────────────
        public int Hydrogen { get; set; }
        public int Ethanol { get; set; }

        // ── Environmental ────────────────────────────────────
        public decimal Temperature { get; set; }
        public decimal Humidity { get; set; }
        public decimal Pressure { get; set; }

        // ── Air Quality ──────────────────────────────────────
        public decimal Pm2_5 { get; set; }
        public decimal Pm1_0 { get; set; }
        public int Tvoc { get; set; }
        public int Eco2 { get; set; }

        // ── Computed ─────────────────────────────────────────
        public decimal HeatIndex { get; set; }

        public DateTime Timestamp { get; set; }

        // ── Alert flags (set by SensorDataProcessor) ─────────
        public bool HasAlert { get; set; }
        public string? AlertType { get; set; }
        public string? AlertMessage { get; set; }
    }
}
