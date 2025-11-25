using RaSed.Application.DTOs;
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
        public Task<PagedResult<AdminResponseDto>> GetAllAdminsAsync(int page = 1, int pageSize = 10);
        public Task<AdminAuthResult> ActivateOrDisactivateAdminAsync(int id, bool isActive);
        public Task<AdminAuthResult> DeleteAdminsByIdsAsync(List<int> ids);
        Task<PagedResult<AdminResponseDto>> GetFilteredAdminsAsync(QueryDto query);

        public Task<AdminAuthResult?> GetAdminByIdAsync(int id);


    }
}
