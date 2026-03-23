using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Configuration
{
    /// <summary>
    /// Tunable parameters for the employee performance-rate feature.
    /// Bound from appsettings.json section "PerformanceSettings".
    /// All values can be changed in config without redeployment.
    /// </summary>
    public class PerformanceSettings
    {
        public const string SectionName = "PerformanceSettings";

        /// <summary>
        /// Rolling window in days — how far back to count violations.
        /// Default: 30 days.
        ///
        /// WHY rolling and not all-time?
        ///   All-time counts permanently punish employees for old violations.
        ///   A rolling window rewards improvement, which is a better safety KPI.
        /// </summary>
        public int WindowDays { get; set; } = 30;

        /// <summary>
        /// Points deducted from 100 per violation found in the window.
        /// Default: 10.
        ///
        /// Score examples with defaults:
        ///   0 violations = 100 = Excellent
        ///   1 violation  =  90 = Excellent
        ///   2 violations =  80 = VeryGood
        ///   3 violations =  70 = Good
        ///   5 violations =  50 = Good (boundary)
        ///   6 violations =  40 = Bad
        /// </summary>
        public double PenaltyPerViolation { get; set; } = 10;

        /// <summary>
        /// How many hours between background job runs.
        /// Default: 6 hours (4 snapshot refreshes per day).
        ///
        /// Set to 1  for near-realtime snapshots.
        /// Set to 24 for a nightly-only job.
        /// </summary>
        public int JobIntervalHours { get; set; } = 6;
    }
}
