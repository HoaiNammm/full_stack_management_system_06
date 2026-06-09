using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<KanbanColumn> KanbanColumns { get; set; }
    public DbSet<TaskItem>     Tasks         { get; set; }
    public DbSet<SubTask>      SubTasks      { get; set; }
    public DbSet<TaskTimeLog>  TaskTimeLogs  { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // KanbanColumn — must be configured before TaskItem (FK dependency)
        modelBuilder.Entity<KanbanColumn>(e =>
        {
            e.HasKey(c => c.Id);
            e.Property(c => c.Id).HasDefaultValueSql("NEWID()");
            e.Property(c => c.Type).HasMaxLength(50).HasDefaultValue("custom");
            e.Property(c => c.Name).HasMaxLength(100).IsRequired();
        });

        // TaskItem — mapped to "Tasks" table (avoids C# keyword conflict)
        modelBuilder.Entity<TaskItem>(e =>
        {
            e.ToTable("Tasks");
            e.HasKey(t => t.Id);
            e.Property(t => t.Id).HasDefaultValueSql("NEWID()");
            e.Property(t => t.Title).HasMaxLength(500).IsRequired();
            e.Property(t => t.EstimatedHours).HasPrecision(10, 2);

            e.HasOne(t => t.Column)
             .WithMany(c => c.Tasks)
             .HasForeignKey(t => t.ColumnId)
             .OnDelete(DeleteBehavior.Restrict);
        });

        // SubTask
        modelBuilder.Entity<SubTask>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Id).HasDefaultValueSql("NEWID()");
            e.Property(s => s.Title).HasMaxLength(500).IsRequired();
            e.Property(s => s.EstimatedHours).HasPrecision(10, 2);

            e.HasOne(s => s.Task)
             .WithMany(t => t.SubTasks)
             .HasForeignKey(s => s.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // TaskTimeLog
        modelBuilder.Entity<TaskTimeLog>(e =>
        {
            e.HasKey(l => l.Id);
            e.Property(l => l.Id).HasDefaultValueSql("NEWID()");
            e.Property(l => l.Hours).HasPrecision(10, 2);

            e.HasOne(l => l.Task)
             .WithMany(t => t.TimeLogs)
             .HasForeignKey(l => l.TaskId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        // Indexes
        modelBuilder.Entity<KanbanColumn>().HasIndex(c => c.ProjectId);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.ProjectId);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.ColumnId);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.AssignedTo);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.SprintId);
        modelBuilder.Entity<TaskItem>().HasIndex(t => t.DeletedAt);
    }
}
