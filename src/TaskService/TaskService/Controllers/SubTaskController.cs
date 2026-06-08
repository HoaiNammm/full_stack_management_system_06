using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using TaskService.Services;

namespace TaskService.Controllers
{
    [ApiController]
    [Route("api/subtasks")]
    public class SubTaskController : ControllerBase
    {
        private readonly SubTaskService _subTaskService;

        public SubTaskController(SubTaskService subTaskService)
        {
            _subTaskService = subTaskService;
        }

        [Authorize]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetSubTasks(Guid taskId)
        {
            var subTasks = await _subTaskService.GetSubTasksByTaskAsync(taskId);
            return Ok(new { success = true, data = subTasks });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetSubTask(Guid id)
        {
            var subTask = await _subTaskService.GetSubTaskByIdAsync(id);
            if (subTask == null)
                return NotFound(new { success = false, error = new { code = "SUBTASK_NOT_FOUND" } });

            return Ok(new { success = true, data = subTask });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateSubTask([FromBody] CreateSubTaskRequest request)
        {
            var subTask = new SubTask
            {
                Id             = Guid.NewGuid(),
                TaskId         = request.TaskId,
                Title          = request.Title,
                Description    = request.Description,
                AssignedTo     = request.AssignedTo,
                Status         = 0,
                EstimatedHours = request.EstimatedHours,
                CreatedAt      = DateTime.UtcNow
            };

            await _subTaskService.CreateSubTaskAsync(subTask);
            return CreatedAtAction(nameof(GetSubTask), new { id = subTask.Id },
                new { success = true, data = subTask });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateSubTask(Guid id, [FromBody] UpdateSubTaskRequest request)
        {
            var subTask = await _subTaskService.UpdateSubTaskAsync(
                id, request.Title, request.Description,
                request.AssignedTo, request.Status, request.EstimatedHours);

            if (subTask == null)
                return NotFound(new { success = false, error = new { code = "SUBTASK_NOT_FOUND" } });

            return Ok(new { success = true, data = subTask });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteSubTask(Guid id)
        {
            var success = await _subTaskService.DeleteSubTaskAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "SUBTASK_NOT_FOUND" } });

            return Ok(new { success = true });
        }
    }

    public class CreateSubTaskRequest
    {
        public Guid TaskId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? AssignedTo { get; set; }
        public decimal? EstimatedHours { get; set; }
    }

    public class UpdateSubTaskRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public Guid? AssignedTo { get; set; }
        public int Status { get; set; }
        public decimal? EstimatedHours { get; set; }
    }
}
