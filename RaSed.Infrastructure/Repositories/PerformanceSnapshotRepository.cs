using Microsoft.EntityFrameworkCore;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;

namespace RaSed.Infrastructure.Repositories
{
    public class PerformanceSnapshotRepository : IPerformanceSnapshotRepository
    {
        private readonly AppDbContext _context;

        public PerformanceSnapshotRepository(AppDbContext context)
        {
            _context = context;
        }

        // ── Read operations ───────────────────────────────────────────────────

        /// <summary>
        /// Returns the snapshot for one employee (no navigation loaded —
        /// the mobile app only needs the scalar fields).
        /// Returns null if the job has never run for this employee yet.
        /// </summary>
        public async Task<PerformanceSnapshot?> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.PerformanceSnapshots
                .FirstOrDefaultAsync(p => p.EmployeeId == employeeId);
        }

        /// <summary>
        /// Projects only the EmployeeId column — no entity materialisation.
        /// The background job uses this to build its work list without
        /// loading any other employee data.
        /// </summary>
        public async Task<IEnumerable<int>> GetAllEmployeeIdsAsync()
        {
            return await _context.Employees
                .Select(e => e.Id)
                .ToListAsync();
        }

        // ── Write operations ──────────────────────────────────────────────────

        /// <summary>
        /// Inserts a new snapshot row or updates an existing one.
        ///
        /// HOW IT WORKS:
        ///   1. Try to find the existing row by EmployeeId.
        ///   2a. If found   → copy the new field values onto the tracked entity.
        ///                    EF will generate UPDATE on SaveChanges.
        ///   2b. If not found → Add the new snapshot object.
        ///                    EF will generate INSERT on SaveChanges.
        ///
        /// SaveChanges is NOT called here — the caller (background job via its
        /// own DbContext scope) is responsible, keeping transaction control
        /// outside the repository as per the UnitOfWork pattern.
        /// </summary>
        public async Task UpsertAsync(PerformanceSnapshot snapshot)
        {
            var existing = await _context.PerformanceSnapshots
                .FirstOrDefaultAsync(p => p.EmployeeId == snapshot.EmployeeId);

            if (existing != null)
            {
                // UPDATE path — mutate the tracked entity so EF generates
                // a targeted UPDATE statement (only changed columns).
                existing.PerformanceRate = snapshot.PerformanceRate;
                existing.Rating = snapshot.Rating;
                existing.ViolationCount = snapshot.ViolationCount;
                existing.WindowDays = snapshot.WindowDays;
                existing.LastCalculatedAt = snapshot.LastCalculatedAt;
            }
            else
            {
                // INSERT path — let EF assign the PK on SaveChanges.
                await _context.PerformanceSnapshots.AddAsync(snapshot);
            }
        }
    }
}
