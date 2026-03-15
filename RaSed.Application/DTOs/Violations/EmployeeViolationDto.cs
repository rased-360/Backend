using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Violations
{
    public class EmployeeViolationDto
    {
        public int ViolationId { get; set; }
        public string ViolationType { get; set; } = string.Empty;
        public DateTime Timestamp { get; set; }
        public string? ImageUrl { get; set; }
    }
}
