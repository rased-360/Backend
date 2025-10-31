using Microsoft.Extensions.Configuration;
using RaSed.Application.DTOs.Authantication;
using RaSed.Application.Interfaces.Authantication;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Services.Authantication
{
    public class EmailService: IEmailService
    {
        private readonly IConfiguration _configuration;
        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task<ServerOperationResult> SendEmailAsync(string email, string subject, string body)
        {
            try
            {

                var smtpHost = _configuration["EmailSettings:SmtpHost"];
                var smtpPortString = _configuration["EmailSettings:SmtpPort"];
                var smtpPort = int.TryParse(smtpPortString, out var portResult) ? portResult : 587;
                var smtpUser = _configuration["EmailSettings:SmtpUser"];
                var smtpPass = _configuration["EmailSettings:SmtpPassword"];
                var fromName = _configuration["EmailSettings:FromName"];

                if (string.IsNullOrEmpty(smtpHost) || string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
                {
                    return new ServerOperationResult 
                    {
                        IsSuccessful = false, 
                        Message = "Email settings are not configured properly."
                    };
                }

                using var client = new SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = true,
                    Credentials = new NetworkCredential(smtpUser, smtpPass)
                };

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(smtpUser, fromName),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);
                return new ServerOperationResult 
                {
                    IsSuccessful = true, 
                    Message = "Email sent successfully."
                };
            }
            catch (Exception ex)
            {
                return new ServerOperationResult 
                {
                    IsSuccessful = false,
                    Message = $"Failed to send email: {ex.Message}"
                };
            }
        }
    }
}
