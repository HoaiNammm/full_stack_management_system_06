using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TaskService.DTOs.SubTasks;
using TaskService.DTOs.Tasks;
using TaskService.DTOs.TimeLogs;
using TaskService.Services;

namespace TaskService.Controllers;

[ApiController]
[Route("api/projects/{projectId:guid}/tasks")]
[Authorize]
public class TasksController : ControllerBase
{
    private readonly ITaskService _svc;
    public TasksController(ITaskService svc) => _svc = svc;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid projectId, [FromQuery] string? status, [FromQuery] string? priority) =>
        Ok(await _svc.GetByProjectAsync(projectId, Me, status, priority));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid projectId, Guid id)
    {
        var t = await _svc.GetByIdAsync(id, Me);
        return t is null ? NotFound() : Ok(t);
    }

    [HttpPost]
    public async Task<IActionResult> Create(Guid projectId, [FromBody] CreateTaskRequest req)
    {
        var t = await _svc.CreateAsync(projectId, req, Me);
        return CreatedAtAction(nameof(GetById), new { projectId, id = t.Id }, t);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid projectId, Guid id, [FromBody] UpdateTaskRequest req) =>
        Ok(await _svc.UpdateAsync(id, req, Me));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid projectId, Guid id)
    {
        await _svc.DeleteAsync(id, Me);
        return NoContent();
    }
}

[ApiController]
[Route("api/tasks/{taskId:guid}/subtasks")]
[Authorize]
public class SubTasksController : ControllerBase
{
    private readonly ISubTaskService _svc;
    public SubTasksController(ISubTaskService svc) => _svc = svc;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid taskId) => Ok(await _svc.GetByTaskAsync(taskId, Me));

    [HttpPost]
    public async Task<IActionResult> Create(Guid taskId, [FromBody] CreateSubTaskRequest req) =>
        Ok(await _svc.CreateAsync(taskId, req, Me));

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid taskId, Guid id, [FromBody] UpdateSubTaskRequest req) =>
        Ok(await _svc.UpdateAsync(id, req, Me));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id)
    {
        await _svc.DeleteAsync(id, Me);
        return NoContent();
    }
}

[ApiController]
[Route("api/tasks/{taskId:guid}/timelogs")]
[Authorize]
public class TimeLogsController : ControllerBase
{
    private readonly ITimeLogService _svc;
    public TimeLogsController(ITimeLogService svc) => _svc = svc;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAll(Guid taskId) => Ok(await _svc.GetByTaskAsync(taskId, Me));

    [HttpPost]
    public async Task<IActionResult> Log(Guid taskId, [FromBody] CreateTimeLogRequest req) =>
        Ok(await _svc.LogAsync(taskId, req, Me));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid taskId, Guid id)
    {
        await _svc.DeleteAsync(id, Me);
        return NoContent();
    }
}

[ApiController]
[Route("api/tasks")]
[Authorize]
public class MyTasksController : ControllerBase
{
    private readonly ITaskService _svc;
    public MyTasksController(ITaskService svc) => _svc = svc;
    private Guid Me => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("mine")]
    public async Task<IActionResult> GetMine() => Ok(await _svc.GetAssignedToMeAsync(Me));
}

[ApiController]
[Route("internal/projects")]
public class TasksInternalController : ControllerBase
{
    private readonly ITaskService _svc;
    public TasksInternalController(ITaskService svc) => _svc = svc;

    [HttpGet("{projectId:guid}/task-stats")]
    public async Task<IActionResult> GetStats(Guid projectId) =>
        Ok(await _svc.GetStatsByProjectAsync(projectId));
}
