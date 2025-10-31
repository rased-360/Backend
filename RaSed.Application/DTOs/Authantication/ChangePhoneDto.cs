using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class ChangePhoneDto
    {
        [Required(ErrorMessage = "Current password is required")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "New phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be exactly 11 digits")]
        public string NewPhoneNumber { get; set; } = string.Empty;
    }
}
