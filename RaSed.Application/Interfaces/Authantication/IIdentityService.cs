using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces
{
    public interface IIdentityService
    {
        public Task<LoginResponseDto> LoginAsync(LoginDto dto);


    }
}
