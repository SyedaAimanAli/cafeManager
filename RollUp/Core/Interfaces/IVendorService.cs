using RollUp.Application.DTOs;

namespace RollUp.Core.Interfaces;

public interface IVendorService
{
    Task<VendorDto?> GetByIdAsync(int id);
    Task<IEnumerable<VendorDto>> GetAllAsync();
    Task<VendorDto> CreateAsync(CreateVendorDto dto);
    Task<VendorDto?> UpdateAsync(int id, CreateVendorDto dto);
    Task<bool> DeleteAsync(int id);
}
