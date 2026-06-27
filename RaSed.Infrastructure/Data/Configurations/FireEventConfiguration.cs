using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Data.Configuration
{

    /// Entity Framework configuration for FireEvent entity.
    public class FireEventConfiguration : IEntityTypeConfiguration<FireEvent>
    {
        public void Configure(EntityTypeBuilder<FireEvent> builder)
        {
            // Table name
            builder.ToTable("FireEvents");

            // Primary key
            builder.HasKey(e => e.Id);

            // Properties
            builder.Property(e => e.DeviceId)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(e => e.StartTime)
                .IsRequired();

            builder.Property(e => e.EndTime)
                .IsRequired(false);  // Nullable - null means fire is still active

            builder.Property(e => e.DurationSeconds)
                .IsRequired(false);  // Nullable - calculated when EndTime is set

            builder.Property(e => e.Status)
                .IsRequired()
                .HasMaxLength(50);

            // Index: DeviceId + Status (most common query: get active fire for device)
            builder.HasIndex(e => new { e.DeviceId, e.Status })
                .HasDatabaseName("IX_FireEvents_DeviceId_Status");

            // Index: StartTime DESC (for history queries)
            builder.HasIndex(e => e.StartTime)
                .IsDescending()
                .HasDatabaseName("IX_FireEvents_StartTime");
        }
    }
}