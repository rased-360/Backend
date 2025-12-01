using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class LoginResponse
    {
        public string Email { get; set; }
        public string FullName { get; set; }

        public string ProfilePictureUrl { get; set; } = string.Empty;
    }
}
