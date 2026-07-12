using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data;

public class TaskDbContext : DbContext
{
    public TaskDbContext(DbContextOptions<TaskDbContext> options) : base(options) { }

    public DbSet<TaskItem> Tasks => Set<TaskItem>();
    public DbSet<SubTask> SubTasks => Set<SubTask>();
    public DbSet<TimeLog> TimeLogs => Set<TimeLog>();
    public DbSet<Comment> Comments => Set<Comment>();
    public DbSet<ActivityLog> ActivityLogs => Set<ActivityLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.HasKey(t => t.Id);
            e.Property(t => t.Title).HasMaxLength(200).IsRequired();
            e.Property(t => t.Description).HasMaxLength(2000);
            e.Property(t => t.Status).HasConversion<string>();
            e.Property(t => t.Type).HasConversion<string>();
            e.Property(t => t.Priority).HasConversion<string>();
            e.Property(t => t.Labels).HasMaxLength(500);
            e.Property(t => t.EstimatedHours).HasPrecision(6, 2);
            e.HasIndex(t => t.ProjectId);
            e.HasIndex(t => t.AssigneeId);
            e.HasIndex(t => t.SprintId);
        });

        modelBuilder.Entity<SubTask>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Title).HasMaxLength(200).IsRequired();
            e.HasOne(s => s.Task)
             .WithMany(t => t.SubTasks)
             .HasForeignKey(s => s.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<TimeLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.HoursLogged).HasPrecision(6, 2);
            e.Property(l => l.Description).HasMaxLength(500);
            e.HasOne(l => l.Task)
             .WithMany(t => t.TimeLogs)
             .HasForeignKey(l => l.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Comment>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Content).HasMaxLength(2000).IsRequired();
            e.HasOne(c => c.Task)
             .WithMany()
             .HasForeignKey(c => c.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ActivityLog>(e =>
        {
            e.HasKey(a => a.Id);
            e.Property(a => a.Action).HasMaxLength(100).IsRequired();
            e.Property(a => a.EntityName).HasMaxLength(200);
            e.Property(a => a.Description).HasMaxLength(500);
            e.HasIndex(a => a.ProjectId);
            e.HasIndex(a => a.CreatedAt);
        });
    }
}
