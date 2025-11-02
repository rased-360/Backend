using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Domain.Entities
{
    public class Employee : ApplicationUser
    {
        public bool MustChangePassword { get; set; } = true;
    }
}
