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
    public class FcmDeviceTokenConfiguration : IEntityTypeConfiguration<FcmDeviceToken>
    {
        public void Configure(EntityTypeBuilder<FcmDeviceToken> builder)
        {
            builder.ToTable("FcmDeviceTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(t => t.Platform)
                .IsRequired()
                .HasMaxLength(20);

            builder.Property(t => t.RegisteredAt)
                .IsRequired();

            // One user can have multiple tokens (multiple devices)
            builder.HasOne(t => t.Employee)
                .WithMany()
                .HasForeignKey(t => t.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            // Index for fast token lookup by userId
            builder.HasIndex(t => t.EmployeeId)
                .HasDatabaseName("IX_FcmDeviceTokens_UserId");

            // Unique index — same token can't be registered twice
            builder.HasIndex(t => t.Token)
                .IsUnique()
                .HasDatabaseName("IX_FcmDeviceTokens_Token");
        }
    }
}
