using RaSed.Application.DTOs.Notify_an_Issue;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    public interface IIssueService
    {
        // Creates a new issue reported by an employee (Mobile app)
        // Automatically sends real-time notification to admin desktop via SignalR
        Task<IssueResponseDto> CreateIssueAsync(CreateIssueDto createIssueDto, int employeeId);

        // Gets all issues for desktop app notification list
        // Ordered by most recent first
        Task<IEnumerable<IssueNotificationPreviewDto>> GetAllIssuesAsync();

        // Gets issue by ID with full details (Desktop app - when notification is clicked)
        Task<IssueResponseDto?> GetIssueByIdAsync(int id);
    }
}
