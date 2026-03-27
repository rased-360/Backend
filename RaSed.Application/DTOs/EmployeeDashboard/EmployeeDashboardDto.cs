using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.EmployeeDashboard
{ /// <summary>
  /// Represents a single employee row in the Employee Dashboard table.
  /// Shows employee performance, violations, and profile info.
  /// Sorted by most recent violation first.
  /// </summary>
    public class EmployeeDashboardDto
    {
        /// <summary>Employee database ID</summary>
        public int EmployeeId { get; set; }

        /// <summary>Full name of the employee</summary>
        public string EmployeeName { get; set; } = string.Empty;

        /// <summary>Profile picture URL (for avatar display)</summary>
        public string? ProfilePictureUrl { get; set; }

        /// <summary>
        /// Performance rate (0-100).
        /// Formula: Max(0, 100 - (ViolationCount × 10))
        /// </summary>
        public double PerformanceRate { get; set; }

        /// <summary>
        /// Human-readable rating:
        ///   >= 90 → "Excellent"
        ///   >= 75 → "VeryGood"
        ///   >= 50 → "Good"
        ///   < 50  → "Bad"
        /// </summary>
        public string PerformanceRating { get; set; } = string.Empty;

        /// <summary>
        /// Timestamp of employee's most recent violation.
        /// Used for sorting (most recent violation first).
        /// Null if employee has no violations.
        /// </summary>
        public DateTime? LastViolationAt { get; set; }

        /// <summary>Section ID (for context)</summary>
        public int SectionId { get; set; }

        /// <summary>Section name (for display)</summary>
        public string SectionName { get; set; } = string.Empty;
    }
}
