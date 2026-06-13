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
        await _context.Users.Where(u => u.IsActive).OrderBy(u => u.FullName).ToListAsync();

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
            IsActive     = true,
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

        user.LastLoginAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
    }
}
