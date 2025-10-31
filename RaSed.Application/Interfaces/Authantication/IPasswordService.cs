using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IPasswordService
    {
        public Task<ServerOperationResult> ChangePasswordAsync(int userId, ChangePasswordDto dto);
        public Task<ServerOperationResult> ResetPasswordAsync(int userId, ResetPasswordDto dto);

    }
}
