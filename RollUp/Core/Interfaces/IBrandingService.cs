using RollUp.Application.DTOs;

namespace RollUp.Core.Interfaces;

public interface IBrandingService
{
    Task<TenantBrandingDto> GetBrandingAsync();
    Task<bool> UpdateBrandingAsync(TenantBrandingDto dto);
}
