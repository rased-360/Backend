using RaSed.Application.DTOs.Violations;
using RaSed.Domain.Entities;
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

        /// <summary>
        /// Deletes violations older than the retention period.
        /// Called by ViolationCleanupService on a schedule.
        /// </summary>
        Task<int> DeleteOldViolationsAsync(int retentionDays = 60);


        /// <summary>
        /// Gets a single violation by ID.
        /// SECURITY: Employee can only view their own violations.
        /// </summary>
        Task<EmployeeViolationDto?> GetViolationByIdAsync(int id, int userId, bool isAdmin);

        /// <summary>
        /// Marks a violation as read.
        /// Called automatically when user views violation details.
        /// SECURITY: Employee can only mark their own violations.
        /// </summary>
        Task<bool> MarkViolationAsReadAsync(int violationId, int userId, bool isAdmin);

        /// <summary>
        /// Gets all violations for a specific employee.
        /// Used by admin to view employee violation history.
        /// </summary>
        Task<IEnumerable<EmployeeViolationDto>> GetViolationsByEmployeeIdAsync(int employeeId);
    }
}
