using RaSed.Application.DTOs.FcmTokens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Realtime
{
    public interface IFcmTokenService
    {
        /// <summary>
        /// Registers or updates a device token for a user.
        /// If the token already exists, just updates LastUsedAt.
        /// </summary>
        Task RegisterTokenAsync(int userId, RegisterFcmTokenDto dto);

        /// <summary>Removes a specific device token — called on logout</summary>
        Task RemoveTokenAsync(string token);
    }
}
