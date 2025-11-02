using Microsoft.AspNetCore.Identity;
using RaSed.Application.DTOs;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class EmployeeService : IEmployeeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;


        public EmployeeService(IUnitOfWork _unitOfWork, UserManager<ApplicationUser> _userManager)
        {
            this._unitOfWork = _unitOfWork;
            this._userManager = _userManager;
        }
        public async Task<EmployeeAuthResult> CreateEmployeeAsync(CreateEmployeeDto dto)
        {
            try
            {
                // Validation: Email
                if (await _unitOfWork._employeeRepository.ExistsByEmailAsync(dto.Email))
                {
                    return EmployeeAuthResult.Failure("Email is already in use");
                }

                // Validation: NationalId
                if (await _unitOfWork._employeeRepository.ExistsByNationalIdAsync(dto.NationalId))
                {
                    return EmployeeAuthResult.Failure("National ID is already in use");
                }

                // Validation: Phone
                if (await _unitOfWork._employeeRepository.ExistsByPhoneAsync(dto.PhoneNumber))
                {
                    return EmployeeAuthResult.Failure("Phone number is already in use");
                }
                var employee = new Employee
                {
                    Email = dto.Email,
                    UserName = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Gender = dto.Gender,
                    NationalId = dto.NationalId,
                    DateOfBirth = dto.DateOfBirth,
                    HireType = dto.HireType,
                    IsActive = true,
                    MustChangePassword = true,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = true
                    
                };

                // create employee
                var result = await _userManager.CreateAsync(employee, dto.Password);    

                if (!result.Succeeded)
                {
                    var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                    return EmployeeAuthResult.Failure(errors, "Fail to create the employee.");
                }

                await _userManager.AddToRoleAsync(employee, "Employee");

                var employeeResponse = new EmployeeResponseDto
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber,
                    Gender = employee.Gender,
                    NationalId = employee.NationalId,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt,
                };

                return EmployeeAuthResult.Success(employeeResponse, employee.MustChangePassword, "Employee created successfully");

            }
            catch (InvalidOperationException ex)
            {
                return EmployeeAuthResult.Failure(ex.Message, "Failed to create employee.");
            }
            catch (Exception)
            {
                return EmployeeAuthResult.Failure("An unexpected error occurred while creating the employee. Please try again later.", null);
            }
        }

        public async Task<EmployeeAuthResult> DeleteEmployeeByIdAsync(int id)
        {
            var employeeToDelete = await _unitOfWork._employeeRepository.GetByIdAsync(id);

            if (employeeToDelete == null)
            {
                return EmployeeAuthResult.Failure("Employee not found.", null);
            }


            _unitOfWork._employeeRepository.Delete(employeeToDelete);
            await _unitOfWork.SaveChangesAsync();

            return EmployeeAuthResult.Success("Employee deleted successfully.");
        }

        public async Task<EmployeeAuthResult> EditEmployeeAsync(int empId, EmployeeEditDto dto)
        {
            try
            {
                var existingEmployee = await _unitOfWork._employeeRepository.GetByIdAsync(empId);

                if (existingEmployee == null)
                {
                    return EmployeeAuthResult.Failure("Employee not found.", null);
                }
                // Validate NationalId if changed
                if (existingEmployee.NationalId != dto.NationalId)
                {
                    if (await _unitOfWork._employeeRepository.ExistsByNationalIdAsync(dto.NationalId))
                    {
                        return EmployeeAuthResult.Failure("National ID is already in use");
                    }
                }

                if (existingEmployee.PhoneNumber != dto.PhoneNumber)
                {
                    if (await _unitOfWork._employeeRepository.ExistsByPhoneAsync(dto.PhoneNumber))
                    {
                        return EmployeeAuthResult.Failure("Phone number is already in use");
                    }
                }
                existingEmployee.FullName = dto.FullName;
                existingEmployee.Email = dto.Email;
                existingEmployee.Gender = dto.Gender;
                existingEmployee.NationalId = dto.NationalId;
                existingEmployee.PhoneNumber = dto.PhoneNumber;
                existingEmployee.DateOfBirth = dto.DateOfBirth;
                existingEmployee.HireType = dto.HireType;

                var updateResult = await _userManager.UpdateAsync(existingEmployee);

                if (!updateResult.Succeeded)
                {
                    var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                    throw new InvalidOperationException($"Fail in updating data {errors}");
                }

                await _unitOfWork.SaveChangesAsync();

                var enployeeDto = new EmployeeResponseDto
                {
                    Email = dto.Email,
                    FullName = dto.FullName,
                    PhoneNumber = dto.PhoneNumber,
                    Gender = dto.Gender,
                    NationalId = dto.NationalId,
                    HireType = dto.HireType,
                    DateOfBirth = dto.DateOfBirth,


                };
                return EmployeeAuthResult.Success(enployeeDto, existingEmployee.MustChangePassword, "Employee updated successfully");

            }
            catch (Exception ex)
            {
                return EmployeeAuthResult.Failure("An unexpected error occurred while updating the employee. Please try again later.", ex.Message);
            }
        }

        public async Task<EmployeeAuthResult?> GetEmployeeByEmailAsync(string email)
        {
            try
            {
                var employee = await _unitOfWork._employeeRepository.GetEmployeeByEmailAsync(email);
                var result = new EmployeeResponseDto
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber,
                    Gender = employee.Gender,
                    NationalId = employee.NationalId,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt,
                };
                return EmployeeAuthResult.Success(result, employee.MustChangePassword, "Employee created successfully");

            }
            catch (Exception ex)
            {
                return EmployeeAuthResult.Failure($"Something went wrong to gat this Employee data {email}", ex.Message);
            }
        }

        public async Task<EmployeeAuthResult?> GetEmployeeByIdAsync(int id)
        {
            try
            {
                var employee = await _unitOfWork._employeeRepository.GetByIdAsync(id);
                if (employee == null)
                {
                    return EmployeeAuthResult.Failure("Employee not found.", null);
                }
                var result = new EmployeeResponseDto
                {
                    Id = employee.Id,
                    Email = employee.Email,
                    FullName = employee.FullName,
                    PhoneNumber = employee.PhoneNumber,
                    Gender = employee.Gender,
                    NationalId = employee.NationalId,
                    DateOfBirth = employee.DateOfBirth,
                    HireType = employee.HireType,
                    IsActive = employee.IsActive,
                    CreatedAt = employee.CreatedAt
                };
                return EmployeeAuthResult.Success(result, employee.MustChangePassword, "Employee created successfully");

            }
            catch (Exception ex)
            {
                return EmployeeAuthResult.Failure($"Something went wrong to gat this Employee data {id}", ex.Message);

            }
        }

        public async Task<PagedResult<EmployeeResponseDto>> GetAllEmployeesAsync(int page = 1, int pageSize = 10)
        {
            try
            {
                // Validation
                if (page < 1) page = 1;
                if (pageSize < 1) pageSize = 10;
                if (pageSize > 100) pageSize = 100;

                var (employee, totalCount) = await _unitOfWork._employeeRepository.GetPagedEmployeesAsync(page, pageSize);

                // تحويل للـ DTO
                var employeeDto = employee.Select(admin => new EmployeeResponseDto
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

                return new PagedResult<EmployeeResponseDto>(employeeDto, totalCount, page, pageSize);
            }
            catch (Exception ex)
            {
                throw new Exception("Something went wrong while getting employee data", ex);
            }
        }
    }
}
