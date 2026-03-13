using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Violations
{
    /// <summary>One employee → one violation type pair inside a detected payload</summary>
    public class ViolationEntryDto
    {
        [Required]
        public int EmployeeId { get; set; }

        [Required]
        [StringLength(100)]
        public string ViolationType { get; set; } = string.Empty;
    }
}
