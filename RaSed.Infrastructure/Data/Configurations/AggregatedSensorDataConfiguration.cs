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
    public class AggregatedSensorDataConfiguration: IEntityTypeConfiguration<AggregatedSensorData>
    {
        public void Configure(EntityTypeBuilder<AggregatedSensorData> builder)
        {
            builder.ToTable("AggregatedSensorData");

            builder.HasKey(e => e.Id);

            // Temperature
            builder.Property(e => e.AvgTemperature).HasPrecision(5, 2).IsRequired();
            builder.Property(e => e.MinTemperature).HasPrecision(5, 2).IsRequired();
            builder.Property(e => e.MaxTemperature).HasPrecision(5, 2).IsRequired();

            // Humidity
            builder.Property(e => e.AvgHumidity).HasPrecision(5, 2).IsRequired();
            builder.Property(e => e.MinHumidity).HasPrecision(5, 2).IsRequired();
            builder.Property(e => e.MaxHumidity).HasPrecision(5, 2).IsRequired();

            // Pressure
            builder.Property(e => e.AvgPressure).HasPrecision(7, 2).IsRequired();
            builder.Property(e => e.MinPressure).HasPrecision(7, 2).IsRequired();
            builder.Property(e => e.MaxPressure).HasPrecision(7, 2).IsRequired();

            // Indexes
            builder.HasIndex(e => e.StartTime)
                .HasDatabaseName("IX_AggregatedSensorData_StartTime");

            builder.HasIndex(e => e.EndTime)
                .HasDatabaseName("IX_AggregatedSensorData_EndTime");

           
            builder.HasIndex(e => new { e.StartTime, e.EndTime })
                .HasDatabaseName("IX_AggregatedSensorData_TimeRange");
        }
    }
}
