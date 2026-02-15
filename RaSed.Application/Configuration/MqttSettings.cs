using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Configuration
{
    /// <summary>
    /// Bound from appsettings.json → "MqttSettings"
    /// </summary>
    public class MqttSettings
    {
        public string Broker { get; set; } = "test.mosquitto.org";
        public int Port { get; set; } = 1883;

        /// <summary>
        /// The hardware device ID used to build topic names:
        ///   rased/{DeviceId}/telemetry
        ///   rased/{DeviceId}/state
        ///   rased/{DeviceId}/alert
        /// </summary>
        public string DeviceId { get; set; } = "device_01";

        public string ClientIdPrefix { get; set; } = "DotNetClient";
    }
}
