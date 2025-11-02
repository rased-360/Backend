using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Intrinsics.Arm;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        //Add a new refresh token(does NOT save changes)
        public Task AddAsync(RefreshToken token);

        public Task<RefreshToken?> GetByTokenAsync(string token);

        // Get active refresh token by user ID
        public Task<RefreshToken> GetByUserIdAsync(int id);

        // Get refresh token by token string
        public Task RemoveAsync(RefreshToken token);
        public Task RemoveExpiredTokensByUserIdAsync(int userId);
        public Task<List<RefreshToken>> GetAllByUserIdAsync(int userId);

        public Task RevokeAllUserTokensAsync(int userId);

        Task<int> GetActiveTokensCountAsync(int userId);
        Task<RefreshToken?> GetOldestActiveTokenAsync(int userId);
        Task<bool> IsTokenReplacedAsync(string token);
    }
}
