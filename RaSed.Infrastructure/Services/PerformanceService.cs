using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Application.DTOs.Performance;
using RaSed.Application.Interfaces;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services
{
    public class PerformanceService : IPerformanceService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<PerformanceService> _logger;

        public PerformanceService(
            IUnitOfWork unitOfWork,
            ILogger<PerformanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<EmployeePerformanceDto?> GetEmployeePerformanceAsync(int employeeId)
        {
            // One indexed lookup — reads the pre-calculated columns from the Employee row.
            // No COUNT query. No math. No fallback path needed.
            var employee = await _unitOfWork._employeeRepository
                .GetEmployeeWithSectionAsync(employeeId);

            if (employee == null)
            {
                _logger.LogWarning(
                    "⚠️ Performance requested for unknown employee {EmployeeId}", employeeId);
                return null;
            }

            _logger.LogInformation(
                "📊 Performance served — Employee: {EmployeeId} | Rate: {Rate} | Rating: {Rating}",
                employee.Id, employee.PerformanceRate, employee.PerformanceRating);

            return new EmployeePerformanceDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                PerformanceRate = employee.PerformanceRate,
                Rating = employee.PerformanceRating,

                // ViolationCount and WindowDays are not stored on the Employee row.
                // They are not needed for the mobile home screen display.
                // If needed in the future, add them as columns (same pattern as PerformanceRate).
                ViolationCount = 0,
                WindowDays = 30, // informational only — matches appsettings default

                LastCalculatedAt = employee.PerformanceLastUpdatedAt ?? DateTime.UtcNow
            };
        }
    }
}
