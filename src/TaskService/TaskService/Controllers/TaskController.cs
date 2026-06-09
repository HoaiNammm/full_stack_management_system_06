using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using TaskService.Services;

namespace TaskService.Controllers;

[ApiController]
[Route("api/tasks")]
public class TaskController : ControllerBase
{
    private readonly TaskItemService           _taskService;
    private readonly KanbanService             _kanbanService;
    private readonly IProjectMembershipService _membership;

    public TaskController(
        TaskItemService           taskService,
        KanbanService             kanbanService,
        IProjectMembershipService membership)
    {
        _taskService   = taskService;
        _kanbanService = kanbanService;
        _membership    = membership;
    }

    [Authorize]
    [HttpGet]
    public async Task<IActionResult> GetTasks(
        [FromQuery] Guid?  projectId,
        [FromQuery] Guid?  columnId,
        [FromQuery] Guid?  assignedTo,
        [FromQuery] Guid?  sprintId)
    {
        if (projectId.HasValue)
        {
            var userId = GetCurrentUserId();
            var token  = GetJwtToken();
            if (!await _membership.IsMemberAsync(projectId.Value, userId, token))
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });
        }

        var tasks = await _taskService.GetAllAsync(projectId, columnId, assignedTo, sprintId);
        return Ok(new { success = true, data = tasks });
    }

    [Authorize]
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTask(Guid id)
    {
        var task = await _taskService.GetByIdAsync(id);
        if (task == null)
            return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(task.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        return Ok(new { success = true, data = task });
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest req)
    {
        var userId = GetCurrentUserId();
        var token  = GetJwtToken();

        if (!await _membership.IsMemberAsync(req.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var col = await _kanbanService.GetColumnByIdAsync(req.ColumnId);
        if (col == null)
        {
            await _kanbanService.SeedDefaultColumnsAsync(req.ProjectId);
            var cols = await _kanbanService.GetColumnsByProjectAsync(req.ProjectId);
            col = cols.FirstOrDefault();
            if (col == null)
                return BadRequest(new { success = false, error = new { code = "COLUMN_NOT_FOUND" } });
        }

        var task = new TaskItem
        {
            ProjectId      = req.ProjectId,
            ColumnId       = col.Id,
            Title          = req.Title,
            Description    = req.Description,
            Priority       = req.Priority,
            AssignedTo     = req.AssignedTo,
            DueDate        = req.DueDate,
            EstimatedHours = req.EstimatedHours,
            SprintId       = req.SprintId,
        };

        var created = await _taskService.CreateAsync(task, userId);
        return Ok(new { success = true, data = created });
    }

    [Authorize]
    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest req)
    {
        var existing = await _taskService.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(existing.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var task = await _taskService.UpdateAsync(
            id, req.Title, req.Description, req.Priority,
            req.AssignedTo, req.DueDate, req.EstimatedHours,
            req.SprintId, req.ClearSprint);

        return Ok(new { success = true, data = task });
    }

    [Authorize]
    [HttpPut("{id}/column")]
    public async Task<IActionResult> MoveToColumn(Guid id, [FromBody] MoveToColumnRequest req)
    {
        var existing = await _taskService.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(existing.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var task = await _taskService.MoveToColumnAsync(id, req.ColumnId, userId);
        if (task == null)
            return NotFound(new { success = false, error = new { code = "TASK_OR_COLUMN_NOT_FOUND" } });

        return Ok(new { success = true, data = task });
    }

    [Authorize]
    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTask(Guid id)
    {
        var existing = await _taskService.GetByIdAsync(id);
        if (existing == null)
            return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

        var userId = GetCurrentUserId();
        var token  = GetJwtToken();
        if (!await _membership.IsMemberAsync(existing.ProjectId, userId, token))
            return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

        var ok = await _taskService.DeleteAsync(id);
        return Ok(new { success = true });
    }

    private Guid   GetCurrentUserId() => Guid.Parse(User.FindFirst("sub")!.Value);
    private string GetJwtToken()      =>
        HttpContext.Request.Headers["Authorization"].ToString().Replace("Bearer ", "");
}

public class CreateTaskRequest
{
    public Guid      ProjectId       { get; set; }
    public Guid      ColumnId        { get; set; }
    public string    Title           { get; set; } = string.Empty;
    public string?   Description     { get; set; }
    public int       Priority        { get; set; } = 2;
    public Guid?     AssignedTo      { get; set; }
    public DateTime? DueDate         { get; set; }
    public decimal?  EstimatedHours  { get; set; }
    public Guid?     SprintId        { get; set; }
}

public class UpdateTaskRequest
{
    public string    Title           { get; set; } = string.Empty;
    public string?   Description     { get; set; }
    public int       Priority        { get; set; } = 2;
    public Guid?     AssignedTo      { get; set; }
    public DateTime? DueDate         { get; set; }
    public decimal?  EstimatedHours  { get; set; }
    public Guid?     SprintId        { get; set; }
    public bool      ClearSprint     { get; set; } = false;
}

public class MoveToColumnRequest
{
    public Guid ColumnId { get; set; }
}
