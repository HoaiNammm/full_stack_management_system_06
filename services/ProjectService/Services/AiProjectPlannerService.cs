using System.Text.Json;
using System.Text.Json.Serialization;
using ProjectService.DTOs.AiProjects;

namespace ProjectService.Services;

public class AiProjectPlannerService : IAiProjectPlannerService
{
    private readonly HttpClient _http;
    private readonly IConfiguration _config;

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    private const string SystemPrompt =
        """
        You are a senior project planning assistant embedded in a project management tool.
        Given a natural-language project description (and optionally the available team), respond with
        ONLY a single JSON object (no markdown fences, no commentary) with exactly this shape:

        {
          "name": string,
          "description": string,
          "startDate": "YYYY-MM-DD",
          "endDate": "YYYY-MM-DD" or null,
          "milestones": [ { "name": string, "description": string, "dueDate": "YYYY-MM-DD" } ],
          "sprints": [ { "name": string, "goal": string, "startDate": "YYYY-MM-DD" } ],
          "tasks": [
            {
              "title": string,
              "priority": 0|1|2|3,
              "sprintIndex": integer or null,
              "estimatedHours": number or null,
              "assigneeName": string or null,
              "subtasks": [string]
            }
          ]
        }

        Planning quality rules:
        - Hierarchy is Milestone -> Sprint -> Task -> Subtask. Milestones are checkpoints across the whole
          project (e.g. "MVP", "Beta", "Release Candidate", "Production Launch" for software — adapt the
          naming to whatever domain the project is actually in). Give 2-4 milestones with a dueDate that
          lines up with a sprint boundary.
        - Each sprint must be built around ONE clear goal or feature area (e.g. "Authentication & user
          accounts", "Checkout & payments"). NEVER split the same feature across sprints just to fill time,
          and NEVER name sprints things like "Phase 1" / "Phase 2" / "Continuation" / "Wrap-up" — every
          sprint name should say what gets built.
        - priority: 0=Low, 1=Medium, 2=High, 3=Urgent. Give earlier sprints/milestones a higher share of
          High/Urgent tasks (foundational work first); later sprints can carry more Low/Medium polish work.
        - sprintIndex is the zero-based index into the "sprints" array, or null if the task isn't tied to
          a sprint (e.g. ongoing/ops tasks).
        - Scale the NUMBER of sprints and tasks to the stated project duration and team size: roughly one
          sprint per 2 weeks of duration, and roughly 3-6 concrete tasks per sprint per team member (use
          judgement — a 1-week solo project needs far fewer tasks than a 6-month 5-person project). Never
          produce a plan with fewer than 2 sprints or fewer than 6 tasks total.
        - Every task title must be concrete and distinct — describe the actual piece of work. Do not use
          generic/repetitive titles like "Part 1", "Continue X", "Finalize X", "More work on X".
        - Give each task 1-3 short, concrete subtasks (skip subtasks only for trivial one-step tasks).
        - estimatedHours is your best-effort estimate of the effort for that task alone (not its subtasks
          combined), as a plain number of hours (e.g. 4, 8, 16, 40). Never leave it null unless truly
          unknowable.
        - If a list of team members is provided in the user message, set assigneeName to the single best-
          fit member's name for each task (match their stated role to the type of work), and spread tasks
          roughly evenly across everyone so no one person is overloaded. If no team members are provided,
          leave assigneeName null for every task.
        - If no explicit start date is given, use today's date.
        """;

    private const string ReportSystemPrompt =
        """
        You are an assistant embedded in a project management tool. You are given a structured
        summary of a single project's current data (status, sprints, milestones, tasks, team
        workload) followed by a question from a project member.

        Answer ONLY using the data provided in the summary — never invent tasks, dates, or names
        that are not present in it. If the summary doesn't contain enough information to answer,
        say so plainly instead of guessing.

        Be concise and concrete: cite actual numbers, task titles, and member names from the
        summary. Reply in the same language the question was asked in. Do not use markdown
        headers or code fences — short paragraphs or a simple bullet list are fine.
        """;

    public AiProjectPlannerService(HttpClient http, IConfiguration config)
    {
        _http = http;
        _config = config;
    }

    public async Task<AiProjectPlanDto> GeneratePlanAsync(string prompt, List<AiTeamMemberInfo> teamMembers)
    {
        var today = DateTime.UtcNow.ToString("yyyy-MM-dd");
        var userMessage =
            $"Today's date is {today}.\n\n" +
            $"{BuildTeamContext(teamMembers)}" +
            $"Project description:\n{prompt}";
        return await CallGroqAsync(userMessage);
    }

    public async Task<AiProjectPlanDto> RefinePlanAsync(AiProjectPlanDto currentPlan, string refinement, List<AiTeamMemberInfo> teamMembers)
    {
        var currentPlanJson = JsonSerializer.Serialize(currentPlan, new JsonSerializerOptions
        {
            DefaultIgnoreCondition = JsonIgnoreCondition.Never
        });
        var userMessage =
            $"{BuildTeamContext(teamMembers)}" +
            $"Here is the current project plan as JSON:\n{currentPlanJson}\n\n" +
            $"Apply this change and return the FULL updated plan as JSON in the same shape:\n{refinement}";
        return await CallGroqAsync(userMessage);
    }

    private static string BuildTeamContext(List<AiTeamMemberInfo> teamMembers)
    {
        if (teamMembers.Count == 0) return "";
        var lines = teamMembers.Select(m => $"- {m.Name} ({m.Role})");
        return "Available team members:\n" + string.Join("\n", lines) + "\n\n";
    }

    public async Task<string> AnswerReportQuestionAsync(string projectContext, string question)
    {
        var userMessage = $"{projectContext}\n\nQuestion: {question}";
        var requestBody = new
        {
            model = _config["Groq:Model"] ?? "llama-3.3-70b-versatile",
            temperature = 0.3,
            messages = new object[]
            {
                new { role = "system", content = ReportSystemPrompt },
                new { role = "user", content = userMessage }
            }
        };
        return await GetCompletionContentAsync(requestBody);
    }

    private async Task<AiProjectPlanDto> CallGroqAsync(string userMessage)
    {
        var requestBody = new
        {
            model = _config["Groq:Model"] ?? "llama-3.3-70b-versatile",
            response_format = new { type = "json_object" },
            temperature = 0.4,
            messages = new object[]
            {
                new { role = "system", content = SystemPrompt },
                new { role = "user", content = userMessage }
            }
        };

        var content = await GetCompletionContentAsync(requestBody);

        AiProjectPlanDto plan;
        try
        {
            plan = JsonSerializer.Deserialize<AiProjectPlanDto>(content, JsonOptions)
                ?? throw new InvalidOperationException("AI service returned an invalid plan.");
        }
        catch (JsonException)
        {
            throw new InvalidOperationException("Could not parse the AI-generated project plan.");
        }

        Normalize(plan);
        return plan;
    }

    private async Task<string> GetCompletionContentAsync(object requestBody)
    {
        HttpResponseMessage response;
        try
        {
            response = await _http.PostAsJsonAsync("chat/completions", requestBody);
        }
        catch (HttpRequestException)
        {
            // Transient network hiccups happen occasionally when calling out to Groq; one retry is enough.
            response = await _http.PostAsJsonAsync("chat/completions", requestBody);
        }

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            throw new InvalidOperationException($"AI service error ({(int)response.StatusCode}): {errorBody}");
        }

        var completion = await response.Content.ReadFromJsonAsync<GroqChatCompletion>(JsonOptions)
            ?? throw new InvalidOperationException("AI service returned an empty response.");

        return completion.Choices.FirstOrDefault()?.Message?.Content
            ?? throw new InvalidOperationException("AI service returned no content.");
    }

    private static void Normalize(AiProjectPlanDto plan)
    {
        plan.Milestones ??= new();
        plan.Sprints ??= new();
        plan.Tasks ??= new();

        foreach (var sprint in plan.Sprints)
        {
            if (sprint.EndDate <= sprint.StartDate)
                sprint.EndDate = sprint.StartDate.AddDays(14);
        }

        foreach (var task in plan.Tasks)
        {
            task.Subtasks ??= new();
            task.Priority = Math.Clamp(task.Priority, 0, 3);
            if (task.SprintIndex is < 0 || task.SprintIndex >= plan.Sprints.Count)
                task.SprintIndex = null;
            if (task.EstimatedHours is <= 0)
                task.EstimatedHours = null;
        }
    }

    private class GroqChatCompletion
    {
        public List<GroqChoice> Choices { get; set; } = new();
    }

    private class GroqChoice
    {
        public GroqMessage? Message { get; set; }
    }

    private class GroqMessage
    {
        public string? Content { get; set; }
    }
}
