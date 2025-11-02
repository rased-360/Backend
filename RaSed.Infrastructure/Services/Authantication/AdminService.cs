using Microsoft.AspNetCore.Identity;
using RaSed.Application.DTOs;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Enums;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Repositories;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Channels;
using System.Threading.Tasks;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Model;

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
        public async Task<AdminAuthResult> CreateAdminAsync(CreateAdminDto dto)
        {
            try
            {
                // Validation: Email
                if (await _unitOfWork._adminRepository.ExistsByEmailAsync(dto.Email))
                {
                    return AdminAuthResult.Failure("Email is already in use");
                }

                // Validation: NationalId
                if (await _unitOfWork._adminRepository.ExistsByNationalIdAsync(dto.NationalId))
                {
                    return AdminAuthResult.Failure("National ID is already in use");
                }

                // Validation: Phone
                if (await _unitOfWork._adminRepository.ExistsByPhoneAsync(dto.PhoneNumber))
                {
                    return AdminAuthResult.Failure("Phone number is already in use");
                }
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
                    return AdminAuthResult.Failure(errors, "Failed to create admin.");
                }

                await _userManager.AddToRoleAsync(admin, "Admin");

                var adminDto = new AdminResponseDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                };

                return AdminAuthResult.Success(adminDto, admin.IsSuperAdmin, admin.MustChangePassword, "Admin created successfully");
                
            }
            catch (InvalidOperationException ex)
            {
               return AdminAuthResult.Failure(ex.Message, "Failed to create admin.");
            }
            catch (Exception)
            {
                return AdminAuthResult.Failure("An unexpected error occurred while creating the admin. Please try again later.", null);
            }
        }

        //Get Admin by Id
        public async Task<AdminAuthResult?> GetAdminByIdAsync(int id)
        {
            try
            {
                var admin = await _unitOfWork._adminRepository.GetByIdAsync(id);
                if (admin == null)
                {
                    return AdminAuthResult.Failure("Admin not found.", null);
                }
                var result = new AdminResponseDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                };
                return AdminAuthResult.Success(result, admin.IsSuperAdmin, admin.MustChangePassword, "Admin created successfully");

            }
            catch (Exception ex)
            {
                return AdminAuthResult.Failure($"Something went wrong to gat this Admin data {id}", ex.Message);

            }
        }

        //Get Admin by Email
        public async Task<AdminAuthResult?> GetAdminByEmailAsync(string email)
        {
            try
            {
                var admin = await _unitOfWork._adminRepository.GetAdminByEmailAsync(email);
                var result = new AdminResponseDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                };
                return AdminAuthResult.Success(result, admin.IsSuperAdmin, admin.MustChangePassword, "Admin created successfully");

            }
            catch (Exception ex)
            {
                return AdminAuthResult.Failure($"Something went wrong to gat this Admin data {email}", ex.Message);
            }
        }

        //Get all Admins
        public async Task<PagedResult<AdminResponseDto>> GetAllAdminsAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // Validation
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100; 

                var (admins, totalCount) = await _unitOfWork._adminRepository.GetPagedAdminsAsync(page, pageSize);

                // تحويل للـ DTO
                var adminDtos = admins.Select(admin => new AdminResponseDto
                {
                    Id = admin.Id,
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    Gender = admin.Gender,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                });

                return new PagedResult<AdminResponseDto>(adminDtos, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong while getting admins data", ex);
            }
        }

        public async Task<AdminAuthResult> EditAdminAsync(int adminId, AdminEditDto dto)
        {
            try
            {
                var existingAdmin = await _unitOfWork._adminRepository.GetByIdAsync(adminId);

                if (existingAdmin == null)
                {
                    return AdminAuthResult.Failure("Admin not found.", null);
                }
                // Validate NationalId if changed
                if (existingAdmin.NationalId != dto.NationalId)
                {
                     if (await _unitOfWork._adminRepository.ExistsByNationalIdAsync(dto.NationalId))
                     {
                            return AdminAuthResult.Failure("National ID is already in use");
                     }
                }

                if (existingAdmin.IsSuperAdmin)
                {
                    return AdminAuthResult.Failure("Cannot Edit a SuperAdmin.", null);
                }

                if (existingAdmin.PhoneNumber != dto.PhoneNumber)
                {
                    if (await _unitOfWork._adminRepository.ExistsByPhoneAsync(dto.PhoneNumber))
                    {
                        return AdminAuthResult.Failure("Phone number is already in use");
                    }
                }
                existingAdmin.FullName = dto.FullName;
                existingAdmin.Email = dto.Email;
                existingAdmin.Gender = dto.Gender;
                existingAdmin.NationalId = dto.NationalId;
                existingAdmin.PhoneNumber = dto.PhoneNumber;
                existingAdmin.DateOfBirth = dto.DateOfBirth;
                existingAdmin.HireType = dto.HireType;

                var updateResult = await _userManager.UpdateAsync(existingAdmin);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Fail in updating data {errors}");
                }

                await _unitOfWork.SaveChangesAsync();

                var adminDto = new AdminResponseDto
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Gender = dto.Gender,
                    NationalId = dto.NationalId,
                    HireType = dto.HireType,
                    DateOfBirth = dto.DateOfBirth,


                };
                 return AdminAuthResult.Success(adminDto, existingAdmin.IsSuperAdmin, existingAdmin.MustChangePassword, "Admin updated successfully");

            }
            catch (Exception ex)
            {
                return AdminAuthResult.Failure("An unexpected error occurred while updating the admin. Please try again later.", ex.Message);
            }
        }
        

        //Delete Admin by Id
        public async Task<AdminAuthResult> DeleteAdminByIdAsync(int id)
        {
            var adminToDelete = await _unitOfWork._adminRepository.GetByIdAsync(id);

            if (adminToDelete == null)
            {
                return AdminAuthResult.Failure("Admin not found.", null);
            }

            if (adminToDelete.IsSuperAdmin)
            {
                return AdminAuthResult.Failure("Cannot delete a super admin.", null);
                throw new InvalidOperationException("Cannot delete a super admin.");

            }

            _unitOfWork._adminRepository.Delete(adminToDelete);
            await _unitOfWork.SaveChangesAsync();

            return AdminAuthResult.Success("Admin deleted successfully.");
        }


    }
}
