using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Configuration;
using RaSed.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RaSed.Infrastructure.Data.Configurations
{
    public class OtpConfiguration :  IEntityTypeConfiguration<Otp>
    {
        public void Configure(EntityTypeBuilder<Otp> builder)
        {
            builder.ToTable("Otps");

            builder.HasKey(x => x.Id);

            builder.Property(x => x.Email)
                .HasMaxLength(255)
                .IsRequired();

            builder.Property(x => x.ExpiresAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.CreatedAt)
                .HasDefaultValueSql("CURRENT_TIMESTAMP")
                .IsRequired();

            builder.Property(x => x.Code)
                .HasMaxLength(6)
                .IsRequired();

            builder.Property(x => x.IsUsed)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.IsVerified)
                .IsRequired()
                .HasDefaultValue(false);

            builder.Property(x => x.FailedAttempts)
                .IsRequired()
                .HasDefaultValue(0);

            builder.HasIndex(x => new { x.Email, x.Code, x.IsUsed, x.ExpiresAt });

            //  1:N ApplicationUser & OTPVerifications
            builder.HasOne(o => o.User)
                .WithMany(u => u.Otp)
                .HasForeignKey(o => o.UserID).OnDelete(DeleteBehavior.Cascade);

        }
    }
}
