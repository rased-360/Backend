using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
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

            // Ensure UserName is unique
            builder.HasIndex(u => u.UserName).IsUnique();

            // Ensure Email is unique
            builder.HasIndex(u => u.Email).IsUnique();

            builder.Property(u => u.NationalId)
                   .HasMaxLength(14)
                   .IsRequired();

            builder.Property(u => u.DateOfBirth)
                   .IsRequired();

            //This will automatically convert every DateOfBirth value to UTC when saving to or reading from the database.
            builder.Property(u => u.DateOfBirth)
                .HasConversion(
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc),
                v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

            // Ensure NationalId is unique
            builder.HasIndex(u => u.NationalId)
                .IsUnique();

            builder.Property(u => u.PhoneNumber)
                   .HasMaxLength(11)
                   .IsRequired();

            // Ensure PhoneNumber is unique
            builder.HasIndex(u => u.PhoneNumber)
                .IsUnique();

            builder.Property(u => u.Gender)
                   .HasConversion<string>()     
                   .HasMaxLength(10)
                   .IsRequired();

            // handel the HireType as enum string
            builder.Property(u => u.HireType)
                   .HasConversion<string>()
                   .HasMaxLength(50)
                   .IsRequired();

            builder.Property(u => u.CreatedAt)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");



        }
    }
}
