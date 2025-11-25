using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IAdminRepository : IGenericRepository<Admin>
    {
        Task<bool> ExistsByEmailAsync(string email);
        Task<bool> ExistsByNationalIdAsync(string nationalId);
        Task<bool> ExistsByPhoneAsync(string phoneNumber);
        Task<(IEnumerable<Admin> Items, int TotalCount)> GetPagedAdminsAsync(int page, int pageSize);

        Task<(IEnumerable<Admin> Items, int TotalCount)> GetFilteredAdminsAsync(
        string? searchTerm,
        bool? isActive,
        string? sortOrder,
        int page,
        int pageSize
        );
    }

}
