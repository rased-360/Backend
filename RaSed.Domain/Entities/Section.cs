using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class Section
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation Property
        public ICollection<Employee> Employees { get; set; } = new List<Employee>();
    }
}
