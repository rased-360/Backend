using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Enums
{

    /// <summary>
    /// Notification priority levels for FCM.
    /// Maps to Android notification importance and iOS interruption level.
    /// </summary>

    public enum FcmNotificationPriority
    {
        /// <summary>Low priority — minimal interruption (e.g., informational updates)</summary>
        Low,

        /// <summary>Normal priority — standard notifications (e.g., task reminders)</summary>
        Normal,

        /// <summary>High priority — important but not urgent (e.g., shift updates)</summary>
        High,

        /// <summary>Critical priority — urgent, time-sensitive (e.g., fire, equipment failure)</summary>
        Critical
    }
}
