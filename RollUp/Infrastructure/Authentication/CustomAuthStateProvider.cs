using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using RollUp.Core.Interfaces;
using Microsoft.AspNetCore.Components.Authorization;

namespace RollUp.Infrastructure.Authentication;

public class CustomAuthStateProvider : AuthenticationStateProvider
{
    private readonly ILocalStorageService _localStorage;
    private readonly ITenantContext _tenantContext;
    private const string TokenKey = "rollup_token";
    private readonly AuthenticationState _anonymousState;

    public CustomAuthStateProvider(ILocalStorageService localStorage, ITenantContext tenantContext)
    {
        _localStorage = localStorage;
        _tenantContext = tenantContext;
        _anonymousState = new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
    }

    public override async Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        try
        {
            var token = await _localStorage.GetItemAsync(TokenKey);
            if (string.IsNullOrWhiteSpace(token))
            {
                _tenantContext.ClearTenant();
                return _anonymousState;
            }

            var handler = new JwtSecurityTokenHandler();
            if (!handler.CanReadToken(token))
            {
                await _localStorage.RemoveItemAsync(TokenKey);
                _tenantContext.ClearTenant();
                return _anonymousState;
            }

            var jwtToken = handler.ReadJwtToken(token);
            if (jwtToken.ValidTo < DateTime.UtcNow)
            {
                // Token expired
                await _localStorage.RemoveItemAsync(TokenKey);
                _tenantContext.ClearTenant();
                return _anonymousState;
            }

            var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
            var user = new ClaimsPrincipal(identity);

            // Populate TenantContext from claims
            var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
            var tenantSlugClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_slug")?.Value;
            var tenantNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_name")?.Value;

            if (int.TryParse(tenantIdClaim, out var tenantId) && tenantId > 0)
            {
                _tenantContext.SetTenant(tenantId, tenantSlugClaim, tenantNameClaim);
            }

            return new AuthenticationState(user);
        }
        catch
        {
            return _anonymousState;
        }
    }

    public async Task MarkUserAsAuthenticated(string token)
    {
        await _localStorage.SetItemAsync(TokenKey, token);
        
        var handler = new JwtSecurityTokenHandler();
        var jwtToken = handler.ReadJwtToken(token);
        var identity = new ClaimsIdentity(jwtToken.Claims, "jwt");
        var user = new ClaimsPrincipal(identity);

        var tenantIdClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_id")?.Value;
        var tenantSlugClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_slug")?.Value;
        var tenantNameClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "tenant_name")?.Value;

        if (int.TryParse(tenantIdClaim, out var tenantId) && tenantId > 0)
        {
            _tenantContext.SetTenant(tenantId, tenantSlugClaim, tenantNameClaim);
        }

        NotifyAuthenticationStateChanged(Task.FromResult(new AuthenticationState(user)));
    }

    public async Task MarkUserAsLoggedOut()
    {
        await _localStorage.RemoveItemAsync(TokenKey);
        _tenantContext.ClearTenant();
        NotifyAuthenticationStateChanged(Task.FromResult(_anonymousState));
    }
}
