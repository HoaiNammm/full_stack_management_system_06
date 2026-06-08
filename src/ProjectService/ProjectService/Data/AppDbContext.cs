using Microsoft.EntityFrameworkCore;
using ProjectService.Models;

namespace ProjectService.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Project> Projects { get; set; }
        public DbSet<Member> Members { get; set; }
        public DbSet<Sprint> Sprints { get; set; }
        public DbSet<Milestone> Milestones { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Project>()
                .HasKey(p => p.Id);
            modelBuilder.Entity<Project>()
                .Property(p => p.Id)
                .HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Project>()
                .Property(p => p.Status)
                .HasDefaultValue(0);

            modelBuilder.Entity<Member>()
                .HasKey(m => m.Id);
            modelBuilder.Entity<Member>()
                .Property(m => m.Id)
                .HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Member>()
                .HasOne(m => m.Project)
                .WithMany(p => p.Members)
                .HasForeignKey(m => m.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Sprint>()
                .HasKey(s => s.Id);
            modelBuilder.Entity<Sprint>()
                .Property(s => s.Id)
                .HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Sprint>()
                .Property(s => s.Status)
                .HasDefaultValue(0);
            modelBuilder.Entity<Sprint>()
                .HasOne(s => s.Project)
                .WithMany(p => p.Sprints)
                .HasForeignKey(s => s.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Milestone>()
                .HasKey(ms => ms.Id);
            modelBuilder.Entity<Milestone>()
                .Property(ms => ms.Id)
                .HasDefaultValueSql("NEWID()");
            modelBuilder.Entity<Milestone>()
                .Property(ms => ms.Status)
                .HasDefaultValue(0);
            modelBuilder.Entity<Milestone>()
                .HasOne(ms => ms.Project)
                .WithMany(p => p.Milestones)
                .HasForeignKey(ms => ms.ProjectId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}