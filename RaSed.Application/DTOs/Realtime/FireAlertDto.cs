using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Realtime
{
    public class FireAlertDto
    {
        /// "FireStarted" or "FireCleared"
        public string Type { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        /// "Active" or "Resolved"
        public string Status { get; set; } = string.Empty;
    }
}
