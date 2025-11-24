using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class AdminQueryDto
    {
        // Pagination
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;

        // Search
        public string? SearchTerm { get; set; } // Global search (Name, National ID)

        // Filter
        public bool? IsActive { get; set; } // null = all, true = active only, false = inactive only

        // Sort 
        public string? SortOrder { get; set; } = "desc"; // LastLogin
        
    }
}
