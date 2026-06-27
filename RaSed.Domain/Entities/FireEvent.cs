using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    /// <summary>
    /// Represents a fire detection event.
    /// Only stores actual fire events (when fire_alarm = 1), not every alert message.
    /// 
    /// WHY: We don't want to store thousands of "0" alerts.
    /// We only care about fire start → fire end → duration.
    /// </summary>
    public class FireEvent
    {
        public int Id { get; set; }

        public string DeviceId { get; set; } = string.Empty;

        // When fire was first detected (fire_alarm changed from 0 → 1)
        public DateTime StartTime { get; set; }

        // When fire was cleared (fire_alarm changed from 1 → 0). NULL if still active.
        public DateTime? EndTime { get; set; }

        public int? DurationSeconds { get; set; }

        public string Status { get; set; } 
    }
}