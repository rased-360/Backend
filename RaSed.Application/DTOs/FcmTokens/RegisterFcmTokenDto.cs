using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.FcmTokens
{
    /// <summary>Sent by the mobile app when registering or refreshing a device token</summary>
    public class RegisterFcmTokenDto
    {
        [Required(ErrorMessage = "Token is required")]
        public string Token { get; set; } = string.Empty;

        [Required(ErrorMessage = "Platform is required")]
        [RegularExpression("android|ios", ErrorMessage = "Platform must be 'android' or 'ios'")]
        public string Platform { get; set; } = string.Empty;
    }
}
