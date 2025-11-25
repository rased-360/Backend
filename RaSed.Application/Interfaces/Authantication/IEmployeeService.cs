using RaSed.Application.DTOs;
using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IEmployeeService
    {
        public Task<EmployeeAuthResult> CreateEmployeeAsync(CreateEmployeeDto dto);
        public Task<EmployeeAuthResult?> GetEmployeeByIdAsync(int id);
        public Task<EmployeeAuthResult> DeleteAdminsByIdsAsync(List<int> ids);
        public Task<PagedResult<EmployeeResponseDto>> GetAllEmployeesAsync(int page = 1, int pageSize = 10);

    }
}
