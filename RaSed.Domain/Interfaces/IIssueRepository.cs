using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IIssueRepository : IGenericRepository<Issue>
    {
        // Gets all issues with employee and section details
        // Ordered by most recent first
        Task<IEnumerable<Issue>> GetAllIssuesWithDetailsAsync();

        // Gets issue by ID with related employee and section data
        Task<Issue?> GetIssueWithDetailsAsync(int id);
    }
}
