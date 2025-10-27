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
    public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
    {
        public void Configure(EntityTypeBuilder<ApplicationUser> builder)
        {
            builder.ToTable("ApplicationUsers");

            builder.Property(u => u.FullName)
                   .HasMaxLength(100)
                   .IsRequired();

            builder.Property(u => u.NationalId)
                   .HasMaxLength(14)
                   .IsRequired();

            builder.Property(u => u.Gender)
                   .HasConversion<string>()     
                   .HasMaxLength(10)
                   .IsRequired();

            builder.Property(u => u.HireType)
                   .HasMaxLength(50);

            builder.Property(u => u.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            
        }
    }
}
