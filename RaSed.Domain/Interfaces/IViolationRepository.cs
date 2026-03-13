using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IViolationRepository : IGenericRepository<Violation>
    {
        /// <summary>Gets violations older than the given date (used by cleanup service)</summary>
        Task<IEnumerable<Violation>> GetViolationsOlderThanAsync(DateTime cutoff);

        /// <summary>Bulk delete — more efficient than deleting one by one</summary>
        Task DeleteRangeAsync(IEnumerable<Violation> violations);

        /// <summary>All violations with employee + section, newest first</summary>
        Task<IEnumerable<Violation>> GetAllWithDetailsAsync();

        /// <summary>Single violation with full navigation properties</summary>
        Task<Violation?> GetByIdWithDetailsAsync(int id);
    }
}
