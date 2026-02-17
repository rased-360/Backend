using RaSed.Application.DTOs.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Realtime
{
    public interface ISensorDataService
    {
        // ── Called by MqttBackgroundService ──────────────────────────────────

        /// <summary>Process telemetry — cache + SignalR only, no DB write.</summary>
        Task ProcessTelemetryAsync(string deviceId, DateTime timestamp, object rawPayload);

        /// <summary>Process device state — cache + conditional SignalR, no DB write.</summary>
        Task ProcessDeviceStateAsync(string deviceId, DateTime timestamp, object rawPayload);

        /// <summary>
        /// Processes fire alert from: rased/{deviceId}/alert
        /// </summary>
        Task ProcessFireAlertAsync(string deviceId, DateTime timestamp, int fireAlarm);


        // ── Called by REST controller ─────────────────────────────────────────

        /// <summary>
        /// Dashboard snapshot: latest reading + device state + threshold alerts.
        /// Pure cache — no DB hit.
        /// </summary>
        Task<DashboardDataDto> GetDashboardDataAsync();

        /// <summary>
        /// Current fire state for GET /api/sensor/fire/status
        ///
        /// Called ONCE on frontend initial load.
        /// No deviceId param — backend reads it from MqttSettings.
        /// Returns: { fireAlarm: 0 } or { fireAlarm: 1 }
        /// </summary>
        Task<FireStatusDto> GetFireStatusAsync();
    }
}
