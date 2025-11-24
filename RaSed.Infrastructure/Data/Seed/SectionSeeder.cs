using Microsoft.EntityFrameworkCore;
using RaSed.Domain.Entities;
using RaSed.Infrastructure.Data.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Data.Seed
{
    public class SectionSeeder
    {
        public static async Task SeedSectionsAsync(AppDbContext context)
        {
            var sectionNames = new[]
            {
                "Production",
                "Quality Control",
                "Maintenance",
                "Warehouse",
                "Research & Development",
                "Process Engineering",
                "Finance",
                "Purchasing",
                "Safety and Environmental",
                "Distillation Section"
            };

            foreach (var sectionName in sectionNames)
            {
                var exists = await context.Sections.AnyAsync(s => s.Name == sectionName);
                if (!exists)
                {
                    var section = new Section
                    {
                        Name = sectionName,
                    };
                    await context.Sections.AddAsync(section);
                }
            }

            await context.SaveChangesAsync();
        }


    }
}
