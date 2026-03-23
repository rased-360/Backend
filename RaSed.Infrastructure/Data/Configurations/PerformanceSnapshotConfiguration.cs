using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Data.Configurations
{
    public class PerformanceSnapshotConfiguration : IEntityTypeConfiguration<PerformanceSnapshot>
    {
        public void Configure(EntityTypeBuilder<PerformanceSnapshot> builder)
        {
            builder.ToTable("PerformanceSnapshots");

            builder.HasKey(p => p.Id);

            // ── Scalar columns ────────────────────────────────────────────────

            builder.Property(p => p.PerformanceRate)
                .IsRequired()
                .HasColumnType("decimal(5,2)"); // e.g. 100.00 / 83.50 / 0.00

            builder.Property(p => p.Rating)
                .IsRequired()
                .HasMaxLength(20); 

            builder.Property(p => p.ViolationCount)
                .IsRequired();

            builder.Property(p => p.WindowDays)
                .IsRequired();

            builder.Property(p => p.LastCalculatedAt)
                .IsRequired();

            // ── Relationship ──────────────────────────────────────────────────

            // One snapshot per employee — CASCADE so the row is automatically
            // removed when the employee is deleted.
            builder.HasOne(p => p.Employee)
                .WithOne(e => e.PerformanceSnapshot)         
                .HasForeignKey<PerformanceSnapshot>(p => p.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // ── Indexes ───────────────────────────────────────────────────────

            // Unique: enforces one-row-per-employee at DB level
            builder.HasIndex(p => p.EmployeeId)
                .IsUnique()
                .HasDatabaseName("UX_PerformanceSnapshots_EmployeeId");

            // Descending: supports the next-sprint admin query
            // "sort all employees by performance rate, highest first"
            builder.HasIndex(p => p.PerformanceRate)
                .HasDatabaseName("IX_PerformanceSnapshots_PerformanceRate")
                .IsDescending();
        }
    }
}
