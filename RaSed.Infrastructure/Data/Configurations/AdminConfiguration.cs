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
    public class AdminConfiguration : IEntityTypeConfiguration<Admin>
    {
        public void Configure(EntityTypeBuilder<Admin> builder)
        {
            builder.ToTable("Admins");

            builder.Property(a => a.IsSuperAdmin)
                   .HasDefaultValue(false);

            builder.Property(a => a.MustChangePassword)
                   .HasDefaultValue(true);

            builder.Property(a => a.PasswordChangedAt)
                   .IsRequired(false);
        }
    }
}
