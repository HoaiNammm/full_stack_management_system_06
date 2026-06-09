using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Services;

public class KanbanService
{
    private readonly AppDbContext _db;

    public KanbanService(AppDbContext db) => _db = db;

    public async Task<List<KanbanColumn>> GetColumnsByProjectAsync(Guid projectId)
        => await _db.KanbanColumns
                    .Where(c => c.ProjectId == projectId)
                    .OrderBy(c => c.Position)
                    .ToListAsync();

    public async Task<KanbanColumn?> GetColumnByIdAsync(Guid id)
        => await _db.KanbanColumns.FindAsync(id);

    // Called by ProjectEventConsumer when project.created event is received.
    // Creates columns exactly as defined by the template.
    public async Task SeedColumnsFromEventAsync(Guid projectId,
        IEnumerable<(string Name, string Type, int Position)> columns)
    {
        var exists = await _db.KanbanColumns.AnyAsync(c => c.ProjectId == projectId);
        if (exists) return;

        var entities = columns.Select(c => new KanbanColumn
        {
            Id        = Guid.NewGuid(),
            ProjectId = projectId,
            Name      = c.Name,
            Type      = c.Type,
            Position  = c.Position,
        });

        _db.KanbanColumns.AddRange(entities);
        await _db.SaveChangesAsync();
    }

    // Fallback: seeds 4-column blank board when no event was received yet.
    public async Task SeedDefaultColumnsAsync(Guid projectId)
    {
        var exists = await _db.KanbanColumns.AnyAsync(c => c.ProjectId == projectId);
        if (exists) return;

        var defaults = new[]
        {
            new KanbanColumn { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Backlog",     Position = 0, Type = "backlog" },
            new KanbanColumn { Id = Guid.NewGuid(), ProjectId = projectId, Name = "To Do",       Position = 1, Type = "active"  },
            new KanbanColumn { Id = Guid.NewGuid(), ProjectId = projectId, Name = "In Progress", Position = 2, Type = "active"  },
            new KanbanColumn { Id = Guid.NewGuid(), ProjectId = projectId, Name = "Done",        Position = 3, Type = "done"    },
        };
        _db.KanbanColumns.AddRange(defaults);
        await _db.SaveChangesAsync();
    }

    public async Task<KanbanColumn> AddColumnAsync(Guid projectId, string name, string type = "custom")
    {
        var maxPos = await _db.KanbanColumns
                              .Where(c => c.ProjectId == projectId)
                              .Select(c => (int?)c.Position)
                              .MaxAsync() ?? -1;

        var col = new KanbanColumn
        {
            Id        = Guid.NewGuid(),
            ProjectId = projectId,
            Name      = name,
            Position  = maxPos + 1,
            Type      = type
        };
        _db.KanbanColumns.Add(col);
        await _db.SaveChangesAsync();
        return col;
    }

    public async Task<KanbanColumn?> UpdateColumnAsync(Guid id, string name, string type)
    {
        var col = await _db.KanbanColumns.FindAsync(id);
        if (col == null) return null;
        col.Name = name;
        col.Type = type;
        await _db.SaveChangesAsync();
        return col;
    }

    public async Task<bool> DeleteColumnAsync(Guid id)
    {
        var col = await _db.KanbanColumns.FindAsync(id);
        if (col == null) return false;

        var hasTask = await _db.Tasks.AnyAsync(t => t.ColumnId == id && t.DeletedAt == null);
        if (hasTask) return false;

        _db.KanbanColumns.Remove(col);
        await _db.SaveChangesAsync();
        return true;
    }
}
