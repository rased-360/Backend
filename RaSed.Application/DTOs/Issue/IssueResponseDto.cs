using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Notify_an_Issue
{
    // DTO for issue response (full details for desktop app)
    public class IssueResponseDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime ReportedAt { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeePhone { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }
}
