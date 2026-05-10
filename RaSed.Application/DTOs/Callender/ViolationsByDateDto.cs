using RaSed.Application.DTOs.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Callender
{
    /// <summary>
    /// Response for violations on a specific date.
    /// Used when employee clicks on a day in the calendar.
    /// </summary>
    public class ViolationsByDateDto
    {
        /// <summary>The date requested</summary>
        public DateTime Date { get; set; }

        /// <summary>Total violations on this date</summary>
        public int Count { get; set; }

        /// <summary>List of violations on this date</summary>
        public List<EmployeeViolationDto> Violations { get; set; } = new();
    }
}
