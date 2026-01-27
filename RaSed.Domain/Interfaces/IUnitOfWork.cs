using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IUnitOfWork
    {
        IAdminRepository _adminRepository { get; }
        IEmployeeRepository _employeeRepository { get; }
        public IRefreshTokenRepository _refreshTokenRepository { get; }
        public IOtpRepository _otpRepository { get; }
        public ISectionRepository _sectionRepository { get; }

        public ISensorReadingRepository _sensorReadingRepository { get; }

        public IAggregatedSensorDataRepository _aggregatedSensorDataRepository { get; }

        Task<int> SaveChangesAsync();
        public void Dispose();
        Task<IDbContextTransaction> BeginTransactionAsync();
    }
}
