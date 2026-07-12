using System.Net.Http.Json;

namespace TaskService.HttpClients;

// Calls ProjectService (N1) /internal endpoints
public class WorkspaceServiceClient
{
    private readonly HttpClient _http;
    public WorkspaceServiceClient(HttpClient http) => _http = http;

    public async Task<List<Guid>> GetProjectMemberIdsAsync(Guid projectId)
    {
        try { return await _http.GetFromJsonAsync<List<Guid>>($"/internal/projects/{projectId}/member-ids") ?? new(); }
        catch { return new(); }
    }

    public async Task<bool> IsProjectMemberAsync(Guid projectId, Guid userId)
        => (await GetProjectMemberIdsAsync(projectId)).Contains(userId);
}

public class NotifyServiceClient
{
    private readonly HttpClient _http;
    public NotifyServiceClient(HttpClient http) => _http = http;

    public async Task CreateNotificationAsync(object payload)
    {
        try { await _http.PostAsJsonAsync("/internal/notifications", payload); }
        catch { /* best-effort */ }
    }
}

public class UserServiceClient
{
    private readonly HttpClient _http;
    public UserServiceClient(HttpClient http) => _http = http;

    public async Task<UserInfo?> GetUserAsync(Guid userId)
    {
        try { return await _http.GetFromJsonAsync<UserInfo>($"/internal/users/{userId}"); }
        catch { return null; }
    }

    public async Task<List<UserInfo>> GetUsersAsync(IEnumerable<Guid> ids)
    {
        try
        {
            var res = await _http.PostAsJsonAsync("/internal/users/batch", ids);
            return await res.Content.ReadFromJsonAsync<List<UserInfo>>() ?? new();
        }
        catch { return new(); }
    }
}

public class UserInfo
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Email { get; set; }
    public string? AvatarUrl { get; set; }
}
