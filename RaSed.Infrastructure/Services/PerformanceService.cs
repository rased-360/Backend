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
        private readonly PerformanceSettings _settings;
        private readonly ILogger<PerformanceService> _logger;

        public PerformanceService(
            IUnitOfWork unitOfWork,
            IOptions<PerformanceSettings> settings,
            ILogger<PerformanceService> logger)
        {
            _unitOfWork = unitOfWork;
            _settings = settings.Value;
            _logger = logger;
        }

        // ── Mobile / single-employee endpoint ─────────────────────────────────

        /// <inheritdoc/>
        public async Task<EmployeePerformanceDto?> GetEmployeePerformanceAsync(int employeeId)
        {
            // ── Step 1: try the snapshot table first ──────────────────────────
            // One indexed lookup — no COUNT, no JOIN to violations.
            var snapshot = await _unitOfWork._performanceSnapshotRepository
                .GetByEmployeeIdAsync(employeeId);

            if (snapshot != null)
            {
                _logger.LogInformation(
                    "📊 Performance served from snapshot — Employee: {EmployeeId}, " +
                    "Rate: {Rate}, Rating: {Rating}",
                    employeeId, snapshot.PerformanceRate, snapshot.Rating);

                // Fetch the name separately (snapshot table stores only the ID).
                var emp = await _unitOfWork._employeeRepository
                    .GetEmployeeWithSectionAsync(employeeId);

                return MapSnapshotToDto(snapshot, emp?.FullName ?? "Unknown");
            }

            // ── Step 2: fallback — calculate live ─────────────────────────────
            // Reached only when the background job has never run for this employee
            // (e.g. fresh deployment, or new employee added after last job run).
            _logger.LogWarning(
                "⚠️ No snapshot found for Employee {EmployeeId} — calculating live " +
                "(background job may not have run yet)", employeeId);

            return await CalculateLiveAsync(employeeId);
        }

        // ── Private helpers ───────────────────────────────────────────────────

        /// <summary>
        /// Calculates the performance rate directly from the violations table.
        /// Used as a fallback when no snapshot exists yet.
        /// Identical algorithm to what PerformanceSnapshotJob uses.
        /// </summary>
        private async Task<EmployeePerformanceDto?> CalculateLiveAsync(int employeeId)
        {
            var employee = await _unitOfWork._employeeRepository
                .GetEmployeeWithSectionAsync(employeeId);

            if (employee == null)
                return null;

            var windowStart = DateTime.UtcNow.AddDays(-_settings.WindowDays);
            var violationCount = await _unitOfWork._violationRepository
                .CountViolationsByEmployeeInWindowAsync(employeeId, windowStart);

            var rate = Math.Max(0, 100 - (violationCount * _settings.PenaltyPerViolation));

            return new EmployeePerformanceDto
            {
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                PerformanceRate = Math.Round(rate, 2),
                Rating = ResolveRating(rate),
                ViolationCount = violationCount,
                WindowDays = _settings.WindowDays,
                LastCalculatedAt = DateTime.UtcNow
            };
        }

        /// <summary>Maps a stored snapshot entity to the API response DTO.</summary>
        private static EmployeePerformanceDto MapSnapshotToDto(
            PerformanceSnapshot snapshot,
            string employeeName)
        {
            return new EmployeePerformanceDto
            {
                EmployeeId = snapshot.EmployeeId,
                EmployeeName = employeeName,
                PerformanceRate = snapshot.PerformanceRate,
                Rating = snapshot.Rating,
                ViolationCount = snapshot.ViolationCount,
                WindowDays = snapshot.WindowDays,
                LastCalculatedAt = snapshot.LastCalculatedAt
            };
        }

        private static string ResolveRating(double rate) => rate switch
        {
            >= 90 => "Excellent",
            >= 75 => "VeryGood",
            >= 50 => "Good",
            _ => "Bad"
        };
    }
}
