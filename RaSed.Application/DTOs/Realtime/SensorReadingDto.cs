using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    public class SensorReadingDto
    {
            public decimal Temperature { get; set; }
            public decimal Humidity { get; set; }
            public decimal Pressure { get; set; }

            public int Hydrogen { get; set; }
            public int Ethanol { get; set; }

            public decimal HeatIndex { get; set; }
            public DateTime Timestamp { get; set; }
        

            public bool HasAlert { get; set; }
            public string? AlertType { get; set; }
            public string? AlertMessage { get; set; }


    }
}
