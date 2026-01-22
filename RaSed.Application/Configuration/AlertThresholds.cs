using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Configuration
{
    public class AlertThresholds
    {
        public decimal TemperatureHigh { get; set; }
        public decimal TemperatureLow { get; set; }
        public int HumidityHigh { get; set; }
        public int HumidityLow { get; set; }
        public int HydrogenHigh { get; set; }
        public int PressureHigh { get; set; }
        public int PressureLow { get; set; }
    }
}
