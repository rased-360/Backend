using RaSed.Application.DTOs.EmployeeDashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    /// <summary>
    /// Service for Employee Dashboard (Admin Desktop).
    /// Shows employees in a section with performance metrics and violations.
    /// </summary>
    public interface IEmployeeDashboardService
    {
        /// <summary>
        /// Gets employees in a section for dashboard display.
        /// Sorted by most recent violation first.
        /// </summary>
        /// <param name="sectionId">Section to filter employees</param>
        /// <param name="searchTerm">Optional search by employee name</param>
        /// <returns>List of employees with performance stats</returns>
        Task<IEnumerable<EmployeeDashboardDto>> GetSectionEmployeesAsync(
            int sectionId,
            string? searchTerm = null);
    }
}
