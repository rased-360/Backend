using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class EmployeeAuthResult
    {
        public bool IsSuccessful { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool MustChangePassword { get; set; }
        public EmployeeResponseDto? Employee { get; set; }

        public static EmployeeAuthResult Success(string accessToken, string refreshToken, EmployeeResponseDto employee, bool mustChangePassword, string message = null) => new EmployeeAuthResult
        {
            IsSuccessful = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Employee = employee,
            MustChangePassword = mustChangePassword,
            Message = message
        };

        public static EmployeeAuthResult Success(string message = null) => new EmployeeAuthResult
        {
            IsSuccessful = true,
            Message = message
        };
        public static EmployeeAuthResult Success(EmployeeResponseDto employee, bool mustChangePassword, string message = null) => new EmployeeAuthResult
        {
            IsSuccessful = true,
            Employee = employee,
            MustChangePassword = mustChangePassword,
            Message = message
        };
        public static EmployeeAuthResult Failure(List<string> errors, string message) => new EmployeeAuthResult
        {
            IsSuccessful = false,
            Errors = errors,
            Message = message
        };
        public static EmployeeAuthResult Failure(string error, string message = null) => new EmployeeAuthResult 
        {
            IsSuccessful = false,
            Errors = new List<string> { error },
            Message = message
        };

    }
}
