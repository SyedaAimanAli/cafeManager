using RollUp.Application.DTOs;
using RollUp.Core.Entities;
using RollUp.Core.Interfaces;

namespace RollUp.Application.Services;

public class VendorService : IVendorService
{
    private readonly IRepository<Vendor> _vendors;

    public VendorService(IRepository<Vendor> vendors)
    {
        _vendors = vendors;
    }

    public async Task<VendorDto?> GetByIdAsync(int id)
    {
        var vendor = await _vendors.GetByIdAsync(id);
        return vendor is null ? null : MapToDto(vendor);
    }

    public async Task<IEnumerable<VendorDto>> GetAllAsync()
    {
        var vendors = await _vendors.GetAllAsync();
        return vendors.Select(MapToDto);
    }

    public async Task<VendorDto> CreateAsync(CreateVendorDto dto)
    {
        var vendor = new Vendor
        {
            Name         = dto.Name,
            ContactEmail = dto.ContactEmail,
            ContactPhone = dto.ContactPhone,
            Address      = dto.Address
        };
        await _vendors.AddAsync(vendor);
        await _vendors.SaveChangesAsync();
        return MapToDto(vendor);
    }

    public async Task<VendorDto?> UpdateAsync(int id, CreateVendorDto dto)
    {
        var vendor = await _vendors.GetByIdAsync(id);
        if (vendor is null) return null;

        vendor.Name         = dto.Name;
        vendor.ContactEmail = dto.ContactEmail;
        vendor.ContactPhone = dto.ContactPhone;
        vendor.Address      = dto.Address;

        _vendors.Update(vendor);
        await _vendors.SaveChangesAsync();
        return MapToDto(vendor);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var vendor = await _vendors.GetByIdAsync(id);
        if (vendor is null) return false;

        // Soft delete
        vendor.IsDeleted = true;
        _vendors.Update(vendor);
        await _vendors.SaveChangesAsync();
        return true;
    }

    private static VendorDto MapToDto(Vendor v) => new()
    {
        Id           = v.Id,
        Name         = v.Name,
        ContactEmail = v.ContactEmail,
        ContactPhone = v.ContactPhone,
        Address      = v.Address,
        IsActive     = v.IsActive,
        OutletCount  = v.Outlets?.Count ?? 0
    };
}
