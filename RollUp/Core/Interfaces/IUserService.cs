using RollUp.Application.DTOs;
using RollUp.Core.Entities;

namespace RollUp.Core.Interfaces;

public interface IUserService
{
    Task<List<UserListItemDto>> GetUsersAsync();
    Task<UserListItemDto?> CreateUserAsync(CreateUserRequestDto request);
    Task<bool> UpdateUserAsync(UpdateUserRequestDto request);
    Task<bool> DeleteUserAsync(int id);
    Task<bool> ResetPasswordAsync(int id, string newPassword);
    Task<List<Outlet>> GetOutletsAsync();
}
