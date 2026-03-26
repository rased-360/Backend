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

            // ── Performance columns ───────────────────────────────────────────

            builder.Property(e => e.PerformanceRate)
                .IsRequired()
                .HasColumnType("decimal(5,2)")   // e.g. 100.00 / 83.50 / 0.00
                .HasDefaultValue(100.0);          // New employee starts at 100

            builder.Property(e => e.PerformanceRating)
                .IsRequired()
                .HasMaxLength(20)                 // "Excellent" = 9 chars; 20 gives headroom
                .HasDefaultValue("Excellent");    // Matches the default score

            builder.Property(e => e.PerformanceLastUpdatedAt)
                .IsRequired(false);               // Null until first violation is recorded

            // ── Index ─────────────────────────────────────────────────────────

            builder.HasIndex(e => e.PerformanceRate)
                .HasDatabaseName("IX_Employees_PerformanceRate")
                .IsDescending();
        }

    }
}
