using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    public interface IIdentityService
    {
        public Task<AdminAuthResult> LoginAsync(LoginDto dto, string ipAddress);
        public Task<AdminAuthResult> RefreshTokenAsync(string refreshToken, string ipAddress);
        public Task<bool> RevokeTokenAsync(string refreshToken, string ipAddress);
        public Task<AdminAuthResult> LogoutAsync(string refreshToken, string ipAddress);
    }
}
