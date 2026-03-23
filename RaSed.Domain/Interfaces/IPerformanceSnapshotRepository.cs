using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IPerformanceSnapshotRepository
    {
        /// <summary>
        /// Returns the latest snapshot for one employee, or null if the
        /// background job has not run yet for this employee.
        /// </summary>
        Task<PerformanceSnapshot?> GetByEmployeeIdAsync(int employeeId);

        /// <summary>
        /// INSERT or UPDATE the snapshot for <paramref name="snapshot"/>.EmployeeId.
        ///
        /// Logic:
        ///   - If a row already exists for that EmployeeId → update all fields.
        ///   - If no row exists yet → insert a new row.
        ///
        /// Caller (UnitOfWork / background job) must call SaveChangesAsync()
        /// after this returns.
        /// </summary>
        Task UpsertAsync(PerformanceSnapshot snapshot);

        /// <summary>
        /// Returns the IDs of ALL active employees that the background job
        /// must process.  Only IDs are fetched — no heavy navigation data —
        /// so the query stays lightweight even with thousands of employees.
        /// </summary>
        Task<IEnumerable<int>> GetAllEmployeeIdsAsync();
    }
}
