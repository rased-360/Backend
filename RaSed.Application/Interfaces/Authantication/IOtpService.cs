using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IOtpService
    {
        public  Task<ServerOperationResult> SendOtpAsync(SendOtpRequest request);
        public Task<ServerOperationResult> VerifyOtpAsync(OtpVerifyRequest otpVerifyRequest);

    }
}
