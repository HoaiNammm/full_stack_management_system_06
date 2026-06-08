using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using TaskService.Services;

namespace TaskService.Controllers
{
    [ApiController]
    [Route("api/tasks")]
    public class TaskController : ControllerBase
    {
        private readonly TaskItemService _taskService;

        public TaskController(TaskItemService taskService)
        {
            _taskService = taskService;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetTasks(
            [FromQuery] Guid? projectId,
            [FromQuery] Guid? sprintId,
            [FromQuery] Guid? assignedTo,
            [FromQuery] Guid? columnId)
        {
            var tasks = await _taskService.GetTasksAsync(projectId, sprintId, assignedTo, columnId);
            return Ok(new { success = true, data = tasks });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetTask(Guid id)
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
                return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

            return Ok(new { success = true, data = task });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateTask([FromBody] CreateTaskRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;

            var task = new TaskItem
            {
                Id             = Guid.NewGuid(),
                ProjectId      = request.ProjectId,
                SprintId       = request.SprintId,
                ColumnId       = request.ColumnId,
                Title          = request.Title,
                Description    = request.Description,
                AssignedTo     = request.AssignedTo,
                Priority       = request.Priority,
                EstimatedHours = request.EstimatedHours,
                Deadline       = request.Deadline,
                CreatedBy      = Guid.Parse(userId!),
                CreatedAt      = DateTime.UtcNow
            };

            await _taskService.CreateTaskAsync(task);
            return CreatedAtAction(nameof(GetTask), new { id = task.Id },
                new { success = true, data = task });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, [FromBody] UpdateTaskRequest request)
        {
            var task = await _taskService.UpdateTaskAsync(
                id, request.Title, request.Description, request.SprintId,
                request.Priority, request.EstimatedHours, request.Deadline);

            if (task == null)
                return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

            return Ok(new { success = true, data = task });
        }

        // Kéo task sang column khác trên Kanban board
        [Authorize]
        [HttpPut("{id}/column")]
        public async Task<IActionResult> MoveToColumn(Guid id, [FromBody] MoveToColumnRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;
            var task = await _taskService.MoveToColumnAsync(id, request.ColumnId, Guid.Parse(userId!));

            if (task == null)
                return NotFound(new { success = false, error = new { code = "TASK_OR_COLUMN_NOT_FOUND" } });

            return Ok(new { success = true, data = task });
        }

        [Authorize]
        [HttpPut("{id}/assign")]
        public async Task<IActionResult> AssignTask(Guid id, [FromBody] AssignTaskRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;
            var task = await _taskService.AssignTaskAsync(id, request.AssignedTo, Guid.Parse(userId!));

            if (task == null)
                return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

            return Ok(new { success = true, data = task });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(Guid id)
        {
            var success = await _taskService.DeleteTaskAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "TASK_NOT_FOUND" } });

            return Ok(new { success = true });
        }
    }

    public class CreateTaskRequest
    {
        public Guid ProjectId { get; set; }
        public Guid? SprintId { get; set; }
        public Guid ColumnId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? AssignedTo { get; set; }
        public int Priority { get; set; } = 1; // Medium default
        public decimal? EstimatedHours { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class UpdateTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? SprintId { get; set; }
        public int Priority { get; set; }
        public decimal? EstimatedHours { get; set; }
        public DateTime? Deadline { get; set; }
    }

    public class MoveToColumnRequest
    {
        public Guid ColumnId { get; set; }
    }

    public class AssignTaskRequest
    {
        public Guid? AssignedTo { get; set; }
    }
}
