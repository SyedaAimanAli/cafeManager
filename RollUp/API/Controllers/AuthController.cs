using RollUp.Application.DTOs;
using RollUp.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace RollUp.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    /// <summary>Authenticate and receive a JWT token.</summary>
    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto request)
    {
        var result = await _auth.LoginAsync(request);
        if (result is null)
            return Unauthorized(new { message = "Invalid email or password." });

        return Ok(result);
    }

    /// <summary>Onboard a new business/tenant with business info and admin user.</summary>
    [HttpPost("onboard")]
    [AllowAnonymous]
    public async Task<IActionResult> Onboard([FromBody] TenantOnboardingRequestDto request)
    {
        try
        {
            var result = await _auth.OnboardTenantAsync(request);
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    /// <summary>Register a new staff user.</summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin,Manager")]
    public async Task<IActionResult> Register([FromBody] RegisterRequestDto request)
    {
        var result = await _auth.RegisterAsync(request);
        if (result is null)
            return Conflict(new { message = "A user with this email already exists." });

        return CreatedAtAction(nameof(Login), result);
    }

    /// <summary>Change the currently authenticated user's password.</summary>
    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDto request)
    {
        var userId = int.Parse(User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value ?? "0");
        var ok = await _auth.ChangePasswordAsync(userId, request.CurrentPassword, request.NewPassword);
        if (!ok) return BadRequest(new { message = "Current password is incorrect." });
        return NoContent();
    }
}

public class ChangePasswordDto
{
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
