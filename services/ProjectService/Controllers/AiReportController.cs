using System.Security.Claims;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectService.Data;
using ProjectService.DTOs.AiProjects;
using ProjectService.DTOs.Members;
using ProjectService.DTOs.Projects;
using ProjectService.DTOs.Sprints;
using ProjectService.HttpClients;
using ProjectService.Models;
using ProjectService.Services;

namespace ProjectService.Controllers;

[ApiController]
[Route("api/workspaces/{workspaceId:guid}/projects/{projectId:guid}/ai-report")]
[Authorize]
public class AiReportController : ControllerBase
{
    private readonly IProjectService _projectService;
    private readonly IAiProjectPlannerService _planner;
    private readonly TaskServiceClient _taskClient;
    private readonly ProjectDbContext _db;

    public AiReportController(
        IProjectService projectService,
        IAiProjectPlannerService planner,
        TaskServiceClient taskClient,
        ProjectDbContext db)
    {
        _projectService = projectService;
        _planner = planner;
        _taskClient = taskClient;
        _db = db;
    }

    private Guid CurrentUserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Generate(Guid workspaceId, Guid projectId, [FromBody] AiReportRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Question))
            throw new InvalidOperationException("Question is required.");

        var userId = CurrentUserId;
        var authHeader = Request.Headers.Authorization.ToString();

        var project = await _projectService.GetByIdAsync(projectId, userId)
            ?? throw new KeyNotFoundException("Project not found.");
        var sprints = await _projectService.GetSprintsAsync(projectId, userId);
        var members = await _projectService.GetMembersAsync(projectId, userId);
        var milestones = await _db.Milestones.Where(m => m.ProjectId == projectId)
            .OrderBy(m => m.DueDate).ToListAsync();
        var tasks = await _taskClient.GetTasksByProjectAsync(authHeader, projectId);

        var context = BuildContext(project, sprints, milestones, members, tasks);
        var answer = await _planner.AnswerReportQuestionAsync(context, request.Question);

        return Ok(new AiReportResponseDto { Report = answer });
    }

    private static string BuildContext(
        ProjectDto project,
        List<SprintDto> sprints,
        List<Milestone> milestones,
        List<ProjectMemberDto> members,
        List<TaskServiceClient.TaskInfo> tasks)
    {
        var now = DateTime.UtcNow;
        var sb = new StringBuilder();

        sb.AppendLine($"DỰ ÁN: {project.Name}");
        if (!string.IsNullOrWhiteSpace(project.Description))
            sb.AppendLine($"Mô tả: {project.Description}");
        sb.AppendLine($"Trạng thái: {project.Status} | Ưu tiên: {project.Priority} | Tiến độ ghi nhận: {project.Progress}%");
        sb.AppendLine($"Thời gian: {FormatDate(project.StartDate)} → {FormatDate(project.EndDate)}");
        sb.AppendLine($"Số thành viên: {members.Count}");
        sb.AppendLine();

        var total = tasks.Count;
        var done = tasks.Count(t => t.Status == "Done");
        var overdue = tasks.Where(t => t.Deadline.HasValue && t.Deadline.Value < now && t.Status != "Done").ToList();

        sb.AppendLine($"THỐNG KÊ TASK (tổng {total}):");
        foreach (var g in tasks.GroupBy(t => t.Status))
            sb.AppendLine($"- {g.Key}: {g.Count()}");
        sb.AppendLine($"- Hoàn thành: {done}/{total} ({(total > 0 ? done * 100 / total : 0)}%)");
        sb.AppendLine($"- Quá hạn (chưa Done nhưng đã qua deadline): {overdue.Count}");
        sb.AppendLine();

        sb.AppendLine($"SPRINT ({sprints.Count}):");
        foreach (var s in sprints)
            sb.AppendLine($"- {s.Name} [{s.Status}]: {s.StartDate:dd/MM/yyyy} → {s.EndDate:dd/MM/yyyy}" +
                (string.IsNullOrWhiteSpace(s.Goal) ? "" : $", mục tiêu: {s.Goal}"));
        sb.AppendLine();

        sb.AppendLine($"MILESTONE ({milestones.Count}):");
        foreach (var m in milestones)
            sb.AppendLine($"- {m.Name} [{m.Status}]{(m.DueDate.HasValue ? $" hạn {m.DueDate.Value:dd/MM/yyyy}" : "")}");
        sb.AppendLine();

        sb.AppendLine("KHỐI LƯỢNG CÔNG VIỆC THEO THÀNH VIÊN:");
        foreach (var m in members)
        {
            var name = m.User?.Name ?? "?";
            var assigned = tasks.Where(t => t.Assignee?.Name == name).ToList();
            sb.AppendLine($"- {name} ({m.Role}): {assigned.Count} task được giao, {assigned.Count(t => t.Status == "Done")} đã xong");
        }
        sb.AppendLine();

        sb.AppendLine("DANH SÁCH TASK (quá hạn / đang làm liệt kê trước, tối đa 80):");
        var ordered = tasks
            .OrderBy(t => t.Status == "Done" ? 1 : 0)
            .ThenBy(t => t.Deadline ?? DateTime.MaxValue)
            .Take(80);
        foreach (var t in ordered)
        {
            var overdueTag = t.Deadline.HasValue && t.Deadline.Value < now && t.Status != "Done" ? " [QUÁ HẠN]" : "";
            sb.AppendLine($"- [{t.Status}] {t.Title} — ưu tiên {t.Priority}, phụ trách: {t.Assignee?.Name ?? "chưa gán"}, hạn: {FormatDate(t.Deadline)}{overdueTag}");
        }

        return sb.ToString();
    }

    private static string FormatDate(DateTime? d) => d.HasValue ? d.Value.ToString("dd/MM/yyyy") : "chưa đặt";
}
