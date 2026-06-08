using Microsoft.EntityFrameworkCore;
using TaskService.Data;
using TaskService.Models;

namespace TaskService.Services
{
    public class KanbanService
    {
        private readonly AppDbContext _context;

        public KanbanService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<KanbanBoard?> GetBoardByProjectAsync(Guid projectId)
        {
            return await _context.KanbanBoards
                .Include(b => b.Columns.OrderBy(c => c.Position))
                .FirstOrDefaultAsync(b => b.ProjectId == projectId);
        }

        public async Task<KanbanColumn?> GetColumnByIdAsync(Guid columnId)
        {
            return await _context.KanbanColumns.FindAsync(columnId);
        }

        public async Task<KanbanBoard> CreateBoardAsync(Guid projectId, string name)
        {
            var board = new KanbanBoard
            {
                Id        = Guid.NewGuid(),
                ProjectId = projectId,
                Name      = name,
                CreatedAt = DateTime.UtcNow
            };

            _context.KanbanBoards.Add(board);

            // Seed 5 columns mặc định với Type tương ứng theo schema
            var defaultColumns = new[]
            {
                (Name: "Backlog",     Type: "backlog"),
                (Name: "To Do",       Type: "active"),
                (Name: "In Progress", Type: "active"),
                (Name: "Review",      Type: "active"),
                (Name: "Done",        Type: "done"),
            };

            for (int i = 0; i < defaultColumns.Length; i++)
            {
                _context.KanbanColumns.Add(new KanbanColumn
                {
                    Id       = Guid.NewGuid(),
                    BoardId  = board.Id,
                    Name     = defaultColumns[i].Name,
                    Type     = defaultColumns[i].Type,
                    Position = i
                });
            }

            await _context.SaveChangesAsync();
            return board;
        }

        public async Task<KanbanColumn> AddColumnAsync(Guid boardId, string name, string type, int position)
        {
            var column = new KanbanColumn
            {
                Id       = Guid.NewGuid(),
                BoardId  = boardId,
                Name     = name,
                Type     = type,
                Position = position
            };

            _context.KanbanColumns.Add(column);
            await _context.SaveChangesAsync();
            return column;
        }

        // Xóa column chỉ thành công nếu không còn task nào (RESTRICT FK)
        public async Task<bool> DeleteColumnAsync(Guid columnId)
        {
            var column = await _context.KanbanColumns.FindAsync(columnId);
            if (column == null) return false;

            var hasTask = await _context.Tasks.AnyAsync(t => t.ColumnId == columnId && t.DeletedAt == null);
            if (hasTask) return false;

            _context.KanbanColumns.Remove(column);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}
