using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    public class ChartDataDto
    {
        public List<ChartPointDto> TemperatureData { get; set; } = new();
        public List<ChartPointDto> HumidityData { get; set; } = new();
    }
}
