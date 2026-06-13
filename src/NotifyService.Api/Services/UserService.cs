using Microsoft.EntityFrameworkCore;
using NotifyService.Api.Data;
using NotifyService.Api.Models;

namespace NotifyService.Api.Services;

public class UserService
{
    private readonly AppDbContext _context;

    public UserService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<User>> GetAllAsync() =>
        await _context.Users.Where(u => u.Status == 1).OrderBy(u => u.FullName).ToListAsync();

    public async Task<User?> GetByIdAsync(Guid id) =>
        await _context.Users.FindAsync(id);

    public async Task<User?> GetByEmailAsync(string email) =>
        await _context.Users.FirstOrDefaultAsync(u =>
            u.Email.ToLower() == email.ToLower());

    public async Task<User> CreateAsync(string email, string fullName, string passwordHash, string role = "Member")
    {
        var user = new User
        {
            Id           = Guid.NewGuid(),
            Email        = email,
            FullName     = fullName,
            PasswordHash = passwordHash,
            Role         = role,
            Status       = 1,
            CreatedAt    = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();
        return user;
    }

    public async Task UpdateLastLoginAsync(Guid userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return;

        user.LastLogin = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }

    public async Task<User?> UpdateProfileAsync(Guid userId, string fullName, string? phoneNumber, string? avatarUrl,
        string? department, string? position)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        user.FullName = fullName.Trim();
        user.PhoneNumber = string.IsNullOrWhiteSpace(phoneNumber) ? null : phoneNumber.Trim();
        user.AvatarUrl = string.IsNullOrWhiteSpace(avatarUrl) ? null : avatarUrl.Trim();
        user.Avatar = user.AvatarUrl;
        user.Department = string.IsNullOrWhiteSpace(department) ? null : department.Trim();
        user.Position = string.IsNullOrWhiteSpace(position) ? null : position.Trim();
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return user;
    }

    public async Task<bool?> ChangePasswordAsync(Guid userId, string currentPassword, string newPassword)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            return false;
        }

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<User?> UpdateAvatarAsync(Guid userId, string avatarUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null) return null;

        user.AvatarUrl = avatarUrl;
        user.Avatar = avatarUrl;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return user;
    }
}
