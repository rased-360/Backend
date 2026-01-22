using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Configuration
{
    public class MqttSettings
    {
        public string Broker { get; set; } = string.Empty;
        public int Port { get; set; }
        public string Topic { get; set; } = string.Empty;
        public string ClientIdPrefix { get; set; } = string.Empty;
    }
}
