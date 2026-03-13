using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Violations
{
    /// <summary>
    /// Top-level payload the AI model sends when it detects one or more violations.
    /// One request can carry violations for multiple employees in the same frame.
    /// </summary>
    public class ViolationDetectedDto
    {
        [Required]
        public DateTime Timestamp { get; set; }

        /// <summary>URL of the captured frame (already uploaded by AI team, or empty)</summary>
        [Required(ErrorMessage = "ImageUrl is required")]
        public string ImageUrl { get; set; } = string.Empty;  // remove the ? too

        [Required]
        [MinLength(1, ErrorMessage = "At least one violation entry is required")]
        public List<ViolationEntryDto> Violations { get; set; } = new();
    }
}
