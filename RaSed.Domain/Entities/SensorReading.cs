using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    /// <summary>
    /// Raw sensor reading - NOT stored in DB anymore (only in cache).
    /// Kept as a domain model for in-memory processing.
    /// </summary>
    public class SensorReading
    {
        public int Id { get; set; }

        // Gas sensors
        public int Hydrogen { get; set; }
        public int Ethanol { get; set; }

        // Environmental
        public decimal Temperature { get; set; }  // °C
        public decimal Humidity { get; set; }      // %
        public decimal Pressure { get; set; }      // hPa

        // Air quality
        public decimal Pm2_5 { get; set; }
        public decimal Pm1_0 { get; set; }
        public int Tvoc { get; set; }
        public int Eco2 { get; set; }

        // Computed
        public decimal HeatIndex { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
