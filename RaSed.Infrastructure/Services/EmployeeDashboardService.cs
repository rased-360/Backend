using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.EmployeeDashboard;
using RaSed.Application.Interfaces;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services
{
    public class EmployeeDashboardService : IEmployeeDashboardService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<EmployeeDashboardService> _logger;

        public EmployeeDashboardService(
            IUnitOfWork unitOfWork,
            ILogger<EmployeeDashboardService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <summary>
        /// Gets employees in a section for dashboard display.
        /// Sorted by most recent violation first (worst performers at top).
        /// </summary>
        public async Task<IEnumerable<EmployeeDashboardDto>> GetSectionEmployeesAsync(
            int sectionId,
            string? searchTerm = null)
        {
            _logger.LogInformation(
                "📊 Fetching dashboard for Section {SectionId}, Search: {Search}",
                sectionId, searchTerm ?? "None");

            // Get employees with their violation data
            var employees = await _unitOfWork._employeeRepository
                .GetEmployeesBySectionFilteredAsync(sectionId, searchTerm);

            var employeeList = employees.ToList();

            if (employeeList.Count == 0)
            {
                _logger.LogInformation(
                    "⚠️ No employees found in Section {SectionId}", sectionId);
                return Enumerable.Empty<EmployeeDashboardDto>();
            }

            // Map to DTOs
            var dashboard = employeeList.Select(e => new EmployeeDashboardDto
            {
                EmployeeId = e.Id,
                EmployeeName = e.FullName,
                ProfilePictureUrl = e.ProfilePictureUrl,
                PerformanceRate = e.PerformanceRate,
                PerformanceRating = e.PerformanceRating,

                // Last violation timestamp (for sorting)
                LastViolationAt = e.Violations
                    .OrderByDescending(v => v.Timestamp)
                    .FirstOrDefault()?.Timestamp,

                SectionId = e.SectionId,
                SectionName = e.Section.Name
            })
            // ✅ SORT: Most recent violation first (worst performers at top)
            .OrderByDescending(dto => dto.LastViolationAt ?? DateTime.MinValue)
            .ToList();

            _logger.LogInformation(
                "✅ Dashboard loaded — Section: {SectionId} | Employees: {Count}",
                sectionId, dashboard.Count);

            return dashboard;
        }

    }
}
