using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IEmployeeAuthService
    {
        public Task<EmployeeAuthResult> LoginAsync(LoginDto dto, string ipAddress);
        public Task<EmployeeAuthResult> RefreshTokenAsync(string refreshToken, string ipAddress);
        public Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        public Task<EmployeeAuthResult> LogoutAsync(string refreshToken, string ipAddress);
    }
}
