using RaSed.Application.DTOs.Performance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    /// <summary>
    /// Business-logic contract for employee performance-rate calculations.
    /// </summary>
    public interface IPerformanceService
    {
        /// <summary>
        /// Calculates the performance rate for a single employee.
        ///
        /// Algorithm (all parameters come from PerformanceSettings):
        ///   1. Count violations for the employee in the last WindowDays days.
        ///   2. Rate = Max(0,  100 − (count × PenaltyPerViolation))
        ///   3. Assign a Rating tier based on the score.
        ///
        /// Returns <c>null</c> when the employee does not exist in the database.
        ///
        /// USED BY:
        ///   - GET /api/performance/me            (logged-in employee, mobile home)
        ///   - GET /api/performance/employee/{id} (admin view of any employee)
        /// </summary>
        /// <param name="employeeId">Database PK of the employee to evaluate.</param>
        Task<EmployeePerformanceDto?> GetEmployeePerformanceAsync(int employeeId);
    }
}
