using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace TaskService.Services;

public interface IProjectMembershipService
{
    Task<bool> IsMemberAsync(Guid projectId, Guid userId, string jwtToken);
}

public class HttpProjectMembershipService : IProjectMembershipService
{
    private readonly HttpClient _http;
    private readonly string     _projectServiceUrl;

    public HttpProjectMembershipService(HttpClient http, IConfiguration config)
    {
        _http              = http;
        _projectServiceUrl = config["Services:ProjectServiceUrl"] ?? "http://localhost:5047";
    }

    public async Task<bool> IsMemberAsync(Guid projectId, Guid userId, string jwtToken)
    {
        using var req = new HttpRequestMessage(
            HttpMethod.Get,
            $"{_projectServiceUrl}/api/projects/{projectId}/members");
        req.Headers.Authorization = new AuthenticationHeaderValue("Bearer", jwtToken);

        try
        {
            var resp = await _http.SendAsync(req);
            if (!resp.IsSuccessStatusCode) return false;

            var json = await resp.Content.ReadAsStringAsync();
            var body = JsonSerializer.Deserialize<MembersResponse>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return body?.Data?.Any(m => m.UserId == userId) ?? false;
        }
        catch
        {
            return false;
        }
    }

    private class MembersResponse
    {
        public bool Success { get; set; }
        public List<MemberDto>? Data { get; set; }
    }

    private class MemberDto
    {
        [JsonPropertyName("userId")]
        public Guid UserId { get; set; }
        [JsonPropertyName("role")]
        public int Role { get; set; }
    }
}
