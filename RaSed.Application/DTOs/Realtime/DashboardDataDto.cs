using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    /// <summary>
    /// Dashboard snapshot returned by GET /api/sensor/dashboard.
    /// Used by the frontend on page load / reconnect to restore state
    /// before the first SignalR message arrives.
    /// </summary>
    public class DashboardDataDto
    {
        /// <summary>Last known sensor telemetry (from in-memory cache)</summary>
        public SensorReadingDto? LatestReading { get; set; }

        /// <summary>Last known fan/pump state (from in-memory cache)</summary>
        public DeviceStateDto? DeviceState { get; set; }

        /// <summary>
        /// Threshold-based alerts derived from the latest reading.
        /// e.g. TemperatureHigh, HumidityLow, HydrogenHigh …
        /// These are separate from fire alerts.
        /// </summary>
        public List<AlertDto> ActiveAlerts { get; set; } = new();
    }
}
