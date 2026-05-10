namespace RaSed.Application.DTOs.Callender
{   
    /// <summary>
    /// Represents a single day in the calendar that has violations.
    /// </summary>
    public class ViolationDayDto
    {
        /// <summary>Day of month (1-31)</summary>
        public int Day { get; set; }

        /// <summary>Full date (for frontend reference)</summary>
        public DateTime Date { get; set; }

        /// <summary>Total violations on this day</summary>
        public int ViolationCount { get; set; }

        /// <summary>
        /// Color indicator for calendar UI:
        /// - "red" = 3+ violations (high risk)
        /// - "yellow" = 1-2 violations (medium risk)
        /// - "green" = 0 violations (no violations - not included in response)
        /// </summary>
        public string ColorIndicator { get; set; } = string.Empty;
    }
}