using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Models;

namespace NotifyService.Api.Data;

public class NotifyDbContext : DbContext
{
    public NotifyDbContext(DbContextOptions<NotifyDbContext> options)
        : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();

    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<LoginHistory> LoginHistories => Set<LoginHistory>();
    public DbSet<PasswordResetToken> PasswordResetTokens => Set<PasswordResetToken>();
    public DbSet<SystemRole> SystemRoles => Set<SystemRole>();
    public DbSet<Notification> Notifications => Set<Notification>();
    public DbSet<UserNotification> UserNotifications => Set<UserNotification>();
    public DbSet<NotificationLog> NotificationLogs => Set<NotificationLog>();
    public DbSet<NotificationTemplate> NotificationTemplates => Set<NotificationTemplate>();
    public DbSet<NotificationChannel> NotificationChannels => Set<NotificationChannel>();

    public DbSet<IncomingEvent> IncomingEvents => Set<IncomingEvent>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<SystemLog> SystemLogs => Set<SystemLog>();
    public DbSet<EventProcessingLog> EventProcessingLogs => Set<EventProcessingLog>();

    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<CommentMention> CommentMentions => Set<CommentMention>();
    public DbSet<CommentAttachment> CommentAttachments => Set<CommentAttachment>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureAuthUser(modelBuilder);
        ConfigureNotifications(modelBuilder);
        ConfigureEventsAndLogs(modelBuilder);
        ConfigureCommentsAndActivity(modelBuilder);
    }

    private static void ConfigureAuthUser(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.ToTable("Users");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.FullName)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Email)
                .HasMaxLength(150)
                .IsRequired();

            entity.HasIndex(x => x.Email)
                .IsUnique();

            entity.Property(x => x.PasswordHash)
                .IsRequired();

            entity.Property(x => x.Role)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.PhoneNumber)
                .HasMaxLength(20);

            entity.Property(x => x.AvatarUrl)
                .HasMaxLength(500);

            entity.Property(x => x.Department)
                .HasMaxLength(100);

            entity.Property(x => x.Position)
                .HasMaxLength(100);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.EmailConfirmed)
                .HasDefaultValue(false);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");
        });

        modelBuilder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Token).IsRequired();
            entity.Property(x => x.IsRevoked).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.User)
                .WithMany(x => x.RefreshTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LoginHistory>(entity =>
        {
            entity.ToTable("LoginHistories");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IpAddress).HasMaxLength(50);
            entity.Property(x => x.UserAgent).HasMaxLength(500);
            entity.Property(x => x.FailureReason).HasMaxLength(300);
            entity.Property(x => x.LoginAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.User)
                .WithMany(x => x.LoginHistories)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<PasswordResetToken>(entity =>
        {
            entity.ToTable("PasswordResetTokens");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Token).IsRequired();
            entity.Property(x => x.IsUsed).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.User)
                .WithMany(x => x.PasswordResetTokens)
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureNotifications(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Notification>(entity =>
        {
            entity.ToTable("Notifications");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(100).IsRequired();
            entity.Property(x => x.SourceService).HasMaxLength(100);
            entity.Property(x => x.SourceEventId).HasMaxLength(100);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.TaskId);
            entity.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<UserNotification>(entity =>
        {
            entity.ToTable("UserNotifications");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.IsRead).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => new { x.UserId, x.IsRead });

            entity.HasOne(x => x.Notification)
                .WithMany(x => x.UserNotifications)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationLog>(entity =>
        {
            entity.ToTable("NotificationLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ErrorMessage).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.Notification)
                .WithMany(x => x.NotificationLogs)
                .HasForeignKey(x => x.NotificationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<NotificationTemplate>(entity =>
        {
            entity.ToTable("NotificationTemplates");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TitleTemplate).HasMaxLength(200).IsRequired();
            entity.Property(x => x.MessageTemplate).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.Type).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsActive).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.Code).IsUnique();
        });

        modelBuilder.Entity<NotificationChannel>(entity =>
        {
            entity.ToTable("NotificationChannels");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Code).HasMaxLength(100).IsRequired();
            entity.Property(x => x.IsEnabled).HasDefaultValue(true);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.Code).IsUnique();
        });
    }

    private static void ConfigureEventsAndLogs(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<IncomingEvent>(entity =>
        {
            entity.ToTable("IncomingEvents");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.EventId).HasMaxLength(150).IsRequired();
            entity.Property(x => x.EventType).HasMaxLength(150).IsRequired();
            entity.Property(x => x.SourceService).HasMaxLength(100).IsRequired();
            entity.Property(x => x.PayloadJson).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ReceivedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.EventId).IsUnique();
        });

        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityName).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EntityId).HasMaxLength(100);
            entity.Property(x => x.IpAddress).HasMaxLength(50);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.EntityName);
        });

        modelBuilder.Entity<SystemLog>(entity =>
        {
            entity.ToTable("SystemLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Level).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Source).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.Level);
            entity.HasIndex(x => x.CreatedAt);
        });

        modelBuilder.Entity<EventProcessingLog>(entity =>
        {
            entity.ToTable("EventProcessingLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Step).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Status).HasMaxLength(50).IsRequired();
            entity.Property(x => x.Message).HasMaxLength(1000);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.IncomingEvent)
                .WithMany(x => x.EventProcessingLogs)
                .HasForeignKey(x => x.IncomingEventId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }

    private static void ConfigureCommentsAndActivity(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Comment>(entity =>
        {
            entity.ToTable("Comments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Content).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.IsDeleted).HasDefaultValue(false);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.TaskId);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.UserId);
        });

        modelBuilder.Entity<CommentMention>(entity =>
        {
            entity.ToTable("CommentMentions");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.MentionedUserId);

            entity.HasOne(x => x.Comment)
                .WithMany(x => x.Mentions)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CommentAttachment>(entity =>
        {
            entity.ToTable("CommentAttachments");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.FileName).HasMaxLength(255).IsRequired();
            entity.Property(x => x.FileUrl).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.ContentType).HasMaxLength(100);
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasOne(x => x.Comment)
                .WithMany(x => x.Attachments)
                .HasForeignKey(x => x.CommentId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLog>(entity =>
        {
            entity.ToTable("ActivityLogs");
            entity.HasKey(x => x.Id);

            entity.Property(x => x.Action).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(1000).IsRequired();
            entity.Property(x => x.CreatedAt).HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.UserId);
            entity.HasIndex(x => x.TaskId);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.CreatedAt);
        });
        modelBuilder.Entity<SystemRole>(entity =>
        {
            entity.ToTable("SystemRoles");

            entity.HasKey(x => x.Id);

            entity.Property(x => x.Code)
                .HasMaxLength(50)
                .IsRequired();

            entity.Property(x => x.Name)
                .HasMaxLength(100)
                .IsRequired();

            entity.Property(x => x.Description)
                .HasMaxLength(300);

            entity.Property(x => x.IsActive)
                .HasDefaultValue(true);

            entity.Property(x => x.CreatedAt)
                .HasDefaultValueSql("GETUTCDATE()");

            entity.HasIndex(x => x.Code)
                .IsUnique();
        });
    }

}