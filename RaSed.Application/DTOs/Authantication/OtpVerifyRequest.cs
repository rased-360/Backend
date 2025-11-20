using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class OtpVerifyRequest
    {
        [Required(ErrorMessage = "Email is required")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "The code is required")]
        [StringLength(6, MinimumLength = 6, ErrorMessage = "Code must be 6 digits")]
        public string Code { get; set; } = string.Empty;
    }
}
