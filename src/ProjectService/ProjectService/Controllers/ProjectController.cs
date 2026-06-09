using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectService.Events;
using ProjectService.Models;
using ProjectService.Services;
using ProjectService.Templates;
using System.Security.Claims;

namespace ProjectService.Controllers
{
    [ApiController]
    [Route("api/projects")]
    public class ProjectController : ControllerBase
    {
        private readonly ProjectService.Services.ProjectService _projectService;
        private readonly MemberService    _memberService;
        private readonly SprintService    _sprintService;
        private readonly MilestoneService _milestoneService;
        private readonly IEventPublisher  _eventPublisher;

        public ProjectController(
            ProjectService.Services.ProjectService projectService,
            MemberService    memberService,
            SprintService    sprintService,
            MilestoneService milestoneService,
            IEventPublisher  eventPublisher)
        {
            _projectService   = projectService;
            _memberService    = memberService;
            _sprintService    = sprintService;
            _milestoneService = milestoneService;
            _eventPublisher   = eventPublisher;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetProjects()
        {
            var userId   = GetCurrentUserId();
            var projects = await _projectService.GetUserProjectsAsync(userId);
            return Ok(new { success = true, data = projects });
        }

        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProject(Guid id)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(id, userId);
            if (role == null)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var project = await _projectService.GetProjectByIdAsync(id);
            if (project == null)
                return NotFound(new { success = false, error = new { code = "PROJECT_NOT_FOUND" } });

            return Ok(new { success = true, data = project });
        }

        [Authorize]
        [HttpPost]
        public async Task<IActionResult> CreateProject([FromBody] CreateProjectRequest request)
        {
            var userId     = GetCurrentUserId();
            var templateId = request.TemplateId ?? "blank";
            var template   = TemplateDefinitions.Get(templateId) ?? TemplateDefinitions.Get("blank")!;
            var baseDate   = request.StartDate ?? DateTime.UtcNow;

            // 1. Create project
            var project = new Project
            {
                Id          = Guid.NewGuid(),
                Name        = request.Name,
                Description = request.Description,
                Status      = 0,
                Color       = request.Color,
                StartDate   = request.StartDate,
                EndDate     = request.EndDate,
                CreatedBy   = userId,
                CreatedAt   = DateTime.UtcNow
            };
            await _projectService.CreateProjectAsync(project);

            // 2. Auto-add creator as Owner (role = 0)
            var ownerMember = new Member
            {
                Id        = Guid.NewGuid(),
                ProjectId = project.Id,
                UserId    = userId,
                Role      = 0,
                JoinedAt  = DateTime.UtcNow
            };
            await _memberService.AddMemberAsync(ownerMember);

            // 3. Create sprints based on template
            for (int i = 0; i < template.SprintCount; i++)
            {
                var sprint = new Sprint
                {
                    Id          = Guid.NewGuid(),
                    ProjectId   = project.Id,
                    Name        = $"Sprint {i + 1}",
                    Goal        = i == 0 ? "Khởi động dự án và thiết lập hạ tầng" : null,
                    StartDate   = baseDate.AddDays(i * 14),
                    Status      = 0,
                    CreatedAt   = DateTime.UtcNow
                };
                await _sprintService.CreateSprintAsync(sprint);
            }

            // 4. Create milestones based on template
            foreach (var msDef in template.Milestones)
            {
                var targetDate = msDef.UseEndDate && project.EndDate.HasValue
                    ? project.EndDate.Value
                    : baseDate.AddDays(msDef.OffsetDays);

                var milestone = new Milestone
                {
                    Id          = Guid.NewGuid(),
                    ProjectId   = project.Id,
                    Name        = msDef.Name,
                    TargetDate  = targetDate,
                    Status      = 0,
                    CreatedAt   = DateTime.UtcNow
                };
                await _milestoneService.CreateMilestoneAsync(milestone);
            }

            // 5. Publish project.created — TaskService will seed Kanban columns from this
            await _eventPublisher.PublishAsync("project.created", new ProjectCreatedEvent
            {
                ProjectId  = project.Id,
                TemplateId = templateId,
                Columns    = template.Columns
                    .Select((c, i) => new ColumnDefinition
                    {
                        Name     = c.Name,
                        Type     = c.Type,
                        Position = i
                    })
                    .ToList()
            });

            return CreatedAtAction(nameof(GetProject), new { id = project.Id },
                new { success = true, data = project });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProject(Guid id, [FromBody] UpdateProjectRequest request)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(id, userId);

            if (role == null || role > 1)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var project = await _projectService.UpdateProjectAsync(
                id, request.Name, request.Description, request.Status,
                request.Color, request.StartDate, request.EndDate);
            if (project == null)
                return NotFound(new { success = false, error = new { code = "PROJECT_NOT_FOUND" } });

            return Ok(new { success = true, data = project });
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(Guid id)
        {
            var userId = GetCurrentUserId();
            var role   = await _memberService.GetUserRoleInProjectAsync(id, userId);

            if (role == null || role != 0)
                return StatusCode(403, new { success = false, error = new { code = "FORBIDDEN" } });

            var success = await _projectService.DeleteProjectAsync(id);
            if (!success)
                return NotFound(new { success = false, error = new { code = "PROJECT_NOT_FOUND" } });

            return Ok(new { success = true });
        }

        private Guid GetCurrentUserId() =>
            Guid.Parse(User.FindFirst("sub")!.Value);
    }

    public class CreateProjectRequest
    {
        public string   Name        { get; set; } = string.Empty;
        public string?  Description { get; set; }
        public string?  Color       { get; set; }
        public DateTime? StartDate  { get; set; }
        public DateTime? EndDate    { get; set; }
        public string?  TemplateId  { get; set; }  // "software-dev" | "research" | "event-mgmt" | "blank"
    }

    public class UpdateProjectRequest
    {
        public string   Name        { get; set; } = string.Empty;
        public string?  Description { get; set; }
        public int      Status      { get; set; }
        public string?  Color       { get; set; }
        public DateTime? StartDate  { get; set; }
        public DateTime? EndDate    { get; set; }
    }
}
