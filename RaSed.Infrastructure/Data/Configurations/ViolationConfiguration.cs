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
    public class ViolationConfiguration : IEntityTypeConfiguration<Violation>
    {
        public void Configure(EntityTypeBuilder<Violation> builder)
        {
            builder.ToTable("Violations");

            builder.HasKey(v => v.Id);

            builder.Property(v => v.Timestamp)
                .IsRequired();

            builder.Property(v => v.ViolationType)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(v => v.ImageUrl)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(v => v.EmployeeId)
               .IsRequired(false);  

            // Indexes for common query patterns
            builder.HasIndex(v => v.EmployeeId)
                .HasDatabaseName("IX_Violations_EmployeeId");

            builder.HasIndex(v => v.Timestamp)
                .HasDatabaseName("IX_Violations_Timestamp")
                .IsDescending(); // Newest first + cleanup cutoff queries

            // Relationship — SetNull so violation history is not cascade-deleted
            // when an employee is removed (admin may still want the audit trail)
            builder.HasOne(v => v.Employee)
                .WithMany(e => e.Violations)
                .HasForeignKey(v => v.EmployeeId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
