using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.FcmTokens
{

    /// <summary>Sent by the mobile app on logout to remove the token</summary>
    public class RemoveFcmTokenDto
    {
        [Required]
        public string Token { get; set; } = string.Empty;
    }
}
