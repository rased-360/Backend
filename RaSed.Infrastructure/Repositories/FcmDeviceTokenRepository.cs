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
    public class FcmDeviceTokenRepository : GenericRepository<FcmDeviceToken>, IFcmDeviceTokenRepository
    {
        public FcmDeviceTokenRepository(AppDbContext context) : base(context) { }

        public async Task<IEnumerable<FcmDeviceToken>> GetByEmployeeIdAsync(int employeeId)
        {
            return await _context.FcmDeviceTokens
                .Where(t => t.EmployeeId == employeeId)
                .OrderByDescending(t => t.RegisteredAt)
                .ToListAsync();
        }

        public async Task<FcmDeviceToken?> GetByTokenAsync(string token)
        {
            return await _context.FcmDeviceTokens
                .FirstOrDefaultAsync(t => t.Token == token);
        }

        public async Task<bool> DeleteByTokenAsync(string token)
        {
            var entity = await _context.FcmDeviceTokens
                .FirstOrDefaultAsync(t => t.Token == token);

            if (entity == null) return false;

            _context.FcmDeviceTokens.Remove(entity);
            return true;
        }

        public async Task DeleteAllByEmployeeIdAsync(int employeeId)
        {
            var tokens = await _context.FcmDeviceTokens
                .Where(t => t.EmployeeId == employeeId)
                .ToListAsync();

            _context.FcmDeviceTokens.RemoveRange(tokens);
        }
    }
}
