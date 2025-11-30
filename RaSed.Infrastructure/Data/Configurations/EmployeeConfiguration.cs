using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Data.Configurations
{
    public class EmployeeConfiguration : IEntityTypeConfiguration<Employee>
    {
        public void Configure(EntityTypeBuilder<Employee> builder)
        {
            builder.ToTable("Employees");


            builder.Property(a => a.MustChangePassword)
                   .HasDefaultValue(true);

            builder.Property(a => a.PasswordChangedAt)
                   .IsRequired(false);

            builder.HasOne(e => e.Section)
                  .WithMany(d => d.Employees)
                  .HasForeignKey(e => e.SectionId)
                  .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
