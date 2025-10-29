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
        public IRefreshTokenRepository _refreshTokenRepository { get; }
        Task<int> SaveChangesAsync();
        public void Dispose();


    }
}
