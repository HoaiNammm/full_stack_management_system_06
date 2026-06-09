using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Models;

namespace NotifyService.Api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users { get; set; }
    public DbSet<Comment> Comments { get; set; }
    public DbSet<CommentMention> CommentMentions { get; set; }
    public DbSet<Notification> Notifications { get; set; }
    public DbSet<UserPreference> UserPreferences { get; set; }
    public DbSet<ActivityLog> ActivityLogs { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User
        modelBuilder.Entity<User>().HasKey(u => u.Id);
        modelBuilder.Entity<User>().Property(u => u.Id).HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();
        modelBuilder.Entity<User>().Property(u => u.Status).HasDefaultValue(1);
        modelBuilder.Entity<User>().Ignore(u => u.IsActive);

        // Comment
        modelBuilder.Entity<Comment>().HasKey(c => c.Id);
        modelBuilder.Entity<Comment>().Property(c => c.Id).HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<Comment>()
            .HasOne(c => c.Author)
            .WithMany(u => u.Comments)
            .HasForeignKey(c => c.AuthorId)
            .OnDelete(DeleteBehavior.Restrict);

        // CommentMention
        modelBuilder.Entity<CommentMention>().HasKey(m => m.Id);
        modelBuilder.Entity<CommentMention>().Property(m => m.Id).HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<CommentMention>()
            .HasOne(m => m.Comment)
            .WithMany(c => c.Mentions)
            .HasForeignKey(m => m.CommentId)
            .OnDelete(DeleteBehavior.Cascade);

        // Notification
        modelBuilder.Entity<Notification>().HasKey(n => n.Id);
        modelBuilder.Entity<Notification>().Property(n => n.Id).HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<Notification>().Property(n => n.IsRead).HasDefaultValue(false);
        modelBuilder.Entity<Notification>()
            .HasOne(n => n.User)
            .WithMany(u => u.Notifications)
            .HasForeignKey(n => n.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // UserPreference (1-1 với User)
        modelBuilder.Entity<UserPreference>().HasKey(p => p.Id);
        modelBuilder.Entity<UserPreference>().Property(p => p.Id).HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<UserPreference>().HasIndex(p => p.UserId).IsUnique();
        modelBuilder.Entity<UserPreference>()
            .HasOne(p => p.User)
            .WithOne(u => u.UserPreference)
            .HasForeignKey<UserPreference>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // ActivityLog
        modelBuilder.Entity<ActivityLog>().HasKey(a => a.Id);
        modelBuilder.Entity<ActivityLog>().Property(a => a.Id).HasDefaultValueSql("NEWID()");
        modelBuilder.Entity<ActivityLog>()
            .HasOne(a => a.User)
            .WithMany(u => u.ActivityLogs)
            .HasForeignKey(a => a.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
