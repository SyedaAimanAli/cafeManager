using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using RollUp.Core.Entities;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace RollUp.Infrastructure.Authentication;

public class JwtProvider
{
    private readonly IConfiguration _config;

    public JwtProvider(IConfiguration config)
    {
        _config = config;
    }

    public (string token, DateTime expiresAt) Generate(User user, Tenant? tenant = null)
    {
        var secret = _config["Jwt:Secret"]
            ?? throw new InvalidOperationException("Jwt:Secret is not configured.");
        var issuer   = _config["Jwt:Issuer"]   ?? "RollUp";
        var audience = _config["Jwt:Audience"] ?? "RollUp";
        var expiryMinutes = int.TryParse(_config["Jwt:ExpiryMinutes"], out var m) ? m : 480; // 8 hours default

        var key   = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret));
        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiresAt = DateTime.UtcNow.AddMinutes(expiryMinutes);

        var tenantId = user.TenantId != 0 ? user.TenantId : tenant?.Id ?? 0;
        var tenantSlug = tenant?.Slug ?? user.Tenant?.Slug ?? "";
        var tenantName = tenant?.Name ?? user.Tenant?.Name ?? "";

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub,   user.Id.ToString()),
            new(ClaimTypes.NameIdentifier,     user.Id.ToString()),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(ClaimTypes.Name,               user.FullName),
            new(ClaimTypes.Role,               user.Role.ToString()),
            new("tenant_id",                   tenantId.ToString()),
            new("tenant_slug",                 tenantSlug),
            new("tenant_name",                 tenantName),
            new("outlet_id",                   user.OutletId?.ToString() ?? ""),
            new(JwtRegisteredClaimNames.Jti,   Guid.NewGuid().ToString()),
        };

        var token = new JwtSecurityToken(
            issuer:   issuer,
            audience: audience,
            claims:   claims,
            expires:  expiresAt,
            signingCredentials: creds
        );

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }
}
