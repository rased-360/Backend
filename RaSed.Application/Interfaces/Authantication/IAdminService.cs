using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IAdminService
    {
        public  Task<AdminAuthResult> CreateAdminAsync(CreateAdminDto dto);
        public Task<AdminAuthResult?> GetAdminByIdAsync(int id);
        public Task<AdminAuthResult?> GetAdminByEmailAsync(string email);
        public Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync();
        public Task<AdminAuthResult> EditAdminAsync(int adminId, AdminEditDto editDto);
        public Task<AdminAuthResult> DeleteAdminByIdAsync(int id);
    }
}
