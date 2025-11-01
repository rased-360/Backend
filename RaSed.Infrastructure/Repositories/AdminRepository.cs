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
    public class AdminRepository : GenericRepository<Admin>, IAdminRepository
    {
        private readonly AppDbContext _dbContext;
        public AdminRepository(AppDbContext dbContext) : base(dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<Admin> GetAdminByEmailAsync(string email)
        {
            return await Task.FromResult(_dbContext.Admins.FirstOrDefault(a => a.Email == email));
        }

        public async Task<bool> ExistsByEmailAsync(string email)
        {
            return await _dbContext.Users.AnyAsync(u => u.Email == email);
        }

        public async Task<bool> ExistsByNationalIdAsync(string nationalId)
        {
            return await _dbContext.Users.AnyAsync(u => u.NationalId == nationalId);
        }

        public async Task<bool> ExistsByPhoneAsync(string phoneNumber)
        {
            return await _dbContext.Users.AnyAsync(u => u.PhoneNumber == phoneNumber);
        }

        public async Task<(IEnumerable<Admin> Items, int TotalCount)> GetPagedAdminsAsync(int page, int pageSize)
        {
            var query = _dbContext.Admins
                .Where(a => !a.IsSuperAdmin )
                .AsQueryable();

            var totalCount = await query.CountAsync();

            var items = await query
                .OrderByDescending(a => a.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }
    }
}
