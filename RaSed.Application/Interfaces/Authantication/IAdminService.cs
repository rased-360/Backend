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
        public  Task<CreateAdminResponseDto> CreateAdminAsync(CreateAdminDto dto);
        public Task<AdminResponseDto?> GetAdminByIdAsync(int id);
        public Task<AdminResponseDto?> GetAdminByEmailAsync(string email);
        public Task<IEnumerable<AdminResponseDto>> GetAllAdminsAsync();
        public Task<AdminEditResponsDto> EditAdminAsync(int adminId, AdminEditDto editDto);
        public Task<bool> DeleteAdminByIdAsync(int id);
    }
}
