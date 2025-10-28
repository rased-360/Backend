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

        //Create Admin
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

        //Get Admin by Id
        public async Task<AdminResponseDto?> GetAdminByIdAsync(int id)
        {
            try
            {
                var admin = await _unitOfWork._adminReposatory.GetByIdAsync(id);
                var result = new AdminResponseDto
                {
                    Success = true,
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsSuperAdmin = admin.IsSuperAdmin,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                };
                return admin != null ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong to gat this Admin data {id}", ex);
            }
        }

        //Get Admin by Email
        public async Task<AdminResponseDto?> GetAdminByEmailAsync(string email)
        {
            try
            {
                var admin = await _unitOfWork._adminReposatory.GetAdminByEmailAsync(email);
                var result = new AdminResponseDto
                {
                    Success = true,
                    Message = "Admin created successfully.",
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsSuperAdmin = admin.IsSuperAdmin,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                };
                return admin != null ? result : null;
            }
            catch (Exception ex)
            {
                throw new Exception($"Something went wrong to gat this Admin data {email}", ex);
            }
        }

        //Get all Admins
        public async Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync()
        {
            try
            {
                var allAdmins = await _unitOfWork._adminReposatory.GetAllAsync();

            var admins = allAdmins.Where(admin =>!admin.IsSuperAdmin);

            return admins.Select(admins => new AdminResponseDto
            {
                Success = true,
                Message = "Admin created successfully.",
                Id = admins.Id,
                Email = admins.Email,
                FullName = admins.FullName,
                PhoneNumber = admins.PhoneNumber,
                Gender = admins.Gender,
                NationalId = admins.NationalId,
                IsSuperAdmin = admins.IsSuperAdmin,
                IsActive = admins.IsActive,
                CreatedAt = admins.CreatedAt,
            });
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong to gat all Admins data", ex);
            }

        }

        public async Task<AdminEditResponsDto> EditAdminAsync(int adminId, AdminEditDto editDto)
        {
            try
            {
                var existingAdmin = await _unitOfWork._adminReposatory.GetByIdAsync(adminId);

                if (existingAdmin == null)
                {
                    throw new KeyNotFoundException("Admin not found.");
                }

                if (existingAdmin.IsSuperAdmin)
                {
                    throw new UnauthorizedAccessException("Cannot Edit a SuperAdmin.");
                }

                existingAdmin.FullName = editDto.FullName;
                existingAdmin.Email = editDto.Email;
                existingAdmin.Gender = editDto.Gender;
                existingAdmin.NationalId = editDto.NationalId;
                existingAdmin.PhoneNumber = editDto.PhoneNumber;
                existingAdmin.DateOfBirth = editDto.DateOfBirth;
                existingAdmin.HireType = editDto.HireType;

                var updateResult = await _userManager.UpdateAsync(existingAdmin);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Fail in updating data {errors}");
                }

                await _unitOfWork.SaveChangesAsync();

                var result = new AdminEditResponsDto
                {
                    Success = true,
                    Message = "Admin created successfully.",
                    Email = editDto.Email,
                    FullName = editDto.FullName,
                    PhoneNumber = editDto.PhoneNumber,
                    Gender = editDto.Gender,
                    NationalId = editDto.NationalId,

                };
                return result;
            }
            catch (Exception ex)
            {
                throw new Exception("Fail in updating data", ex);
            }
        }
        

        //Delete Admin by Id
        public async Task<bool> DeleteAdminByIdAsync(int id)
        {
            var adminToDelete = await _unitOfWork._adminReposatory.GetByIdAsync(id);

            if (adminToDelete == null)
            {
                throw new KeyNotFoundException("Admin not found.");
            }

            if (adminToDelete.IsSuperAdmin)
            {
                throw new InvalidOperationException("Cannot delete a super admin.");

            }

            _unitOfWork._adminReposatory.Delete(adminToDelete);
            await _unitOfWork.SaveChangesAsync();

            return true;
        }


    }
}
