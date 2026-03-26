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
        /// KNOWN TRADE-OFF (Option B — event-driven update):
        ///   The rate only updates when a violation is ADDED or when the cleanup
        ///   job DELETES old violations.  If a violation naturally ages out of
        ///   the 30-day window between those two events, the score improvement
        ///   is deferred until the next event triggers a recalculation.
        ///   This is an accepted trade-off at graduation-project scale.
        /// </summary>
        public int WindowDays { get; set; } = 30;

        /// <summary>
        /// Points deducted from 100 per violation in the window.
        /// Default: 10.
        ///
        ///   0 violations = 100 = Excellent
        ///   1 violation  =  90 = Excellent
        ///   2 violations =  80 = VeryGood
        ///   3 violations =  70 = Good
        ///   5 violations =  50 = Good (boundary)
        ///   6+ violations <= 40 = Bad
        /// </summary>
        public double PenaltyPerViolation { get; set; } = 10;
    }
}
