using RollUp.Application.DTOs;

namespace RollUp.Core.Interfaces;

public interface IAuthService
{
    Task<AuthResponseDto?> LoginAsync(LoginRequestDto request);
    Task<AuthResponseDto?> RegisterAsync(RegisterRequestDto request);
    Task<OnboardingResponseDto> OnboardTenantAsync(TenantOnboardingRequestDto request);
    Task<bool> ChangePasswordAsync(int userId, string currentPassword, string newPassword);
}
