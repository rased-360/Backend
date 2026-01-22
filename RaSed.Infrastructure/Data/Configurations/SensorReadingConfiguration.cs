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
    public class SensorReadingConfiguration : IEntityTypeConfiguration<SensorReading>
    {
        public void Configure(EntityTypeBuilder<SensorReading> builder)
        {
            builder.ToTable("SensorReadings");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Temperature)
                .HasPrecision(5, 2);

            builder.Property(e => e.HeatIndex)
                .HasPrecision(6, 2)
                .IsRequired();

            builder.HasIndex(e => e.Timestamp)
                .HasDatabaseName("IX_SensorReadings_Timestamp");

            builder.Property(e => e.AlertType)
                .HasMaxLength(50);

            builder.Property(e => e.AlertMessage)
                .HasMaxLength(500);

            builder.Property(e => e.Timestamp)
                   .HasDefaultValueSql("NOW()");
        }
    }
}
