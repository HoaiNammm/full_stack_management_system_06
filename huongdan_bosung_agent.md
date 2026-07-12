# Hướng dẫn bổ sung tính năng AI Agent — Hệ thống QLDA

> Tài liệu này dành cho agent code (Claude Code / Cursor / Windsurf) thực hiện trực tiếp.
> Mỗi tính năng có đủ: file cần tạo/sửa, code mẫu đầy đủ, thứ tự thực hiện, điều kiện hoàn thành.
> Không làm thêm gì ngoài những gì được liệt kê — không tự sáng tạo thêm endpoint hay schema.

---

## Tổng quan 6 tính năng

| # | Tính năng | Service chính | Độ phức tạp |
|---|---|---|---|
| F-A | Tạo project từ prompt + gợi ý task/subtask | ProjectService | Trung bình |
| F-B | Báo cáo tiến độ bằng ngôn ngữ tự nhiên | ProjectService + TaskService | Trung bình |
| F-C | Email notification qua SendGrid | NotifyService | Thấp |
| F-D | Tinh chỉnh kết quả AI (conversational refinement) | ProjectService | Thấp |
| F-E | Log prompt AI | ProjectService | Thấp |
| F-F | Invitation flow: Accept / Decline qua email và in-app | ProjectService + NotifyService | Trung bình |

**Thứ tự thực hiện: F-C → F-F → F-B → F-A → F-D → F-E**
- F-F thực hiện ngay sau F-C vì dùng chung EmailService
- F-D và F-E phụ thuộc F-A đã xong — không làm trước

---

## F-C — Email Notification qua SendGrid

### Cài đặt package

```bash
# Trong thư mục NotifyService.Api
dotnet add package SendGrid
```

### Cấu hình `appsettings.json`

Thêm vào `NotifyService.Api/appsettings.json`:

```json
{
  "SendGrid": {
    "ApiKey": "",
    "FromEmail": "noreply@projectmanager.app",
    "FromName": "Project Manager"
  }
}
```

Thêm vào `appsettings.Development.json`:

```json
{
  "SendGrid": {
    "ApiKey": "SG.your_sendgrid_api_key_here"
  }
}
```

> Lấy API key tại https://app.sendgrid.com → Settings → API Keys → Create API Key → Restricted Access → Mail Send.

### File cần tạo

**`NotifyService.Api/Services/EmailService.cs`**

```csharp
using SendGrid;
using SendGrid.Helpers.Mail;
using Microsoft.Extensions.Options;

namespace NotifyService.Api.Services;

public class SendGridOptions
{
    public string ApiKey { get; set; } = string.Empty;
    public string FromEmail { get; set; } = string.Empty;
    public string FromName { get; set; } = string.Empty;
}

public interface IEmailService
{
    Task SendAsync(string toEmail, string toName, string subject, string htmlContent);
}

public class EmailService : IEmailService
{
    private readonly SendGridClient _client;
    private readonly SendGridOptions _options;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IOptions<SendGridOptions> options, ILogger<EmailService> logger)
    {
        _options = options.Value;
        _client = new SendGridClient(_options.ApiKey);
        _logger = logger;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlContent)
    {
        if (string.IsNullOrWhiteSpace(_options.ApiKey))
        {
            _logger.LogWarning("SendGrid API key chưa được cấu hình, bỏ qua gửi email.");
            return;
        }

        var msg = new SendGridMessage
        {
            From = new EmailAddress(_options.FromEmail, _options.FromName),
            Subject = subject,
            HtmlContent = htmlContent
        };
        msg.AddTo(new EmailAddress(toEmail, toName));

        var response = await _client.SendEmailAsync(msg);

        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Body.ReadAsStringAsync();
            _logger.LogError("SendGrid lỗi {StatusCode}: {Body}", response.StatusCode, body);
        }
    }
}
```

**`NotifyService.Api/Services/EmailTemplateService.cs`**

```csharp
namespace NotifyService.Api.Services;

public static class EmailTemplateService
{
    private static string BaseTemplate(string content) => $"""
        <!DOCTYPE html>
        <html>
        <head>
          <meta charset="utf-8">
          <style>
            body {{ font-family: Arial, sans-serif; background: #f5f5f5; margin: 0; padding: 20px; }}
            .container {{ max-width: 600px; margin: 0 auto; background: white;
                          border-radius: 8px; overflow: hidden; box-shadow: 0 2px 4px rgba(0,0,0,0.1); }}
            .header {{ background: #4A90D9; color: white; padding: 24px; text-align: center; }}
            .header h1 {{ margin: 0; font-size: 20px; }}
            .body {{ padding: 24px; color: #333; line-height: 1.6; }}
            .btn {{ display: inline-block; background: #4A90D9; color: white; padding: 12px 24px;
                    border-radius: 6px; text-decoration: none; margin-top: 16px; }}
            .footer {{ padding: 16px 24px; background: #f9f9f9; color: #999; font-size: 12px; }}
          </style>
        </head>
        <body>
          <div class="container">
            <div class="header"><h1>Project Manager</h1></div>
            <div class="body">{content}</div>
            <div class="footer">Bạn nhận email này vì là thành viên của một dự án trên Project Manager.</div>
          </div>
        </body>
        </html>
        """;

    public static string ProjectInvitation(
        string inviterName, string projectName,
        string acceptUrl, string declineUrl) =>
        BaseTemplate($"""
            <h2>Bạn được mời vào dự án</h2>
            <p><strong>{inviterName}</strong> đã mời bạn tham gia dự án <strong>{projectName}</strong>.</p>
            <p style="margin-top:24px;">
              <a href="{acceptUrl}" class="btn" style="background:#22c55e;margin-right:12px;">
                ✅ Chấp nhận lời mời
              </a>
              <a href="{declineUrl}" class="btn" style="background:#ef4444;">
                ❌ Từ chối
              </a>
            </p>
            <p style="color:#999;font-size:12px;margin-top:16px;">
              Liên kết này có hiệu lực trong 7 ngày.
              Nếu bạn không nhận ra lời mời này, hãy bỏ qua email này.
            </p>
            """);

    public static string TaskAssigned(string assignerName, string taskTitle, string projectName, string appUrl) =>
        BaseTemplate($"""
            <h2>Bạn được giao một công việc mới</h2>
            <p><strong>{assignerName}</strong> đã giao cho bạn công việc:</p>
            <p><strong>{taskTitle}</strong> trong dự án <strong>{projectName}</strong></p>
            <a href="{appUrl}" class="btn">Xem công việc</a>
            """);

    public static string TaskDeadlineReminder(string taskTitle, string projectName, int hoursLeft, string appUrl) =>
        BaseTemplate($"""
            <h2>⏰ Nhắc nhở deadline</h2>
            <p>Công việc <strong>{taskTitle}</strong> trong dự án <strong>{projectName}</strong>
            sẽ đến hạn sau <strong>{hoursLeft} giờ</strong>.</p>
            <a href="{appUrl}" class="btn">Xem công việc</a>
            """);

    public static string MentionInComment(string mentionerName, string taskTitle, string commentPreview, string appUrl) =>
        BaseTemplate($"""
            <h2>Bạn được nhắc đến trong bình luận</h2>
            <p><strong>{mentionerName}</strong> đã nhắc đến bạn trong công việc <strong>{taskTitle}</strong>:</p>
            <blockquote style="border-left:3px solid #4A90D9;padding-left:12px;color:#555;">
              {commentPreview}
            </blockquote>
            <a href="{appUrl}" class="btn">Xem bình luận</a>
            """);

    public static string SprintStarted(string sprintName, string projectName, string goal, string endDate, string appUrl) =>
        BaseTemplate($"""
            <h2>Sprint mới bắt đầu</h2>
            <p>Sprint <strong>{sprintName}</strong> của dự án <strong>{projectName}</strong> đã bắt đầu.</p>
            <p><strong>Mục tiêu:</strong> {goal}</p>
            <p><strong>Hạn chót:</strong> {endDate}</p>
            <a href="{appUrl}" class="btn">Xem bảng Kanban</a>
            """);
}
```

### File cần sửa

**`NotifyService.Api/Services/NotificationService.cs`** (hoặc file xử lý event hiện tại)

Tìm các chỗ xử lý event sau và thêm gọi `IEmailService` song song với tạo `UserNotification`:

```csharp
// Inject vào constructor
private readonly IEmailService _emailService;

// Khi xử lý project.member.added
if (userPrefs.EmailEnabled)
{
    await _emailService.SendAsync(
        toEmail: user.Email,
        toName: user.FullName,
        subject: $"Bạn được mời vào dự án {projectName}",
        htmlContent: EmailTemplateService.ProjectInvitation(inviterName, projectName, invitation.AcceptUrl, invitation.DeclineUrl)
    );
}

// Khi xử lý task.assigned
if (userPrefs.EmailEnabled)
{
    await _emailService.SendAsync(
        toEmail: user.Email,
        toName: user.FullName,
        subject: $"Công việc mới: {taskTitle}",
        htmlContent: EmailTemplateService.TaskAssigned(assignerName, taskTitle, projectName, _appUrl)
    );
}
```

> Quy tắc: **chỉ gửi email khi `UserNotificationPreferences.EmailEnabled == true`**. Mặc định là `false` — user phải tự bật trong cài đặt.

**`NotifyService.Api/Models/UserNotificationPreferences.cs`**

Thêm hai trường nếu chưa có:

```csharp
public bool EmailEnabled { get; set; } = false;
public string? EmailAddress { get; set; }  // null = dùng email tài khoản
```

**`NotifyService.Api/Program.cs`**

```csharp
builder.Services.Configure<SendGridOptions>(
    builder.Configuration.GetSection("SendGrid"));
builder.Services.AddScoped<IEmailService, EmailService>();
```

### Background job nhắc deadline (tùy chọn)

**`NotifyService.Api/Jobs/DeadlineReminderJob.cs`**

```csharp
// Cài thêm: dotnet add package Hangfire.AspNetCore (hoặc dùng IHostedService đơn giản hơn)
// Chạy mỗi giờ, quét task sắp đến hạn trong 24h
public class DeadlineReminderJob
{
    // Gọi TaskService internal API để lấy danh sách task sắp hết hạn
    // GET /internal/tasks/due-soon?withinHours=24
    // Gửi email nhắc nhở cho assignee
}
```

> Nếu không muốn thêm Hangfire, bỏ phần này — ba loại email (mời dự án, giao task, mention) là đủ cho demo.

### Điều kiện hoàn thành F-C

- [ ] Build NotifyService thành công sau khi thêm package SendGrid
- [ ] Gửi được email thật khi một thành viên được mời vào dự án (test bằng SendGrid sandbox hoặc email thật)
- [ ] Không gửi email khi `EmailEnabled = false`
- [ ] Log lỗi rõ ràng nếu SendGrid API key chưa cấu hình (không throw exception)

---

## F-B — Báo cáo tiến độ bằng ngôn ngữ tự nhiên

### Cài đặt package

```bash
# Trong ProjectService (service điều phối báo cáo)
dotnet add package Anthropic.SDK
# Hoặc dùng HttpClient gọi thẳng nếu không muốn thêm package
```

### Cấu hình `appsettings.json`

Thêm vào `ProjectService/appsettings.json`:

```json
{
  "Anthropic": {
    "ApiKey": "",
    "Model": "claude-sonnet-4-6",
    "MaxTokens": 1024
  },
  "Services": {
    "TaskServiceUrl": "http://taskservice:8080",
    "ProjectServiceUrl": "http://projectservice:8080"
  }
}
```

### File cần tạo

**`ProjectService/Services/ProjectReportDataService.cs`**

Thu thập dữ liệu thực từ DB trước khi gọi Claude — Claude không được tự bịa số liệu:

```csharp
namespace ProjectService.Services;

public record SprintSummary(
    string Name, string Goal,
    DateOnly EndDate, int DaysLeft,
    int TotalTasks, int DoneTasks, int InProgressTasks,
    int BlockedTasks, int OverdueTasks);

public record MemberSummary(
    string Name, int AssignedTasks,
    int DoneTasks, int InProgressTasks);

public record ProjectReportContext(
    string ProjectName, string Status,
    DateOnly StartDate, DateOnly? EndDate, int DaysRemaining,
    int TotalTasks, int DoneTasks, int CompletionRate,
    SprintSummary? ActiveSprint,
    List<MemberSummary> Members);

public class ProjectReportDataService
{
    private readonly ProjectDbContext _db;
    private readonly ITaskServiceClient _taskClient;

    public ProjectReportDataService(ProjectDbContext db, ITaskServiceClient taskClient)
    {
        _db = db;
        _taskClient = taskClient;
    }

    public async Task<ProjectReportContext> GatherContextAsync(Guid projectId)
    {
        var project = await _db.Projects
            .FirstOrDefaultAsync(p => p.Id == projectId && p.DeletedAt == null)
            ?? throw new NotFoundException("Dự án không tồn tại");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);

        // Lấy sprint đang active
        var activeSprint = await _db.Sprints
            .Where(s => s.ProjectId == projectId && s.Status == SprintStatus.Active)
            .FirstOrDefaultAsync();

        // Lấy dữ liệu task từ TaskService
        var taskStats = await _taskClient.GetProjectTaskStatsAsync(projectId);
        var memberStats = await _taskClient.GetMemberTaskStatsAsync(projectId);

        SprintSummary? sprintSummary = null;
        if (activeSprint != null)
        {
            var sprintTaskStats = await _taskClient.GetSprintTaskStatsAsync(activeSprint.Id);
            sprintSummary = new SprintSummary(
                Name: activeSprint.Name,
                Goal: activeSprint.Goal ?? "",
                EndDate: activeSprint.EndDate,
                DaysLeft: Math.Max(0, activeSprint.EndDate.DayNumber - today.DayNumber),
                TotalTasks: sprintTaskStats.Total,
                DoneTasks: sprintTaskStats.Done,
                InProgressTasks: sprintTaskStats.InProgress,
                BlockedTasks: sprintTaskStats.Blocked,
                OverdueTasks: sprintTaskStats.Overdue
            );
        }

        return new ProjectReportContext(
            ProjectName: project.Name,
            Status: project.Status.ToString(),
            StartDate: project.StartDate,
            EndDate: project.EndDate,
            DaysRemaining: project.EndDate.HasValue
                ? Math.Max(0, project.EndDate.Value.DayNumber - today.DayNumber)
                : -1,
            TotalTasks: taskStats.Total,
            DoneTasks: taskStats.Done,
            CompletionRate: taskStats.Total > 0 ? taskStats.Done * 100 / taskStats.Total : 0,
            ActiveSprint: sprintSummary,
            Members: memberStats.Select(m => new MemberSummary(
                Name: m.Name,
                AssignedTasks: m.Assigned,
                DoneTasks: m.Done,
                InProgressTasks: m.InProgress
            )).ToList()
        );
    }
}
```

**`ProjectService/Services/ProjectAiReportService.cs`**

```csharp
using System.Text;
using System.Text.Json;

namespace ProjectService.Services;

public class ProjectAiReportService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ProjectReportDataService _dataService;
    private readonly ILogger<ProjectAiReportService> _logger;

    public ProjectAiReportService(
        IHttpClientFactory httpClientFactory,
        IConfiguration config,
        ProjectReportDataService dataService,
        ILogger<ProjectAiReportService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("anthropic");
        _config = config;
        _dataService = dataService;
        _logger = logger;
    }

    public async Task<string> GenerateReportAsync(Guid projectId, string question)
    {
        // Bước 1: thu thập dữ liệu thật
        var context = await _dataService.GatherContextAsync(projectId);
        var contextJson = JsonSerializer.Serialize(context, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        // Bước 2: xây dựng prompt
        var systemPrompt = """
            Bạn là trợ lý quản lý dự án. Dựa trên dữ liệu thực tế được cung cấp,
            hãy trả lời câu hỏi của người dùng bằng tiếng Việt, ngắn gọn và rõ ràng (tối đa 200 từ).

            Quy tắc bắt buộc:
            - Chỉ dùng số liệu từ dữ liệu được cung cấp, không tự bịa thêm
            - Nếu phát hiện rủi ro (task bị chặn, quá hạn, member overload >5 task InProgress),
              hãy cảnh báo rõ ràng với emoji ⚠️
            - Kết thúc bằng 1-2 khuyến nghị hành động cụ thể nếu có vấn đề
            - Không dùng markdown heading, chỉ dùng văn xuôi và bullet point đơn giản
            """;

        var userMessage = $"""
            Dữ liệu dự án hiện tại:
            {contextJson}

            Câu hỏi: {question}
            Hôm nay: {DateTime.UtcNow:yyyy-MM-dd}
            """;

        // Bước 3: gọi Anthropic API
        return await CallAnthropicAsync(systemPrompt, userMessage);
    }

    private async Task<string> CallAnthropicAsync(string systemPrompt, string userMessage)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        var model = _config["Anthropic:Model"] ?? "claude-sonnet-4-6";
        var maxTokens = _config.GetValue<int>("Anthropic:MaxTokens", 1024);

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            system = systemPrompt,
            messages = new[]
            {
                new { role = "user", content = userMessage }
            }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Anthropic API lỗi: {Error}", error);
            throw new ExternalServiceException("Không thể tạo báo cáo lúc này, vui lòng thử lại.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}
```

**`ProjectService/Controllers/ProjectReportController.cs`**

```csharp
[ApiController]
[Route("api/projects/{projectId}/report")]
[Authorize]
public class ProjectReportController : ControllerBase
{
    private readonly ProjectAiReportService _reportService;

    public ProjectReportController(ProjectAiReportService reportService)
    {
        _reportService = reportService;
    }

    [HttpPost]
    public async Task<IActionResult> GetReport(
        Guid projectId,
        [FromBody] ReportRequest request)
    {
        // Kiểm quyền: chỉ thành viên dự án được hỏi
        var userId = User.GetUserId();
        // ... gọi CheckPermissionAsync như các controller khác

        var report = await _reportService.GenerateReportAsync(projectId, request.Question);
        return Ok(new { report });
    }
}

public record ReportRequest(string Question);
```

**`TaskService/Controllers/InternalTaskStatsController.cs`** (thêm vào InternalController hiện tại)

```csharp
// Thêm vào InternalController của TaskService
[HttpGet("projects/{projectId}/task-stats")]
public async Task<IActionResult> GetProjectTaskStats(Guid projectId)
{
    var stats = await _taskService.GetProjectStatsAsync(projectId);
    return Ok(stats);
}

[HttpGet("projects/{projectId}/member-stats")]
public async Task<IActionResult> GetMemberTaskStats(Guid projectId)
{
    var stats = await _taskService.GetMemberStatsAsync(projectId);
    return Ok(stats);
}

[HttpGet("sprints/{sprintId}/task-stats")]
public async Task<IActionResult> GetSprintTaskStats(Guid sprintId)
{
    var stats = await _taskService.GetSprintStatsAsync(sprintId);
    return Ok(stats);
}
```

### File cần sửa

**`ProjectService/Program.cs`**

```csharp
builder.Services.AddHttpClient("anthropic");
builder.Services.AddScoped<ProjectReportDataService>();
builder.Services.AddScoped<ProjectAiReportService>();
```

### Điều kiện hoàn thành F-B

- [ ] `POST /api/projects/{projectId}/report` trả về văn bản tiếng Việt mô tả tiến độ
- [ ] Số liệu trong báo cáo khớp với dữ liệu thật trong DB (kiểm tra bằng tay)
- [ ] Khi project có task bị blocked hoặc overdue, báo cáo có cảnh báo ⚠️
- [ ] Không gọi được nếu không phải thành viên dự án (403)
- [ ] Lỗi Anthropic API trả về thông báo người dùng đọc được, không trả stack trace

---

## F-A — Tạo project từ prompt + gợi ý task/subtask

### File cần tạo

**`ProjectService/Models/GenerateProjectRequest.cs`**

```csharp
namespace ProjectService.Models;

public record GenerateProjectRequest(
    string Prompt,           // mô tả dự án bằng ngôn ngữ tự nhiên
    bool Preview = true      // true = chỉ preview, chưa lưu; false = tạo ngay
);

public record GeneratedProjectDto(
    string Name,
    string Description,
    string StartDate,        // YYYY-MM-DD
    string? EndDate,
    string Color,
    List<GeneratedSprintDto> Sprints,
    List<GeneratedMilestoneDto> Milestones,
    List<GeneratedTaskDto> Tasks
);

public record GeneratedSprintDto(
    string Name, string Goal,
    string StartDate, string EndDate);

public record GeneratedMilestoneDto(
    string Name, string DueDate);

public record GeneratedTaskDto(
    string Title, string Description,
    int Priority,            // 0=thấp, 1=trung bình, 2=cao, 3=khẩn
    int? SprintIndex,        // index trong mảng Sprints
    int EstimatedHours,
    List<GeneratedSubtaskDto> Subtasks);

public record GeneratedSubtaskDto(string Title);
```

**`ProjectService/Services/ProjectGeneratorService.cs`**

```csharp
using System.Text;
using System.Text.Json;

namespace ProjectService.Services;

public class ProjectGeneratorService
{
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _config;
    private readonly ILogger<ProjectGeneratorService> _logger;

    // Schema JSON để nhét vào system prompt
    private const string JsonSchema = """
        {
          "project": {
            "name": "string",
            "description": "string",
            "startDate": "YYYY-MM-DD",
            "endDate": "YYYY-MM-DD hoặc null",
            "color": "hex color string, ví dụ #4A90D9"
          },
          "sprints": [
            {
              "name": "string",
              "goal": "string",
              "startDate": "YYYY-MM-DD",
              "endDate": "YYYY-MM-DD"
            }
          ],
          "milestones": [
            {
              "name": "string",
              "dueDate": "YYYY-MM-DD"
            }
          ],
          "tasks": [
            {
              "title": "string",
              "description": "string",
              "priority": 0,
              "sprintIndex": 0,
              "estimatedHours": 4,
              "subtasks": [
                { "title": "string" }
              ]
            }
          ]
        }
        """;

    public ProjectGeneratorService(
        IHttpClientFactory factory,
        IConfiguration config,
        ILogger<ProjectGeneratorService> logger)
    {
        _httpClient = factory.CreateClient("anthropic");
        _config = config;
        _logger = logger;
    }

    public async Task<GeneratedProjectDto> GenerateAsync(string prompt)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

        var systemPrompt = $"""
            Bạn là một project manager AI chuyên nghiệp.
            Người dùng mô tả dự án bằng ngôn ngữ tự nhiên (tiếng Việt hoặc tiếng Anh).
            Hãy phân tích và trả về DUY NHẤT một JSON object hợp lệ theo schema dưới đây.
            Không được có bất kỳ text, markdown, hay giải thích nào trước hoặc sau JSON.

            Schema bắt buộc:
            {JsonSchema}

            Quy tắc bắt buộc:
            - startDate của project không được trước hôm nay ({today})
            - Chia sprint 1–2 tuần, tự tính số sprint từ tổng thời gian dự án
            - Mỗi task có sprintIndex là vị trí trong mảng sprints (0-based), null nếu chưa thuộc sprint nào
            - Chỉ tạo subtask khi task phức tạp (estimatedHours > 6) hoặc cần chia nhỏ rõ ràng
            - Tối đa 15 task, 3 subtask mỗi task
            - priority: 0=thấp, 1=trung bình, 2=cao, 3=khẩn
            - color: chọn màu phù hợp với tính chất dự án (hex string)
            - Tất cả text dùng tiếng Việt nếu prompt bằng tiếng Việt
            """;

        var userMessage = $"Tạo kế hoạch dự án: {prompt}\nHôm nay: {today}";

        var responseText = await CallAnthropicAsync(systemPrompt, userMessage, maxTokens: 2000);

        // Parse JSON — thêm fallback nếu Claude bọc trong ```json ... ```
        var json = responseText.Trim();
        if (json.StartsWith("```"))
        {
            var start = json.IndexOf('\n') + 1;
            var end = json.LastIndexOf("```");
            json = json[start..end].Trim();
        }

        try
        {
            return JsonSerializer.Deserialize<GeneratedProjectDto>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
                ?? throw new InvalidOperationException("JSON rỗng");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Không parse được JSON từ Claude: {Json}", json);
            throw new ExternalServiceException("Không thể tạo kế hoạch dự án, vui lòng thử lại với mô tả chi tiết hơn.");
        }
    }

    private async Task<string> CallAnthropicAsync(string systemPrompt, string userMessage, int maxTokens)
    {
        var apiKey = _config["Anthropic:ApiKey"];
        var model = _config["Anthropic:Model"] ?? "claude-sonnet-4-6";

        var requestBody = new
        {
            model,
            max_tokens = maxTokens,
            system = systemPrompt,
            messages = new[] { new { role = "user", content = userMessage } }
        };

        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.anthropic.com/v1/messages");
        request.Headers.Add("x-api-key", apiKey);
        request.Headers.Add("anthropic-version", "2023-06-01");
        request.Content = new StringContent(
            JsonSerializer.Serialize(requestBody),
            Encoding.UTF8,
            "application/json");

        var response = await _httpClient.SendAsync(request);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Anthropic API lỗi: {Error}", error);
            throw new ExternalServiceException("Không thể kết nối AI, vui lòng thử lại.");
        }

        var json = await response.Content.ReadAsStringAsync();
        using var doc = JsonDocument.Parse(json);
        return doc.RootElement
            .GetProperty("content")[0]
            .GetProperty("text")
            .GetString() ?? string.Empty;
    }
}
```

**`ProjectService/Services/ProjectOrchestratorService.cs`**

Tạo project và toàn bộ dữ liệu con tuần tự — mỗi bước phụ thuộc kết quả bước trước:

```csharp
namespace ProjectService.Services;

public class ProjectOrchestratorService
{
    private readonly ProjectDbContext _db;
    private readonly ILogger<ProjectOrchestratorService> _logger;

    public ProjectOrchestratorService(ProjectDbContext db, ILogger<ProjectOrchestratorService> logger)
    {
        _db = db;
        _logger = logger;
    }

    public async Task<Guid> CreateFromGeneratedAsync(GeneratedProjectDto dto, Guid ownerId)
    {
        using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            // Bước 1: tạo Project
            var project = new Project
            {
                Id = Guid.NewGuid(),
                Name = dto.Name,
                Description = dto.Description,
                StartDate = DateOnly.Parse(dto.StartDate),
                EndDate = dto.EndDate != null ? DateOnly.Parse(dto.EndDate) : null,
                Color = dto.Color,
                Status = ProjectStatus.Active,
                OwnerId = ownerId,
                CreatedAt = DateTime.UtcNow
            };
            _db.Projects.Add(project);

            // Gán owner làm thành viên
            _db.Members.Add(new ProjectMember
            {
                ProjectId = project.Id,
                UserId = ownerId,
                Role = MemberRole.Owner,
                JoinedAt = DateTime.UtcNow
            });

            await _db.SaveChangesAsync();

            // Bước 2: tạo Sprint, lưu mapping index → Id
            var sprintIds = new Dictionary<int, Guid>();
            for (int i = 0; i < dto.Sprints.Count; i++)
            {
                var s = dto.Sprints[i];
                var sprint = new Sprint
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    Name = s.Name,
                    Goal = s.Goal,
                    StartDate = DateOnly.Parse(s.StartDate),
                    EndDate = DateOnly.Parse(s.EndDate),
                    Status = SprintStatus.Planning
                };
                _db.Sprints.Add(sprint);
                sprintIds[i] = sprint.Id;
            }
            await _db.SaveChangesAsync();

            // Bước 3: tạo Milestone
            foreach (var m in dto.Milestones)
            {
                _db.Milestones.Add(new Milestone
                {
                    Id = Guid.NewGuid(),
                    ProjectId = project.Id,
                    Name = m.Name,
                    DueDate = DateOnly.Parse(m.DueDate)
                });
            }
            await _db.SaveChangesAsync();

            // Bước 4 & 5: task và subtask — gọi TaskService qua HTTP
            // (TaskDB khác ProjectDB — không dùng chung DbContext)
            // Phát event project.created với payload đầy đủ
            // TaskService consumer nhận và tạo task/subtask

            await transaction.CommitAsync();
            return project.Id;
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }
}
```

> **Lưu ý quan trọng cho task/subtask:** vì TaskDB và ProjectDB là hai database khác nhau, không thể dùng chung transaction. Phát event `project.generated` với payload chứa toàn bộ task/subtask, TaskService consumer nhận và tạo. Nếu TaskService lỗi, task không được tạo nhưng project vẫn tồn tại — đây là đánh đổi chấp nhận được cho đồ án.

**`ProjectService/Controllers/ProjectGeneratorController.cs`**

```csharp
[ApiController]
[Route("api/projects/generate")]
[Authorize]
public class ProjectGeneratorController : ControllerBase
{
    private readonly ProjectGeneratorService _generatorService;
    private readonly ProjectOrchestratorService _orchestratorService;

    [HttpPost("preview")]
    public async Task<IActionResult> Preview([FromBody] GenerateProjectRequest request)
    {
        // Chỉ gọi AI, trả về kết quả để user xem trước, chưa lưu gì vào DB
        var generated = await _generatorService.GenerateAsync(request.Prompt);
        return Ok(generated);
    }

    [HttpPost("confirm")]
    public async Task<IActionResult> Confirm([FromBody] GeneratedProjectDto dto)
    {
        // User đã xem preview và xác nhận → lưu vào DB
        var ownerId = User.GetUserId();
        var projectId = await _orchestratorService.CreateFromGeneratedAsync(dto, ownerId);
        return Ok(new { projectId });
    }
}
```

> Tách thành hai endpoint `preview` và `confirm` thay vì một endpoint — tránh lưu dữ liệu rác khi user thấy kết quả AI không ưng ý và đóng luôn cửa sổ.

### File cần sửa

**`ProjectService/Program.cs`**

```csharp
builder.Services.AddScoped<ProjectGeneratorService>();
builder.Services.AddScoped<ProjectOrchestratorService>();
// HttpClient "anthropic" đã đăng ký ở F-B, không cần thêm
```

### Điều kiện hoàn thành F-A

- [ ] `POST /api/projects/generate/preview` trả về JSON có cấu trúc đầy đủ (project, sprints, milestones, tasks, subtasks)
- [ ] `POST /api/projects/generate/confirm` tạo được project thật trong DB với sprint và milestone
- [ ] Prompt tiếng Việt → kết quả tiếng Việt
- [ ] Parse JSON không lỗi khi Claude trả về có bọc trong code block
- [ ] Không được tạo project với `startDate` ở quá khứ (kiểm tra ở bước orchestrate, không chỉ tin AI)
- [ ] Lỗi Anthropic API trả về thông báo người dùng đọc được

---

## F-D — Tinh chỉnh kết quả AI (Conversational Refinement)

> Yêu cầu: F-A đã hoàn thành. F-D bổ sung thêm vào `ProjectGeneratorService` và `ProjectGeneratorController` đã có.

### Ba UX pattern trên màn hình preview

Màn hình preview sau khi AI tạo xong hiển thị ba lựa chọn cho ba tình huống khác nhau:

```
[Preview kết quả AI]

 ✏️  Chỉnh sửa trực tiếp    ← sửa text/ngày inline, không gọi AI
 💬  Yêu cầu thay đổi...    ← mở ô input, gọi AI lần 2 (F-D)
 🔄  Tạo lại từ đầu         ← reset, nhập prompt mới → gọi /preview
 ✅  Xác nhận tạo dự án     ← gọi /confirm
```

### File cần sửa

**`ProjectService/Models/GenerateProjectRequest.cs`** — thêm record mới:

```csharp
public record RefineProjectRequest(
    GeneratedProjectDto PreviousResult,  // kết quả preview lần trước
    string RefinementRequest             // yêu cầu thay đổi của user
);
```

**`ProjectService/Services/ProjectGeneratorService.cs`** — thêm method `RefineAsync`:

```csharp
public async Task<GeneratedProjectDto> RefineAsync(
    GeneratedProjectDto previousResult,
    string refinementRequest)
{
    var today = DateTime.UtcNow.ToString("yyyy-MM-dd");

    // System prompt giữ nguyên như GenerateAsync
    var systemPrompt = $"""
        Bạn là một project manager AI chuyên nghiệp.
        Người dùng muốn chỉnh sửa kế hoạch dự án hiện tại.
        Hãy áp dụng thay đổi được yêu cầu và trả về DUY NHẤT một JSON object hợp lệ theo đúng schema cũ.
        Không được có bất kỳ text, markdown, hay giải thích nào trước hoặc sau JSON.

        Schema bắt buộc:
        {JsonSchema}

        Quy tắc bắt buộc:
        - Giữ nguyên các phần không được yêu cầu thay đổi
        - Chỉ chỉnh sửa đúng phần user yêu cầu
        - startDate của project không được trước hôm nay ({today})
        - Tối đa 15 task, 3 subtask mỗi task
        - Tất cả text dùng tiếng Việt nếu kế hoạch cũ bằng tiếng Việt
        """;

    var previousJson = JsonSerializer.Serialize(previousResult,
        new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });

    var userMessage = $"""
        Kế hoạch dự án hiện tại:
        {previousJson}

        Yêu cầu thay đổi: {refinementRequest}
        Hôm nay: {today}
        """;

    var responseText = await CallAnthropicAsync(systemPrompt, userMessage, maxTokens: 2000);

    // Parse JSON — giữ nguyên fallback như GenerateAsync
    var json = responseText.Trim();
    if (json.StartsWith("```"))
    {
        var start = json.IndexOf('\n') + 1;
        var end = json.LastIndexOf("```");
        json = json[start..end].Trim();
    }

    try
    {
        return JsonSerializer.Deserialize<GeneratedProjectDto>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true })
            ?? throw new InvalidOperationException("JSON rỗng");
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Không parse được JSON từ Claude (refine): {Json}", json);
        throw new ExternalServiceException("Không thể cập nhật kế hoạch, vui lòng thử lại.");
    }
}
```

**`ProjectService/Controllers/ProjectGeneratorController.cs`** — thêm endpoint `/refine`:

```csharp
[HttpPost("refine")]
public async Task<IActionResult> Refine([FromBody] RefineProjectRequest request)
{
    if (string.IsNullOrWhiteSpace(request.RefinementRequest))
        return BadRequest(new { message = "Vui lòng mô tả thay đổi bạn muốn thực hiện." });

    var refined = await _generatorService.RefineAsync(
        request.PreviousResult,
        request.RefinementRequest);

    return Ok(refined);
}
```

### Luồng hoàn chỉnh từ phía frontend

```
User nhập prompt
    ↓
POST /generate/preview   → hiển thị preview lần 1
    ↓
User thấy không ưng một phần
    ↓
User nhập yêu cầu chỉnh sửa vào ô input
    ↓
POST /generate/refine    → hiển thị preview lần 2 (có thể refine nhiều lần)
    ↓
User hài lòng
    ↓
POST /generate/confirm   → tạo dự án thật
```

Không giới hạn số lần refine — mỗi lần gọi `/refine` đều truyền `previousResult` là kết quả preview mới nhất, không phải kết quả gốc.

### Điều kiện hoàn thành F-D

- [ ] `POST /api/projects/generate/refine` trả về JSON cùng schema với `/preview`
- [ ] Phần không được yêu cầu thay đổi giữ nguyên (kiểm bằng tay: yêu cầu "thêm sprint 3" không được xóa sprint 1, 2)
- [ ] Yêu cầu thay đổi rỗng → 400 Bad Request
- [ ] Có thể gọi `/refine` nhiều lần liên tiếp trước khi `/confirm`

---

## F-E — Log Prompt AI

> Yêu cầu: F-A và F-B đã hoàn thành. F-E bổ sung logging vào các service đã có, không tạo controller mới.

### Migration — thêm bảng `AiPromptLogs`

**`ProjectService/Migrations/AddAiPromptLogs.cs`** (tạo bằng `dotnet ef migrations add AddAiPromptLogs`)

Hoặc thêm thủ công vào `ProjectDbContext`:

```csharp
// ProjectService/Data/ProjectDbContext.cs
public DbSet<AiPromptLog> AiPromptLogs { get; set; }

protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    // ... các config hiện tại ...

    modelBuilder.Entity<AiPromptLog>(e =>
    {
        e.HasKey(x => x.Id);
        e.Property(x => x.Feature).HasMaxLength(50).IsRequired();
        e.Property(x => x.Prompt).IsRequired();
        e.HasIndex(x => x.UserId);
        e.HasIndex(x => x.ProjectId);
        e.HasIndex(x => x.CreatedAt);
    });
}
```

**`ProjectService/Models/AiPromptLog.cs`**

```csharp
namespace ProjectService.Models;

public class AiPromptLog
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    public Guid? ProjectId { get; set; }

    /// <summary>
    /// Loại tính năng: 'generate_project' | 'refine_project' | 'progress_report'
    /// </summary>
    public string Feature { get; set; } = string.Empty;

    public string Prompt { get; set; } = string.Empty;
    public string? Response { get; set; }
    public int? TokensUsed { get; set; }
    public int DurationMs { get; set; }
    public bool Success { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

### File cần sửa

**`ProjectService/Services/ProjectGeneratorService.cs`** — wrap `CallAnthropicAsync` bằng logging:

```csharp
// Inject thêm vào constructor
private readonly ProjectDbContext _db;

// Method mới — thay thế mọi chỗ gọi CallAnthropicAsync trực tiếp
private async Task<string> CallWithLoggingAsync(
    string systemPrompt,
    string userMessage,
    int maxTokens,
    string feature,
    Guid userId,
    Guid? projectId = null)
{
    var sw = System.Diagnostics.Stopwatch.StartNew();
    var log = new AiPromptLog
    {
        UserId = userId,
        ProjectId = projectId,
        Feature = feature,
        Prompt = userMessage   // không log systemPrompt — dài và không thay đổi
    };

    try
    {
        var response = await CallAnthropicAsync(systemPrompt, userMessage, maxTokens);
        sw.Stop();

        log.Response = response.Length > 5000
            ? response[..5000] + "... [truncated]"
            : response;
        log.DurationMs = (int)sw.ElapsedMilliseconds;
        log.Success = true;

        // Tạm thời không parse TokensUsed từ response header
        // (Anthropic trả về trong body usage.input_tokens + usage.output_tokens)

        return response;
    }
    catch (Exception ex)
    {
        sw.Stop();
        log.DurationMs = (int)sw.ElapsedMilliseconds;
        log.Success = false;
        log.ErrorMessage = ex.Message;
        throw;
    }
    finally
    {
        // Fire-and-forget — không để log chặn response
        _ = Task.Run(async () =>
        {
            try { _db.AiPromptLogs.Add(log); await _db.SaveChangesAsync(); }
            catch (Exception logEx) { _logger.LogWarning(logEx, "Không thể lưu AI log"); }
        });
    }
}
```

Sửa `GenerateAsync` và `RefineAsync` để gọi `CallWithLoggingAsync` thay vì `CallAnthropicAsync`:

```csharp
// Trong GenerateAsync — thêm userId parameter
public async Task<GeneratedProjectDto> GenerateAsync(string prompt, Guid userId)
{
    // ... build systemPrompt, userMessage như cũ ...
    var responseText = await CallWithLoggingAsync(
        systemPrompt, userMessage, maxTokens: 2000,
        feature: "generate_project", userId: userId);
    // ... parse JSON như cũ ...
}

// Trong RefineAsync
public async Task<GeneratedProjectDto> RefineAsync(
    GeneratedProjectDto previousResult,
    string refinementRequest,
    Guid userId)
{
    // ...
    var responseText = await CallWithLoggingAsync(
        systemPrompt, userMessage, maxTokens: 2000,
        feature: "refine_project", userId: userId);
    // ...
}
```

**`ProjectService/Services/ProjectAiReportService.cs`** — tương tự, thêm logging vào `GenerateReportAsync`:

```csharp
public async Task<string> GenerateReportAsync(Guid projectId, string question, Guid userId)
{
    var context = await _dataService.GatherContextAsync(projectId);
    // ... build prompt như cũ ...

    return await CallWithLoggingAsync(
        systemPrompt, userMessage, maxTokens: 1024,
        feature: "progress_report", userId: userId, projectId: projectId);
}
```

**`ProjectService/Controllers/ProjectGeneratorController.cs`** — truyền userId vào service:

```csharp
[HttpPost("preview")]
public async Task<IActionResult> Preview([FromBody] GenerateProjectRequest request)
{
    var userId = User.GetUserId();
    var generated = await _generatorService.GenerateAsync(request.Prompt, userId);
    return Ok(generated);
}

[HttpPost("refine")]
public async Task<IActionResult> Refine([FromBody] RefineProjectRequest request)
{
    var userId = User.GetUserId();
    var refined = await _generatorService.RefineAsync(
        request.PreviousResult, request.RefinementRequest, userId);
    return Ok(refined);
}
```

### Endpoint xem log (chỉ Owner xem log dự án của mình)

Thêm vào `ProjectGeneratorController`:

```csharp
[HttpGet("/api/projects/{projectId}/ai-logs")]
public async Task<IActionResult> GetAiLogs(Guid projectId, [FromQuery] int page = 1)
{
    var userId = User.GetUserId();
    // Kiểm quyền: chỉ Owner/Manager mới xem được log
    // ... CheckPermissionAsync như các controller khác ...

    var logs = await _db.AiPromptLogs
        .Where(l => l.ProjectId == projectId)
        .OrderByDescending(l => l.CreatedAt)
        .Skip((page - 1) * 20)
        .Take(20)
        .Select(l => new {
            l.Id, l.Feature, l.Prompt,
            l.DurationMs, l.Success, l.ErrorMessage,
            l.CreatedAt
            // Không trả Response — có thể dài và chứa dữ liệu nhạy cảm
        })
        .ToListAsync();

    return Ok(logs);
}
```

> `Response` không trả trong danh sách — chỉ trả khi xem chi tiết từng log nếu cần debug. Tránh leak nội dung AI response ra ngoài không cần thiết.

### Điều kiện hoàn thành F-E

- [ ] Migration `AddAiPromptLogs` chạy thành công trên ProjectDB
- [ ] Mỗi lần gọi `/preview`, `/refine`, `/report` đều tạo một bản ghi `AiPromptLogs`
- [ ] Log ghi đúng: `Success=true` khi AI trả về, `Success=false` khi có lỗi
- [ ] `GET /api/projects/{projectId}/ai-logs` trả danh sách, chỉ Owner/Manager được gọi
- [ ] Log không chặn response — nếu lưu log lỗi, API vẫn trả kết quả bình thường
- [ ] `Response` không xuất hiện trong response của endpoint danh sách

---

## F-F — Invitation Flow (Accept / Decline)

> Yêu cầu: F-C đã hoàn thành. F-F bổ sung flow xác nhận lời mời qua cả hai kênh: link trong email và nút trong app — giống GitHub.

### Tổng quan luồng

```
Owner/Manager mời thành viên
        ↓
ProjectService tạo bản ghi Invitation (trạng thái Pending)
        ↓
Phát event project.member.invited
        ↓
NotifyService nhận event:
  ├── Gửi email có nút Accept / Decline (link token 7 ngày)
  └── Tạo in-app notification có nút Đồng ý / Từ chối
        ↓
User click nút (email hoặc app)
        ↓
ProjectService xử lý Accept → thêm thành viên thật
                hoặc Decline → đánh dấu Declined
        ↓
Phát event project.member.added (nếu Accept)
NotifyService gửi thông báo cho Owner biết kết quả
```

---

### Migration — thêm bảng `ProjectInvitations`

Thêm vào `ProjectService/Data/ProjectDbContext.cs`:

```csharp
public DbSet<ProjectInvitation> ProjectInvitations { get; set; }

modelBuilder.Entity<ProjectInvitation>(e =>
{
    e.HasKey(x => x.Id);
    e.Property(x => x.Token).HasMaxLength(128).IsRequired();
    e.HasIndex(x => x.Token).IsUnique();
    e.HasIndex(x => new { x.ProjectId, x.InviteeEmail });
});
```

**`ProjectService/Models/ProjectInvitation.cs`**

```csharp
namespace ProjectService.Models;

public enum InvitationStatus { Pending, Accepted, Declined, Expired }

public class ProjectInvitation
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public Guid ProjectId { get; set; }
    public Guid InviterId { get; set; }       // người mời
    public Guid? InviteeId { get; set; }      // null nếu mời qua email chưa có tài khoản
    public string InviteeEmail { get; set; } = string.Empty;
    public MemberRole Role { get; set; } = MemberRole.Member;
    public string Token { get; set; } = string.Empty;   // UUID ngẫu nhiên, dùng trong link
    public InvitationStatus Status { get; set; } = InvitationStatus.Pending;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? RespondedAt { get; set; }
}
```

---

### File cần sửa

**`ProjectService/Controllers/MemberController.cs`** (hoặc file xử lý mời thành viên hiện tại)

Sửa endpoint mời thành viên, thêm bước tạo Invitation trước khi phát event:

```csharp
[HttpPost("{projectId}/invitations")]
public async Task<IActionResult> InviteMember(Guid projectId, [FromBody] InviteMemberRequest request)
{
    // ... kiểm quyền Owner/Manager như hiện tại ...

    // Kiểm không mời người đã trong dự án
    var alreadyMember = await _db.Members
        .AnyAsync(m => m.ProjectId == projectId && m.UserId == request.UserId);
    if (alreadyMember)
        return BadRequest(new { message = "Người này đã là thành viên của dự án." });

    // Kiểm không có invitation Pending trùng
    var existingInvite = await _db.ProjectInvitations
        .AnyAsync(i => i.ProjectId == projectId
                    && i.InviteeEmail == request.Email
                    && i.Status == InvitationStatus.Pending
                    && i.ExpiresAt > DateTime.UtcNow);
    if (existingInvite)
        return BadRequest(new { message = "Đã có lời mời đang chờ phản hồi cho email này." });

    // Tạo invitation với token ngẫu nhiên
    var invitation = new ProjectInvitation
    {
        ProjectId = projectId,
        InviterId = User.GetUserId(),
        InviteeId = request.UserId,
        InviteeEmail = request.Email,
        Role = request.Role,
        Token = Guid.NewGuid().ToString("N"),   // token không đoán được
        ExpiresAt = DateTime.UtcNow.AddDays(7)
    };
    _db.ProjectInvitations.Add(invitation);
    await _db.SaveChangesAsync();

    // Phát event — NotifyService sẽ gửi email và tạo in-app notification
    await _eventBus.PublishAsync("project.member.invited", new
    {
        ProjectId = projectId,
        ProjectName = project.Name,
        InviterId = invitation.InviterId,
        InviterName = inviter.FullName,
        InviteeEmail = invitation.InviteeEmail,
        InviteeId = invitation.InviteeId,
        Token = invitation.Token,
        Role = invitation.Role.ToString(),
        ExpiresAt = invitation.ExpiresAt
    });

    return Ok(new { message = "Đã gửi lời mời." });
}
```

**`ProjectService/Controllers/InvitationController.cs`** — hai endpoint xử lý click từ email hoặc app:

```csharp
[ApiController]
[Route("api/invitations")]
public class InvitationController : ControllerBase
{
    private readonly ProjectDbContext _db;
    private readonly IEventBus _eventBus;
    private readonly IConfiguration _config;

    // Accept qua link email — không cần JWT (user click link từ email)
    [HttpGet("accept")]
    public async Task<IActionResult> AcceptViaEmail([FromQuery] string token)
    {
        return await HandleInvitation(token, accept: true, userId: null);
    }

    // Decline qua link email
    [HttpGet("decline")]
    public async Task<IActionResult> DeclineViaEmail([FromQuery] string token)
    {
        return await HandleInvitation(token, accept: false, userId: null);
    }

    // Accept trong app — cần JWT
    [HttpPost("{token}/accept")]
    [Authorize]
    public async Task<IActionResult> AcceptInApp(string token)
    {
        return await HandleInvitation(token, accept: true, userId: User.GetUserId());
    }

    // Decline trong app
    [HttpPost("{token}/decline")]
    [Authorize]
    public async Task<IActionResult> DeclineInApp(string token)
    {
        return await HandleInvitation(token, accept: false, userId: User.GetUserId());
    }

    private async Task<IActionResult> HandleInvitation(string token, bool accept, Guid? userId)
    {
        var invitation = await _db.ProjectInvitations
            .Include(i => i.Project)
            .FirstOrDefaultAsync(i => i.Token == token);

        if (invitation == null)
            return NotFound(new { message = "Lời mời không tồn tại." });

        if (invitation.Status != InvitationStatus.Pending)
            return BadRequest(new { message = "Lời mời này đã được phản hồi trước đó." });

        if (invitation.ExpiresAt < DateTime.UtcNow)
        {
            invitation.Status = InvitationStatus.Expired;
            await _db.SaveChangesAsync();
            return BadRequest(new { message = "Lời mời đã hết hạn." });
        }

        invitation.Status = accept ? InvitationStatus.Accepted : InvitationStatus.Declined;
        invitation.RespondedAt = DateTime.UtcNow;

        if (accept)
        {
            // Thêm thành viên thật vào dự án
            _db.Members.Add(new ProjectMember
            {
                ProjectId = invitation.ProjectId,
                UserId = invitation.InviteeId ?? userId!.Value,
                Role = invitation.Role,
                JoinedAt = DateTime.UtcNow
            });

            // Phát event để NotifyService thông báo cho Owner/Manager biết kết quả
            await _eventBus.PublishAsync("project.member.added", new
            {
                ProjectId = invitation.ProjectId,
                UserId = invitation.InviteeId ?? userId!.Value,
                Role = invitation.Role.ToString()
            });
        }
        else
        {
            await _eventBus.PublishAsync("project.invitation.declined", new
            {
                ProjectId = invitation.ProjectId,
                InviteeEmail = invitation.InviteeEmail,
                InviterId = invitation.InviterId
            });
        }

        await _db.SaveChangesAsync();

        // Redirect về app thay vì trả JSON thuần — UX tốt hơn khi click từ email
        var appUrl = _config["App:BaseUrl"];
        var redirectUrl = accept
            ? $"{appUrl}/projects/{invitation.ProjectId}?joined=true"
            : $"{appUrl}?invitation=declined";

        return Redirect(redirectUrl);
    }
}
```

---

**`NotifyService.Api/Consumers/ProjectEventConsumer.cs`** — thêm xử lý event `project.member.invited`:

```csharp
case "project.member.invited":
    await HandleMemberInvitedAsync(payload);
    break;

case "project.invitation.declined":
    await HandleInvitationDeclinedAsync(payload);
    break;
```

```csharp
private async Task HandleMemberInvitedAsync(dynamic payload)
{
    var appUrl = _config["App:BaseUrl"];
    var token = (string)payload.Token;

    var acceptUrl  = $"{appUrl}/api/invitations/accept?token={token}";
    var declineUrl = $"{appUrl}/api/invitations/decline?token={token}";

    // 1. Gửi email có nút Accept / Decline
    var user = await _userService.GetByEmailAsync((string)payload.InviteeEmail);
    if (user != null)
    {
        await _emailService.SendAsync(
            toEmail: user.Email,
            toName: user.FullName,
            subject: $"Bạn được mời vào dự án {payload.ProjectName}",
            htmlContent: EmailTemplateService.ProjectInvitation(
                inviterName: (string)payload.InviterName,
                projectName: (string)payload.ProjectName,
                acceptUrl: acceptUrl,
                declineUrl: declineUrl)
        );

        // 2. Tạo in-app notification kèm metadata token để frontend render nút
        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            UserId = user.Id,
            Title = $"{payload.InviterName} mời bạn vào dự án",
            Body = $"Dự án: {payload.ProjectName}",
            Type = "project_invitation",
            Metadata = new Dictionary<string, string>
            {
                { "projectId",   (string)payload.ProjectId.ToString() },
                { "projectName", (string)payload.ProjectName },
                { "token",       token },
                { "role",        (string)payload.Role }
            }
        });
    }
}

private async Task HandleInvitationDeclinedAsync(dynamic payload)
{
    // Thông báo cho người mời biết lời mời bị từ chối
    var inviter = await _userService.GetByIdAsync((Guid)payload.InviterId);
    if (inviter != null)
    {
        await _notificationService.CreateAsync(new CreateNotificationDto
        {
            UserId = inviter.Id,
            Title = "Lời mời bị từ chối",
            Body = $"{payload.InviteeEmail} đã từ chối lời mời vào dự án.",
            Type = "invitation_declined",
            Metadata = new Dictionary<string, string>
            {
                { "projectId", (string)payload.ProjectId.ToString() }
            }
        });
    }
}
```

---

### In-app notification — frontend render nút

Notification có `Type = "project_invitation"` thì frontend render khác với notification thông thường — hiển thị thêm hai nút:

```vue
<!-- NotificationItem.vue -->
<template>
  <div class="notification-item">
    <p>{{ notification.body }}</p>

    <!-- Chỉ render nút khi là lời mời -->
    <div v-if="notification.type === 'project_invitation' && !responded">
      <button
        @click="respond('accept')"
        class="btn-accept"
        :disabled="loading">
        ✅ Chấp nhận
      </button>
      <button
        @click="respond('decline')"
        class="btn-decline"
        :disabled="loading">
        ❌ Từ chối
      </button>
    </div>
    <p v-if="responded" class="responded-text">{{ respondedMessage }}</p>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import axios from 'axios'

const props = defineProps(['notification'])
const loading = ref(false)
const responded = ref(false)
const respondedMessage = ref('')

async function respond(action) {
  loading.value = true
  const token = props.notification.metadata.token
  try {
    await axios.post(`/api/invitations/${token}/${action}`)
    responded.value = true
    respondedMessage.value = action === 'accept'
      ? '✅ Đã tham gia dự án'
      : '❌ Đã từ chối lời mời'
    if (action === 'accept') {
      // Reload danh sách dự án
      emit('project-joined', props.notification.metadata.projectId)
    }
  } finally {
    loading.value = false
  }
}
</script>
```

---

### Cấu hình `appsettings.json`

Thêm vào `ProjectService/appsettings.json`:

```json
{
  "App": {
    "BaseUrl": "http://localhost:5173"
  }
}
```

---

### Điều kiện hoàn thành F-F

- [ ] Migration `AddProjectInvitations` chạy thành công
- [ ] Mời thành viên tạo bản ghi `ProjectInvitations` trạng thái Pending thay vì thêm thành viên ngay
- [ ] Email gửi đi có đúng hai nút Accept (xanh) và Decline (đỏ) với link chứa token
- [ ] Click Accept trong email → redirect về app, thành viên được thêm vào dự án
- [ ] Click Decline trong email → redirect về app, bản ghi chuyển Declined
- [ ] In-app notification loại `project_invitation` hiển thị hai nút Chấp nhận / Từ chối
- [ ] Sau khi phản hồi (cả email lẫn app), token không dùng được lần hai — trả lỗi "Lời mời đã được phản hồi"
- [ ] Token hết hạn sau 7 ngày → trả lỗi "Lời mời đã hết hạn"
- [ ] Không mời người đã là thành viên → 400
- [ ] Không tạo invitation Pending trùng cho cùng email trong cùng dự án → 400
- [ ] Người mời nhận in-app notification khi lời mời bị từ chối

---

## Lưu ý chung cho cả 5 tính năng

**API key không được commit vào git.** Chỉ để trong `appsettings.Development.json` (đã có trong `.gitignore`) hoặc biến môi trường Docker. Kiểm tra `.gitignore` trước khi push.

**Thứ tự thực hiện trong mỗi tính năng:**
1. Cài package và cấu hình `appsettings`
2. Tạo các file service mới
3. Sửa `Program.cs` đăng ký DI
4. Tạo controller
5. Build và kiểm tra compile
6. Test từng endpoint bằng Swagger

**Xử lý lỗi nhất quán:** mọi exception từ external service (Anthropic, SendGrid) phải được bắt và chuyển thành response HTTP hợp lý, không để lộ stack trace ra client.