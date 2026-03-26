using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class Employee : ApplicationUser
    {
        public int SectionId { get; set; }

        // Navigation Property
        public Section Section { get; set; } = null!;

        public ICollection<Issue> Issues { get; set; } = new List<Issue>();
        public ICollection<Violation> Violations { get; set; } = new List<Violation>();
        public double PerformanceRate { get; set; } = 100.0;

        /// <summary>
        /// Human-readable tier derived from PerformanceRate:
        ///   ≥ 90 → "Excellent"
        ///   ≥ 75 → "VeryGood"
        ///   ≥ 50 → "Good"
        ///   else → "Bad"
        ///
        /// Stored as a string to avoid a round-trip calculation on every read.
        /// Default is "Excellent" matching the default PerformanceRate of 100.
        /// </summary>
        public string PerformanceRating { get; set; } = "Excellent";
        public DateTime? PerformanceLastUpdatedAt { get; set; }
    }
}
