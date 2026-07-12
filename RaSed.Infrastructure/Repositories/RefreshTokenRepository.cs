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
        public void Add(RefreshToken token)
        {
            _context.RefreshTokens.Add(token);
        }

        public async Task RemoveExpiredTokensByUserIdAsync(int userId)
        {
            var revokedCutoff = DateTime.UtcNow.AddDays(-30);

            var oldTokens = await _context.RefreshTokens
                .Where(rt => rt.UserId == userId &&
                            (
                                // Expired and not revoked — safe to delete quickly
                                (rt.Revoked == null && rt.Expires < DateTime.UtcNow) ||
                                // Revoked — only delete after 30-day audit window
                                (rt.Revoked != null && rt.Revoked < revokedCutoff)
                            ))
                .ToListAsync();

            if (oldTokens.Any())
                _context.RefreshTokens.RemoveRange(oldTokens);
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
        //  Get count of active tokens
        public async Task<int> GetActiveTokensCountAsync(int userId)
        {
            return await _context.RefreshTokens
                .CountAsync(rt => rt.UserId == userId
                               && rt.Revoked == null
                               && rt.Expires > DateTime.UtcNow);
        }

        // Get oldest active token
        public async Task<RefreshToken?> GetOldestActiveTokenAsync(int userId)
        {
            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId
                          && rt.Revoked == null
                          && rt.Expires > DateTime.UtcNow)
                .OrderBy(rt => rt.Created)
                .FirstOrDefaultAsync();
        }

        // Check if token was replaced (for reuse detection)
        public async Task<bool> IsTokenReplacedAsync(string token)
        {
            var storedToken = await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.Token == token);

            return storedToken?.ReplacedByToken != null;
        }
        public async Task<List<RefreshToken>> GetTokenChainAsync(string token)
        {
            var tokens = new List<RefreshToken>();
            var currentToken = await GetByTokenAsync(token);

            if (currentToken == null)
                return tokens;

            tokens.Add(currentToken);

            // Follow the replacement chain backwards
            while (currentToken?.ReplacedByToken != null)
            {
                currentToken = await GetByTokenAsync(currentToken.ReplacedByToken);
                if (currentToken != null)
                    tokens.Add(currentToken);
            }

            return tokens;
        }

    }
}
