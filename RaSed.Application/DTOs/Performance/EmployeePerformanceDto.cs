using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Performance
{
    /// <summary>
    /// API response shape for a single employee's performance rate.
    /// Returned by:
    ///   GET /api/performance/me              (mobile home page)
    ///   GET /api/performance/employee/{id}   (admin view)
    /// </summary>
    public class EmployeePerformanceDto
    {
        /// <summary>Database PK of the employee.</summary>
        public int EmployeeId { get; set; }

        /// <summary>Full name — used as a greeting on the mobile home page.</summary>
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>
        /// Score in the range [0.00, 100.00].
        /// Formula: Max(0,  100 minus (ViolationCount x PenaltyPerViolation))
        /// </summary>
        public double PerformanceRate { get; set; }

        /// <summary>
        /// Human-readable tier:
        ///   >= 90 = Excellent
        ///   >= 75 = VeryGood
        ///   >= 50 = Good
        ///   else  = Bad
        /// </summary>
        public string Rating { get; set; } = string.Empty;

        /// <summary>
        /// Number of violations found inside the rolling window.
        /// Lets the mobile UI show "3 violations in the last 30 days".
        /// </summary>
        public int ViolationCount { get; set; }

        /// <summary>
        /// The window size used for this calculation (mirrors PerformanceSettings.WindowDays).
        /// Returned so the client can label the figure correctly without hard-coding it.
        /// </summary>
        public int WindowDays { get; set; }

        /// <summary>
        /// UTC moment when this snapshot was last calculated by the background job.
        /// When served from the live fallback (no snapshot yet) this equals DateTime.UtcNow.
        /// The mobile app can use this to show "last updated X hours ago".
        /// </summary>
        public DateTime LastCalculatedAt { get; set; }
    }
}
