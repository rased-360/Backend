using Microsoft.EntityFrameworkCore;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Repositories
{
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly AppDbContext _context;
        public RefreshTokenRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task AddAsync(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
        }
        public async Task<RefreshToken?> GetByUserIdAsync(int id)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(rt => rt.UserId == id);
        }
        public async Task RemoveAsync(RefreshToken token)
        {
            _context.RefreshTokens.Remove(token);

        }
        public async Task RemoveExpiredTokensByUserIdAsync(int userId)
        {
            var oldTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId &&
                            (rt.Revoked != null || rt.Expires < DateTime.UtcNow))
                .ToListAsync();

            if (oldTokens.Any())
            {
                _context.RefreshTokens.RemoveRange(oldTokens);
            }
        }
        public async Task<List<RefreshToken>> GetAllByUserIdAsync(int userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId)
                .OrderByDescending(rt => rt.Created)
                .ToListAsync();
        }
        public async Task<RefreshToken?> GetByTokenAsync(string token)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);
        }
        public async Task RevokeAllUserTokensAsync(int userId)
        {
            var userTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.Revoked == null)
                .ToListAsync();
            foreach (var token in userTokens)
            {
                token.Revoked = DateTime.UtcNow;
            }

        }

    }
}
