using RaSed.Application.DTOs.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.Interfaces.Authantication
{
    public interface IEmailService
    {
        public Task<ServerOperationResult> SendEmailAsync(string email, string subject, string body);

    }
}
