using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IEmployeeRepository : IGenericRepository<Employee>
    {
        public Task<Employee> GetEmployeeByEmailAsync(string email);
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByNationalIdAsync(string nationalId);
        Task<bool> ExistsByPhoneAsync(string phoneNumber);
        Task<(IEnumerable<Employee> Items, int TotalCount)> GetPagedEmployeesAsync(int page, int pageSize);

        Task<(IEnumerable<Employee> Items, int TotalCount)> GetFilteredEmployeesAsync(
        string? searchTerm,
        bool? isActive,
        string? sortOrder,
        int page,
        int pageSize
        );
    }
}
