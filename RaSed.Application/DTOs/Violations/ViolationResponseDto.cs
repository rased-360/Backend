using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Violations
{
    /// <summary>Full details for a single saved violation (admin detail view)</summary>
    public class ViolationResponseDto
    {
        public int Id { get; set; }
        public DateTime Timestamp { get; set; }
        public string? ImageUrl { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public int EmployeeId { get; set; }
        public string EmployeeName { get; set; } = string.Empty;
        public string EmployeePhone { get; set; } = string.Empty;
        public string SectionName { get; set; } = string.Empty;
    }
}
