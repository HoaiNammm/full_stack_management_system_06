using System.Net.Http.Json;
using ProjectService.DTOs.Members;

namespace ProjectService.HttpClients;

public class NotifyServiceClient
{
    private readonly HttpClient _http;

    public NotifyServiceClient(HttpClient http) => _http = http;

    public async Task<UserInfo?> GetUserAsync(Guid userId)
    {
        try
        {
            return await _http.GetFromJsonAsync<UserInfo>($"/internal/users/{userId}");
        }
        catch { return null; }
    }

    public async Task<UserInfo?> GetUserByEmailAsync(string email)
    {
        try
        {
            return await _http.GetFromJsonAsync<UserInfo>($"/internal/users/by-email?email={Uri.EscapeDataString(email)}");
        }
        catch { return null; }
    }

    public async Task<List<UserInfo>> GetUsersAsync(IEnumerable<Guid> ids)
    {
        try
        {
            var response = await _http.PostAsJsonAsync("/internal/users/batch", ids);
            return await response.Content.ReadFromJsonAsync<List<UserInfo>>() ?? new();
        }
        catch { return new(); }
    }
}
