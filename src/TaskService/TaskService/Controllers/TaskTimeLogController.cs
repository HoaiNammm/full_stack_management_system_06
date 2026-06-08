using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.Models;
using TaskService.Services;

namespace TaskService.Controllers
{
    [ApiController]
    [Route("api/timelogs")]
    public class TaskTimeLogController : ControllerBase
    {
        private readonly TaskTimeLogService _timeLogService;

        public TaskTimeLogController(TaskTimeLogService timeLogService)
        {
            _timeLogService = timeLogService;
        }

        [Authorize]
        [HttpGet("task/{taskId}")]
        public async Task<IActionResult> GetLogs(Guid taskId)
        {
            var logs = await _timeLogService.GetLogsByTaskAsync(taskId);
            return Ok(new { success = true, data = logs });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateLog([FromBody] CreateTimeLogRequest request)
        {
            var userId = User.FindFirst("sub")?.Value;

            var log = new TaskTimeLog
            {
                Id          = Guid.NewGuid(),
                TaskId      = request.TaskId,
                LoggedBy    = Guid.Parse(userId!),
                Hours       = request.Hours,
                Description = request.Description,
                LoggedDate  = request.LoggedDate,
                CreatedAt   = DateTime.UtcNow
            };

            await _timeLogService.CreateLogAsync(log);
            return Ok(new { success = true, data = log });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLog(Guid id)
        {
            var success = await _timeLogService.DeleteLogAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "TIMELOG_NOT_FOUND" } });

            return Ok(new { success = true });
        }
    }

    public class CreateTimeLogRequest
    {
        public Guid TaskId { get; set; }
        public decimal Hours { get; set; }
        public string? Description { get; set; }
        public DateTime LoggedDate { get; set; }
    }
}
