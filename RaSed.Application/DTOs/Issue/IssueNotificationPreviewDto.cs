using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Notify_an_Issue
{
    // DTO for SignalR notification preview (shows in notification list)
    // Contains minimal info to display in notification card
    public class IssueNotificationPreviewDto
    {
        public int IssueId { get; set; }
        public string Title { get; set; } = string.Empty;
        public DateTime ReportedAt { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }
}
