using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Violations
{
    /// <summary>
    /// Lightweight preview pushed over SignalR to the admin desktop.
    /// Next sprint: also used to warn the employee's mobile app.
    /// </summary>
    public class ViolationNotificationDto
    {
        public int ViolationId { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
    }
}
