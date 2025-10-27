using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class Admin: ApplicationUser
    {
        public bool IsSuperAdmin { get; set; } = false;
        public bool MustChangePassword { get; set; } = true;
        public DateTime? PasswordChangedAt { get; set; }
    }
}
