using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class AggregatedSensorData
    {
        public int Id { get; set; }

        // Time
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }

        // Temperature
        public decimal AvgTemperature { get; set; }
        public decimal MinTemperature { get; set; }
        public decimal MaxTemperature { get; set; }

        // Humidity
        public decimal AvgHumidity { get; set; }
        public decimal MinHumidity { get; set; }
        public decimal MaxHumidity { get; set; }

        // Pressure
        public decimal AvgPressure { get; set; }
        public decimal MinPressure { get; set; }
        public decimal MaxPressure { get; set; }

        //  Hydrogen
        public int AvgHydrogen { get; set; }
        public int MinHydrogen { get; set; }
        public int MaxHydrogen { get; set; }

        //  Ethanol
        public int AvgEthanol { get; set; }
        public int MinEthanol { get; set; }
        public int MaxEthanol { get; set; }

        //Num of readings
        public int ReadingCount { get; set; }

        // Num of Alerts
        public int AlertCount { get; set; }
    }
}
