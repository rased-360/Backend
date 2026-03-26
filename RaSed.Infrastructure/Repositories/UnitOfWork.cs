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

        // Repositories instances
        private IAdminRepository? _adminRepositoryInstance;
        private IEmployeeRepository? _employeeRepositoryInstance;
        private IRefreshTokenRepository? _refreshTokenRepositoryInstance;
        private IOtpRepository? _otpRepositoryInstance;
        private ISectionRepository? _sectionRepositoryInstance;
        private IIssueRepository? _issueRepositoryInstance;
        private IFireEventRepository? _fireEventRepositoryInstance;
        private IViolationRepository? _violationRepositoryInstance;
        private IGeneralNotificationRepository _generalNotificationRepositoryInstance;

        // Properties to access repositories
        public IAdminRepository _adminRepository =>
        _adminRepositoryInstance ??= new AdminRepository(_context);

        public IEmployeeRepository _employeeRepository =>
            _employeeRepositoryInstance ??= new EmployeeRepository(_context);

        public IRefreshTokenRepository _refreshTokenRepository =>
            _refreshTokenRepositoryInstance ??= new RefreshTokenRepository(_context);

        public IOtpRepository _otpRepository =>
            _otpRepositoryInstance ??= new OtpRepository(_context);

        public ISectionRepository _sectionRepository =>
            _sectionRepositoryInstance ??= new SectionRepository(_context);

        public IIssueRepository _issueRepository =>
            _issueRepositoryInstance ??= new IssueRepository(_context);

        public IFireEventRepository _fireEventRepository =>
            _fireEventRepositoryInstance ??= new FireEventRepository(_context);
        public IViolationRepository _violationRepository => 
            _violationRepositoryInstance ??= new ViolationRepository(_context);

        public IGeneralNotificationRepository _generalNotificationRepository =>
            _generalNotificationRepositoryInstance ??= new GeneralNotificationRepository(_context);

        public UnitOfWork(AppDbContext context)
        {
            _context = context;
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
