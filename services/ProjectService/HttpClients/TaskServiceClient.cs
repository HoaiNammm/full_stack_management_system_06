using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace ProjectService.HttpClients;

public class TaskServiceClient
{
    private readonly HttpClient _http;

    public TaskServiceClient(HttpClient http) => _http = http;

    public async Task<Guid> CreateTaskAsync(string authorizationHeader, Guid projectId, object createTaskRequest)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/projects/{projectId}/tasks")
        {
            Content = JsonContent.Create(createTaskRequest)
        };
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadFromJsonAsync<CreatedTask>();
        return body!.Id;
    }

    public async Task CreateSubtaskAsync(string authorizationHeader, Guid taskId, string title)
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, $"/api/tasks/{taskId}/subtasks")
        {
            Content = JsonContent.Create(new { title })
        };
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);

        var response = await _http.SendAsync(request);
        response.EnsureSuccessStatusCode();
    }

    public async Task<List<TaskInfo>> GetTasksByProjectAsync(string authorizationHeader, Guid projectId)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"/api/projects/{projectId}/tasks");
        request.Headers.Authorization = AuthenticationHeaderValue.Parse(authorizationHeader);

        var response = await _http.SendAsync(request);
        if (!response.IsSuccessStatusCode) return new List<TaskInfo>();

        return await response.Content.ReadFromJsonAsync<List<TaskInfo>>() ?? new List<TaskInfo>();
    }

    private class CreatedTask
    {
        public Guid Id { get; set; }
    }

    public class TaskInfo
    {
        public Guid Id { get; set; }
        public Guid? SprintId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string Priority { get; set; } = string.Empty;
        public TaskAssigneeInfo? Assignee { get; set; }
        public DateTime? Deadline { get; set; }
        public int SubTaskCount { get; set; }
        public int CompletedSubTaskCount { get; set; }
    }

    public class TaskAssigneeInfo
    {
        public string Name { get; set; } = string.Empty;
    }
}
