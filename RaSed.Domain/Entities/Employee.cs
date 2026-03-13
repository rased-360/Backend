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
    }
}
