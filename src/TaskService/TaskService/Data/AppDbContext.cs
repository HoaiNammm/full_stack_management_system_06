using Microsoft.EntityFrameworkCore;
using TaskService.Models;

namespace TaskService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<TaskItem> Tasks { get; set; }
        public DbSet<SubTask> SubTasks { get; set; }
        public DbSet<KanbanBoard> KanbanBoards { get; set; }
        public DbSet<KanbanColumn> KanbanColumns { get; set; }
        public DbSet<TaskTimeLog> TaskTimeLogs { get; set; }
        public DbSet<TaskAssignmentHistory> TaskAssignmentHistories { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // KanbanBoard
            modelBuilder.Entity<KanbanBoard>().HasKey(b => b.Id);
            modelBuilder.Entity<KanbanBoard>()
                .Property(b => b.Id).HasDefaultValueSql("NEWID()");

            // KanbanColumn
            modelBuilder.Entity<KanbanColumn>().HasKey(c => c.Id);
            modelBuilder.Entity<KanbanColumn>()
                .Property(c => c.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<KanbanColumn>()
                .Property(c => c.Type).HasMaxLength(50).HasDefaultValue("custom");
            modelBuilder.Entity<KanbanColumn>()
                .HasOne(c => c.Board)
                .WithMany(b => b.Columns)
                .HasForeignKey(c => c.BoardId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskItem → table "Tasks"
            modelBuilder.Entity<TaskItem>().ToTable("Tasks");
            modelBuilder.Entity<TaskItem>().HasKey(t => t.Id);
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.Priority).HasDefaultValue(1);
            modelBuilder.Entity<TaskItem>()
                .Property(t => t.EstimatedHours).HasPrecision(10, 2);
            // ColumnId FK → KanbanColumns RESTRICT (xóa column phải chuyển task trước)
            modelBuilder.Entity<TaskItem>()
                .HasOne(t => t.Column)
                .WithMany(c => c.Tasks)
                .HasForeignKey(t => t.ColumnId)
                .OnDelete(DeleteBehavior.Restrict);

            // SubTask
            modelBuilder.Entity<SubTask>().HasKey(s => s.Id);
            modelBuilder.Entity<SubTask>()
                .Property(s => s.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<SubTask>()
                .Property(s => s.Status).HasDefaultValue(0);
            modelBuilder.Entity<SubTask>()
                .Property(s => s.EstimatedHours).HasPrecision(10, 2);
            modelBuilder.Entity<SubTask>()
                .HasOne(s => s.Task)
                .WithMany(t => t.SubTasks)
                .HasForeignKey(s => s.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskTimeLog
            modelBuilder.Entity<TaskTimeLog>().HasKey(l => l.Id);
            modelBuilder.Entity<TaskTimeLog>()
                .Property(l => l.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<TaskTimeLog>()
                .Property(l => l.Hours).HasPrecision(10, 2);
            modelBuilder.Entity<TaskTimeLog>()
                .HasOne(l => l.Task)
                .WithMany(t => t.TimeLogs)
                .HasForeignKey(l => l.TaskId)
                .OnDelete(DeleteBehavior.Cascade);

            // TaskAssignmentHistory
            modelBuilder.Entity<TaskAssignmentHistory>().HasKey(h => h.Id);
            modelBuilder.Entity<TaskAssignmentHistory>()
                .Property(h => h.Id).HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<TaskAssignmentHistory>()
                .HasOne(h => h.Task)
                .WithMany(t => t.AssignmentHistory)
                .HasForeignKey(h => h.TaskId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
