using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    /// <summary>
    /// Pre-calculated performance snapshot for one employee.
    /// Written exclusively by <c>PerformanceSnapshotJob</c>.
    /// Read by <c>PerformanceService</c> to serve API responses.
    /// </summary>
    public class PerformanceSnapshot
    {
        public int Id { get; set; }

        /// <summary>
        /// FK to the Employee table.
        /// Unique constraint (see configuration) ensures one snapshot per employee.
        /// </summary>
        public int EmployeeId { get; set; }

        public Employee? Employee { get; set; }
        public double PerformanceRate { get; set; }

        /// <summary>
        /// Human-readable tier:  Excellent | VeryGood | Good | Bad
        /// Stored as a string so the API can return it without re-computing.
        /// </summary>
        public string Rating { get; set; } = string.Empty;

        /// <summary>
        /// Number of violations found inside the rolling window at the time
        /// this snapshot was last calculated.
        /// Stored so the mobile app can show "3 violations in the last 30 days"
        /// without a second query.
        /// </summary>
        public int ViolationCount { get; set; }

        /// <summary>
        /// The window size (in days) used for this snapshot.
        /// Mirrors PerformanceSettings.WindowDays at the time of calculation.
        /// Stored alongside the rate so the displayed label stays consistent
        /// even if the setting is later changed between job runs.
        /// </summary>
        public int WindowDays { get; set; }

        public DateTime LastCalculatedAt { get; set; }
    }
}
