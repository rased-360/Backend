using RaSed.Domain.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Application.DTOs.Authantication
{
    public class CreateEmployeeDto
    {
        //Full Name
        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100, ErrorMessage = "Full name cannot exceed 100 characters")]
        public string FullName { get; set; } = string.Empty;

        //Email
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        public string Email { get; set; } = string.Empty;

        //Phone Number
        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number format")]
        [RegularExpression(@"^\d{11}$", ErrorMessage = "Phone number must be exactly 11 digits")]
        public string PhoneNumber { get; set; } = string.Empty;

        //National ID
        [Required(ErrorMessage = "National ID is required")]
        [StringLength(14, MinimumLength = 14, ErrorMessage = "National ID must be 14 digits")]
        public string NationalId { get; set; } = string.Empty;

        // Date of Birth
        [Required(ErrorMessage = "Date of birth is required")]
        public DateTime DateOfBirth { get; set; }

        // Gender
        [Required(ErrorMessage = "Gender is required")]
        public Gender Gender { get; set; }

        // Hire Type
        [Required(ErrorMessage = "Hire type is required")]
        public HireType HireType { get; set; }

        // Section Id
        [Required(ErrorMessage = "Section is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a valid section")]
        public int SectionId { get; set; }
    }
}
