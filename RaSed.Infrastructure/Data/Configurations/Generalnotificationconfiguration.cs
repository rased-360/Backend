using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using RaSed.Domain.Entities;

namespace RaSed.Infrastructure.Data.Configurations
{
    public class GeneralNotificationConfiguration : IEntityTypeConfiguration<GeneralNotification>
    {
        public void Configure(EntityTypeBuilder<GeneralNotification> builder)
        {
            builder.ToTable("GeneralNotifications");

            // ── Primary Key ────────────────────────────────────────────────
            builder.HasKey(n => n.Id);

            // ── Properties ─────────────────────────────────────────────────

            builder.Property(n => n.UserId)
                .IsRequired();

            builder.Property(n => n.Type)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(n => n.Message)
                .IsRequired()
                .HasMaxLength(500);

            builder.Property(n => n.Timestamp)
                .IsRequired();

            builder.Property(n => n.IsRead)
                .IsRequired()
                .HasDefaultValue(false);

            // ── Relationships ──────────────────────────────────────────────

            builder.HasOne(n => n.User)
                .WithMany(u => u.GeneralNotifications) 
                .HasForeignKey(n => n.UserId)
                .OnDelete(DeleteBehavior.Cascade); // Delete notifications when user is deleted

            // ── Indexes ────────────────────────────────────────────────────

            // Index for fetching user notifications (most common query)
            builder.HasIndex(n => new { n.UserId, n.Timestamp })
                .HasDatabaseName("IX_GeneralNotifications_UserId_Timestamp")
                .IsDescending(false, true);  // UserId ASC, Timestamp DESC

            // Index for unread count query
            builder.HasIndex(n => new { n.UserId, n.IsRead })
                .HasDatabaseName("IX_GeneralNotifications_UserId_IsRead");

            // Index for cleanup job (delete old notifications)
            builder.HasIndex(n => n.Timestamp)
                .HasDatabaseName("IX_GeneralNotifications_Timestamp");
        }
    }
}