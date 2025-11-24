using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class AdminAuthResult
    {
        public bool IsSuccessful { get; set; }
        public List<string> Errors { get; set; } = new();
        public string Message { get; set; }
        public string AccessToken { get; set; }
        public string RefreshToken { get; set; }
        public bool IsSuperAdmin { get; set; }
        public bool MustChangePassword { get; set; }
        public AdminResponseDto? Admin { get; set; }
        public LoginResponse LoginResponse { get; set; }

        public static AdminAuthResult Success(string accessToken, string refreshToken, LoginResponse admin, bool isSuperAdmin, bool mustChangePassword, string message = null) => new AdminAuthResult
        {
            IsSuccessful = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            LoginResponse = admin,
            IsSuperAdmin = isSuperAdmin,
            MustChangePassword = mustChangePassword,
            Message = message
        };

        public static AdminAuthResult Success(string accessToken, string refreshToken, string message = null) => new AdminAuthResult
        {
            IsSuccessful = true,
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Message = message
        };

        public static AdminAuthResult Success(string message = null) => new AdminAuthResult
        {
            IsSuccessful = true,
            Message = message
        };
        public static AdminAuthResult Success(AdminResponseDto admin, bool isSuperAdmin, bool mustChangePassword, string message = null) => new AdminAuthResult
        {
            IsSuccessful = true,
            Admin = admin,
            IsSuperAdmin = isSuperAdmin,
            MustChangePassword = mustChangePassword,
            Message = message
        };
        public static AdminAuthResult Success(AdminResponseDto admin, string message = null) => new AdminAuthResult
        {
            IsSuccessful = true,
            Admin = admin,
            Message = message
        };
        public static AdminAuthResult Failure(List<string> errors, string message) => new AdminAuthResult
        {
            IsSuccessful = false,
            Errors = errors,
            Message = message
        };
        public static AdminAuthResult Failure(string error, string message = null) => new AdminAuthResult
        {
            IsSuccessful = false,
            Errors = new List<string> { error },
            Message = message
        };



    }
}
