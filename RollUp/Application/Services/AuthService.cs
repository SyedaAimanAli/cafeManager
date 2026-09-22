using System.Text.RegularExpressions;
using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Enums;
using RollUp.Core.Interfaces;
using RollUp.Infrastructure.Authentication;
using RollUp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace RollUp.Application.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly JwtProvider _jwt;
    private readonly ITenantContext _tenantContext;

    public AuthService(AppDbContext db, JwtProvider jwt, ITenantContext tenantContext)
    {
        _db = db;
        _jwt = jwt;
        _tenantContext = tenantContext;
    }

    public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request)
    {
        // Bypass query filter to find user across tenants on login if needed
        var user = await _db.Users
            .IgnoreQueryFilters()
            .Include(u => u.Tenant)
            .FirstOrDefaultAsync(u => u.Email.ToLower() == request.Email.ToLower() && !u.IsDeleted && u.IsActive);

        if (user is null) return null;

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            return null;

        // Set active tenant context
        if (user.Tenant != null)
        {
            _tenantContext.SetTenant(user.TenantId, user.Tenant.Slug, user.Tenant.Name);
        }

        var (token, expiresAt) = _jwt.Generate(user, user.Tenant);
        return BuildResponse(user, user.Tenant, token, expiresAt);
    }

    public async Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request)
    {
        var emailLower = request.Email.ToLower();
        var exists = await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.ToLower() == emailLower && !u.IsDeleted);
        if (exists) return null; // email taken

        var role = Enum.TryParse<Role>(request.Role, true, out var r) ? r : Role.Cashier;
        var tenantId = request.TenantId ?? _tenantContext.CurrentTenantId ?? 1;

        var user = new User
        {
            FullName     = request.FullName,
            Email        = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role         = role,
            TenantId     = tenantId,
            OutletId     = request.OutletId
        };

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        var tenant = await _db.Tenants.FindAsync(tenantId);
        var (token, expiresAt) = _jwt.Generate(user, tenant);
        return BuildResponse(user, tenant, token, expiresAt);
    }

    public async Task<OnboardingResponseDto> OnboardTenantAsync(TenantOnboardingRequestDto request)
    {
        var emailLower = request.AdminEmail.ToLower();
        var emailExists = await _db.Users.IgnoreQueryFilters().AnyAsync(u => u.Email.ToLower() == emailLower && !u.IsDeleted);
        if (emailExists)
        {
            throw new InvalidOperationException($"An account with email '{request.AdminEmail}' already exists.");
        }

        // Generate clean unique slug
        var baseSlug = !string.IsNullOrWhiteSpace(request.Slug) 
            ? Slugify(request.Slug) 
            : Slugify(request.BusinessName);
        
        var uniqueSlug = baseSlug;
        var counter = 1;
        while (await _db.Tenants.IgnoreQueryFilters().AnyAsync(t => t.Slug == uniqueSlug))
        {
            uniqueSlug = $"{baseSlug}-{counter++}";
        }

        // 1. Create Tenant & Vendor
        var tenant = new Tenant
        {
            Name         = request.BusinessName.Trim(),
            Slug         = uniqueSlug,
            LogoUrl      = request.LogoUrl,
            Country      = request.Country,
            City         = request.City,
            Currency     = string.IsNullOrWhiteSpace(request.Currency) ? "USD" : request.Currency.ToUpper(),
            ContactEmail = request.AdminEmail,
            ContactPhone = request.ContactPhone ?? string.Empty,
            Address      = request.Address ?? string.Empty,
            Tagline      = request.Tagline,
            IsActive     = true
        };

        _db.Tenants.Add(tenant);

        var vendor = new Vendor
        {
            Name         = tenant.Name,
            ContactEmail = tenant.ContactEmail,
            ContactPhone = tenant.ContactPhone,
            Address      = tenant.Address,
            IsActive     = true
        };
        _db.Vendors.Add(vendor);

        await _db.SaveChangesAsync();

        // 2. Create Default Outlet
        var outlet = new Outlet
        {
            Name      = "Main Branch",
            TenantId  = tenant.Id,
            VendorId  = vendor.Id,
            Address   = !string.IsNullOrWhiteSpace(request.Address) ? request.Address : $"{request.City}, {request.Country}",
            Phone     = request.ContactPhone ?? string.Empty,
            IsActive  = true
        };

        _db.Outlets.Add(outlet);
        await _db.SaveChangesAsync();

        // 3. Create Default Categories
        var defaultCategories = new List<Category>
        {
            new() { Name = "Hot Coffee", SortOrder = 1, TenantId = tenant.Id },
            new() { Name = "Cold Brews & Iced", SortOrder = 2, TenantId = tenant.Id },
            new() { Name = "Bakery & Pastries", SortOrder = 3, TenantId = tenant.Id },
            new() { Name = "Artisan Sandwiches", SortOrder = 4, TenantId = tenant.Id },
            new() { Name = "Desserts", SortOrder = 5, TenantId = tenant.Id }
        };

        _db.Categories.AddRange(defaultCategories);

        // 4. Create Admin User
        var adminUser = new User
        {
            FullName     = request.AdminFullName.Trim(),
            Email        = request.AdminEmail.Trim(),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.AdminPassword),
            Role         = Role.Admin,
            TenantId     = tenant.Id,
            OutletId     = outlet.Id,
            IsActive     = true
        };

        _db.Users.Add(adminUser);
        await _db.SaveChangesAsync();

        // 5. Set Tenant Context & Generate JWT
        _tenantContext.SetTenant(tenant.Id, tenant.Slug, tenant.Name);
        var (token, expiresAt) = _jwt.Generate(adminUser, tenant);

        return new OnboardingResponseDto
        {
            Token     = token,
            ExpiresAt = expiresAt,
            OutletId  = outlet.Id,
            Tenant    = MapTenantDto(tenant),
            User      = MapUserDto(adminUser, tenant)
        };
    }

    public async Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword)
    {
        var user = await _db.Users.FindAsync(userId);
        if (user is null) return false;

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
            return false;

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        _db.Users.Update(user);
        await _db.SaveChangesAsync();
        return true;
    }

    private static AuthResponseDto BuildResponse(User user, Tenant? tenant, string token, DateTime expiresAt) => new()
    {
        Token     = token,
        ExpiresAt = expiresAt,
        Tenant    = tenant != null ? MapTenantDto(tenant) : null,
        User      = MapUserDto(user, tenant)
    };

    private static UserDto MapUserDto(User user, Tenant? tenant) => new()
    {
        Id         = user.Id,
        FullName   = user.FullName,
        Email      = user.Email,
        Role       = user.Role.ToString(),
        TenantId   = user.TenantId,
        TenantSlug = tenant?.Slug ?? user.Tenant?.Slug,
        TenantName = tenant?.Name ?? user.Tenant?.Name,
        OutletId   = user.OutletId
    };

    private static TenantDto MapTenantDto(Tenant tenant) => new()
    {
        Id           = tenant.Id,
        Name         = tenant.Name,
        Slug         = tenant.Slug,
        LogoUrl      = tenant.LogoUrl,
        Country      = tenant.Country,
        City         = tenant.City,
        Currency     = tenant.Currency,
        ContactEmail = tenant.ContactEmail,
        ContactPhone = tenant.ContactPhone,
        Address      = tenant.Address,
        Tagline      = tenant.Tagline,
        IsActive     = tenant.IsActive
    };

    private static string Slugify(string phrase)
    {
        if (string.IsNullOrWhiteSpace(phrase)) return "tenant";
        var str = phrase.ToLowerInvariant();
        str = Regex.Replace(str, @"[^a-z0-9\s-]", "");
        str = Regex.Replace(str, @"\s+", " ").Trim();
        str = Regex.Replace(str, @"\s", "-");
        return string.IsNullOrWhiteSpace(str) ? "tenant" : str;
    }
}
