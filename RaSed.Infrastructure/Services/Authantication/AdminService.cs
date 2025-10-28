using Microsoft.AspNetCore.Identity;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Enums;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class AdminService : IAdminService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;


        public AdminService(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            this._unitOfWork = _unitOfWork;
            this._userManager = _userManager;
        }

        public async Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminDto dto)
        {
            try
            {
                var admin = new Admin
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Gender = dto.Gender,
                    NationalId = dto.NationalId,
                    DateOfBirth = dto.DateOfBirth,
                    HireType = dto.HireType,
                    IsSuperAdmin = false,
                    IsActive = true,
                    MustChangePassword = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                };

                // create admin
                var result = await _userManager.CreateAsync(admin, dto.Password);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Fail to create the admin: {errors}");
                }

                await _userManager.AddToRoleAsync(admin, "Admin");

                return new CreateAdminResponseDto
                {
                    Success = true,
                    Message = "Admin created successfully.",
                    Id = admin.Id,
                    Email = admin.Email,
                    generatePassword = dto.Password,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsSuperAdmin = admin.IsSuperAdmin,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                };
            }
            catch (InvalidOperationException ex)
            {
                throw new InvalidOperationException($"Fail: {ex.Message}");
            }
            catch (Exception)
            {
                throw new Exception("An unexpected error occurred while creating the admin. Please try again later.");
            }
        }
    }
}
