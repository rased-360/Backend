using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    /// <summary>
    /// Represents a single occupational safety violation detected by the AI camera model.
    /// One AI payload can produce multiple Violation rows (one per employee in the frame).
    /// </summary>
    public class Violation
    {
        public int Id { get; set; }

        /// <summary>UTC timestamp reported by the AI model (not server receive time)</summary>
        public DateTime Timestamp { get; set; }

        /// <summary>Cloudinary URL of the frame snapshot captured at violation time</summary>
        public string ImageUrl { get; set; }

        /// <summary>e.g. "MISSING_VEST", "MISSING_HELMET", "MISSING_GLOVES"</summary>
        public string ViolationType { get; set; } = null!;

        public bool IsRead { get; set; }

        public int? EmployeeId { get; set; }

        /// <summary>Navigation — nullable because the employee might be deleted later</summary>
        public Employee? Employee { get; set; }
    }
}
