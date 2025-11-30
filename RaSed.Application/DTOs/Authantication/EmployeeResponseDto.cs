using RaSed.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class EmployeeResponseDto
    {
        public string Email { get; set; }
        public string FullName { get; set; }
        public string InitialPassword { get; set; }
        public string PhoneNumber { get; set; }
        public string NationalId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool MustChangePassword { get; set; }
        public DateTime? PasswordChangedAt { get; set; }
        public DateTime? LastLogin { get; set; }
    }
}
