using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    /// <summary>
    /// Sent to the frontend via SignalR "ReceiveDeviceState"
    /// Only sent when state actually changes (0→1 or 1→0).
    /// </summary>
    public class DeviceStateDto
    {
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>1 = Fan running, 0 = Fan off</summary>
        public int Fan { get; set; }

        /// <summary>1 = Pump running, 0 = Pump off</summary>
        public int Pump { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
