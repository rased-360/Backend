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
    public class IssueConfiguration : IEntityTypeConfiguration<Issue>
    {
        public void Configure(EntityTypeBuilder<Issue> builder)
        {
            // Table name
            builder.ToTable("Issues");

            // Primary key
            builder.HasKey(i => i.Id);

            // Properties configuration
            builder.Property(i => i.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(i => i.Description)
                .IsRequired()
                .HasMaxLength(2000);

            builder.Property(i => i.ImageUrl)
                .IsRequired(false)
                .HasMaxLength(500);

            builder.Property(i => i.ReportedAt)
                .IsRequired();

            builder.Property(i => i.EmployeeId)
                .IsRequired()
                .HasMaxLength(450);

            // Indexes for better query performance
            builder.HasIndex(i => i.EmployeeId)
                .HasDatabaseName("IX_Issues_EmployeeId");

            builder.HasIndex(i => i.ReportedAt)
                .HasDatabaseName("IX_Issues_ReportedAt")
                .IsDescending(); // Most recent first

            // Relationships
            builder.HasOne(i => i.Employee)
                .WithMany(e => e.Issues)
                .HasForeignKey(i => i.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
