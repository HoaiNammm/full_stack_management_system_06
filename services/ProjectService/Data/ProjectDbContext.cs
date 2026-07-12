using Microsoft.EntityFrameworkCore;
using ProjectService.Models;

namespace ProjectService.Data;

public class ProjectDbContext : DbContext
{
    public ProjectDbContext(DbContextOptions<ProjectDbContext> options) : base(options) { }

    public DbSet<Workspace> Workspaces => Set<Workspace>();
    public DbSet<WorkspaceMember> WorkspaceMembers => Set<WorkspaceMember>();
    public DbSet<Project> Projects => Set<Project>();
    public DbSet<ProjectMember> ProjectMembers => Set<ProjectMember>();
    public DbSet<Sprint> Sprints => Set<Sprint>();
    public DbSet<Milestone> Milestones => Set<Milestone>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Workspace>(e =>
        {
            e.HasKey(w => w.Id);
            e.HasIndex(w => w.Slug).IsUnique();
            e.Property(w => w.Name).HasMaxLength(100).IsRequired();
            e.Property(w => w.Slug).HasMaxLength(100).IsRequired();
            e.Property(w => w.Description).HasMaxLength(500);
            e.Property(w => w.ImageUrl).HasMaxLength(500);
        });

        modelBuilder.Entity<WorkspaceMember>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => new { m.WorkspaceId, m.UserId }).IsUnique();
            e.Property(m => m.InviteMessage).HasMaxLength(300);
            e.Property(m => m.Role).HasConversion<string>();
            e.HasOne(m => m.Workspace)
             .WithMany(w => w.Members)
             .HasForeignKey(m => m.WorkspaceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Project>(e =>
        {
            e.HasKey(p => p.Id);
            e.Property(p => p.Name).HasMaxLength(150).IsRequired();
            e.Property(p => p.Description).HasMaxLength(1000);
            e.Property(p => p.Color).HasMaxLength(20).HasDefaultValue("#3B82F6");
            e.Property(p => p.Priority).HasConversion<string>();
            e.Property(p => p.Status).HasConversion<string>();
            e.HasOne(p => p.Workspace)
             .WithMany(w => w.Projects)
             .HasForeignKey(p => p.WorkspaceId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ProjectMember>(e =>
        {
            e.HasKey(m => m.Id);
            e.HasIndex(m => new { m.ProjectId, m.UserId }).IsUnique();
            e.Property(m => m.Role).HasConversion<string>();
            e.HasOne(m => m.Project)
             .WithMany(p => p.Members)
             .HasForeignKey(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Sprint>(e =>
        {
            e.HasKey(s => s.Id);
            e.Property(s => s.Name).HasMaxLength(100).IsRequired();
            e.Property(s => s.Goal).HasMaxLength(500);
            e.Property(s => s.Status).HasConversion<string>();
            e.HasOne(s => s.Project)
             .WithMany(p => p.Sprints)
             .HasForeignKey(s => s.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Milestone>(e =>
        {
            e.HasKey(m => m.Id);
            e.Property(m => m.Name).HasMaxLength(200).IsRequired();
            e.Property(m => m.Description).HasMaxLength(1000);
            e.Property(m => m.Status).HasConversion<string>();
            e.HasIndex(m => m.ProjectId);
            e.HasOne(m => m.Project)
             .WithMany()
             .HasForeignKey(m => m.ProjectId)
             .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
