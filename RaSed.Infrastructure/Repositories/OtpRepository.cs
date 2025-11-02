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
    public class OtpRepository: GenericRepository<Otp> , IOtpRepository
    {
        private readonly AppDbContext _dbContext;
        public OtpRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }
        public async Task<int> CountRecentOtpAsync(string email, int minutes)
        {
            var outOfRangeTime = DateTime.UtcNow.AddMinutes(-minutes);
            return await _dbContext.Otps.CountAsync(o =>
                o.Email == email &&
                o.CreatedAt >= outOfRangeTime);
        }

        public async Task<Otp> GetLatestOtpAsync(string email)
        {
            return await _dbContext.Otps
                .Where(o => o.Email == email)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }

        public async Task<Otp?> GetValidOtpAsync(string email, string code, int maxAttempts)
        {
            return await _dbContext.Otps
                .Include(o => o.User)
                .Where(o =>
                    o.Email == email &&
                    o.Code == code &&
                    !o.IsUsed &&
                    !o.IsVerified &&
                    o.ExpiresAt > DateTime.UtcNow &&
                    o.FailedAttempts < maxAttempts)
                .OrderByDescending(o => o.CreatedAt)
                .FirstOrDefaultAsync();
        }
        public async Task InvalidateUserOtpsAsync(int userId)
        {
            var otps = await _dbContext.Otps
                .Where(o => o.UserID == userId && !o.IsUsed)
                .ToListAsync();

            foreach (var otp in otps)
            {
                otp.ExpiresAt = DateTime.UtcNow;
                otp.IsUsed = true;
                otp.UsedAt = DateTime.UtcNow;
            }

        }

        public async Task<Otp?> GetRecentlyVerifiedOtpAsync(string email, int minutesAgo)
        {
            var timeThreshold = DateTime.UtcNow.AddMinutes(-minutesAgo);

            return await _dbContext.Otps
                .Where(o =>
                    o.Email == email &&
                    o.IsVerified &&  // لازم يكون اتعمله verify (IsUsed = true)
                    o.VerifiedAt.HasValue &&
                    o.VerifiedAt.Value >= timeThreshold &&
                    o.ExpiresAt > DateTime.UtcNow)  // اتعمله verify في آخر X دقائق
                .OrderByDescending(o => o.VerifiedAt)
                .FirstOrDefaultAsync();
        }
    }
}
