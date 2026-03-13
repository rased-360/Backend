using RaSed.Application.DTOs.Violations;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    public interface IViolationService
    {
        /// <summary>
        /// Processes one AI detection payload:
        ///   1. Saves each violation row to the DB
        ///   2. Fires a SignalR notification per violation to admin desktop
        ///   (Next sprint: also notify the employee's mobile app)
        /// Returns all saved violation DTOs so the controller can respond.
        /// </summary>
        Task<IEnumerable<ViolationResponseDto>> ProcessViolationsAsync(ViolationDetectedDto dto);

        /// <summary>All violations — newest first — for admin list view</summary>
        Task<IEnumerable<ViolationResponseDto>> GetAllViolationsAsync();

        /// <summary>Single violation by ID for admin detail view</summary>
        Task<ViolationResponseDto?> GetViolationByIdAsync(int id);

        /// <summary>
        /// Deletes violations older than the retention period.
        /// Called by ViolationCleanupService on a schedule.
        /// </summary>
        Task<int> DeleteOldViolationsAsync(int retentionDays = 60);
    }
}
