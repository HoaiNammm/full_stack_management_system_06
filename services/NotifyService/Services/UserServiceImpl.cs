using Microsoft.EntityFrameworkCore;
using NotifyService.Data;
using NotifyService.DTOs.Auth;
using NotifyService.DTOs.Users;

namespace NotifyService.Services;

public class UserServiceImpl : IUserService
{
    private readonly NotifyDbContext _db;

    public UserServiceImpl(NotifyDbContext db) => _db = db;

    public async Task<UserDto?> GetByIdAsync(Guid id)
    {
        var user = await _db.Users.FindAsync(id);
        return user is null ? null : MapToDto(user);
    }

    public async Task<UserDto?> GetByEmailAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email.ToLower());
        return user is null ? null : MapToDto(user);
    }

    public async Task<List<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids)
    {
        var users = await _db.Users
            .Where(u => ids.Contains(u.Id))
            .ToListAsync();
        return users.Select(MapToDto).ToList();
    }

    public async Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        if (request.Name is not null) user.Name = request.Name;
        if (request.AvatarUrl is not null) user.AvatarUrl = request.AvatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        return MapToDto(user);
    }

    public async Task ChangePasswordAsync(Guid id, ChangePasswordRequest request)
    {
        var user = await _db.Users.FindAsync(id)
            ?? throw new KeyNotFoundException("User not found.");

        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
            throw new UnauthorizedAccessException("Current password is incorrect.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

    private static UserDto MapToDto(Models.User u) => new()
    {
        Id = u.Id,
        Name = u.Name,
        Email = u.Email,
        AvatarUrl = u.AvatarUrl,
        CreatedAt = u.CreatedAt,
        UpdatedAt = u.UpdatedAt
    };
}
