using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Services;

namespace TaskService.Controllers
{
    [ApiController]
    [Route("api/kanban")]
    public class KanbanController : ControllerBase
    {
        private readonly KanbanService _kanbanService;

        public KanbanController(KanbanService kanbanService)
        {
            _kanbanService = kanbanService;
        }

        [Authorize]
        [HttpGet("project/{projectId}")]
        public async Task<IActionResult> GetBoard(Guid projectId)
        {
            var board = await _kanbanService.GetBoardByProjectAsync(projectId);
            if (board == null)
                return NotFound(new { success = false, error = new { code = "BOARD_NOT_FOUND" } });

            return Ok(new { success = true, data = board });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateBoard([FromBody] CreateBoardRequest request)
        {
            var board = await _kanbanService.CreateBoardAsync(request.ProjectId, request.Name);
            return CreatedAtAction(nameof(GetBoard), new { projectId = board.ProjectId },
                new { success = true, data = board });
        }

        [Authorize]
        [HttpPost("{boardId}/columns")]
        public async Task<IActionResult> AddColumn(Guid boardId, [FromBody] AddColumnRequest request)
        {
            var column = await _kanbanService.AddColumnAsync(boardId, request.Name, request.Type, request.Position);
            return Ok(new { success = true, data = column });
        }

        [Authorize]
        [HttpDelete("columns/{columnId}")]
        public async Task<IActionResult> DeleteColumn(Guid columnId)
        {
            var success = await _kanbanService.DeleteColumnAsync(columnId);
            if (!success)
                return NotFound(new { success = false, error = new { code = "COLUMN_NOT_FOUND" } });

            return Ok(new { success = true });
        }
    }

    public class CreateBoardRequest
    {
        public Guid ProjectId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public class AddColumnRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = "custom"; // backlog / active / done / custom
        public int Position { get; set; }
    }
}
