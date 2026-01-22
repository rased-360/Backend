using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class SensorReading
    {
        public int Id { get; set; }
        public int Hydrogen { get; set; }
        public int Ethanol { get; set; }
        public decimal Temperature { get; set; } //(°C)
        public decimal Humidity { get; set; } //(%)
        public decimal Pressure { get; set; }//(hPa)

        public decimal HeatIndex { get; set; }
        public DateTime Timestamp { get; set; }

        // Processed data properties
        public string? AlertType { get; set; }
        public string? AlertMessage { get; set; }
        public bool HasAlert { get; set; }
    }
}
