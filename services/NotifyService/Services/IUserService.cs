using NotifyService.DTOs.Auth;
using NotifyService.DTOs.Users;

namespace NotifyService.Services;

public interface IUserService
{
    Task<UserDto?> GetByIdAsync(Guid id);
    Task<UserDto?> GetByEmailAsync(string email);
    Task<List<UserDto>> GetByIdsAsync(IEnumerable<Guid> ids);
    Task<UserDto> UpdateAsync(Guid id, UpdateUserRequest request);
    Task ChangePasswordAsync(Guid id, ChangePasswordRequest request);
}
