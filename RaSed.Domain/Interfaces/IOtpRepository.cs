using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Interfaces
{
    public interface IOtpRepository : IGenericRepository<Otp>
    {
        public Task<int> CountRecentOtpAsync(string email, int minutes);

        public Task<Otp> GetLatestOtpAsync(string email);
        public Task<Otp> GetValidOtpAsync(string email, string code, int maxAttempts);
        public  Task InvalidateUserOtpsAsync(int userId);



    }
}
