using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        public Task AddAsync(RefreshToken token);
        public Task<RefreshToken?> GetByTokenAsync(string token);
        public Task<RefreshToken> GetByUserIdAsync(int id);

        public Task RemoveAsync(RefreshToken token);
        public Task RemoveExpiredTokensByUserIdAsync(int userId);
        public Task<List<RefreshToken>> GetAllByUserIdAsync(int userId);
    }
}
