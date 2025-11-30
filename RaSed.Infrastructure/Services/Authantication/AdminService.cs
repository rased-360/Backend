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

                var generatedPassword = GenerateStrongPassword(8);

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
                    InitialPassword = generatedPassword,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true,
                };

                // create admin
                var result = await _userManager.CreateAsync(admin, generatedPassword);

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return AdminAuthResult.Failure(errors, "Failed to create admin.");
                }

                await _userManager.AddToRoleAsync(admin, "Admin");

                var adminDto = new AdminResponseDto
                {
                    Email = admin.Email,
                    FullName = admin.FullName,
                    InitialPassword = generatedPassword
                };

                return AdminAuthResult.Success(adminDto, "Admin created successfully");
                
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

                // Map to DTOs
                var adminDtos = admins.Select(admin => new AdminResponseDto
                {
                    Email = admin.Email,
                    InitialPassword = admin.InitialPassword,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                    MustChangePassword = admin.MustChangePassword,
                    PasswordChangedAt = admin.PasswordChangedAt,
                    LastLogin = admin.LastLogin
                });

                return new PagedResult<AdminResponseDto>(adminDtos, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong while getting admins data", ex);
            }
        }

        //Activate or Disactivate Admin
        public async Task<AdminAuthResult> ActivateOrDisactivateAdminAsync(int id, bool isActive)
        {
            try
            {
                var admin = await _unitOfWork._adminRepository.GetByIdAsync(id);
                if (admin == null)
                {
                    return AdminAuthResult.Failure("Admin not found.");
                }
                if (admin.IsSuperAdmin)
                {
                    return AdminAuthResult.Failure("Cannot change activation status of a super admin.", null);
                }
                admin.IsActive = isActive;
                _unitOfWork._adminRepository.Update(admin);
                await _unitOfWork.SaveChangesAsync();
                string status = isActive ? "activated" : "deactivated";
                return AdminAuthResult.Success($"Admin has been successfully {status}.");
            }
            catch (Exception ex)
            {
                return AdminAuthResult.Failure("An error occurred while updating admin status.", ex.Message);
            }
        }

        // Delete Multiple Admins by IDs
        public async Task<AdminAuthResult> DeleteAdminsByIdsAsync(List<int> ids)
        {
            // Validation
            if (ids == null || !ids.Any())
            {
                return AdminAuthResult.Failure("No admin IDs provided.", null);
            }

            // Remove duplicates
            ids = ids.Distinct().ToList();

            // Get all admins to delete
            var adminsToDelete = await _unitOfWork._adminRepository
                .GetAllByIdsAsync(a => ids.Contains(a.Id));

            // Check if all IDs exist
            if (adminsToDelete.Count() != ids.Count)
            {
                var foundIds = adminsToDelete.Select(a => a.Id).ToList();
                var notFoundIds = ids.Except(foundIds).ToList();
                return AdminAuthResult.Failure(
                    $"Some admins not found. Missing IDs: {string.Join(", ", notFoundIds)}",
                    null
                );
            }

            // Check for Super Admins
            var superAdmins = adminsToDelete.Where(a => a.IsSuperAdmin).ToList();
            if (superAdmins.Any())
            {
                var superAdminIds = string.Join(", ", superAdmins.Select(a => a.Id));
                return AdminAuthResult.Failure(
                    $"Cannot delete super admins. Super Admin IDs: {superAdminIds}",
                    null
                );
            }

            // Delete all admins
            foreach (var admin in adminsToDelete)
            {
                _unitOfWork._adminRepository.Delete(admin);
            }

            await _unitOfWork.SaveChangesAsync();

            return AdminAuthResult.Success(
                $"{adminsToDelete.Count()} admin(s) deleted successfully."
            );
        }


        // Get Filtered Admins with Search, Filter, Sort
        public async Task<PagedResult<AdminResponseDto>> GetFilteredAdminsAsync(QueryDto query)
        {
            try
            {
                // Validation
                if (query.Page < 1) query.Page = 1;
                if (query.PageSize < 1) query.PageSize = 10;
                if (query.PageSize > 100) query.PageSize = 100;

                var (admins, totalCount) = await _unitOfWork._adminRepository.GetFilteredAdminsAsync(
                        searchTerm: query.SearchTerm,
                        isActive: query.IsActive,
                        sortOrder: query.SortOrder,
                        page: query.Page,
                        pageSize: query.PageSize
                    );

                // Map to DTOs
                var adminDtos = admins.Select(admin => new AdminResponseDto
                {
                    Email = admin.Email,
                    InitialPassword = admin.InitialPassword,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
                    NationalId = admin.NationalId,
                    IsActive = admin.IsActive,
                    CreatedAt = admin.CreatedAt,
                    MustChangePassword = admin.MustChangePassword,
                    PasswordChangedAt = admin.PasswordChangedAt,
                    LastLogin = admin.LastLogin
                });

                return new PagedResult<AdminResponseDto>(adminDtos, totalCount, query.Page, query.PageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong while getting filtered admins data", ex);
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
                    Email = admin.Email,
                    FullName = admin.FullName,
                    PhoneNumber = admin.PhoneNumber,
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


        #region Helper Methods
        public string GenerateStrongPassword(int length = 8)
        {
            const string upper = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lower = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";
            const string special = "#!@$^*_";

            var random = new Random();

            // Ensure at least one of each group
            string password = string.Empty;
            password += upper[random.Next(upper.Length)];
            password += lower[random.Next(lower.Length)];
            password += digits[random.Next(digits.Length)];
            password += special[random.Next(special.Length)];

            // Fill the rest randomly from all categories
            string allChars = upper + lower + digits + special;
            for (int i = password.Length; i < length; i++)
            {
                password += allChars[random.Next(allChars.Length)];
            }

            // Shuffle the password to avoid fixed pattern
            return new string(password.OrderBy(x => random.Next()).ToArray());
        }
        #endregion

    }
}
