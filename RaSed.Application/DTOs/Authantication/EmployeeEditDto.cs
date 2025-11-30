using RaSed.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class EmployeeEditDto
    {
        [Required(ErrorMessage = "UserId is required")]
        public int userId { get; set; }

        [Required(ErrorMessage = "Is Active is required")]
        public bool IsActive { get; set; }
    }
}
