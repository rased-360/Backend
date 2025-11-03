using Microsoft.EntityFrameworkCore.Storage;
using RaSed.Domain.Entities;
using RaSed.Domain.Interfaces;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Repositories
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        private readonly AppDbContext _context;

        public IAdminRepository _adminRepository { get; private set; }
        public IEmployeeRepository _employeeRepository { get; private set; }

        public IRefreshTokenRepository _refreshTokenRepository { get; private set; }

        public IOtpRepository _otpRepository { get; private set; }


        public UnitOfWork(AppDbContext context)
        {
            _context = context;
            _adminRepository = new AdminRepository(_context);
            _employeeRepository = new EmployeeRepository(_context);
            _refreshTokenRepository = new RefreshTokenRepository(_context);
            _otpRepository = new OtpRepository(_context);
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync()
        {
            return await _context.Database.BeginTransactionAsync();
        }
        public void Dispose()
        {
            _context.Dispose();
        }
    }
}
