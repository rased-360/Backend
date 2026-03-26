using FirebaseAdmin.Messaging;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RaSed.Application.Configuration;
using RaSed.Application.DTOs.Notifications;
using RaSed.Application.DTOs.Violations;
using RaSed.Application.Interfaces;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services
{
    public class ViolationService : IViolationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly PerformanceSettings _settings;
        private readonly ILogger<ViolationService> _logger;

        public ViolationService(
            IUnitOfWork unitOfWork,
            IRealtimeNotificationService notificationService,
             IOptions<PerformanceSettings> settings,
            ILogger<ViolationService> logger)
        {
            _unitOfWork = unitOfWork;
            _notificationService = notificationService;
            _settings = settings.Value;
            _logger = logger;
        }

        /// <inheritdoc/>
        public async Task<IEnumerable<ViolationResponseDto>> ProcessViolationsAsync(ViolationDetectedDto dto)
        {
            _logger.LogInformation(
                "🚨 Processing {Count} violation(s) from AI model at {Timestamp}",
                dto.Violations.Count, dto.Timestamp);

            var savedViolations = new List<ViolationResponseDto>();

            foreach (var entry in dto.Violations)
            {
                try
                {
                    // Try to find the employee — violation is saved regardless
                    var employee = await _unitOfWork._employeeRepository
                        .GetEmployeeWithSectionAsync(entry.EmployeeId);

                    if (employee == null)
                    {
                        _logger.LogWarning(
                            "⚠️ Violation saved with unknown employee — EmployeeId {EmployeeId} not found in DB",
                            entry.EmployeeId);
                    }

                    var violation = new Violation
                    {
                        Timestamp = dto.Timestamp,
                        ImageUrl = dto.ImageUrl,
                        ViolationType = entry.ViolationType.ToUpperInvariant().Trim(),
                        EmployeeId = employee != null ? entry.EmployeeId : null
                    };

                    await _unitOfWork._violationRepository.AddAsync(violation);
                    await _unitOfWork.SaveChangesAsync();

                    _logger.LogInformation(
                        "✅ Violation saved — ID: {ViolationId}, Employee: {EmployeeId}, Type: {Type}",
                        violation.Id,
                        violation.EmployeeId?.ToString() ?? $"Unknown (sent ID: {entry.EmployeeId})",
                        violation.ViolationType);

                    // ── Recalculate performance immediately (NEW) ─────────────
                    if (employee != null)
                        await RecalculateAndPersistAsync(employee.Id);

                    var responseDto = MapToResponseDto(violation, employee, entry.EmployeeId);
                    savedViolations.Add(responseDto);

                    // ── Real-time notification to admin desktop ──────────────
                    // Capture loop variables explicitly before entering Task.Run
                    // to avoid closure bugs with foreach loop variables
                    var capturedViolation = violation;
                    var capturedEmployee = employee;
                    var capturedEntryId = entry.EmployeeId;

                    _ = Task.Run(async () =>
                    {
                        try
                        {
                            var notification = new ViolationNotificationDto
                            {
                                ViolationId = capturedViolation.Id,
                                ViolationType = capturedViolation.ViolationType,
                                Timestamp = capturedViolation.Timestamp,
                                EmployeeId = capturedEmployee?.Id ?? 0,
                                EmployeeName = capturedEmployee?.FullName
                                                    ?? $"Unknown (ID: {capturedEntryId})",
                                SectionName = capturedEmployee?.Section?.Name ?? "Unknown",
                                ImageUrl = capturedViolation.ImageUrl
                            };

                            await _notificationService.SendViolationNotificationAsync(notification);

                            _logger.LogInformation(
                                "📢 Violation notification sent — Violation ID: {ViolationId}",
                                capturedViolation.Id);

                            // ── NEXT SPRINT HOOK ─────────────────────────────
                            // TODO: Also call _notificationService.SendViolationWarningToEmployeeAsync(notification)
                            // That method will push a warning to the specific employee's mobile app
                            // using their EmployeeId as the SignalR user identifier or an FCM token.
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex,
                                "❌ Failed to send notification for Violation ID: {ViolationId}",
                                capturedViolation.Id);
                        }
                    });
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "❌ Error saving violation for Employee {EmployeeId}", entry.EmployeeId);
                    // Continue processing remaining violations in the batch
                }
            }

            return savedViolations;
        }


        /// <inheritdoc/>
        public async Task<int> DeleteOldViolationsAsync(int retentionDays = 60)
        {
            var cutoff = DateTime.UtcNow.AddDays(-retentionDays);
            var old = await _unitOfWork._violationRepository.GetViolationsOlderThanAsync(cutoff);
            var list = old.ToList();

            if (list.Count == 0)
            {
                _logger.LogInformation("🧹 Cleanup: no violations older than {Days} days", retentionDays);
                return 0;
            }

            // Collect distinct employeeIds BEFORE deleting — the rows will be gone after
            var affectedEmployeeIds = list
                .Where(v => v.EmployeeId.HasValue)
                .Select(v => v.EmployeeId!.Value)
                .Distinct()
                .ToList();

            await _unitOfWork._violationRepository.DeleteRangeAsync(list);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation(
                "🧹 Cleanup: deleted {Count} violation(s) older than {Days} days (cutoff: {Cutoff})",
                list.Count, retentionDays, cutoff);

            // Recalculate performance for employees whose violation window just shrank
            foreach (var employeeId in affectedEmployeeIds)
            {
                try { await RecalculateAndPersistAsync(employeeId); }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "❌ Performance recalc failed after cleanup for Employee {EmployeeId}", employeeId);
                }
            }

            if (affectedEmployeeIds.Count > 0)
                _logger.LogInformation(
                    "📊 Performance recalculated for {Count} employee(s) after cleanup.",
                    affectedEmployeeIds.Count);

            return list.Count;
        }


        /// <summary>
        /// Gets a single violation by ID.
        /// SECURITY: Employee can only view their own violations.
        /// </summary>
                public async Task<IEnumerable<EmployeeViolationDto>> GetViolationsByEmployeeIdAsync(int employeeId)
        {
            var violations = await _unitOfWork._violationRepository.GetViolationsByEmployeeIdAsync(employeeId);
            return violations
                .Select(v => new EmployeeViolationDto
                {
                    ViolationId   = v.Id,
                    Timestamp     = v.Timestamp,
                    ImageUrl      = v.ImageUrl,
                    ViolationType = v.ViolationType
                })
                .OrderByDescending(v => v.Timestamp)
                .ToList();
        }

        public async Task<EmployeeViolationDto?> GetViolationByIdAsync(int id, int userId, bool isAdmin)
        {
            var violation = await _unitOfWork._violationRepository.GetByIdWithDetailsAsync(id);
            if (violation == null) return null;
            if (!isAdmin && violation.EmployeeId != userId) return null;

            return new EmployeeViolationDto
            {
                ViolationId = violation.Id,
                Timestamp = violation.Timestamp,
                ImageUrl = violation.ImageUrl,
                ViolationType = violation.ViolationType
            };
        }

        /// <summary>
        /// Marks a violation as read.
        /// Called automatically when user views violation details.
        /// SECURITY: Employee can only mark their own violations.
        /// </summary>
        public async Task<bool> MarkViolationAsReadAsync(int violationId, int userId, bool isAdmin)
        {
            var success = await _unitOfWork._violationRepository.MarkAsReadAsync(violationId, userId, isAdmin);

            if (success)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("✅ Violation {ViolationId} marked as read by user {UserId}", violationId, userId);
            }

            return success;
        }

        // ── Mapping helpers ────────────────────────────────────────────────────
        /// <summary>
        /// Recalculates the performance rate for one employee and writes it to
        /// the three columns on the Employees table.
        ///
        /// ALGORITHM (same formula as previous sprints — only storage changed):
        ///   1. COUNT violations in rolling window  (pure COUNT(*), no entity load)
        ///   2. Rate = Max(0, 100 - count * PenaltyPerViolation)
        ///   3. ExecuteUpdateAsync — targeted single-row UPDATE, no SaveChanges needed
        /// </summary>
        private async Task<bool> RecalculateAndPersistAsync(int employeeId)
        {
            var windowStart = DateTime.UtcNow.AddDays(-_settings.WindowDays);
            var violationCount = await _unitOfWork._violationRepository
                .CountViolationsByEmployeeInWindowAsync(employeeId, windowStart);

            var rate = Math.Max(0, 100 - (violationCount * _settings.PenaltyPerViolation));
            var rating = ResolveRating(rate);
            var rowsAffected = await _unitOfWork._employeeRepository
                .UpdatePerformanceAsync(employeeId, Math.Round(rate, 2), rating, DateTime.UtcNow);

            if (rowsAffected > 0)
            {
                _logger.LogInformation(
                    "📊 Performance updated — Employee: {EmployeeId} | " +
                    "Violations in last {Days}d: {Count} | Score: {Rate} | Rating: {Rating}",
                    employeeId, _settings.WindowDays, violationCount, rate, rating);
                return true;
            }

            _logger.LogWarning("⚠️ Performance update — Employee {EmployeeId} not found", employeeId);
            return false;
        }

        private static string ResolveRating(double rate) => rate switch
        {
            >= 90 => "Excellent",
            >= 75 => "VeryGood",
            >= 50 => "Good",
            _ => "Bad"
        };

        /// <param name="v">The saved Violation entity</param>
        /// <param name="employee">Nullable — null when the AI sent an unknown employee ID</param>
        /// <param name="originalEmployeeId">
        ///     The raw ID sent by the AI team. Preserved in the response so the admin
        ///     can see which ID was unrecognised, even though EmployeeId is NULL in the DB.
        ///     Defaults to 0 when called from GetAll/GetById where the original sent ID is unavailable.
        /// </param>
        private static ViolationResponseDto MapToResponseDto(
            Violation v,
            Employee? employee,
            int originalEmployeeId = 0)
        {
            return new ViolationResponseDto
            {
                Id = v.Id,
                Timestamp = v.Timestamp,
                ImageUrl = v.ImageUrl,
                ViolationType = v.ViolationType,
                EmployeeId = v.EmployeeId ?? originalEmployeeId,
                EmployeeName = employee?.FullName
                                    ?? (originalEmployeeId > 0
                                        ? $"Unknown (ID: {originalEmployeeId})"
                                        : "Unknown"),
                EmployeePhone = employee?.PhoneNumber ?? string.Empty,
                SectionName = employee?.Section?.Name ?? "Unknown"
            };
        }
    }
}
