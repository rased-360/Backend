using Microsoft.Extensions.Logging;
using RaSed.Application.DTOs.Notify_an_Issue;
using RaSed.Application.Interfaces;
using RaSed.Application.Interfaces.Realtime;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services
{
    public class IssueService : IIssueService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly IRealtimeNotificationService _notificationService;
        private readonly ILogger<IssueService> _logger;

        public IssueService(
            IUnitOfWork unitOfWork,
            ICloudinaryService cloudinaryService,
            IRealtimeNotificationService notificationService,
            ILogger<IssueService> logger)
        {
            _unitOfWork = unitOfWork;
            _cloudinaryService = cloudinaryService;
            _notificationService = notificationService;
            _logger = logger;
        }

        /// <summary>
        /// Creates a new issue and sends real-time notification to admin
        /// </summary>
        public async Task<IssueResponseDto> CreateIssueAsync(CreateIssueDto createIssueDto, int employeeId)
        {
            try
            {
                _logger.LogInformation("📝 Creating new issue for employee: {EmployeeId}", employeeId);

                // Get employee with section details
                var employee = await _unitOfWork._employeeRepository.GetEmployeeWithSectionAsync(employeeId);

                if (employee == null)
                {
                    _logger.LogError("❌ Employee not found: {EmployeeId}", employeeId);
                    throw new Exception("Employee not found");
                }

                // Upload image to Cloudinary if provided
                string? imageUrl = null;
                string? imagePublicId = null;

                if (createIssueDto.Image != null)
                {
                    try
                    {
                        _logger.LogInformation("📤 Uploading issue image to Cloudinary for employee: {EmployeeId}", employeeId);
                        imageUrl = await _cloudinaryService.UploadImageAsync(createIssueDto.Image, "issues");

                        // Extract public ID from URL for future deletion
                        var urlParts = imageUrl.Split('/');
                        if (urlParts.Length > 0)
                        {
                            var fileNameWithExtension = urlParts[^1];
                            var fileName = fileNameWithExtension.Split('.')[0];
                            imagePublicId = $"issues/{fileName}";
                        }

                        _logger.LogInformation("✅ Image uploaded successfully: {ImageUrl}", imageUrl);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to upload image for issue");
                        throw new Exception($"Image upload failed: {ex.Message}");
                    }
                }

                // Create issue entity
                var issue = new Issue
                {
                    Title = createIssueDto.Title,
                    Description = createIssueDto.Description,
                    ImageUrl = imageUrl,
                    EmployeeId = employeeId,
                    ReportedAt = DateTime.UtcNow
                };

                // Save to database
                await _unitOfWork._issueRepository.AddAsync(issue);
                await _unitOfWork.SaveChangesAsync();

                _logger.LogInformation("✅ Issue created successfully with ID: {IssueId}", issue.Id);

                // Prepare response DTO
                var responseDto = new IssueResponseDto
                {
                    Id = issue.Id,
                    Title = issue.Title,
                    Description = issue.Description,
                    ImageUrl = issue.ImageUrl,
                    ReportedAt = issue.ReportedAt,
                    EmployeeName = employee.FullName,
                    EmployeePhone = employee.PhoneNumber,
                    SectionName = employee.Section.Name
                };

                // Send real-time notification to admin desktop via SignalR
                var notification = new IssueNotificationPreviewDto
                {
                    IssueId = issue.Id,
                    Title = issue.Title,
                    ReportedAt = issue.ReportedAt,
                    EmployeeName = employee.FullName,
                    SectionName = employee.Section.Name
                };

                // Send notification immediately and asynchronously
                _ = Task.Run(async () =>
                {
                    try
                    {
                        await _notificationService.SendIssueNotificationAsync(notification);
                        _logger.LogInformation("📢 Notification sent successfully for Issue ID: {IssueId}", issue.Id);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "❌ Failed to send notification for Issue ID: {IssueId}", issue.Id);
                    }
                });

                return responseDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "❌ Error creating issue for employee: {EmployeeId}", employeeId);
                throw;
            }
        }

        /// Gets all issues with full details
        /// Ordered by most recent first for desktop notification display
        public async Task<IEnumerable<IssueNotificationPreviewDto>> GetAllIssuesAsync()
        {
            var issues = await _unitOfWork._issueRepository.GetAllIssuesAsync();
            return issues.Select(MapToPreviewDto).ToList();
        }

        /// Gets issue by ID with full details
        /// Used when admin clicks on a notification
        public async Task<IssueResponseDto?> GetIssueByIdAsync(int id)
        {
            var issue = await _unitOfWork._issueRepository.GetIssueWithDetailsAsync(id);

            if (issue == null)
                return null;

            return MapToResponseDto(issue);
        }

        /// <summary>
        /// Marks an issue as read.
        /// Called automatically when admin views issue details.
        /// </summary>
        public async Task<bool> MarkIssueAsReadAsync(int issueId)
        {
            var success = await _unitOfWork._issueRepository.MarkAsReadAsync(issueId);

            if (success)
            {
                await _unitOfWork.SaveChangesAsync();
                _logger.LogInformation("✅ Issue {IssueId} marked as read", issueId);
            }

            return success;
        }


        #region
        /// Maps Issue entity to IssueNotificationPreviewDto (minimal data)
        private IssueNotificationPreviewDto MapToPreviewDto(Issue issue)
        {
            return new IssueNotificationPreviewDto
            {
                IssueId = issue.Id,
                Title = issue.Title,
                ReportedAt = issue.ReportedAt,
                EmployeeName = issue.Employee.FullName,
                SectionName = issue.Employee.Section.Name
            };
        }
        /// Maps Issue entity to IssueResponseDto
        private IssueResponseDto MapToResponseDto(Issue issue)
        {
            return new IssueResponseDto
            {
                Id = issue.Id,
                Title = issue.Title,
                Description = issue.Description,
                ImageUrl = issue.ImageUrl,
                ReportedAt = issue.ReportedAt,
                EmployeeName = issue.Employee.FullName,
                EmployeePhone = issue.Employee.PhoneNumber,
                SectionName = issue.Employee.Section.Name
            };
        }
        #endregion
    }
}
