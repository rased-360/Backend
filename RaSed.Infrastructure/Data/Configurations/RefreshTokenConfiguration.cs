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
    public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
    {
        public void Configure(EntityTypeBuilder<RefreshToken> builder)
        {
            builder.ToTable("RefreshTokens");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Token)
                   .HasMaxLength(500)
                   .IsRequired();

            builder.Property(t => t.Expires)
                   .IsRequired();

            builder.Property(t => t.CreatedByIp)
                     .HasMaxLength(50);

            builder.Property(t => t.RevokedByIp)
                   .HasMaxLength(50)
                   .IsRequired(false);

            builder.Property(t => t.ReplacedByToken)
                     .HasMaxLength(500)
                     .IsRequired(false);

            builder.Property(t => t.ReasonRevoked)
                     .HasMaxLength(200)
                     .IsRequired(false);


            builder.Property(t => t.Created)
                   .HasDefaultValueSql("CURRENT_TIMESTAMP");

            //  1:N ApplicationUser & RefreshTokens
            builder.HasOne(t => t.User)
                    .WithMany(u => u.RefreshTokens)
                    .HasForeignKey(t => t.UserId)
                    .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
