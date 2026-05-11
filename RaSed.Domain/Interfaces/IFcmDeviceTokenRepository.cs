using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IFcmDeviceTokenRepository : IGenericRepository<FcmDeviceToken>
    {
        /// <summary>Get all tokens for a specific user (they may have multiple devices)</summary>
        Task<IEnumerable<FcmDeviceToken>> GetByEmployeeIdAsync(int employeeId);

        /// <summary>Get token by its value — used to check if already registered</summary>
        Task<FcmDeviceToken?> GetByTokenAsync(string token);

        /// <summary>Delete a specific token — called on logout</summary>
        Task<bool> DeleteByTokenAsync(string token);

        /// <summary>Delete all tokens for a user — called on account deactivation</summary>
        Task DeleteAllByEmployeeIdAsync(int employeeId);
    }
}
