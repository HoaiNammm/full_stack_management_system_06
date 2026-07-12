using Microsoft.EntityFrameworkCore;
using NotifyService.Models;

namespace NotifyService.Data;

public class NotifyDbContext : DbContext
{
    public NotifyDbContext(DbContextOptions<NotifyDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => u.Email).IsUnique();
            e.Property(u => u.Name).HasMaxLength(100).IsRequired();
            e.Property(u => u.Email).HasMaxLength(200).IsRequired();
            // Stores either a real URL or a base64 data: URL (client-side avatar upload), so no fixed length.
            e.Property(u => u.AvatarUrl).HasColumnType("nvarchar(max)");
        });

        modelBuilder.Entity<RefreshToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).HasMaxLength(500).IsRequired();
            e.HasOne(r => r.User)
             .WithMany(u => u.RefreshTokens)
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(e =>
        {
            e.HasKey(r => r.Id);
            e.Property(r => r.Token).HasMaxLength(200).IsRequired();
            e.HasIndex(r => r.Token).IsUnique();
            e.HasOne(r => r.User)
             .WithMany()
             .HasForeignKey(r => r.UserId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Notification>(e =>
        {
            e.HasKey(n => n.Id);
            e.Property(n => n.Type).HasMaxLength(50).IsRequired();
            e.Property(n => n.Title).HasMaxLength(200).IsRequired();
            e.Property(n => n.Message).HasMaxLength(1000).IsRequired();
        });

        modelBuilder.Entity<UserNotification>(e =>
        {
            e.HasKey(u => u.Id);
            e.HasIndex(u => new { u.UserId, u.NotificationId }).IsUnique();
            e.HasOne(u => u.Notification)
             .WithMany(n => n.UserNotifications)
             .HasForeignKey(u => u.NotificationId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
