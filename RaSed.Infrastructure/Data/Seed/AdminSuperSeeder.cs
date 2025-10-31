using Microsoft.AspNetCore.Identity;
using RaSed.Domain.Entities;
using RaSed.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Data.Seed
{
    public class AdminSuperSeeder
    {
        public static async Task SeedSuperAdminAsync(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager)
        {
            await RolesSeeder.SeedRolesAsync(roleManager);

            var superAdminEmail = "superadmin@factory.com";

            var existingAdmin = await userManager.FindByEmailAsync(superAdminEmail);
            if (existingAdmin != null) return; 
            var superAdmin = new Admin
            {
                Email = "superadmin@factory.com",
                UserName = "superadmin@factory.com",
                FullName = "Super Administrator",
                PhoneNumber = "01000000000",
                Gender = Gender.Male,
                NationalId = "00000000000000",
                DateOfBirth = DateTime.SpecifyKind(new DateTime(1990, 1, 1), DateTimeKind.Utc),
                HireType = HireType.FullTime,
                IsActive = true,
                CreatedAt = DateTime.UtcNow,
                EmailConfirmed = true,
                PhoneNumberConfirmed = true,
                IsSuperAdmin = true,
                MustChangePassword = false
            };

            var result = await userManager.CreateAsync(superAdmin, "Super@1234");

            if (result.Succeeded)
            {
                await userManager.AddToRoleAsync(superAdmin, "SuperAdmin");
            }
        }
    }
}
