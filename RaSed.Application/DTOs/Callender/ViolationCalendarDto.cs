using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Callender
{
    /// <summary>
    /// Response for calendar view showing violations grouped by date.
    /// Used by Mobile app to display violation calendar for employee.
    /// </summary>
    public class ViolationCalendarDto
    {
        /// <summary>The month being displayed (e.g., 2025-04 for April 2025)</summary>
        public string Month { get; set; } = string.Empty;

        /// <summary>Year (e.g., 2025)</summary>
        public int Year { get; set; }

        /// <summary>Month number (1-12)</summary>
        public int MonthNumber { get; set; }

        /// <summary>Days in the month that have violations</summary>
        public List<ViolationDayDto> DaysWithViolations { get; set; } = new();
    }
}
