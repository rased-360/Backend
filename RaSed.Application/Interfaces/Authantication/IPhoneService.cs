using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IPhoneService
    {
        Task<ServerOperationResult> VerifyPasswordAsync(int userId, string password);
        Task<ServerOperationResult> ChangePhoneNumberAsync(int userId, string password, string newPhoneNumber);

    }
}
