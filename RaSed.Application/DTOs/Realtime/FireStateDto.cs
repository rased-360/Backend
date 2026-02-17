using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    /// <summary>
    /// 
    /// Stored in in-memory cache to track previous fire_alarm value.
    /// NOT sent to frontend — internal only.
    ///
    /// WHY: We compare old vs new fire_alarm to detect state changes:
    ///   cached=0, new=1 → Fire started!
    ///   cached=1, new=0 → Fire cleared!
    ///   same value      → Skip
    ///   
    /// </summary>
    public class FireStateDto
    {
        public string DeviceId { get; set; } = string.Empty;

        /// <summary>0 = No fire, 1 = Fire active</summary>
        public int FireAlarm { get; set; }

        /// <summary>Active DB event ID (if FireAlarm = 1), used to close the event later</summary>
        public int? ActiveEventId { get; set; }

        public DateTime Timestamp { get; set; }
    }
}
