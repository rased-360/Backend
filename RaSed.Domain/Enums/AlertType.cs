using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Enums
{
    public enum AlertType
    {
        None = 0,
        TemperatureHigh = 1,
        TemperatureLow = 2,
        HumidityHigh = 3,
        HumidityLow = 4,
        HydrogenHigh = 5,
        PressureAbnormal = 6
    }
}
