using Microsoft.EntityFrameworkCore;
using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Interfaces;
using RollUp.Infrastructure.Persistence;

namespace RollUp.Application.Services;

public class UserService : IUserService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenantContext;

    public UserService(AppDbContext db, ITenantContext tenantContext)
    {
        _db = db;
        _tenantContext = tenantContext;
    }

    public async Task<List<UserListItemDto>> GetUsersAsync()
    {
        var tenantId = _tenantContext.CurrentTenantId;
        
        var query = _db.Users
            .Include(u => u.Outlet)
            .AsNoTracking();

        if (tenantId.HasValue)
        {
            query = query.Where(u => u.TenantId == tenantId.Value);
        }

        return await query
            .OrderBy(u => u.Role)
            .ThenBy(u => u.FullName)
            .Select(u => new UserListItemDto
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Role = u.Role,
                IsActive = u.IsActive,
                OutletId = u.OutletId,
                OutletName = u.Outlet != null ? u.Outlet.Name : "All Outlets",
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();
    }

    public async Task<UserListItemDto?> CreateUserAsync(CreateUserRequestDto request)
    {
        var tenantId = _tenantContext.CurrentTenantId ?? 1;

        // Check for duplicate email
        var emailLower = request.Email.Trim().ToLower();
        var exists = await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.ToLower() == emailLower && !u.IsDeleted);
        if (exists)
        {
            throw new InvalidOperationException($"A user with email '{request.Email}' already exists.");
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = emailLower,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = request.Role,
            TenantId = tenantId,
            OutletId = request.OutletId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var outlet = request.OutletId.HasValue 
            ? await _db.Outlets.FindAsync(request.OutletId.Value) 
            : null;

        return new UserListItemDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            IsActive = user.IsActive,
            OutletId = user.OutletId,
            OutletName = outlet?.Name ?? "All Outlets",
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<bool> UpdateUserAsync(UpdateUserRequestDto request)
    {
        var user = await _db.Users.FindAsync(request.Id);
        if (user == null) return false;

        user.FullName = request.FullName.Trim();
        user.Role = request.Role;
        user.OutletId = request.OutletId;
        user.IsActive = request.IsActive;
        user.UpdatedAt = DateTime.UtcNow;

        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> DeleteUserAsync(int id)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        user.IsDeleted = true;
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ResetPasswordAsync(int id, string newPassword)
    {
        var user = await _db.Users.FindAsync(id);
        if (user == null) return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.UpdatedAt = DateTime.UtcNow;
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Outlet>> GetOutletsAsync()
    {
        return await _db.Outlets.AsNoTracking().ToListAsync();
    }
}
