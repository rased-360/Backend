using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    /// <summary>
    /// 
    /// Returned by GET /api/sensor/fire/status
    ///
    /// Called ONCE on frontend initial load to know current fire state.
    /// After that, SignalR "ReceiveFireAlert" handles all future changes.
    ///
    /// Intentionally simple — frontend only needs 0 or 1.
    /// 
    /// </summary>
    public class FireStatusDto
    {
        /// <summary>
        /// 0 = No fire → normal dashboard
        /// 1 = Fire active → show emergency screen
        /// </summary>
        public int FireAlarm { get; set; }
    }
}
