using Microsoft.AspNetCore.Identity;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class IdentityService : IIdentityService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        public IdentityService(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> userManager)
        {
            this._unitOfWork = _unitOfWork;
            _userManager = userManager;
        }

        public async Task<LoginResponseDto> LoginAsync(LoginDto dto)
        {

            try
            {

            var user = await _userManager.FindByEmailAsync(dto.Email);
            if (user == null || !await _userManager.CheckPasswordAsync(user, dto.Password))
                throw new UnauthorizedAccessException("The login information is incorrect.");


            // Generate Tokens


            user.LastLogin = DateTime.UtcNow;

            // ✅ Check if Admin
            var mustChangePassword = false;
            if (user is Admin admin)
            {
                mustChangePassword = admin.MustChangePassword;
            }

            return new LoginResponseDto
            {
                //    AccessToken = token,
                //    RefreshToken = refreshToken,
                MustChangePassword = mustChangePassword,
                Admin = user is Admin adm ? new AdminResponseDto
                {
                    Success = true,
                    Message = "Login successful.",
                    Id = adm.Id,
                    Email = adm.Email,
                    FullName = adm.FullName,
                    PhoneNumber = adm.PhoneNumber,
                    Gender = adm.Gender,
                    NationalId = adm.NationalId,
                    IsSuperAdmin = adm.IsSuperAdmin,
                    IsActive = adm.IsActive,
                    CreatedAt = adm.CreatedAt
                } : null
            };
            }
            catch (UnauthorizedAccessException ex)
            {
                // نرمي الاستثناء تاني أو نرجع null حسب السياسة
                throw new UnauthorizedAccessException(ex.Message);
            }
            catch (Exception ex)
            {
                // أي خطأ غير متوقع
                throw new Exception("An unexpected error occurred while logging in.", ex);
            }
        }

    }
}
